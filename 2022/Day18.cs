using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayEighteen
   {
      private const string InputFilePath = "Input-Day18.txt";
    
      public static void SolvePartOne()
      {
         Solve(false);
      }
      
      public static void SolvePartTwo()
      {
         Solve(true);
      }
    
      public static void Solve(bool excludeInteriorSpace)
      {
         var cubes = File.ReadAllLines(InputFilePath).Select(LavaCube.FromString);
         var grid = new Grid3D(cubes);
         if (excludeInteriorSpace)
            grid.DetectInteriorSpace();
         Console.WriteLine(grid.CountSurfaceArea());
      }
   }

   public class Point3D
   {
      public readonly int X;
      public readonly int Y;
      public readonly int Z;
      
      public Point3D(int x, int y, int z)
      {
         X = x;
         Y = y;
         Z = z;
      }
      
      public Point3D OffsetBy(Point3D offset)
      {
         return new Point3D(X + offset.X, Y + offset.Y, Z + offset.Z);
      }
   }

   public interface INeighbour
   {
      // When this point neighbours another, what does it contribute
      // to that neighbour's surface area?
      public int SurfaceAreaContribution { get; }
   }

   public class LavaCube : Point3D, INeighbour
   {
      private LavaCube(int x, int y, int z)
         : base(x,y,z)
      {
      }
      
      public static LavaCube FromString(string commaSeparatedLine)
      {
         var split = commaSeparatedLine.Split(",").Select(d => int.Parse(d)).ToArray();
         return new LavaCube(split[0], split[1], split[2]);
      }
      
      public int SurfaceAreaContribution {get;} = 0;
   }

   public class Space : INeighbour
   {
      private Space() {}   
      public static readonly Space Instance = new Space();
      public int SurfaceAreaContribution {get;} = 1;
   }

   // Represents space outside the lava droplet, reachable by the water
   public class ExteriorSpace : INeighbour
   {
      private ExteriorSpace() {}
      public static readonly ExteriorSpace Instance = new ExteriorSpace();
      public int SurfaceAreaContribution {get;} = 1;
   }

   // Represents space inside the lava droplet, where the water doesn't reach
   public class InteriorSpace : INeighbour
   {
      private InteriorSpace() {}
      public static readonly InteriorSpace Instance = new InteriorSpace();
      public int SurfaceAreaContribution {get;} = 0;
   }

   public class Grid3D
   {
      private const int DimX = 0;
      private const int DimY = 1;
      private const int DimZ = 2;
      private readonly Point3D mOffset;
      private readonly int[] mCountInDimension;
      private readonly LavaCube[] mLavaCubes;
      private readonly INeighbour[,,] mGrid;
      private static readonly Point3D[] NeighbourOffsets = new [] {
         new Point3D(-1,0,0), new Point3D(1,0,0),
         new Point3D(0,-1,0), new Point3D(0,1,0),
         new Point3D(0,0,-1), new Point3D(0,0,1)
      };


      public Grid3D(IEnumerable<LavaCube> occupiedPoints)
      {
         mLavaCubes = occupiedPoints.ToArray();
         mCountInDimension = new int[3];
         // Make the grid one bigger than the occupied points, in every dimension
         var allX = mLavaCubes.Select(c => c.X);
         var allY = mLavaCubes.Select(c => c.Y);
         var allZ = mLavaCubes.Select(c => c.Z);
         mOffset = new Point3D(1 - allX.Min(), 1 - allY.Min(), 1 - allZ.Min());
         // it's +2 here because +1 for being zero-based and +1 for the outside padding
         mCountInDimension[DimX] = allX.Max() + mOffset.X + 2;
         mCountInDimension[DimY] = allY.Max() + mOffset.Y + 2;
         mCountInDimension[DimZ] = allZ.Max() + mOffset.Z + 2;
         
         mGrid = new INeighbour[mCountInDimension[DimX],mCountInDimension[DimY],mCountInDimension[DimZ]];
         
         // Everywhere is space to begin with
         for (var x = 0; x < mCountInDimension[DimX]; x++)
            for (var y = 0; y < mCountInDimension[DimY]; y++)
               for (var z = 0; z < mCountInDimension[DimZ]; z++)
                  mGrid[x,y,z] = Space.Instance;
         
         // Fill in the lava cubes
         foreach (var point in mLavaCubes)
         {
            var offsetPoint = point.OffsetBy(mOffset);
            mGrid[offsetPoint.X, offsetPoint.Y, offsetPoint.Z] = point;
         }
      }
      
      public int CountSurfaceArea()
      {
         var result = 0;
         foreach (var point in mLavaCubes)
         {
            var offsetPoint = point.OffsetBy(mOffset);
            foreach (var neighbourOffset in NeighbourOffsets)
            {
               var neighbourPoint = offsetPoint.OffsetBy(neighbourOffset);
               result += mGrid[neighbourPoint.X, neighbourPoint.Y, neighbourPoint.Z].SurfaceAreaContribution;
            }
         }
         return result;
      }
      
      public void DetectInteriorSpace()
      {      
         // By design, the outer edges of the grid are known to be exterior space
         MarkOutsideOfGrid();
            
         // Start by marking all other space as interior
         for (var x = 1; x < mCountInDimension[DimX] - 1; x++)
            for (var y = 1; y < mCountInDimension[DimY] - 1; y++)
               for (var z = 1; z < mCountInDimension[DimZ] - 1; z++)
                  if (mGrid[x,y,z] is Space)
                     mGrid[x,y,z] = InteriorSpace.Instance;
         
         // Now sweep across the space, changing any interior space to exterior if it neighbours
         // an exterior space. Keep sweeping until nothing changes.
         bool sweepAgain = true;
         while (sweepAgain)
         {
            var changesThisSweep = 0;
            sweepAgain = false;
            for (var x = 1; x < mCountInDimension[DimX] - 1; x++)
            for (var y = 1; y < mCountInDimension[DimY] - 1; y++)
            for (var z = 1; z < mCountInDimension[DimZ] - 1; z++)
            {
               var currentPoint = new Point3D(x,y,z);
               if (mGrid[currentPoint.X, currentPoint.Y, currentPoint.Z] is InteriorSpace)
               {
                  foreach (var neighbourOffset in NeighbourOffsets)
                  {
                     var neighbourPoint = currentPoint.OffsetBy(neighbourOffset);
                     if (mGrid[neighbourPoint.X, neighbourPoint.Y, neighbourPoint.Z] is ExteriorSpace)
                     {
                        mGrid[currentPoint.X, currentPoint.Y, currentPoint.Z] = ExteriorSpace.Instance;
                        sweepAgain = true;
                        changesThisSweep++;
                     }
                  }
               }            
            }
            Console.WriteLine("Changed {0} this sweep.", changesThisSweep);
         }
      }
      
      private void MarkOutsideOfGrid()
      {
         for (var y = 0; y < mCountInDimension[DimY]; y++)
            for (var z = 0; z < mCountInDimension[DimZ]; z++)
            {
               mGrid[0,y,z] = ExteriorSpace.Instance;
               mGrid[mCountInDimension[DimX]-1,y,z] = ExteriorSpace.Instance;
            }
         for (var x = 0; x < mCountInDimension[DimX]; x++)
            for (var z = 0; z < mCountInDimension[DimZ]; z++)
            {
               mGrid[x,0,z] = ExteriorSpace.Instance;
               mGrid[x,mCountInDimension[DimY]-1,z] = ExteriorSpace.Instance;
            }
         for (var x = 0; x < mCountInDimension[DimX]; x++)
            for (var y = 0; y < mCountInDimension[DimY]; y++)
            {
               mGrid[x,y,0] = ExteriorSpace.Instance;
               mGrid[x,y,mCountInDimension[DimZ]-1] = ExteriorSpace.Instance;
            }
      }
      
      public void DetectInteriorSpace_Bad()
      {
         MarkOutsideOfGrid();
         
         // Start 2 from the edge. The outside is Boundary, by design, and
         // interior space would need at least one lava cube in each direction
         for (var x = 2; x < mCountInDimension[DimX] - 2; x++)
            for (var y = 2; y < mCountInDimension[DimY] - 2; y++)
               for (var z = 2; z < mCountInDimension[DimZ] - 2; z++)
               {
                  if (mGrid[x,y,z] is Space)
                  {
                     // Essentially: if we can get from here to a boundary in any
                     // direction, without hitting lava, it's not interior space
                     // TODO: Not correct - imagine a point within an L-shaped protrusion
                     // on the surface - we'll mark it interior but it's not.
                     var isInterior = true;
                     var testPosition = new Point3D(x,y,z);
                     foreach (var directionOffset in NeighbourOffsets)
                     {
                        INeighbour kindOfCube;
                        do 
                        {
                           testPosition = testPosition.OffsetBy(directionOffset);
                           kindOfCube = mGrid[testPosition.X,testPosition.Y,testPosition.Z];
                           isInterior = !(kindOfCube is ExteriorSpace);
                        } while (isInterior && !(kindOfCube is LavaCube));
                     }
                     if (isInterior)
                     {
                        mGrid[x,y,z] = InteriorSpace.Instance;
                     }
                  }
               }
      }
      
      public override string ToString()
      {
         return string.Join("x", mCountInDimension);
      }
   }
}

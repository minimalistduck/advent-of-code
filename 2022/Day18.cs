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
         var cubes = File.ReadAllLines(InputFilePath).Select(LavaCube.FromString);
         var grid = new Grid3D(cubes);
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
      private Space()
      {
      }
      
      public static readonly Space Instance = new Space();

      public int SurfaceAreaContribution {get;} = 1;
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
         for (var x = 0; x < mCountInDimension[DimX]; x++)
            for (var y = 0; y < mCountInDimension[DimY]; y++)
               for (var z = 0; z < mCountInDimension[DimZ]; z++)
                  mGrid[x,y,z] = Space.Instance;
         foreach (var point in mLavaCubes)
         {
            var offsetPoint = point.OffsetBy(mOffset);
            mGrid[offsetPoint.X, offsetPoint.Y, offsetPoint.Z] = point;
         }
      }
      
      public int CountSurfaceArea()
      {
         var result = 0;
         var neighbourOffsets = new [] {
            new Point3D(-1,0,0), new Point3D(1,0,0),
            new Point3D(0,-1,0), new Point3D(0,1,0),
            new Point3D(0,0,-1), new Point3D(0,0,1)
         };
         foreach (var point in mLavaCubes)
         {
            var offsetPoint = point.OffsetBy(mOffset);
            foreach (var neighbourOffset in neighbourOffsets)
            {
               var neighbourPoint = offsetPoint.OffsetBy(neighbourOffset);
               result += mGrid[neighbourPoint.X, neighbourPoint.Y, neighbourPoint.Z].SurfaceAreaContribution;
            }
         }
         return result;
      }
      
      public override string ToString()
      {
         return string.Join("x", mCountInDimension);
      }
   }
}

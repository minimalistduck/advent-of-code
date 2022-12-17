using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static class DaySeventeen
   {
      private const string ExampleInput = ">>><<><>><<<>><>>><<<>>><<<><<<>><>><<>>";
      private const string InputFilePath = "Input-Day17.txt";

      public static void SolveExamplePartOne()
      {
         Console.WriteLine("Example: {0}", RockFall.Simulate(ExampleInput, 2022) + 1);
      }
      
      public static string RealInput { get { return File.ReadAllText(InputFilePath).Trim(); } }
      
      public static void SolvePartOne()
      {
         // This gives the wrong answer based on my input.
         // I know this because I generated and submitted the correct answer!
         // But I can't get the code back to a state where it generates that answer now.
         // On the example input, it matches the 3068 given in the problem statement.
         Console.WriteLine("Part one: {0}", RockFall.Simulate(RealInput, 2022) + 1);
      }
      
      // Used to demonstrate that this approach does not scale well enough
      // to give an answer for part two today.
      public static void MeasurePerformance(int rockCount)
      {
         var timer = Stopwatch.StartNew();
         RockFall.Simulate(RealInput, rockCount);
         Console.WriteLine(timer.Elapsed);
      }
   }

   public static class RockFall
   {
      public static int Simulate(string jetPattern, int rockCount)
      {
         var gasJetSource = new GasJet(jetPattern);
         var chamber = new Chamber();
         var gravity = new Point(0,-1);
         for (int r = 1; r <= rockCount; r++)
         {
            var rock = RockType.Next(chamber.NextEntryPoint);
            //Console.WriteLine($"Start rock at {chamber.NextEntryPoint}");
            do
            {
               chamber.TryMoveAcross(rock, gasJetSource.NextMovement());
            } while (chamber.TryMoveDown(rock, gravity));
            chamber.StopRock(rock);
            //Console.WriteLine(r + " " + chamber.ToString());
         }
         return chamber.TopOfTower;
      }
   }

   public class RockType
   {
      public readonly Point[] FilledPoints;

      private RockType(params int[] filledPointsXThenY)
      {
         List<Point> filledPoints = new List<Point>();
         for (int i = 0; i < filledPointsXThenY.Length; i += 2)
         {
            filledPoints.Add(new Point(filledPointsXThenY[i], filledPointsXThenY[i+1]));
         }
         FilledPoints = filledPoints.ToArray();
      }
      
      private static RockType[] mRockTypes = null;
      private static int mNextRockType = 0;
      
      public static Rock Next(Point startingPosition)
      {
         return new Rock(TakeRockType(), startingPosition);
      }
      
      private static RockType TakeRockType()
      {
         if (mRockTypes == null)
         {
            mRockTypes = new [] {
               new RockType(0,0, 1,0, 2,0, 3,0),
               new RockType(1,0, 0,1, 1,1, 2,1, 1,2),
               new RockType(0,0, 1,0, 2,0, 2,1, 2,2),
               new RockType(0,0, 0,1, 0,2, 0,3),
               new RockType(0,0, 1,0, 0,1, 1,1)
            };
         }
         var result = mRockTypes[mNextRockType];
         mNextRockType = (mNextRockType + 1) % mRockTypes.Length;
         return result;
      }
   }

   public class Rock
   {
      public readonly RockType Type;
      public Point Position;
      
      public Rock(RockType type, Point startingPosition)
      {
         Type = type;
         Position = startingPosition;
      }
      
      public IEnumerable<Point> OccupiedPositions
      {
         get
         {
            foreach (var offset in Type.FilledPoints)
            {
               var result = Position;
               result.Offset(offset);
               yield return result;
            }
         }
      }
   }

   public class GasJet
   {
      private readonly string mPattern;
      private int mNextJet = 0;

      public GasJet(string pattern)
      {
         mPattern = pattern;
      }
      
      public Point NextMovement()
      {
         char nextJet = mPattern[mNextJet];
         mNextJet = (mNextJet + 1) % mPattern.Length;
         // '<' is 60, '>' is 62, which is handy :-)
         return new Point((int)nextJet - 61, 0);
      }
   }

   public class Chamber
   {
      // Convention: Bottom, Left and Right are all *inside* the chamber
      public readonly long Bottom = 0;
      public readonly int Left = 0;
      public readonly int Right = 6;
      public int TopOfTower = -1;
      public readonly HashSet<Point> StoppedRocks = new HashSet<Point>();
      public Point NextEntryPoint => new Point(2,TopOfTower+4);

      public Chamber()
      {
      }
      
      public void StopRock(Rock rock)
      {
         StoppedRocks.UnionWith(rock.OccupiedPositions);
         TopOfTower = StoppedRocks.Max(p => p.Y);
         PurgeLowerPositions();
      }
      
      // A performance optimisation - get rid of rock positions that can no longer influence
      // where a new rock lands.
      private void PurgeLowerPositions()
      {
         int[] highestAtEachX = new int[Right - Left + 1];
         foreach (var p in StoppedRocks)
         {
            highestAtEachX[p.X] = Math.Max(p.Y, highestAtEachX[p.X]);
         }
         var lowestHigh = highestAtEachX.Min();
         StoppedRocks.RemoveWhere(p => p.Y < lowestHigh);
      }
      
      public void TryMoveAcross(Rock rockToMove, Point offset)
      {
         TryMove(rockToMove, offset, newPositions => newPositions.All(p => Left <= p.X && p.X <= Right));
      }
      
      private void TryMove(Rock rockToMove, Point offset, Func<IEnumerable<Point>,bool> isInBounds)
      {
         var newPositions = new HashSet<Point>(rockToMove.OccupiedPositions.Select(
            p => { p.Offset(offset); return p; }));
         if (isInBounds(newPositions) && !StoppedRocks.Overlaps(newPositions))
         {
            rockToMove.Position.Offset(offset);
            //Console.WriteLine($"Moved rock to {rockToMove.Position}");
         }
         else
         {
            //Console.WriteLine($"Couldn't move rock by {offset}");
         }
      }

      public bool TryMoveDown(Rock rockToMove, Point offset)
      {
         var previousPosition = rockToMove.Position;
         TryMove(rockToMove, offset, newPositions => newPositions.All(p => p.Y >= Bottom));
         return rockToMove.Position != previousPosition;
      }
      
      public override string ToString()
      {
         return $"Tower height {TopOfTower}";
      }
   }
}

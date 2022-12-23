using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayTwentyThree
   {
      public const string InputFilePath = "Input-Day23.txt";

      public static string[] RealInput
      {
         get { return System.IO.File.ReadAllLines(InputFilePath); }
      }

      public static string[] ExampleInput = new []
      {
         "....#..",
         "..###.#",
         "#...#.#",
         ".#...##",
         "#.###..",
         "##.#.##",
         ".#..#.."
      };

      [Test]
      public static void SolvePartOne()
      {
         RunSimulation(RealInput, 10);
      }

      [Test]
      public static void SolvePartTwoExample()
      {
         RunSimulation(ExampleInput, 50, verbose: true);
      }

      [Test]
      public static void SolvePartTwo()
      {
         RunSimulation(RealInput, 10_000);
      }

      private static void DrawCurrentMap(IEnumerable<Elf> elves)
      {
         var allX = elves.Select(e => e.Position.X);
         var allY = elves.Select(e => e.Position.Y);
         var minY = allY.Min();
         var yRange = allY.Max() - minY + 1;
         char[][] endMapBuilder = new char[yRange][];
         var minX = allX.Min();
         var xRange = allX.Max() - minX + 1;
         for (int row = 0; row < endMapBuilder.Length; row++)
            endMapBuilder[row] = new string('.', xRange).ToCharArray();
         foreach (var elf in elves)
         {
            endMapBuilder[elf.Position.Y - minY][elf.Position.X - minX] = '#';
         }
         StringBuilder endMap = new StringBuilder();
         foreach (var row in endMapBuilder.Reverse())
         {
            endMap.AppendLine(new string(row));
         }
         Console.WriteLine(endMap.ToString());
      }

      private static IEnumerable<Elf> RunSimulation(string[] mapLines, int roundCount, bool verbose = false)
      {
         var elves = Elf.LoadFromMap(mapLines).ToArray();
         Console.WriteLine($"Loaded {elves.Length} elves.");
         var moveRules = new MoveRules();

         var anyMoves = true;
         var r = 0;
         while (r < roundCount && anyMoves)
         {
            HashSet<Point> currentPositions = new HashSet<Point>(elves.Select(e => e.Position));
            Dictionary<Point, Elf> proposedMoves = new Dictionary<Point, Elf>();
            foreach (var elf in elves)
            {
               var notAlone = elf.HasNeighbourAtAny(Elf.SurroundingOffsets, currentPositions);
               if (notAlone)
               {
                  foreach (var possMove in moveRules.CurrentRules)
                  {
                     if (!elf.HasNeighbourAtAny(possMove.OffsetsToCheck, currentPositions))
                     {
                        // this either adds a new proposed move, or knocks out an existing one
                        var proposedPosition = elf.Position;
                        proposedPosition.Offset(possMove.DestinationOffset);
                        if (proposedMoves.ContainsKey(proposedPosition))
                        {
                           proposedMoves[proposedPosition] = null;
                        }
                        else
                        {
                           proposedMoves.Add(proposedPosition, elf);
                        }
                        break;
                     }
                  }
               }
            }
            anyMoves = false;
            foreach (var proposedMove in proposedMoves.Where(kvp => kvp.Value != null))
            {
               anyMoves = true;
               proposedMove.Value.Position = proposedMove.Key;
            }
            r++;
            moveRules.NextRound();
            if (verbose)
            {
               Console.WriteLine($"==== Round {r} ====");
               DrawCurrentMap(elves);
               Console.WriteLine();
            }
         }
         var allX = elves.Select(e => e.Position.X);
         var allY = elves.Select(e => e.Position.Y);
         var xRange = allX.Max() - allX.Min() + 1;
         var yRange = allY.Max() - allY.Min() + 1;
         var freeSquareCount = xRange * yRange - elves.Length;
         Console.WriteLine($"Number of free squares after {r} rounds is {freeSquareCount}");
         if (!anyMoves)
         {
            Console.WriteLine($"There were no moves in round {r}.");
         }
         return elves;
      }
   }

   public class Elf
   {
      public static readonly Point[] SurroundingOffsets = new []
      {
         new Point(-1,-1), new Point(0,-1), new Point(1,-1),
         new Point(-1,0),                   new Point(1,0),
         new Point(-1,1),  new Point(0,1),  new Point(1,1)
      };

      public Point Position;

      private IEnumerable<Point> NeighbouringPoints(IEnumerable<Point> offsets)
      {
         foreach (var offset in offsets)
         {
            var result = Position; // copying a value type
            result.Offset(offset);
            yield return result;
         }
      }

      public bool HasNeighbourAtAny(IEnumerable<Point> offsets, HashSet<Point> allOccupiedPositions)
      {
         var hasNeighbour = false;
         foreach (var p in NeighbouringPoints(offsets))
         {
            hasNeighbour = hasNeighbour || allOccupiedPositions.Contains(p);
         }
         return hasNeighbour;
      }

      public static IEnumerable<Elf> LoadFromMap(string[] mapLines)
      {
         for (int y = mapLines.Length; y > 0; y--)
         {
            var lineIndex = mapLines.Length - y;
            for (int x = 0; x < mapLines[lineIndex].Length; x++)
            {
               if (mapLines[lineIndex][x] == '#')
               {
                  yield return new Elf { Position = new Point(x, y) };
               }
            }
         }
      }

   }

   public class MoveRule
   {
      public readonly Point[] OffsetsToCheck;
      public readonly Point DestinationOffset;

      public MoveRule(params int[] destinationFirstThenOtherOffsets)
      {
         OffsetsToCheck = new Point[destinationFirstThenOtherOffsets.Length / 2];
         for (int i = 0; i < destinationFirstThenOtherOffsets.Length; i += 2)
         {
            OffsetsToCheck[i / 2] = new Point(destinationFirstThenOtherOffsets[i], destinationFirstThenOtherOffsets[i+1]);
         }
         DestinationOffset = OffsetsToCheck[0];
      }
   }

   public class MoveRules
   {
      private static readonly MoveRule[] Rules = new []
      {
         new MoveRule(0,1, -1,1, 1,1),
         new MoveRule(0,-1, -1,-1, 1,-1),
         new MoveRule(-1,0, -1,-1, -1,1),
         new MoveRule(1,0, 1,-1, 1,1)
      };

      private int mRuleOffset = -1;

      public MoveRules()
      {
         NextRound();
      }

      public void NextRound()
      {
         mRuleOffset++;
         MoveRule[] newOrder = new MoveRule[Rules.Length];
         for (int i = 0; i < Rules.Length; i++)
         {
            newOrder[i] = Rules[(mRuleOffset + i) % Rules.Length];
         }
         CurrentRules = newOrder;
      }

      public MoveRule[] CurrentRules { get; private set; }
   }
}

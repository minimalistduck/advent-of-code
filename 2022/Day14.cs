using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayFourteen
   {
      const string InputFilePath = "Input-Day14.txt";

      [Test]
      public static void SolveExample()
      {
         CaveGrid exampleGrid = LoadGrid("Example-Input-Day14.txt");
         CaveGrid gridWithFloor = exampleGrid.WithFloor();
         Solve(exampleGrid);
         Solve(gridWithFloor);
         Console.WriteLine(gridWithFloor);
      }

      [Test]
      public static void SolvePartOne()
      {
         Solve(LoadGrid(InputFilePath));
      }

      [Test]
      public static void SolvePartTwo()
      {
         Solve(LoadGrid(InputFilePath).WithFloor());
      }

      private static void Solve(CaveGrid grid)
      {
         Point sandStart = new Point(500 - grid.XOffset, 0 - grid.YOffset);
         Point[] moves = new []
         {
            new Point(0, 1), // down one step
            new Point(-1, 1), // one step down and to the left
            new Point(1, 1), // one step down and to the right
         };
         var result = 0;
         while (grid.DropSandUnit(sandStart, moves))
            result++;
         Console.WriteLine(result);
      }

      public static CaveGrid LoadGrid(string inputFilePath)
      {
         var rockPaths = File.ReadAllLines(inputFilePath).Select(line =>
            line.Split(" -> ").Select(p => {
               var parts = p.Split(",");
               return new Point(int.Parse(parts[0]), int.Parse(parts[1]));
            }).ToArray()
         ).ToArray();

         var allPoints = rockPaths.SelectMany(p => p);
         var allX = allPoints.Select(p => p.X);
         var minX = allX.Min() - 1; // leave space either side
         var maxX = allX.Max() + 1;
         var allY = allPoints.Select(p => p.Y);
         var minY = 0;
         var maxY = allY.Max();
         var width = maxX - minX + 1;
         var height = maxY - minY + 1;

         CaveCell[,] cells = new CaveCell[width,height];
         for (int c = 0; c < width; c++)
            for (int r = 0; r < height; r++)
               cells[c,r] = new CaveCell(c,r);

         foreach (var rockPath in rockPaths)
         {
            for (var segmentEnd = 1; segmentEnd < rockPath.Length; segmentEnd++)
            {
               var segmentStart = segmentEnd - 1;
               var delta = new Point(
                  Math.Sign(rockPath[segmentEnd].X - rockPath[segmentStart].X),
                  Math.Sign(rockPath[segmentEnd].Y - rockPath[segmentStart].Y)
               );
               for (var p = rockPath[segmentStart]; p != rockPath[segmentEnd]; p.Offset(delta))
               {
                  cells[p.X - minX, p.Y - minY].Content = CellContent.Rock;
               }
            }
            var end = rockPath.Last();
            cells[end.X - minX, end.Y - minY].Content = CellContent.Rock;
         }

         return new CaveGrid(cells, width, height, minX, minY);
      }
   }

   public class CaveGrid : Grid<CaveCell>
   {
      public int XOffset { get; private set; }
      public int YOffset { get; private set; }

      public CaveGrid(CaveCell[,] cells, int width, int height, int xOffset, int yOffset)
         : base(cells, width, height)
      {
         XOffset = xOffset;
         YOffset = yOffset;
      }

      public bool DropSandUnit(Point start, Point[] moves)
      {
         if (Cells[start.X, start.Y].Content != CellContent.Air)
            return false;
         var currentPosition = start;
         bool moved;
         do
         {
            moved = false;
            foreach (var move in moves)
            {
               var nextPosition = currentPosition;
               nextPosition.Offset(move);
               if (nextPosition.Y >= Height)
               {
                  return false; // dropped into the abyss
               }
               if (Cells[nextPosition.X, nextPosition.Y].Content == CellContent.Air)
               { 
                  moved = true;
                  currentPosition = nextPosition;
                  break;
               }
            }
         } while (moved);
         Cells[currentPosition.X, currentPosition.Y].Content = CellContent.Sand;
         return true;
      }

      public CaveGrid WithFloor()
      {
         var newHeight = Height + 2;
         var newWidth = Width + Height * 2; // guessing this will be enough?
         CaveCell[,] gridWithFloor = new CaveCell[newWidth, newHeight];
         for (int c = 0; c < newWidth; c++)
         {
            for (int r = 0; r < newHeight; r++)
            {
               gridWithFloor[c,r] = new CaveCell(c, r);
            }
            gridWithFloor[c,newHeight-1].Content = CellContent.Rock; // add floor
         }
         for (int c = 0; c < Width; c++)
         {
            for (int r = 0; r < Height; r++)
            {
               gridWithFloor[c + Height, r].Content = Cells[c,r].Content;
            }
         }
         return new CaveGrid(gridWithFloor, newWidth, newHeight, XOffset - Height, YOffset);
      }

      public override string ToString()
      {
         StringBuilder result = new StringBuilder();
         for (var r = 0; r < Height; r++)
         {
            for (var c = 0; c < Width; c++)
            { 
               switch (Cells[c,r].Content)
               {
                  case CellContent.Air:
                     result.Append('.');
                     break;
                  case CellContent.Rock:
                     result.Append('#');
                     break;
                  case CellContent.Sand:
                     result.Append('o');
                     break;
               }
            }
            result.AppendLine();
         }
         return result.ToString();
      }
   }

   public enum CellContent
   {
      Air,
      Rock,
      Sand
   }

   public class CaveCell : GridCell
   {
      public CellContent Content { get; set; } = CellContent.Air;

      private const char HeightNotRelevant = '-';
      public CaveCell(int column, int row)
         : base(column, row, HeightNotRelevant)
      {
      }
   }
}

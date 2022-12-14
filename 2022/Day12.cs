using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayTwelve
   {
      const string InputFilePath = "Input-Day12.txt";

      [Test]
      public static void SolvePartOne()
      {
         var grid = LoadGrid();
         Console.WriteLine(Solve(grid, Enumerable.Repeat(grid.Start,1)));
      }

      [Test]
      public static void SolvePartTwo()
      {
         var grid = LoadGrid();
         Console.WriteLine(Solve(grid, grid.GetLowestCells()));
      }

      public static int Solve(ForestGrid grid, IEnumerable<GridCell> startCells)
      {
         //Console.WriteLine($"Start at {grid.Start}");
         //Console.WriteLine(string.Join(',', grid.GetReachableCells(grid.Start).Select(c => c.ToString())));
         Dictionary<Point,int> distances = new Dictionary<Point, int>();
         int distance = 0;
         Queue<GridCell> nextFrontier = new Queue<GridCell>();
         foreach (var startCell in startCells)
         {
            distances.Add(startCell.Position, distance);
            nextFrontier.Enqueue(startCell);
         }
         while (!distances.ContainsKey(grid.End.Position))
         {
            distance++;
            var currentFrontier = nextFrontier;
            nextFrontier = new Queue<GridCell>();

            while (currentFrontier.Any())
            {
               var frontierCell = currentFrontier.Dequeue();
               foreach (var reachableCell in grid.GetReachableCells(frontierCell))
               {
                  if (!distances.ContainsKey(reachableCell.Position))
                  {
                     nextFrontier.Enqueue(reachableCell);
                     distances.Add(reachableCell.Position, distance);
                  }
               }
            }
         }
         return distances[grid.End.Position];
      }

      public static ForestGrid LoadGrid()
      {
         string[] lines = File.ReadAllLines(InputFilePath);
         GridCell start = null;
         GridCell end = null;
         GridCell[,] grid = new GridCell[lines[0].Length,lines.Length];
         for (int r = 0; r < lines.Length; r++)
         {
            for (int c = 0; c < lines[0].Length; c++)
            {
               char height = lines[r][c];
               if (height == 'S')
               {
                  start = new GridCell(c, r, 'a');
                  grid[c,r] = start;
               }
               else if (height == 'E')
               {
                  end = new GridCell(c,r,'z');
                  grid[c,r] = end;
               }
               else
               {
                  grid[c,r] = new GridCell(c,r,height);
               }
            }
         }
         return new ForestGrid(grid, start, end, lines[0].Length, lines.Length);
      }
   }

   public class Grid<T>
   {
      public T[,] Cells { get; private set; }
      public int Height { get; private set; }
      public int Width { get; private set; }

      public Grid(T[,] cells, int width, int height)
      {
         Cells = cells;
         Width = width;
         Height = height;
      }
   }

   public class ForestGrid : Grid<GridCell>
   {
      public GridCell Start { get; private set; }
      public GridCell End { get; private set; }

      public ForestGrid(GridCell[,] cells, GridCell start, GridCell end, int width, int height)
         : base(cells, width, height)
      {
         Start = start;
         End = end;
      }

      public IEnumerable<GridCell> GetLowestCells()
      {
         for (int c = 0; c < Width; c++)
         {
            for (int r = 0; r < Height; r++)
            {
               if (Cells[c, r].Height == 'a')
               {
                  yield return Cells[c, r];
               }
            }
         }
      }

      public IEnumerable<GridCell> GetReachableCells(GridCell fromCell)
      {
         if (fromCell.Position.X > 0)
         {
            var cellToLeft = Cells[fromCell.Position.X - 1, fromCell.Position.Y];
            if (fromCell.CanGetTo(cellToLeft))
            {
               yield return cellToLeft;
            }
         }
         if (fromCell.Position.X < Width - 1)
         {
            var cellToRight = Cells[fromCell.Position.X + 1, fromCell.Position.Y];
            if (fromCell.CanGetTo(cellToRight))
            {
               yield return cellToRight;
            }
         }
         if (fromCell.Position.Y > 0)
         {
            var cellAbove = Cells[fromCell.Position.X, fromCell.Position.Y - 1];
            if (fromCell.CanGetTo(cellAbove))
            {
               yield return cellAbove;
            }
         }
         if (fromCell.Position.Y < Height - 1)
         {
            var cellBelow = Cells[fromCell.Position.X, fromCell.Position.Y + 1];
            if (fromCell.CanGetTo(cellBelow))
            {
               yield return cellBelow;
            }
         }
      }
   }

   public class GridCell
   {
      public Point Position { get; private set; }
      public char Height { get; private set; }

      public GridCell(int column, int row, char height)
      {
         Position = new Point(column, row);
         Height = height;
      }

      public bool CanGetTo(GridCell toCell)
      {
         return toCell.Height <= this.Height + 1;
      }

      public override string ToString()
      {
         return $"({Position.X},{Position.Y}): {Height}";
      }
   }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day12Program
{
  public static void Main(string[] args)
  {
     //var lines = LoadInput();
     var lines = Example();
     var width = lines[0].Length;
     var height = lines.Length;
     
     Console.WriteLine("Grid is {0} x {1}", width, height);

     Func<Point,bool> isInBounds = p =>
        p.X >= 0 &&
        p.X < width &&
        p.Y >= 0 &&
        p.Y < height;
        
     var remainingPoints = new HashSet<Point>();
     for (int y = 0; y < height; y++)
     {
        for (int x = 0; x < width; x++)
        {
           remainingPoints.Add(new Point(x,y));
        }
     }
     
     List<Region> regions = new List<Region>();
     while (remainingPoints.Any())
     {
        var startPoint = remainingPoints.First();
        var regionLetter = lines[startPoint.Y][startPoint.X];
        //Console.WriteLine("Finding region of {0} starting at {1}", regionLetter, startPoint);
        
        HashSet<Point> pointsInRegion = new HashSet<Point>();
        pointsInRegion.Add(startPoint);
        
        Queue<Point> frontier = new Queue<Point>();
        frontier.Enqueue(startPoint);
        while (frontier.Any())
        {
           var current = frontier.Dequeue();
           foreach (var d in Direction.All)
           {
              var adjacent = current;
              adjacent.Offset(d.Offset);
              if (isInBounds(adjacent) &&
                 lines[adjacent.Y][adjacent.X] == regionLetter)
              {
                 bool isNew = pointsInRegion.Add(adjacent);
                 if (isNew)
                 {
                    frontier.Enqueue(adjacent);
                 }
              }
           }
        }
        regions.Add(new Region(pointsInRegion, regionLetter));
        remainingPoints.ExceptWith(pointsInRegion);
     }
        
     Console.WriteLine("Found {0} regions", regions.Count);
     
     var partOne = regions.Sum(r => r.CalculatePrice());
     Console.WriteLine("Part one: {0}", partOne);
     
     var partTwo = regions.Sum(r => r.CalculateDiscountedPrice());
     Console.WriteLine("Part two: {0}", partTwo);
     // 1421715 is too high
     // 513734 is too low
     // 770241 is too low
  }

  public static string[] LoadInput()
  {
     const string folder = @"D:\David\Coding\AdventOfCode2024\Day12";
     string inputPath = Path.Combine(folder, "input.txt");
     return File.ReadAllLines(inputPath).ToArray();
  }

  public static string[] Example()
  {
     return new string[] {
  "RRRRIICCFF",
  "RRRRIICCCF",
  "VVRRRCCFFF",
  "VVRCCCJFFF",
  "VVVVCJJCFE",
  "VVIVCCJJEE",
  "VVIIICJJEE",
  "MIIIIIJJEE",
  "MIIISIJEEE",
  "MMMISSJEEE"
     };
  }
}

public class Region
{
   public static char DebugLetter = 'I';
   public static bool DebugAll = false;

   private readonly HashSet<Point> _points;
   private readonly char _regionLetter;
   
   public Region(HashSet<Point> points, char regionLetter)
   {
      _points = points;
      _regionLetter = regionLetter;
   }
   
   public int CalculatePerimiter()
   {
      var result = 0;
      foreach (var p in _points)
      {
         foreach (var d in Direction.All)
         {
            var adjacent = p;
            adjacent.Offset(d.Offset);
            if (!_points.Contains(adjacent))
            {
               result++;
            }
         }
      }
      return result;
   }
   
   public int Area => _points.Count;
   
   public int CalculatePrice()
   {
      return CalculatePerimiter() * Area;
   }
   
   public int CountEdges()
   {
      var minX = _points.Min(p => p.X);
      var minY = _points.Min(p => p.Y);
      var maxX = _points.Max(p => p.X);
      var maxY = _points.Max(p => p.Y);
      
      var xOffset = minX - 1;
      var xRange = (maxX - minX) + 3; // +1 for self, +1 for each extremity
      
      var yOffset = minY - 1;
      var yRange = (maxY - minY) + 3;
      
      List<char[]> outline = new List<char[]>();
      for (int y = 0; y < yRange; y++)
      {
         outline.Add(
            new string('.', xRange).ToCharArray()
         );
      }
      
      foreach (var p in _points)
      {
         foreach (var d in Direction.All)
         {
            var adjacent = p;
            adjacent.Offset(d.Offset);
            if (!_points.Contains(adjacent))
            {
               var existingSymbol = outline[adjacent.Y - yOffset][adjacent.X - xOffset];
               var newSymbol = existingSymbol == '.' ? d.EdgeSymbol : '+';
               outline[adjacent.Y - yOffset][adjacent.X - xOffset] = newSymbol;
            }
         }
      }
      
      if (DebugAll || _regionLetter == DebugLetter)
      {
         foreach (var line in outline)
         {
            Console.WriteLine(new string(line));
         }
      }
      
      int result = 0;
      char lastChar = '.';
      for (int y = 0; y < yRange; y++)
      {
         for (int x = 0; x < xRange; x++)
         {
            if (lastChar == '.' && (outline[y][x] == '-' || outline[y][x] == '+'))
            {
               // start of new horizontal edge
               result++;
            }
            lastChar = outline[y][x];
         }
      }
      
      lastChar = '.';
      for (int x = 0; x < xRange; x++)
      {
         for (int y = 0; y < yRange; y++)
         {
            if (lastChar == '.' && (outline[y][x] == '|' || outline[y][x] == '+'))
            {
               // start of new vertical edge
               result++;
            }
            lastChar = outline[y][x];
         }
      }
      
      if (DebugAll || _regionLetter == DebugLetter)
      {
         Console.WriteLine("Region {1} has {0} edges", result, _regionLetter);
      }
      
      return result;
   }
   
   public int CalculateDiscountedPrice()
   {
      return CountEdges() * Area;
   }
}

public class Direction
{
  public readonly Point Offset;
  public readonly char EdgeSymbol;
  
  private Direction(Point offset, char edgeSymbol)
  {
    Offset = offset;
    EdgeSymbol = edgeSymbol;
  }
  
  public static Direction Up = new Direction(new Point(0,-1), '-');
  public static Direction Left = new Direction(new Point(1,0), '|');
  public static Direction Down = new Direction(new Point(0,1), '-');
  public static Direction Right = new Direction(new Point(-1,0), '|');
  
  public static Direction[] All = new Direction[] {
    Up, Left, Down, Right
  };
}
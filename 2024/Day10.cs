using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day10Program
{
  public static void Main()
  {
      var originalStack = new Stack<string>();
      originalStack.Push("First");
      originalStack.Push("Second");
      originalStack.Push("Third");
      
      var copyStack = new Stack<string>(originalStack);
      Console.WriteLine(string.Join(",", originalStack));
      Console.WriteLine(string.Join(",", copyStack));
      
     const string inputFilePath = @"C:\Users\davidt\Coding\AdventOfCode\2024\Day10-input.txt";
     var lines = File.ReadAllLines(inputFilePath);
     var width = lines[0].Length;
     var height = lines.Length;
          
     Func<Point, bool> isInBounds = p =>
        p.X >= 0 && p.X < width &&
        p.Y >= 0 && p.Y < height;
     
     // 7:15
     List<Stack<Point>> pathsSoFar = new List<Stack<Point>>();
     for (int y = 0; y < height; y++)
     {
         for (int x = 0; x < width; x++)
         {
             if (lines[y][x] == '0')
             {
                 var newPath = new Stack<Point>();
                 newPath.Push(new Point(x,y));
                 pathsSoFar.Add(newPath);
             }
         }
     }
     
     for (char h = '1'; h <= '9'; h = (char)((int)h + 1))
     {
         Console.WriteLine(h);
        var prevPaths = pathsSoFar;
        pathsSoFar = new List<Stack<Point>>();
        
        foreach (var partialPath in prevPaths)
        {
            var currentPoint = partialPath.Peek();
            foreach (var d in Direction.All)
            {
                var nextPoint = currentPoint;
                nextPoint.Offset(d.Offset);
                if (isInBounds(nextPoint) && lines[nextPoint.Y][nextPoint.X] == h)
                {
                    var longerPath = new Stack<Point>(partialPath.Reverse());
                    longerPath.Push(nextPoint);
                    pathsSoFar.Add(longerPath);
                }
            }
        }
     }
     
     HashSet<string> distinctTrails = new HashSet<string>();
     foreach (var trail in pathsSoFar)
     {
         var endPoint = trail.Peek();
         var startPoint = trail.Last();
         var key = $"{startPoint}-{endPoint}";
         distinctTrails.Add(key);
     }
     
     // 1960 is too high
     Console.WriteLine("Part one: {0}", distinctTrails.Count);
  }
}

public class Direction
{
    public readonly Point Offset;
    public readonly char Symbol;
    
    private Direction(int x, int y, char symbol)
    {
        Offset = new Point(x, y);
        Symbol = symbol;
    }
    
    public static Direction[] All = new Direction[]
    {
        new Direction(0,-1,'^'),
        new Direction(1,0,'>'),
        new Direction(0,1,'v'),
        new Direction(-1,0,'<')
    };
}
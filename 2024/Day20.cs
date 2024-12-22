using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day20Program
{
  public static void Main()
  {
    const string inputFilePath = @"D:\David\Coding\AdventOfCode\2024\Day20-input.txt";
    var lines = File.ReadAllLines(inputFilePath);
    //var lines = Example();
    var width = lines[0].Length;
    var height = lines.Length;
     
    var startPoint = new Point(-1,-1);
    var endPoint = startPoint;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        if (lines[y][x] == 'S')
        {
          startPoint = new Point(x,y);
        }
        else if (lines[y][x] == 'E')
        {
          endPoint = new Point(x,y);
        }
      }
    }
    
    var forwardTracker = ExploreFrom(startPoint, lines);
    var backwardTracker = ExploreFrom(endPoint, lines);
    
    var distanceWithoutCheating = forwardTracker.DistanceTo(endPoint).Value;
    Console.WriteLine("Without cheating, it takes {0} picoseconds to finish the race", distanceWithoutCheating);
    
    var targetDistance = distanceWithoutCheating - 100;
    //var targetDistance = distanceWithoutCheating - 2;
    var partOneResult = 0;
    foreach (var forwardEntry in forwardTracker.Entries)
    {
      foreach (var d in Direction.All)
      {
        var afterCheat = forwardEntry.Key;
        afterCheat.Offset(d.Offset);
        if (lines[afterCheat.Y][afterCheat.X] == '#')
        {
          afterCheat.Offset(d.Offset);
          var remainingDistance = backwardTracker.DistanceTo(afterCheat);
          if (remainingDistance.HasValue)
          {
            // cheat itself has a distance of 2
            var distanceWithCheat = forwardEntry.Value + 2 + remainingDistance.Value;
            if (distanceWithCheat <= targetDistance)
            {
              //Console.WriteLine("Cheat from {0} to {1} saves {2}", forwardEntry.Key, afterCheat,
              //  distanceWithoutCheating - distanceWithCheat);
              partOneResult++;
            }
          }
        }
      }
    }
    
    Console.WriteLine("Part one: {0}", partOneResult);
    
    // For part 2, the method is similar but pairs each point with any other point within
    // 20 moves of this one, which is not a wall.
    var partTwoResult = 0;
    foreach (var forwardEntry in forwardTracker.Entries)
    {
      for (int y = -20; y <= 20; y++)
      {
        var xMagnitude = 20 - Math.Abs(y);
        for (int x = -xMagnitude; x <= xMagnitude; x++)
        {
          var afterCheat = new Point(forwardEntry.Key.X + x, forwardEntry.Key.Y + y);
          var remainingDistance = backwardTracker.DistanceTo(afterCheat);
          if (remainingDistance.HasValue)
          {
            var cheatDistance = Math.Abs(x) + Math.Abs(y);
            var distanceWithCheat = forwardEntry.Value + cheatDistance + remainingDistance.Value;
            if (distanceWithCheat <= targetDistance)
            {
              partTwoResult++;
            }
          }
        }
      }
    }
    
    Console.WriteLine("Part two: {0}", partTwoResult);
  }
  
  public static ResultTracker ExploreFrom(Point position, string[] lines)
  {
    var result = new ResultTracker();    
    result.ConsiderResult(position);
    var worklist = new Point[] { position };
    while (worklist.Length > 0)
    {
      result.NextDistance();
      foreach (var prevPosition in worklist)
      {
        foreach (var d in Direction.All)
        {
          var adjacent = prevPosition;
          adjacent.Offset(d.Offset);
          if (lines[adjacent.Y][adjacent.X] != '#')
          {
            result.ConsiderResult(adjacent);
          }
        }
      }
      worklist = result.Frontier().ToArray();
    }
    return result;
  }
  
  public static string[] Example()
  {
    return new string[] {
"###############",
"#...#...#.....#",
"#.#.#.#.#.###.#",
"#S#...#.#.#...#",
"#######.#.#.###",
"#######.#.#...#",
"#######.#.###.#",
"###..E#...#...#",
"###.#######.###",
"#...###...#...#",
"#.#####.#.###.#",
"#.#...#.#.#...#",
"#.#.#.#.#.#.###",
"#...#...#...###",
"###############"
    };
  }
}

public class ResultTracker
{
  private readonly Dictionary<Point, int> _shortest = new Dictionary<Point, int>();
  private int _distance = 0;
    
  public void NextDistance()
  {
    _distance++;
  }
  
  public int Distance => _distance;
    
  public void ConsiderResult(Point position)
  {
    if (!_shortest.ContainsKey(position))
    {
      _shortest.Add(position, _distance);
    }
  }
    
  public IEnumerable<Point> Frontier() =>
    _shortest.Where(kvp => kvp.Value == _distance).Select(kvp => kvp.Key);
    
  public int? DistanceTo(Point endPosition)
  {
    if (!_shortest.TryGetValue(endPosition, out var result))
    {
      return null;
    }
    return result;
  }
  
  public IEnumerable<KeyValuePair<Point, int>> Entries => _shortest;
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
    
  public static readonly Direction North = new Direction(0,-1,'^');
  public static readonly Direction East = new Direction(1,0,'>');
  public static readonly Direction South = new Direction(0,1,'v');
  public static readonly Direction West = new Direction(-1,0,'<');
    
  public static Direction[] All = new Direction[] {
    North, East, South, West
  };
    
  public override string ToString() => Symbol.ToString();
}
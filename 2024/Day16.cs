using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day16Program
{
  public static void Main()
  {
     const string inputFilePath = @"C:\Users\davidt\Coding\AdventOfCode\2024\Day16-example.txt";
     var lines = File.ReadAllLines(inputFilePath);
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
     
     var startDirection = Direction.East;

     var tracker = new ResultTracker();
     
     List<PathEnd> worklist = new List<PathEnd>();
     worklist.Add(new PathEnd(startPoint, startDirection, 0));
     
     while (worklist.Count > 0)
     {
         foreach (var head in worklist)
         {
             // if moving forward is allowed, add to tracker
             var candidatePosition = head.Position;
             candidatePosition.Offset(head.Direction.Offset);
             if (lines[candidatePosition.Y][candidatePosition.X] != '#')
             {
                 tracker.ConsiderResult(candidatePosition, head.Direction, head.Score + 1);
             }
             
             // add turning left and right to the tracker
             var newDirections = new Direction[] {
                 Direction.TurnClockwise(head.Direction),
                 Direction.TurnAnticlockwise(head.Direction)
             };
             foreach (var newDirection in newDirections)
             {
                 tracker.ConsiderResult(head.Position, newDirection, head.Score + 1000);
             }
         }
         
         // Eventually we'll stop finding improvements so ActiveEnds will be empty
         worklist = tracker.ActiveEnds().ToList();
         tracker.NextIteration();
     }
      
     Console.WriteLine("Part one: {0}", tracker.FindBestSolution(endPoint));
     
     var spectatorPoints = tracker.Backtrack(endPoint, startPoint);
     Console.WriteLine("Part two: {0}", spectatorPoints.Count);
     // 429 is too low
     // Gives 37 on the example input; should be 45
  }
}

public class ResultTracker
{
    private readonly Dictionary<string, Result> _bestSoFar = new Dictionary<string, Result>();
    private int _iteration = 0;
    
    public void NextIteration()
    {
        _iteration++;
    }
    
    public void ConsiderResult(Point pos, Direction dir, int score)
    {
        var pathEnd = new PathEnd(pos, dir, score);
        var key = pathEnd.Key;
        if (!_bestSoFar.TryGetValue(key, out var previousBestResult) ||
            score < previousBestResult.Score)
        {
            _bestSoFar[key] = new Result(pathEnd, _iteration);
        }
    }
    
    public IEnumerable<PathEnd> ActiveEnds() =>
        _bestSoFar.Values.Where(r => r.Iteration == _iteration).Select(r => r.PathEnd);
        
    public int FindBestSolution(Point destination)
    {
        return LowestScoreAt(destination).Value;
    }
    
    private int? LowestScoreAt(Point position)
    {
        List<int> candidateScores = new List<int>();
        foreach (var d in Direction.All)
        {
            var key = new PathEnd(position, d, -1).Key;
            if (_bestSoFar.TryGetValue(key, out var pathEnd))
            {
                candidateScores.Add(pathEnd.Score);
            }
        }
        return candidateScores.Count == 0 ? null : candidateScores.Min();
    }
    
    // Returns the set of points that are on one of the best paths to the finish
    public HashSet<Point> Backtrack(Point destination, Point start)
    {
        var result = new HashSet<Point>();
        result.Add(destination);
        
        HashSet<Point> frontier = new HashSet<Point>();
        frontier.Add(destination);
        
        while (!frontier.Contains(start)) // not totally sure about this condition?
        {
            HashSet<Point> nextFrontier = new HashSet<Point>();    
            
            foreach (var f in frontier)
            {
                var four = Direction.All.Length;
                var candidates = new Point[four];
                var scores = new int?[four];
                for (var d = 0; d < four; d++)
                {
                    var candidate = f;
                    candidate.Offset(Direction.All[d].Offset);
                    candidates[d] = candidate;
                    scores[d] = LowestScoreAt(candidate);
                }
                if (!scores.Any(s => s.HasValue))
                {
                    continue;
                }
                var minScore = scores.Where(s => s.HasValue).Min(s => s.Value);
                for (int d = 0; d < four; d++)
                {
                    if (scores[d].HasValue && scores[d].Value == minScore)
                    {
                        nextFrontier.Add(candidates[d]);
                    }
                }
            }
            result.UnionWith(nextFrontier);
            frontier = nextFrontier;
        }
        
        return result;
    }
    
    private class Result
    {
        public readonly PathEnd PathEnd;
        public int Score => PathEnd.Score;
        public readonly int Iteration;
        
        public Result(PathEnd pathEnd, int iteration)
        {
            PathEnd = pathEnd;
            Iteration = iteration;
        }
    }
}

public class PathEnd
{
    public readonly Point Position;
    public readonly Direction Direction;
    public readonly int Score;
    
    public PathEnd(Point pos, Direction dir, int score)
    {
        Position = pos;
        Direction = dir;
        Score = score;
    }
    
    public string Key => $"{Position} {Direction}";
    
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
    
    public static Direction TurnClockwise(Direction original)
    {
        switch (original.Symbol)
        {
            case '^':
                return Direction.East;
            case '>':
                return Direction.South;
            case 'v':
                return Direction.West;
            case '<':
                return Direction.North;
            default:
                throw new InvalidOperationException();
        }
    }
    
    public static Direction TurnAnticlockwise(Direction original)
    {
        switch (original.Symbol)
        {
            case '^':
                return Direction.West;
            case '>':
                return Direction.North;
            case 'v':
                return Direction.East;
            case '<':
                return Direction.South;
            default:
                throw new InvalidOperationException();
        }
    }
    
    public static Direction[] All = new Direction[] {
        North, East, South, West
    };
    
    public override string ToString() => Symbol.ToString();
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static class ChristmasEve
   {
      public static readonly string InputFilePath = "Input-Day24.txt";
      
      public static string[] RealInput
      {
         get { return System.IO.File.ReadAllLines(InputFilePath); }
      }
      
      public static readonly string[] ExampleInput = new []
      {
         "#.######",
         "#>>.<^<#",
         "#.<..<<#",
         "#>v.><>#",
         "#<^v^^>#",
         "######.#",
      };
      
      public static void SolveExamplePartOne()
      {
         var solution = ExploreMaze(ExampleInput);
         Console.WriteLine(solution.ToPathString());
      }
      
      public static void SolvePartOne()
      {
         var solution = ExploreMaze(RealInput);
         Console.WriteLine(solution.ToPathString());
      }
      
      public static void SolvePartTwo()
      {
         ExploreMaze(RealInput, 3);
      }
      
      public static MazeMove ExploreMaze(string[] mazeInput, int trips = 1)
      {
         var maze = Maze.FromLines(mazeInput);
         var maxSteps = maze.Boundary.LongestSide * trips * 3; // a guess at a reasonable upper bound
         
         // The set of points occupied at time t can be shared across searches
         var occupiedAtT = new HashSet<Point>[maxSteps + 1];
         int t;
         for (t = 0; t <= maxSteps; t++)
         {
            var occupiedPoints = new HashSet<Point>(maze.Boundary.OccupiedPoints);
            occupiedPoints.UnionWith(maze.Blizzards.Select(b => b.Position));
            occupiedAtT[t] = occupiedPoints;
            Array.ForEach(maze.Blizzards, b => b.Move());
         }
         
         var initialMove = MazeMove.EnterMaze(maze);
         var closestSoFar = initialMove.DistanceFromGoal;
         MazeMove lastStepOfTrip = null;
         IEnumerable<MazeMove> worklist = new MazeMove[] { initialMove };
         t = 1;
         for (var trip = 0; trip < trips; trip++)
         {
            while (t <= maxSteps && closestSoFar > 0)
            {
               List<MazeMove> generatedMoves = new List<MazeMove>();
               foreach (var oldMove in worklist)
               {
                  foreach (var offset in MazeMove.PossibleMoveOffsets)
                  {
                     var nextMove = oldMove.WithStep(offset);
                     if (!occupiedAtT[t].Contains(nextMove.Position))
                     {
                        generatedMoves.Add(nextMove);
                     }
                  }
               }
               worklist = Focus(generatedMoves);
               closestSoFar = worklist.Min(m => m.DistanceFromGoal);
               Console.WriteLine($"   Got within {closestSoFar} after {t}. Continuing with {worklist.Count()} possibilities.");
               t++;
            }
            // prepare for next trip
            lastStepOfTrip = worklist.Where(m => m.DistanceFromGoal == 0).First();
            var firstStepOfNextTrip = lastStepOfTrip.WithNewGoal(trip % 2 == 0 ? maze.Boundary.Entry : maze.Boundary.Exit);
            closestSoFar = firstStepOfNextTrip.DistanceFromGoal;
            worklist = new [] { firstStepOfNextTrip };
         }
         return lastStepOfTrip;
      }
      
      private static IEnumerable<MazeMove> Focus(List<MazeMove> generatedMoves)
      {
         // assumption: generatedMoves are all for the same t
         Dictionary<Point, MazeMove> chosenMoves = new Dictionary<Point, MazeMove>();
         foreach (var move in generatedMoves)
         {
            chosenMoves[move.Position] = move; // deliberately overwriting
         }
         var best = chosenMoves.Values.Min(m => m.DistanceFromGoal);
         return chosenMoves.Values;
      }
   }

   public class MazeMove
   {
      public static readonly Point[] PossibleMoveOffsets = new [] {
         new Point(-1,0),
         new Point(0,-1),
         new Point(1,0),
         new Point(0,1),
         new Point(0,0)
      };
      public static Point NoMove => PossibleMoveOffsets[4];

      public readonly MazeMove PreviousMove;
      public readonly Point MoveOffset;
      public readonly Point Position;
      private readonly Point mGoalPosition;
      public readonly int DistanceFromGoal;
      
      private MazeMove(MazeMove previousMove, Point moveOffset, Point goalPosition)
      {
         PreviousMove = previousMove;
         MoveOffset = moveOffset;
         Position = previousMove.Position; // copy value type
         Position.Offset(moveOffset);
         mGoalPosition = goalPosition;
         DistanceFromGoal = Math.Abs(goalPosition.X - Position.X) + Math.Abs(goalPosition.Y - Position.Y);
      }
      
      private MazeMove(Point initialPosition, Point goalPosition)
      {
         PreviousMove = null;
         MoveOffset = NoMove;
         Position = initialPosition;
         mGoalPosition = goalPosition;
         DistanceFromGoal = Math.Abs(goalPosition.X - Position.X) + Math.Abs(goalPosition.Y - Position.Y);
      }
      
      public static MazeMove EnterMaze(Maze maze)
      {
         return new MazeMove(maze.Boundary.Entry, maze.Boundary.Exit);
      }
      
      public MazeMove WithStep(Point offset)
      {
         return new MazeMove(this, offset, mGoalPosition);
      }
      
      public MazeMove WithNewGoal(Point newGoal)
      {
         return new MazeMove(this, NoMove, newGoal);
      }
      
      private static readonly string[] MoveRepresentations = new [] {
         "<", "^", ">", "v", "."
      };
      public override string ToString()
      {
         var pos = Array.IndexOf(PossibleMoveOffsets, MoveOffset);
         return MoveRepresentations[pos];
      }
      
      public string ToPathString()
      {
         var currentMove = this;
         List<string> breadcrumb = new List<string>();
         while (currentMove != null)
         {
            breadcrumb.Add(currentMove.ToString());
            currentMove = currentMove.PreviousMove;
         }
         breadcrumb.Reverse();
         return string.Join("", breadcrumb);
      }
   }

   public class Maze
   {
      public readonly Boundary Boundary;
      public readonly Blizzard[] Blizzards;
      
      public Maze(Boundary boundary, Blizzard[] blizzards)
      {
         Boundary = boundary;
         Blizzards = blizzards;
      }
      
      public static Maze FromLines(string[] lines)
      {
         Dictionary<char, Point> blizzardOffsets = new Dictionary<char, Point>();
         blizzardOffsets['^'] = new Point(0,-1);
         blizzardOffsets['v'] = new Point(0,1);
         blizzardOffsets['>'] = new Point(1,0);
         blizzardOffsets['<'] = new Point(-1,0);
         Boundary boundary = new Boundary(lines[0].Length, lines.Length);
         List<Blizzard> blizzards = new List<Blizzard>();
         for (var y = 1; y < lines.Length - 1; y++)
         {
            for (var x = 1; x < lines[y].Length - 1; x++)
            {
               if (blizzardOffsets.TryGetValue(lines[y][x], out Point offset))
               {
                  blizzards.Add(new Blizzard(x, y, offset, boundary));
               }
            }
         }
         return new Maze(boundary, blizzards.ToArray()); 
      }
   }

   public class Boundary
   {
      public readonly Point Entry;
      public readonly Point Exit;
      public readonly Point TopLeft;
      public readonly Point BottomRight;
      public readonly int LongestSide;
      public readonly HashSet<Point> OccupiedPoints;

      public Boundary(int width, int height)
      {
         TopLeft = new Point(0,0);
         BottomRight = new Point(width - 1, height - 1);   
         Entry = TopLeft;
         Entry.Offset(new Point(1,0));      
         Exit = BottomRight;
         Exit.Offset(new Point(-1,0));
         LongestSide = Math.Max(width, height);
         
         OccupiedPoints = new HashSet<Point>();
         for (var x = TopLeft.X; x <= BottomRight.X; x++)
         {
            OccupiedPoints.Add(new Point(x,TopLeft.Y));
            OccupiedPoints.Add(new Point(x,BottomRight.Y));
         }
         for (var y = TopLeft.Y; y <= BottomRight.Y; y++)
         {
            OccupiedPoints.Add(new Point(TopLeft.X, y));
            OccupiedPoints.Add(new Point(BottomRight.X, y));
         }
         OccupiedPoints.Remove(Entry);
         OccupiedPoints.Remove(Exit);
         // To avoid separate bounds checking, block the entry and exit, otherwise
         // going round the outside will be easier than any other option :-)
         var blockEntry = Entry;
         blockEntry.Offset(0,-1);
         OccupiedPoints.Add(blockEntry);
         var blockExit = Exit;
         blockExit.Offset(0,1);
         OccupiedPoints.Add(blockExit);
      }
   }

   public class Blizzard
   {
      public Point Position;
      public readonly Point MoveOffset;
      private readonly Boundary Boundary;
      
      public Blizzard(int x, int y, Point moveOffset, Boundary boundary)
      {
         Position = new Point(x,y);
         MoveOffset = moveOffset;
         Boundary = boundary;
      }
      
      // Observation: there are no vertical blizzards in the entry and exit columns
      
      public void Move()
      {
         string previous = Position.ToString();
         Position.Offset(MoveOffset);
         if (Position.X == Boundary.TopLeft.X)
            Position = new Point(Boundary.BottomRight.X - 1, Position.Y);
         if (Position.X == Boundary.BottomRight.X)
            Position = new Point(Boundary.TopLeft.X + 1, Position.Y);
         if (Position.Y == Boundary.TopLeft.Y)
            Position = new Point(Position.X, Boundary.BottomRight.Y - 1);
         if (Position.Y == Boundary.BottomRight.Y)
            Position = new Point(Position.X, Boundary.TopLeft.Y + 1);
         //Console.WriteLine($"Moved blizzard from {previous} by {MoveOffset.ToString()} to {Position.ToString()}");
      }
   }
}

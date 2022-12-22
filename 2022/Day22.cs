using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public class DayTwentyTwo
   {
      public static readonly string[] ExampleBoard = new string[] {
         "        ...#",
         "        .#..",
         "        #...",
         "        ....",
         "...#.......#",
         "........#...",
         "..#....#....",
         "..........#.",
         "        ...#....",
         "        .....#..",
         "        .#......",
         "        ......#."
      };
      public const string ExampleRoute = "10R5L5R10L4R5L5";
      
      public static string[] RealBoard
      {
         get
         {
            return File.ReadAllLines("Input-Day22-Board.txt");
         }
      }
      
      public static string RealRoute
      {
         get
         {
            return File.ReadAllText("Input-Day22-Route.txt");
         }
      }
      
      public static void SolveExamplePartOne()
      {
         Solve(ExampleBoard, ExampleRoute);
      }
      
      public static void SolvePartOne()
      {
         Solve(RealBoard, RealRoute);
      }
      
      public static void SolvePartTwo()
      {
         Solve(RealBoard, RealRoute, makeCube: true);
      }
      
      public static void Solve(string[] rawBoard, string rawRoute, bool makeCube = false)
      {
         var board = BoardBuilder.FromLines(rawBoard, makeCube);
         var route = new Route(rawRoute);
         route.Follow(board);
         Console.WriteLine($"Ended at row {route.FinalCell.Y}, column {route.FinalCell.X}, facing {route.Facing.Current}");
         var result = route.FinalCell.Y * 1000 + route.FinalCell.X * 4 + route.Facing.Current;
         Console.WriteLine(result);
      }
   }

   public class Route
   {
      private readonly int[] mDistances;
      private readonly Func<Facing, Facing>[] mTurns;
      public Facing Facing { get; private set; }
      public EmptyCell FinalCell { get; private set; }

      // encodedRoute must start with a distance. Prepend 0 if necessary.
      public Route(string encodedRoute)
      {
         mDistances = Regex.Matches(encodedRoute, @"\d+").Select(m => int.Parse(m.Groups[0].Value)).ToArray();
         var turns = new Dictionary<string, Func<Facing, Facing>>();
         turns["L"] = f => f.TurnCounterClockwise();
         turns["R"] = f => f.TurnClockwise();
         mTurns = Regex.Matches(encodedRoute, @"[LR]").Select(m => turns[m.Groups[0].Value]).ToArray();
         Facing = Facing.Right;
      }
      
      public void Follow(Board board)
      {
         var distances = new Queue<int>(mDistances);
         var turns = new Queue<Func<Facing, Facing>>(mTurns);
         var currentCell = board.StartCell;
         while (distances.Any())
         {
            var nextDistance = distances.Dequeue();
            for (var i = 0; i < nextDistance; i++)
            {
               var lastCell = currentCell;
               currentCell = Facing.Move(lastCell);
               Facing = board.FacingAfterMove(lastCell, currentCell, Facing);
            }
            if (turns.Any())
            {
               Facing = turns.Dequeue()(Facing);
            }
         }
         FinalCell = currentCell;
      }
   }

   public class Board
   {
      public readonly EmptyCell StartCell;
      private readonly Dictionary<string, Facing> mFacingAdjustments = new Dictionary<string, Facing>();
      
      public Board(EmptyCell startCell, Dictionary<string, Facing> facingAdjustments)
      {
         StartCell = startCell;
         mFacingAdjustments = facingAdjustments;
      }
      
      public Facing FacingAfterMove(EmptyCell fromCell, EmptyCell toCell, Facing currentFacing)
      {
         if (mFacingAdjustments.TryGetValue(MoveKey(fromCell, toCell), out Facing result))
             return result;
         return currentFacing;
      }
      
      public static string MoveKey(BoardCell fromCell, BoardCell toCell)
      {
         return $"{fromCell.X},{fromCell.Y}->{toCell.X},{toCell.Y}";
      }
   }

   public class BoardBuilder
   {
      public static Board FromLines(string[] lines, bool makeCube)
      {
         Dictionary<char, Func<int, int, BoardCell>> cellMaker = new Dictionary<char, Func<int, int, BoardCell>>();
         cellMaker[' '] = (x, y) => OobCell.Instance;
         cellMaker['.'] = (x, y) => new EmptyCell(x, y);
         cellMaker['#'] = (x, y) => new RockCell(x, y);
         List<List<BoardCell>> rows = new List<List<BoardCell>>();
         List<List<BoardCell>> columns = new List<List<BoardCell>>();
         for (int y = 1; y <= lines.Length; y++)
         {
            List<BoardCell> currentRow = new List<BoardCell>();
            for (int x = 1; x <= lines[y-1].Length; x++)
            {
               var newCell = cellMaker[lines[y-1][x-1]](x,y);
               currentRow.Add(newCell);
               // Generate any new columns that are necessary
               while (columns.Count < x)
               {
                  var newColumn = new List<BoardCell>();
                  // pad the new column down to our current y
                  for (var r = 1; r < y; r++)
                     newColumn.Add(OobCell.Instance);
                  columns.Add(newColumn);
               }
               columns[x-1].Add(newCell);
            }
            rows.Add(currentRow);
         }
         
         for (var i = 0; i < rows.Count; i++)
         {
            rows[i] = rows[i].Where(c => c != OobCell.Instance).ToList();
            if (!makeCube)
            {
               var first = rows[i].First();
               var last = rows[i].Last();
               first.JoinLeft(last);
               last.JoinRight(first);
            }
            for (var j = 1; j < rows[i].Count; j++)
            {
               rows[i][j].JoinLeft(rows[i][j-1]);
               rows[i][j-1].JoinRight(rows[i][j]);
            }
         }
         for (var i = 0; i < columns.Count; i++)
         {
            columns[i] = columns[i].Where(c => c != OobCell.Instance).ToList();
            if (!makeCube)
            {
               var first = columns[i].First();
               var last = columns[i].Last();   
               first.JoinTop(last);
               last.JoinBottom(first);
            }
            for (var j = 1; j < columns[i].Count; j++)
            {
               columns[i][j].JoinTop(columns[i][j-1]);
               columns[i][j-1].JoinBottom(columns[i][j]);
            }
         }
         
         var specialMoves = new Dictionary<string, Facing>();

         void StitchEdges(IEnumerable<BoardCell> first, IEnumerable<BoardCell> second, Action<BoardCell, BoardCell> joins)
         {
            using (var firstIter = first.GetEnumerator())
            using (var secondIter = second.GetEnumerator())
            {
               while (firstIter.MoveNext() && secondIter.MoveNext())
               {
                  joins(firstIter.Current, secondIter.Current);
               }
            }
         }
         void AddSpecialMove(BoardCell p, BoardCell q, Facing newFacing)
         {
            specialMoves.Add(Board.MoveKey(p,q), newFacing);
         }
         
         if (makeCube)
         {
            // only works for RealInput!
            StitchEdges(rows[49].Skip(50), columns[99].Skip(50).Take(50), (p, q) =>
            {
               // quirk of the implementation is that you have to ask for the opposite
               // p.JoinLeft(q) means "tell q that I am to its right" etc
               p.JoinLeft(q);
               AddSpecialMove(p, q, Facing.Left);
               q.JoinTop(p);
               AddSpecialMove(q, p, Facing.Up);
            });
            StitchEdges(columns[50].Skip(50).Take(50), rows[100].Take(50), (p, q) =>
            {
               p.JoinBottom(q);
               AddSpecialMove(p, q, Facing.Down); 
               q.JoinRight(p);
               AddSpecialMove(q, p, Facing.Right);
            });
            StitchEdges(rows[149].Skip(50), columns[49].Skip(50), (p, q) =>
            {
               p.JoinLeft(q);
               AddSpecialMove(p, q, Facing.Left);
               q.JoinTop(p);
               AddSpecialMove(q, p, Facing.Up);
            });
            StitchEdges(columns[0].Take(50), columns[50].Take(50).Reverse(), (p, q) =>
            {
               p.JoinRight(q);
               AddSpecialMove(p, q, Facing.Right);
               q.JoinRight(p);
               AddSpecialMove(q, p, Facing.Right);
            });
            StitchEdges(columns[0].Skip(50), rows[0].Take(50), (p, q) =>
            {
               p.JoinBottom(q);
               AddSpecialMove(p, q, Facing.Down); 
               q.JoinRight(p);
               AddSpecialMove(q, p, Facing.Right);
            });
            StitchEdges(rows[0].Skip(50), rows[199], (p, q) =>
            {
               p.JoinTop(q);
               AddSpecialMove(p, q, Facing.Up);
               q.JoinBottom(p);
               AddSpecialMove(q, p, Facing.Down); 
            });
            StitchEdges(columns[99].Skip(100), Enumerable.Reverse(columns[149]), (p, q) =>
            {
               p.JoinLeft(q);
               AddSpecialMove(p, q, Facing.Left);
               q.JoinLeft(p);
               AddSpecialMove(q, p, Facing.Left);
            });
         }
         
         return new Board((EmptyCell)rows.First().First(), specialMoves);
      }
   }

   public abstract class BoardCell
   {
      public readonly int X;
      public readonly int Y;
      public EmptyCell Up;
      public EmptyCell Down;
      public EmptyCell Left;
      public EmptyCell Right;

      protected BoardCell(int x, int y)
      {
         X = x;
         Y = y;
      }
      
      public abstract void JoinTop(BoardCell cellAbove);
      public abstract void JoinBottom(BoardCell cellBelow);
      public abstract void JoinLeft(BoardCell cellToLeft);
      public abstract void JoinRight(BoardCell cellToRight);
   }

   public class EmptyCell : BoardCell
   {
      public EmptyCell(int x, int y)
         : base(x, y) {}
         
      public override void JoinTop(BoardCell cellAbove)
      {
         cellAbove.Down = this;
      }
      public override void JoinBottom(BoardCell cellBelow)
      {
         cellBelow.Up = this;
      }
      public override void JoinLeft(BoardCell cellToLeft)
      {
         cellToLeft.Right = this;
      }
      public override void JoinRight(BoardCell cellToRight)
      {
         cellToRight.Left = this;
      }
   }

   public class RockCell : BoardCell
   {
      public RockCell(int x, int y)
         : base(x,y) {}
      public override void JoinTop(BoardCell cellAbove) { }
      public override void JoinBottom(BoardCell cellBelow) { }
      public override void JoinLeft(BoardCell cellToLeft) { }
      public override void JoinRight(BoardCell cellToRight) { }
   }

   public class OobCell : BoardCell
   {
      public static readonly OobCell Instance = new OobCell();

      private OobCell()
         : base(-1, -1) {}
         
      public override void JoinTop(BoardCell cellAbove) { }
      public override void JoinBottom(BoardCell cellBelow) { }
      public override void JoinLeft(BoardCell cellToLeft) { }
      public override void JoinRight(BoardCell cellToRight) { }
   }

   public class Facing
   {
      private static readonly Func<EmptyCell, EmptyCell>[] Moves = new Func<EmptyCell, EmptyCell>[] {
         c => c.Right ?? c,
         c => c.Down ?? c,
         c => c.Left ?? c,
         c => c.Up ?? c
      };
      
      public static readonly Facing Right = new Facing(0);
      public static readonly Facing Down = new Facing(1);
      public static readonly Facing Left = new Facing(2);
      public static readonly Facing Up = new Facing(3);
      
      public readonly int Current;
      
      private Facing(int facing)
      {
         Current = facing;
      }
      
      public Facing TurnClockwise()
      {
         return new Facing((Current + 1) % Moves.Length);
      }
      
      public Facing TurnCounterClockwise()
      {
         return new Facing((Current + Moves.Length - 1) % Moves.Length);
      }
      
      public EmptyCell Move(EmptyCell from)
      {
         return Moves[Current](from);
      }
   }
}

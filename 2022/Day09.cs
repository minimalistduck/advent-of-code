using System;
using System.Collections.Generic;
using System.Drawing;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayNine
   {
      private const string InputFilePath = "Input-Day09.txt";

      public static void Prepare()
      {
         Point p1 = new Point(3,4);
         Point p2 = new Point(6,7);
         Point p3 = new Point(3,4);
         HashSet<Point> set = new HashSet<Point>(new [] { p1, p2, p3 });
         Console.WriteLine(set.Count);
      }

      public static void SolvePartOne()
      {
         var head = new RopeHead();
         var tail = new RopeTail();
         head.Moved += tail.PredecessorMoved;
         Solve(head, tail);
      }
      
      public static void SolvePartTwo()
      {
         var head = new RopeHead();
         RopeKnot previousKnot = head;
         for (int i = 2; i <= 9; i++)
         {
            var knot = new RopeKnot();
            previousKnot.Moved += knot.PredecessorMoved;
            previousKnot = knot;
         }
         var tail = new RopeTail();
         previousKnot.Moved += tail.PredecessorMoved;
         Solve(head, tail);
      }
      
      private static void Solve(RopeHead head, RopeTail tail)
      {
         foreach (var line in File.ReadAllLines(InputFilePath))
         {
            head.Move(
               Direction.FromInstruction(line[0]),
               int.Parse(line.Substring(2))
            );
         }
         HashSet<Point> distinctPoints = new HashSet<Point>(tail.History);
         Console.WriteLine(distinctPoints.Count);
      }
   }

   public class RopeKnot
   {
      public Point Position = new Point(0,0);
      
      public event RopeKnotMovedEventHandler Moved;
      
      protected void OnMoved()
      {
         if (Moved != null)
            Moved(this, new RopeKnotMovedEventArgs(Position));
      }
      
      public virtual void PredecessorMoved(object sender, RopeKnotMovedEventArgs args)
      {
         int horizDiff = args.NewPosition.X - Position.X;
         int vertDiff = args.NewPosition.Y - Position.Y;
         if (Math.Abs(horizDiff) > 1 || Math.Abs(vertDiff) > 1)
         {
            Position = new Point(Position.X + Math.Sign(horizDiff), Position.Y + Math.Sign(vertDiff));
            OnMoved();
         }
      }
   }

   public delegate void RopeKnotMovedEventHandler(object sender, RopeKnotMovedEventArgs args);

   public class RopeKnotMovedEventArgs : EventArgs
   {
      public Point NewPosition { get; private set; }
      
      public RopeKnotMovedEventArgs(Point newPosition)
      {
         NewPosition = newPosition;
      }
   }

   public class Direction
   {
      private static readonly Dictionary<char,Direction> Decoder = new Dictionary<char,Direction>();

      public readonly int HorizontalDelta;
      public readonly int VerticalDelta;
      
      private Direction(int horizontalDelta, int verticalDelta)
      {
         HorizontalDelta = horizontalDelta;
         VerticalDelta = verticalDelta;
      }
      
      public static readonly Direction Up = new Direction(0,-1);
      public static readonly Direction Down = new Direction(0,1);
      public static readonly Direction Left = new Direction(1,0);
      public static readonly Direction Right = new Direction(-1,0);
      
      public static Direction FromInstruction(char instruction)
      {
         if (!Decoder.Any())
         {
            Decoder['U'] = Direction.Up;
            Decoder['D'] = Direction.Down;
            Decoder['L'] = Direction.Left;
            Decoder['R'] = Direction.Right;
         }
         return Decoder[instruction];
      }
   }

   public class RopeHead : RopeKnot
   {
      public void Move(Direction direction, int distance)
      {
         for (int d = 0; d < distance; d++)
         {
            Position = new Point(Position.X + direction.HorizontalDelta, Position.Y + direction.VerticalDelta);
            OnMoved();
         }
      }
   }

   public class RopeTail : RopeKnot
   {
      private readonly List<Point> mHistory = new List<Point>();
      
      public RopeTail()
      {
         mHistory.Add(Position);
      }
      
      public override void PredecessorMoved(object sender, RopeKnotMovedEventArgs args)
      {
         base.PredecessorMoved(sender, args);
         mHistory.Add(Position);
      }
      
      public IEnumerable<Point> History => mHistory;
   }
}

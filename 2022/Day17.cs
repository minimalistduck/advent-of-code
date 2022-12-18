using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static class DaySeventeen
   {
      private const string ExampleInput = ">>><<><>><<<>><>>><<<>>><<<><<<>><>><<>>";
      private const string InputFilePath = @"D:\David\Coding\AdventOfCode2022\Input-Day17.txt";
      private const long PartTwoRockCount = 1000000000000L;

      public static void ReproduceProblemStatement()
      {
         Console.WriteLine(RockFall.SimulateVerbose(ExampleInput, 11L));
      }

      public static void SolveExamplePartOne()
      {
         Console.WriteLine(RockFall.Simulate(ExampleInput, 2022L));
      }
      
      public static string RealInput { get { return File.ReadAllText(InputFilePath).Trim(); } }
      
      public static void SolvePartOne()
      {
         Console.WriteLine(RockFall.Simulate(RealInput, 2022L));
      }
      
      public static void EstimateTimeToSolvePartTwo()
      {
         var timer = Stopwatch.StartNew();
         RockFall.Simulate(RealInput, PartTwoRockCount / 1_000_000L);
         Console.WriteLine("Estimate that solving part two will take {0} hours", timer.Elapsed.TotalSeconds * 1_000_000 / 60 / 60);
      }
   }

   public static class RockFall
   {
      public static long Simulate(string jetPattern, long rockCount)
      {
         var rockFactory = new RockFactory();
         var gasJetSource = new GasJet(jetPattern);
         var chamber = new Chamber();
         var movements = new Action<Chamber, Rock>[] {
            (ch, rk) => ch.TryMoveLeft(rk),
            (ch, rk) => throw new InvalidOperationException(),
            (ch, rk) => ch.TryMoveRight(rk)
         };
         for (long r = 1L; r <= rockCount; r++)
         {
            var rock = rockFactory.Next(chamber.TopOfTower + 4L);
            do
            {
               movements[gasJetSource.NextMovement()](chamber, rock);
            } while (chamber.TryMoveDown(rock));
         }
         return chamber.TopOfTower;
      }
      
      public static long SimulateVerbose(string jetPattern, long rockCount)
      {
         var rockFactory = new RockFactory();
         var gasJetSource = new GasJet(jetPattern);
         var chamber = new Chamber();
         var movements = new Action<Chamber, Rock>[] {
            (ch, rk) => ch.TryMoveLeft(rk),
            (ch, rk) => throw new InvalidOperationException(),
            (ch, rk) => ch.TryMoveRight(rk)
         };
         for (long r = 1L; r <= rockCount; r++)
         {
            var rock = rockFactory.Next(chamber.TopOfTower + 4L);
            do
            {
               Console.WriteLine(chamber.ToString(rock));
               Console.WriteLine();
               movements[gasJetSource.NextMovement()](chamber, rock);
            } while (chamber.TryMoveDown(rock));
         }
         return chamber.TopOfTower;
      }

   }

   public class RockFactory
   {
      public static readonly int TemplateCount;
      private static uint[][] mRockTemplates;
      private int mNextRockTemplate;
      
      static RockFactory()
      {
         var rockTemplates = new List<uint[]>();
         // Each template encodes the rock at its starting position over the chamber
         rockTemplates.Add(new uint[] {
            0b000111100u
         });
         rockTemplates.Add(new uint[] {
            0b000010000u,
            0b000111000u,
            0b000010000u
         });
         rockTemplates.Add(new uint[] {
            0b000001000u,
            0b000001000u,
            0b000111000u
         }.Reverse().ToArray());  // written top-to-bottom but used bottom-to-top
         rockTemplates.Add(new uint[] {
            0b000100000u,
            0b000100000u,
            0b000100000u,
            0b000100000u
         });
         rockTemplates.Add(new uint[] {
            0b000110000u,
            0b000110000u
         });
         mRockTemplates = rockTemplates.ToArray();
         TemplateCount = mRockTemplates.Length;
      }
      
      public RockFactory()
      {
         mNextRockTemplate = 0;
      }
      
      public Rock Next(long startingHeight)
      {
         var template = mRockTemplates[mNextRockTemplate];
         mNextRockTemplate = (mNextRockTemplate + 1) % TemplateCount;
         var mutableCopy = template.ToArray();
         return new Rock(mutableCopy, startingHeight);
      }
   }

   public class Rock
   {
      // Conventions: Height is the height of the bottom of this rock
      // Outline[i] is the rock's row at Height + i
      public long Height;
      public readonly uint[] Outline;
      
      public Rock(uint[] outline, long startingHeight)
      {
         Outline = outline;
         Height = startingHeight;
      }
      
      public void MoveLeft()
      {
         for (var i = 0; i < Outline.Length; i++)
         {
            Outline[i] = Outline[i] << 1;
         }
      }
      
      public void MoveRight()
      {
         for (var i = 0; i < Outline.Length; i++)
         {
            Outline[i] = Outline[i] >> 1;
         }
      }
      
      public void MoveDown()
      {
         Height -= 1L;
      }
      
      public void MoveUp()
      {
         Height += 1L;
      }
   }

   public class GasJet
   {
      private readonly string mPattern;
      private int mNextJet = 0;

      public GasJet(string pattern)
      {
         mPattern = pattern;
      }
      
      public int NextMovement()
      {
         char nextJet = mPattern[mNextJet];
         mNextJet = (mNextJet + 1) % mPattern.Length;
         // '<' is 60, '>' is 62, which is handy :-)
         return (int)(nextJet - '<');
      }
   }

   public class Chamber
   {
      public const uint FullRow = 0x1FFu;
      public const uint EmptyRow = 0x101u;

      public long TopOfTower = 0L;
      private List<uint> mLiveContent = new List<uint>();
      private long mPurgedRowCount = 0L;
      private const int MinPurgeBenefit = 100;
      private const int EmptyRowsAtTop = 10;

      public Chamber()
      {
         mLiveContent.Add(FullRow);
         for (int h = 0; h < EmptyRowsAtTop; h++)
            mLiveContent.Add(EmptyRow);   
      }
      
      public bool TryMoveLeft(Rock rockToMove)
      {
         rockToMove.MoveLeft();
         bool result = HasFeasiblePosition(rockToMove);
         if (!result)
            rockToMove.MoveRight();
         return result;
      }
      
      public bool TryMoveRight(Rock rockToMove)
      {
         rockToMove.MoveRight();
         bool result = HasFeasiblePosition(rockToMove);
         if (!result)
            rockToMove.MoveLeft();
         return result;
      }
      
      public bool TryMoveDown(Rock rockToMove)
      {
         rockToMove.MoveDown();
         bool result = HasFeasiblePosition(rockToMove);
         if (!result)
         {
            rockToMove.MoveUp();
            Stop(rockToMove);
         }
         return result;
      }
      
      private bool HasFeasiblePosition(Rock rock)
      {
         // Convention: when rock is at height h, this corresponds to element h of mLiveContent.
         // In other words, height 0 is the rock that forms the base of the chamber
         if (rock.Height - mPurgedRowCount > int.MaxValue)
            throw new ArithmeticException("Tower is too high");
         var chamberOffset = (int)(rock.Height - mPurgedRowCount);
         for (var i = 0; i < rock.Outline.Length; i++)
         {
            if ((rock.Outline[i] & mLiveContent[i + chamberOffset]) > 0)
              return false;
         }
         return true;
      }
      
      private void Stop(Rock rock)
      {
         // Update the chamber with this rock in its current position
         if (rock.Height - mPurgedRowCount > int.MaxValue)
            throw new ArithmeticException("Tower is too high");
         var chamberOffset = (int)(rock.Height - mPurgedRowCount);
         for (var i = 0; i < rock.Outline.Length; i++)
         {
            mLiveContent[i + chamberOffset] = mLiveContent[i + chamberOffset] | rock.Outline[i];
         }
         
         // Resize the chamber if necessary
         TopOfTower = Math.Max(TopOfTower, rock.Height + rock.Outline.Length - 1);
         if (TopOfTower + EmptyRowsAtTop - mPurgedRowCount - mLiveContent.Count > int.MaxValue)
            throw new ArithmeticException("Tower is too high");
         int extraHeightNeeded = (int)(TopOfTower + EmptyRowsAtTop - mPurgedRowCount - mLiveContent.Count);
         for (int i = 0; i < extraHeightNeeded; i++)
         {
            mLiveContent.Add(EmptyRow);
         }
         
         // Purge any of mLiveContent that is no longer reachable from above
         var h = mLiveContent.Count - 1;
         var blocked = EmptyRow;
         while (blocked != FullRow && h >= 0)
         {
            blocked |= mLiveContent[h];
            h--;
         }
         if (h > MinPurgeBenefit)
         {
            mPurgedRowCount += h;
            mLiveContent = new List<uint>(mLiveContent.Skip(h));
         }
      }
      
      private string ToChamberString(uint line)
      {
         char[] result = new char[9];
         result[0] = '|';
         result[8] = '|';
         for (int i = 1; i < 8; i++)
         {
            uint mask = (0x1u << i);
            result[8 - i] = (mask & line) > 0 ? '#' : '.';
         }
         return new string(result);
      }
      
      private string WithRock(string line, uint rockLine)
      {
         char[] result = line.ToCharArray();
         for (int i = 1; i < 8; i++)
         {
            uint mask = (0x1u << i);
            if ((mask & rockLine) > 0)
            {
               result[8 - i] = '@';         
            }
         }
         return new string(result);
      }
      
      private string WithTopOfTower(string line)
      {
         char[] result = line.ToCharArray();
         result[0] = '>';
         result[8] = '<';
         return new string(result);
      }
         
      public string ToString(Rock rock)
      {
         var result = mLiveContent.Select(ToChamberString).ToArray();
         if (mPurgedRowCount == 0)
            result[0] = "+-------+";
         else
            result[0] = ":::::::::";
         if (TopOfTower > 0)
            result[TopOfTower-mPurgedRowCount] = WithTopOfTower(result[TopOfTower-mPurgedRowCount]);
         for (int h = 0; h < rock.Outline.Length; h++)
            result[h+rock.Height-mPurgedRowCount] = WithRock(result[h+rock.Height-mPurgedRowCount], rock.Outline[h]); 
         return string.Join(Environment.NewLine, result.Reverse());
      }
   }
}

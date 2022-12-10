using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayTen
   {
      private const string InputFilePath = "Input-Day10.txt";
    
      public static void Solve()
      {
         var timeline = Timeline.FromInstructions(File.ReadAllLines(InputFilePath));
         //DayTen.SolvePartOne(timeline);
         DayTen.SolvePartTwo(timeline);
      }
      
      public static void SolvePartOne(Timeline timeline)
      {
         int result = 0;
         for (int cycle = 20; cycle <= 220; cycle += 40)
         {
            result += timeline.GetSignalStrength(cycle);
         }
         Console.WriteLine($"Part one: {result}");
      }
      
      public static void SolvePartTwo(Timeline timeline)
      {
         StringBuilder screen = new StringBuilder();
         for (int cycle = 1; cycle <= 240; cycle++)
         {
            int pixelX = (cycle - 1) % 40;
            screen.Append(
               timeline.IsInRange(cycle, pixelX-1, pixelX+1) ? '#' : '.'
            );
         }
         var display = screen.ToString();
         for (int startOfRow = 0; startOfRow < 240; startOfRow += 40)
         {
            Console.WriteLine(display.Substring(startOfRow, 40));      
         }
      }
   }

   public class Timeline
   {
      private const int InitialRegisterValue = 1;
      private readonly List<int> mValueAtCycle = new List<int>();
      private int mCurrentRegisterValue = InitialRegisterValue;

      private Timeline()
      {
         // insert a dummy value so that the first cycle is at position 1
         mValueAtCycle.Add(InitialRegisterValue);
      }
      
      public void Advance()
      {
         mValueAtCycle.Add(mCurrentRegisterValue);
      }
      
      public void AddToRegister(int amount)
      {
         mCurrentRegisterValue += amount;
      }
      
      public int GetSignalStrength(int cycleNumber)
      {
         return cycleNumber * mValueAtCycle[cycleNumber];
      }
      
      public bool IsInRange(int cycleNumber, int lowerInclusive, int higherInclusive)
      {
         return lowerInclusive <= mValueAtCycle[cycleNumber] && mValueAtCycle[cycleNumber] <= higherInclusive;
      }
      
      public override string ToString()
      {
         return string.Join(",", mValueAtCycle.Skip(1));
      }

      public static Timeline FromInstructions(IEnumerable<string> instructions)
      {
         var result = new Timeline();
         foreach (string inst in instructions)
         {
            if (inst.StartsWith("addx"))
            {
               result.Advance();
               result.Advance();
               result.AddToRegister(int.Parse(inst.Substring(5)));
            }
            else if (inst.Equals("noop"))
            {
               result.Advance();
            }
            else
            {
               throw new InvalidDataException($"Unrecognised instruction: {inst}");
            }
         }
         return result;
      }
   }
}
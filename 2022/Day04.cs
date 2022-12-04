using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public class DayFour
   {
      const string InputFilePath = "Input-Day04.txt";

      public static void Solve()
      {
         var parsedLines = File.ReadAllLines(InputFilePath).Select(line => line.Split('-',','))
            .Select(splitLine => splitLine.Select(item => int.Parse(item)).ToArray());
         //Console.WriteLine(SolvePartOne(parsedLines));
         Console.WriteLine(SolvePartTwo(parsedLines));
      }

      public static int SolvePartOne(IEnumerable<int[]> parsedLines)
      {
         return parsedLines.Count(boundaries => IsFullyContained(boundaries[0], boundaries[1], boundaries[2], boundaries[3]));
      }

      private static bool IsFullyContained(int startOfRangeA, int endOfRangeA, int startOfRangeB, int endOfRangeB)
      {
         return (startOfRangeA <= startOfRangeB && endOfRangeA >= endOfRangeB) ||
            (startOfRangeB <= startOfRangeA && endOfRangeB >= endOfRangeA);
      }

      public static int SolvePartTwo(IEnumerable<int[]> parsedLines)
      {
         return parsedLines.Count(boundaries => HasOverlap(boundaries[0], boundaries[1], boundaries[2], boundaries[3]));
      }

      private static bool HasOverlap(int startOfRangeA, int endOfRangeA, int startOfRangeB, int endOfRangeB)
      {
         if (endOfRangeA < startOfRangeB)
            return false;
         if (endOfRangeB < startOfRangeA)
            return false;
         return true;
      }
   }
}

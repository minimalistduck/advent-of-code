using System;
using System.Collections.Generic;

namespace DavidDylan.AdventOfCode2022
{
   public static class ChristmasDay
   {
      public static string[] ExampleInput = new []
      {
         "1=-0-2",
         "12111",
         "2=0=",
         "21",
         "2=01",
         "111",
         "20012",
         "112",
         "1=-1=",
         "1-12",
         "12",
         "1=",
         "122"
      };

      public static void SolvePartOneExample()
      {
         Console.WriteLine(ExampleInput.Select(Snafu.Parse).Sum());
      }
      
      public const string InputFilePath = "Input-Day25.txt";
      
      public static string[] RealInput
      {
         get { return System.IO.File.ReadAllLines(InputFilePath); }
      }
      
      public static void SolvePartOne()
      {
         var decimalResult = RealInput.Select(Snafu.Parse).Sum();
         Console.WriteLine($"Result in decimal is {decimalResult}");
         
         // Find an upper bound to use as a starting guess
         string guess = "2";
         while (Snafu.Parse(guess) < decimalResult)
         {
            guess = guess + "0";
         }
         Guess(guess, decimalResult);
      }
      
      // I used this method and trial-and-error to convert my answer back to SNAFU
      public static void Guess(string snafuNumber, long target)
      {
         var decimalNumber = Snafu.Parse(snafuNumber);
         var delta = Math.Abs(target - decimalNumber);
         Console.WriteLine($"{snafuNumber} is {decimalNumber} ... {delta} away.");
      }
   }

   public static class Snafu
   {
      public static readonly Dictionary<char, long> DigitValues = new Dictionary<char, long>
      {
         ['='] = -2L,
         ['-'] = -1L,
         ['0'] = 0L,
         ['1'] = 1L,
         ['2'] = 2L
      };
      
      public static long Parse(string snafuNumber)
      {
         long placeMultiplier = 1L;
         Stack<char> snafuChars = new Stack<char>(snafuNumber);
         long result = 0L;
         while (snafuChars.Any())
         {
            result += DigitValues[snafuChars.Pop()] * placeMultiplier;
            placeMultiplier *= 5L;
         }
         return result;
      }
   }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DavidDylan.AdventOfCode2022
{
   public class DayThree
   {
      const string InputFilePath = "Input-Day03.txt";

      [Test]
      public static void Prepare()
      {
         var firstExample = "vJrwpWtwJgWrhcsFMMfFFhFp";
         var split = SplitContents(firstExample);
         Assert.That(split[0].Length, Is.EqualTo(split[1].Length));

         var commonItem = FindCommonItem(firstExample);
         Assert.That(commonItem, Is.EqualTo('p'));

         // 16 (p), 38 (L)
         Assert.That(GetPriority('p'), Is.EqualTo(16));
         Assert.That(GetPriority('L'), Is.EqualTo(38));
      }

      [Test]
      public static void SolvePartOne()
      {
         int sumOfPriorities = File.ReadAllLines(InputFilePath).Select(
            line => GetPriority(FindCommonItem(line))).Sum();
         Console.WriteLine(sumOfPriorities);
      }

      [Test]
      public static void SolvePartTwo()
      {
         int sumOfPriorities = 0;
         var lines = File.ReadAllLines(InputFilePath);
         int lineIndex = 0;
         while (lineIndex < lines.Length)
         {
            HashSet<char> one = new HashSet<char>(lines[lineIndex]);
            HashSet<char> two = new HashSet<char>(lines[lineIndex + 1]);
            HashSet<char> three = new HashSet<char>(lines[lineIndex + 2]);
            one.IntersectWith(two);
            one.IntersectWith(three);
            sumOfPriorities += GetPriority(one.Single());
            lineIndex += 3;
         }
         Console.WriteLine(sumOfPriorities);
      }

      private static string[] SplitContents(string contents)
      {
         var result = new string[2];
         result[0] = contents.Substring(0, contents.Length / 2);
         result[1] = contents.Substring(contents.Length / 2);
         return result;
      }

      private static char FindCommonItem(string contents)
      {
         var split = SplitContents(contents);
         HashSet<char> one = new HashSet<char>(split[0]);
         HashSet<char> two = new HashSet<char>(split[1]);
         one.IntersectWith(two);
         return one.Single();
      }

      private static int GetPriority(char item)
      {
         if (char.IsLower(item))
         {
            return item - 'a' + 1;
         }
         else if (char.IsUpper(item))
         {
            return item - 'A' + 27;
         }
         else
            throw new ArgumentOutOfRangeException("item");
      }
   }
}

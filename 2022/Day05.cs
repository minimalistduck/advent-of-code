using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayFive
   {
      const string InputFilePath = @"Input-Day05.txt";

      public static void Solve()
      {
         Action<Stack<char>[], int, int, int> moveOperation = MoveUsingCrateMover9001;
         var rawStacks = new string[] {
            "", // for simplicity, we'll stay 1-based and leave element 0 empty and untouched
            "BSVZGPW",
            "JVBCZF",
            "VLMHNZDC",
            "LDMZPFJB",
            "VFCGJBQH",
            "GFQTSLB",
            "LGCZV",
            "NLG",
            "JFHC"
         };
         Stack<char>[] stacks = rawStacks.Select(stck => new Stack<char>(stck)).ToArray();
         //Console.WriteLine(stacks[2].Peek());
         var moveLines = File.ReadAllLines(InputFilePath).Where(line => line.StartsWith("move"));
         Console.WriteLine("Found {0} move instructions.", moveLines.Count());
         var instructions = moveLines.Select(line => line.Split(' '));
         foreach (var i in instructions)
         {
            var from = int.Parse(i[3]);
            var to = int.Parse(i[5]);
            var count = int.Parse(i[1]);
            moveOperation(stacks, from, to, count);
         }
         var result = new string(stacks.Skip(1).Select(stck => stck.Peek()).ToArray());
         Console.WriteLine(result);
      }

      private static void MoveUsingCrateMover9001(Stack<char>[] stacks, int from, int to, int count)
      {
         Stack<char> interimStack = new Stack<char>();
         for (int n = 0; n < count; n++)
         {
            interimStack.Push(stacks[from].Pop());
         }
         for (int n = 0; n < count; n++)
         {
            stacks[to].Push(interimStack.Pop());
         }
      }

     private static void MoveUsingCrateMover9000(Stack<char>[] stacks, int from, int to, int count)
     {
        for (int n = 0; n < count; n++)
        {
           stacks[to].Push(stacks[from].Pop());
        }
     }
   }
}

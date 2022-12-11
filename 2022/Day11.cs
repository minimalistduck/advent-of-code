using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static partial class DayEleven
   {
      const string InputFilePath = @"Input-Day11.txt";

      public static void GenerateOtherPartOfClass()
      {
         Queue<string> inputQueue = new Queue<string>(File.ReadAllLines(InputFilePath));
         Console.WriteLine("partial class DayEleven");
         Console.WriteLine("{");
         Console.WriteLine("public static Monkey[] CreateMonkeys()");
         Console.WriteLine("{");
         Console.WriteLine("   List<Monkey> monkeys = new List<Monkey>();");
         Console.WriteLine("   Monkey monkey;");
         while (inputQueue.Any())
         {
            var indexLine = inputQueue.Dequeue();
            var itemsLine = inputQueue.Dequeue();
            var opLine = inputQueue.Dequeue();
            var testLine = inputQueue.Dequeue();
            var ifTrueLine = inputQueue.Dequeue();
            var ifFalseLine = inputQueue.Dequeue();
            if (inputQueue.Any())
            {
               var blankLine = inputQueue.Dequeue();
               if (!string.IsNullOrWhiteSpace(blankLine))
               {
                  throw new InvalidOperationException($"Unexpected input after {indexLine} {blankLine}");
               }
            }

            Console.WriteLine("   monkey = new Monkey() {");
            
            var indexPart = indexLine.Replace("Monkey ", "").Replace(":","");
            Console.WriteLine("      Index = {0},  // {1}", indexPart, indexLine);
            
            var funcPart = ExtractPayload(opLine).Replace("new =","old =>");
            Console.WriteLine("      Operation = {0},  // {1}", funcPart, opLine);
            
            var testPart = ExtractPayload(testLine).Replace("divisible by", "%");
            Console.WriteLine("      Test = x => x {0} == 0,  // {1}", testPart, testLine);
            
            var ifTrueMonkeyIndex = ExtractPayload(ifTrueLine).Replace("throw to monkey ","");
            Console.WriteLine("      TargetIndexIfTrue = {0},  // {1}", ifTrueMonkeyIndex, ifTrueLine);
            
            var ifFalseMonkeyIndex = ExtractPayload(ifFalseLine).Replace("throw to monkey ","");
            Console.WriteLine("      TargetIndexIfFalse = {0}  // {1}", ifFalseMonkeyIndex, ifFalseLine);
            
            Console.WriteLine("   };");
            Console.WriteLine("   monkey.StartWithItems({0});  // {1}", ExtractPayload(itemsLine), itemsLine);
            Console.WriteLine("   monkeys.Add(monkey);");
            Console.WriteLine("");
         }
         Console.WriteLine("   return monkeys.ToArray();");
         Console.WriteLine("}");
         Console.WriteLine("}");
      }
      
      private static string ExtractPayload(string line)
      {
         return line.Split(':')[1].Trim();
      }

      public static void SolvePartOne()
      {
         Solve(20, x => x/3);
      }
      
      // TODO: Ideally would change Monkey definition so this can be calculated
      // from the monkeys array.
      const long CombinedFactors = 5L * 2L * 13L * 19L * 11L * 3L * 7L * 17L;
      //const long ExampleCombinedFactors = 13L * 17L * 19L * 23L;
      
      public static void SolvePartTwo()
      {
         // We can just look at the remainder when divided by CombinedFactors.
         // Modelling an item's current worry level as x * CombinedFactors + c.
         // Addition will only change c.
         // x * CombinedFactors will pass each monkey's divisibility test, so
         // the result of the test is only determined by c.
         // Squaring yields x^2 * CombinedFactors + 2 * x * CombinedFactors * c + c^2
         // and only c^2 affects the result of the test
         // Multipying by y yields x * y * CombinedFactors + y * c
         // and only y * c affects the result of the test
         Func<long, long> adjuster = x => x % CombinedFactors;
         Solve(10000, adjuster);
      }

      public static void Solve(int roundCount, Func<long, long> worryAdjuster)
      {
         // You need to autogenerate CreateMonkeys from the input - see GenerateOtherPartOfClass
         var monkeys = CreateMonkeys();
         foreach (var monkey in monkeys)
         {
            monkey.Adjustment = worryAdjuster;
         }
         for (int round = 1; round <= roundCount; round++)
         {
            foreach (var monkey in monkeys)
            {
               monkey.TakeTurn(monkeys[monkey.TargetIndexIfTrue], monkeys[monkey.TargetIndexIfFalse]);
            }
            if (round == 1 || round == 20 || round % 1000 == 0)
            {
               Console.WriteLine("== After round {0} ==", round);
               foreach (var monkey in monkeys)
               {
                  Console.WriteLine(monkey.ToString());
               }
            }
         }
         long result = 1;
         foreach (var monkey in monkeys.OrderByDescending(m => m.InspectionCount).Take(2))
         {
            result *= monkey.InspectionCount;
         }
         Console.WriteLine(result);
      }
   }

   public class Monkey
   {
      public int Index { get; set; }
      public Func<long, long> Operation { get; set; }
      public Func<long, long> Adjustment { get; set; }
      public Predicate<long> Test { get; set; }
      public int TargetIndexIfTrue { get; set; }
      public int TargetIndexIfFalse { get; set; }
      private readonly Queue<long> mCurrentItems = new Queue<long>();
      public int InspectionCount { get; private set; }
      
      public void StartWithItems(params int[] items)
      {
         foreach (var item in items)
         {
            mCurrentItems.Enqueue(item);
         }
      }
      
      public void ReceiveItem(long item)
      {
         mCurrentItems.Enqueue(item);
      }
      
      public void TakeTurn(Monkey targetIfTrue, Monkey targetIfFalse)
      {
         InspectionCount += mCurrentItems.Count;
         while (mCurrentItems.Any())
         {
            var currentItem = mCurrentItems.Dequeue();
            currentItem = Operation(currentItem);
            currentItem = Adjustment(currentItem);
            var target = Test(currentItem) ? targetIfTrue : targetIfFalse;
            target.ReceiveItem(currentItem);
         }
      }
      
      public override string ToString()
      {
         return $"Monkey {Index} inspected items {InspectionCount} times.";
      }
   }
}

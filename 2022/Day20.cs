using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayTwenty
   {
      const string InputFilePath = "Input-Day20.txt";

      public static IEnumerable<long> ExampleInput
      {
         get
         {
            return new long[] { 1,2,-3,3,-2,0,4 };
         }
      }

      public const long DecryptionKey = 811589153L;

      public static IEnumerable<long> LoadRealInput()
      {
         return File.ReadAllLines(InputFilePath).Select(line => long.Parse(line));
      }

      [Test]
      public static void Prepare()
      {
         long neg = -123456;
         Console.WriteLine(neg % 40L);
      }

      [Test]
      public static void SolveExample()
      {
         Console.WriteLine(Decrypt(ExampleInput));
      }

      [Test]
      public static void SolvePartOne()
      {
         Console.WriteLine(Decrypt(LoadRealInput()));
      }

      [Test]
      public static void SolvePartTwoExample()
      {
         var decryptedValues = ExampleInput.Select(v => v * DecryptionKey);
         Assert.That(Decrypt(decryptedValues, 10), Is.EqualTo(1623178306L));
      }

      [Test]
      public static void SolvePartTwo()
      {
         var decryptedValues = LoadRealInput().Select(v => v * DecryptionKey);
         Console.WriteLine(Decrypt(decryptedValues, 10));
      }

      public static long Decrypt(IEnumerable<long> values, int numberOfPasses = 1)
      {
         var arrangement = new Arrangement(values);
         for (int p = 0; p < numberOfPasses; p++)
         {
            arrangement.Mix();
         }
         List<long> groveCoordinates = new List<long>();
         var currentElement = arrangement.ZeroElement;
         for (int i = 1; i <= 3000; i++)
         {
            currentElement = currentElement.Next;
            if (i % 1000 == 0)
               groveCoordinates.Add(currentElement.Value);
         }
         Console.WriteLine(string.Join(",", groveCoordinates.Select(v => v.ToString())));
         return groveCoordinates.Sum();
      }
   }

   public class Arrangement
   {
      private readonly Element[] mElementsInOriginalOrder;
      public readonly Element ZeroElement;

      public Arrangement(IEnumerable<long> values)
      {
         mElementsInOriginalOrder = values.Select(v => new Element(v)).ToArray();
         ZeroElement = mElementsInOriginalOrder.Single(elt => elt.Value == 0L);
         mElementsInOriginalOrder.Last().JoinTo(mElementsInOriginalOrder.First());
         for (int i = 1; i < mElementsInOriginalOrder.Length; i++)
         {
            mElementsInOriginalOrder[i - 1].JoinTo(mElementsInOriginalOrder[i]);
         }
      }

      public void Mix()
      {
         foreach (var element in mElementsInOriginalOrder)
         {
            element.Move(mElementsInOriginalOrder.Length);
         }
      }
   }

   public class Element
   {
      public readonly long Value;

      public Element(long elementValue)
      {
         Value = elementValue;
      }

      public Element Next;
      public Element Previous;

      public void JoinTo(Element next)
      {
         next.Previous = this;
         this.Next = next;
      }

      public void Move(long length)
      {
         var sign = Math.Sign(Value);
         var moveCount = Math.Abs(Value);

         // after passing each of the other (length - 1) elements,
         // this element will be back in its original position
         // so the final position is only affected by the remainder
         long truncatedMoveCount = moveCount % (length - 1L);
         if (truncatedMoveCount == 0L)
            return;

         // Take out
         Previous.JoinTo(Next);
         var cursor = Previous;
         Previous = null;
         Next = null;

         // Find insertion point
         Func<Element,Element> moveCursor = sign > 0L ? elt => elt.Next : elt => elt.Previous;
         for (long m = 0; m < truncatedMoveCount; m++)
         {
            cursor = moveCursor(cursor);
         }
         // insert after
         var next = cursor.Next;
         cursor.JoinTo(this);
         this.JoinTo(next);           
      }
   }
}

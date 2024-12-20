using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day11Program
{
  public static void Main()
  {
    const int blinkCount = 25;
    const string input = "337 42493 1891760 351136 2 6932 73 0";
    var initialStones = input.Split(" ").Select(n => new Stone(n)).ToArray();
    
    var oldBag = new StoneBag();
    oldBag.AddRange(initialStones);
    for (int i = 0; i < blinkCount; i++)
    {
      var nextBag = new StoneBag();
      foreach (var kvp in oldBag.Entries)
      {
        foreach (var newStone in kvp.Key.Blink())
        {
          nextBag.Add(newStone, kvp.Value);
        }
      }
      oldBag = nextBag;
    }

    var result = 0L;
    foreach (var kvp in oldBag.Entries)
    {
      result += kvp.Value;
    }
    Console.WriteLine("After {0} blinks, have {1} stones", blinkCount, result);
  }
}

public class Stone
{
   private readonly string _label;
   private readonly long _number;
   
   public Stone(string label)
   {
      _label = label;
      _number = long.Parse(label);
   }
   
   public Stone(long number)
   {
      _label = number.ToString();
      _number = number;
   }
   
   public string Label => _label;
   
   public Stone[] Blink()
   {
      foreach (var rule in Rules)
      {
         var possibleResult = rule(this);
         if (possibleResult != null)
         {
            return possibleResult;
         }
      }
      throw new InvalidOperationException("None of the rules applied to this stone.");
   }
   
   private static Func<Stone, Stone[]>[] Rules = new Func<Stone, Stone[]>[] {
      ZeroToOne,
      SplitIfEven,
      Multiply
   };
   
   private static Stone[] ZeroToOne(Stone originalStone)
   {
      if (originalStone._number == 0L)
      {
         return new Stone[] { new Stone("1") };
      }
      return null;
   }
   
   private static Stone[] SplitIfEven(Stone originalStone)
   {
      var len = originalStone._label.Length;
      if (len % 2 == 0)
      {
         var partOne = originalStone._label.Substring(0, len / 2);
         var partTwo = long.Parse(originalStone._label.Substring(len / 2));
         return new Stone[] { new Stone(partOne), new Stone(partTwo) };
      }
      return null;
   }
   
   private static Stone[] Multiply(Stone originalStone)
   {
      return new Stone[] { new Stone(originalStone._number * 2024L) };
   }
}

public class StoneBag
{
  private readonly Dictionary<string, long> _entries = new Dictionary<string, long>();
  
  public void Add(Stone stone, long count)
  {
    var oldValue;
    _entries.TryGetValue(stone.Label, out oldValue);
    _entries[stone.Label] = oldValue + count;
  }
  
  public void AddRange(IEnumerable<Stone> stones)
  {
    foreach (var stone in stones)
    {
      Add(stone, 1L);
    }
  }
  
  public IEnumerable<KeyValuePair<Stone, long>> Entries =>
    _entries.Select(kvp => new KeyValuePair(new Stone(kvp.Key), kvp.Value));
}

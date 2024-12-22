using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day22Program
{
  public static void Main()
  {
     const string inputFilePath = @"D:\David\Coding\AdventOfCode\2024\Day22-input.txt";
     var startNumbers = File.ReadAllLines(inputFilePath).Select(ulong.Parse).ToArray();
     
     // first example
     var secret = 123UL;
     for (int i = 1; i <= 10; i++)
     {
       secret = NextSecret(secret);
       //Console.WriteLine(secret);
     }
     
     var partTwoTracker = new Dictionary<string, ulong>();
     var partOneResult = 0UL;
     foreach (var startSecret in startNumbers)
     {
       var prices = new ulong[2001];
       var deltas = new long[2001];
       var instructionPrices = new Dictionary<string, ulong>();
       secret = startSecret;
       prices[0] = startSecret % 10;
       for (int i = 1; i < prices.Length; i++)
       {
         secret = NextSecret(secret);
         prices[i] = secret % 10;
         deltas[i] = (long)prices[i] - (long)prices[i-1];
       }
       partOneResult += secret;
       
       // Work backwards so that earlier occurrences overwrite later ones
       for (int i = prices.Length - 1; i >= 4; i--)
       {
         var instruction = string.Join(",", deltas.Skip(i-3).Take(4));
         instructionPrices[instruction] = prices[i];
       }
       
       Merge(partTwoTracker, instructionPrices);
     }
     
     Console.WriteLine("Part one: {0}", partOneResult);
     
     var partTwoResult = partTwoTracker.Values.Max();
     Console.WriteLine("Most amount of bananas is {0}", partTwoResult);
  }
  
  public static ulong NextSecret(ulong currentSecret)
  {
    var mult = currentSecret * 64;
    var result = Mix(currentSecret, mult);
    result = Prune(result);
    var div = result / 32;
    result = Mix(result, div);
    result = Prune(result);
    var mult2 = result * 2048;
    result = Mix(result, mult2);
    result = Prune(result);
    return result;
  }
  
  private static ulong Mix(ulong first, ulong second)
  {
    return second ^ first;
  }
  
  private static ulong Prune(ulong input)
  {
    return input % 16777216;
  }
  
  private static void Merge(Dictionary<string, ulong> accumulator, Dictionary<string, ulong> increment)
  {
    foreach (var kvp in increment)
    {
      accumulator.TryGetValue(kvp.Key, out var oldValue);
      accumulator[kvp.Key] = oldValue + kvp.Value;
    }
  }
  
  //public static ulong[] Example = 
}


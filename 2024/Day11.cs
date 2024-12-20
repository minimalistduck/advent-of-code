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

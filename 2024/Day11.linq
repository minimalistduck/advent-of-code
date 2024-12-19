<Query Kind="Program">
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

void Main()
{
   const string input = "337 42493 1891760 351136 2 6932 73 0";
   var initialStones = input.Split(" ").Select(n => new Stone(n)).ToArray();
   
   SolvePartOne(initialStones);
   
   SolveFaster(initialStones, 75);
}

private static void SolvePartOne(Stone[] initialStones)
{
   HashSet<string> distinctLabels = new HashSet<string>();
   var stones = initialStones;
   for (int i = 0; i < 25; i++)
   {
      stones = stones.SelectMany(s => s.Blink()).ToArray();
      distinctLabels.UnionWith(stones.Select(s => s.Label));
   }
   
   Console.WriteLine("Part one: {0} with {1} distinct labels seen",
      stones.Length, distinctLabels.Count);
}

private static void SolveFaster(Stone[] initialStones, int blinkCount)
{
   var prevFactory = Stone._factory;
   Stone._factory = new CachingStoneFactory();
   
   ConcurrentDictionary<string, Stone[]> blinkCache = new ConcurrentDictionary<string, Stone[]>();
   //Func<Stone, Stone[]> blinker = s => blinkCache.GetOrAdd(
   //   s.Label,
   //   _ => s.Blink().ToArray()
   //);
   
   var timer = Stopwatch.StartNew();
   var total = 0L;
   foreach (var stone in initialStones)
   {
      Console.WriteLine("Doing {0} at {1} ...", stone.Label, timer.Elapsed);
      total += Recurse(0, blinkCount, stone, blinkCache);
   }
   //var stones = initialStones;
   //for (int i = 0; i < blinkCount; i++)
   //{
   //   stones = stones.SelectMany(blinker).ToArray();
   //}
   
   Console.WriteLine("{2} blinks (with caching): {0}, with {1} distinct stones",
      total, Stone._factory.Count, blinkCount);
   Stone._factory = prevFactory;
}

private static long Recurse(int depth, int maxDepth, Stone stone, ConcurrentDictionary<string, Stone[]> blinkCache)
{
   if (depth == maxDepth)
      return 1L;
   var afterBlink = blinkCache.GetOrAdd(
      stone.Label,
      _ => stone.Blink()
   );
   var result = 0L;
   foreach (var newStone in afterBlink)
   {
      result += Recurse(depth + 1, maxDepth, newStone, blinkCache);
   }
   return result;
}

public class Stone
{
   public static IStoneFactory _factory = new SimpleStoneFactory();
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
         return new Stone[] { _factory.Get("1") };
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
         return new Stone[] { _factory.Get(partOne), _factory.Get(partTwo) };
      }
      return null;
   }
   
   private static Stone[] Multiply(Stone originalStone)
   {
      return new Stone[] { _factory.Get(originalStone._number * 2024L) };
   }
}

public interface IStoneFactory
{
   Stone Get(string label);
   Stone Get(long number);
   int Count { get; }
}

public class SimpleStoneFactory : IStoneFactory
{
   public Stone Get(string label)
   {
      return new Stone(label);
   }
   
   public Stone Get(long number)
   {
      return new Stone(number);
   }
   
   public int Count { get { return 0; } }
}

public class CachingStoneFactory : IStoneFactory
{
   private readonly ConcurrentDictionary<string, Stone> _cache = new ConcurrentDictionary<string, Stone>();
   
   public Stone Get(string label)
   {
      return _cache.GetOrAdd(label, new Stone(label));
   }
   
   public Stone Get(long number)
   {
      var newStone = new Stone(number);
      return _cache.GetOrAdd(newStone.Label, newStone);
   }
   
   public int Count { get { return _cache.Count; } }
}
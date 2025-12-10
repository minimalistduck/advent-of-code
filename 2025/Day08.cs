public static class DayEightProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartOne(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
  var things = new List<Thing>();
  foreach (var line in lines)
  {
    things.Add(new Thing(line));
  }
  
  var pairs = new List<PairOfThing>();
  for (var i = 0; i < things.Count - 1; i++)
  {
    for (var j = i+1; j < things.Count; j++)
    {
      pairs.Add(new PairOfThing(things[i], things[j]));
    }
  }
  
  var nextGroupNum = 1;
  foreach (var thing in things)
  {
    thing.GroupNum = nextGroupNum;
    nextGroupNum++;
  }
  
  pairs.Sort(p => p.DistanceRank);
  
  foreach (var mergingPair in pairs.Take(1000))
  {
    var firstMergingGroup = mergingPair.First.GroupNum;
    var secondMergingGroup = mergingPair.Second.GroupNum;
    foreach (var member in things.Where(t => t.GroupNum == firstMergingGroup || t.GroupNum == secondMergingGroup).ToArray())
    {
      member.GroupNum = nextGroupNum;
    }
    nextGroupNum++;
  }
  
  var groupSizes = new Dictionary<int, int>();
  foreach (var thing in things)
  {
    groupSizes[thing.GroupNum] = 0;
  }
  foreach (var thing in things)
  {
    groupSizes[thing.GroupNum] = groupSizes[thing.GroupNum] + 1;
  }
  
  var sizes = groupSizes.Values.ToArray();
  Array.Sort(sizes);
  Array.Reverse(sizes); // ?
  
  var partOne = 0L;
  foreach (var sz in sizes.Take(3))
  {
    partOne *= sz;
  }
  
  Console.WriteLine(partOne);  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

public class PairOfThings
{
  public readonly Thing First;
  public readonly Thing Second;
  public readonly long DistanceRank; 

  public PairOfThings(Thing first, Thing second)
  {
    First = first;
    Second = second;
    DistanceRank = Thing.DistanceRank(first, second);
  }
}

public class Thing
{
  public readonly string Name;
  public readonly long X;
  public readonly long Y;
  public readonly long Z;
  public int GroupNum;
  
  public Thing(string inputLine)
  {
    Name = inputLine;
    var split = inputLine.Split(",");
    X = long.Parse(split[0]);
    Y = long.Parse(split[1]);
    Z = long.Parse(split[2]);
  }
  
  // using suqare of distance to stay in integer space
  public static long DistanceRank(long first, long second)
  {
    var result = 0L;
    result += Math.Pow(first.X - second.X, 2);
    result += Math.Pow(first.Y - second.Y, 2);
    result += Math.Pow(first.Z - second.Z, 2);
    return result;
  }
}

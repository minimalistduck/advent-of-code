public static class DayEightProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    Solve(lines, false);
    Solve(lines, true);
  }

  private static void Solve(string[] lines, bool isPartTwo)
  {
    var things = new List<Thing>();
    foreach (var line in lines)
    {
      things.Add(new Thing(line));
    }
    
    var pairs = new List<PairOfThings>();
    for (var i = 0; i < things.Count - 1; i++)
    {
      for (var j = i+1; j < things.Count; j++)
      {
        pairs.Add(new PairOfThings(things[i], things[j]));
      }
    }
    
    var nextGroupNum = 1;
    var distinctGroupNums = new HashSet<int>();
    foreach (var thing in things)
    {
      thing.GroupNum = nextGroupNum;
      distinctGroupNums.Add(nextGroupNum);
      nextGroupNum++;
    }
    
    pairs.Sort((p, q) => p.DistanceRank.CompareTo(q.DistanceRank));

    var debugCount = 0;
    var worklist = isPartTwo ? pairs : pairs.Take(1000);
    foreach (var mergingPair in worklist)
    {
      var firstMergingGroup = mergingPair.First.GroupNum;
      var secondMergingGroup = mergingPair.Second.GroupNum;
      if (debugCount % 100 == 0)
      {
        Console.WriteLine($"Merging group {firstMergingGroup} with group {secondMergingGroup}. New group is {nextGroupNum}. Distance rank was {mergingPair.DistanceRank}");
      }
      foreach (var member in things.Where(t => t.GroupNum == firstMergingGroup || t.GroupNum == secondMergingGroup).ToArray())
      {
        member.GroupNum = nextGroupNum;
      }
      
      distinctGroupNums.Remove(firstMergingGroup);
      distinctGroupNums.Remove(secondMergingGroup);
      distinctGroupNums.Add(nextGroupNum);
      if (distinctGroupNums.Count == 1)
      {
        Console.WriteLine($"Part Two: {mergingPair.First.X * mergingPair.Second.X}");
        break;
      }
      
      nextGroupNum++;
      debugCount++;
    }

    if (!isPartTwo)
    {
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
      
      var partOne = 1L;
      foreach (var sz in sizes.Take(3))
      {
        partOne *= sz;
      }
      
      Console.WriteLine($"Part One: {partOne}");
    }

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

  public static long SquareOfDiff(long a, long b)
  {
    var diff = b - a;
    return diff * diff;
  }
  
  // using suqare of distance to stay in integer space
  public static long DistanceRank(Thing first, Thing second)
  {
    var result = 0L;
    result += SquareOfDiff(first.X, second.X);
    result += SquareOfDiff(first.Y, second.Y);
    result += SquareOfDiff(first.Z, second.Z);
    return result;
  }
}

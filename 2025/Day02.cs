public static class DayTwoProgram
{
  public static void Main(string[] args)
  {
    var input = File.ReadAllText(args[1]).TrimEnd();

    Solve(input.Split(","), PartTwo);
  }

  private static bool PartTwo(long x)
  {
    var xStr = x.ToString();
    for (var i = 2; i <= xStr.Length; i++)
    {
      if ((xStr.Length) % i != 0)
        continue;
      if (IsFakeWhenSplitInto(xStr,i))
        return true;
    }
    return false;
  }

  private static bool PartOne(long x)
  {
    var xStr = x.ToString();
    if ((xStr.Length) % 2 != 0)
      return false;
    return IsFakeWhenSplitInto(xStr,2);
  }

  private static bool IsFakeWhenSplitInto(string xStr, int partCount)
  {
    var subLen = xStr.Length / partCount;
    var parts = new List<string>();
    for (var p = 0; p < partCount; p++)
    {
      parts.Add(xStr.Substring(p*subLen,subLen));
    }
    for (var p = 1; p < partCount; p++)
    {
      if (!parts[p-1].Equals(parts[p]))
        return false;
    }
    return true;
  }

  private static void Solve(string[] items, Predicate<long> isFake)
  {
    var answer = 0L;

    foreach (var item in items)
    {
      var split = item.Split("-");
      var low = long.Parse(split[0]);
      var high = long.Parse(split[1]);

      for (var i = low; i <= high; i++)
      {
        if (isFake(i))
          answer += i;
      }
    }
    
    Console.WriteLine(answer);
  }
}

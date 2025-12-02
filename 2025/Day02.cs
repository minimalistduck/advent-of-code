public static class DayTwoProgram
{
  public static void Main(string[] args)
  {
    var input = File.ReadAllText(args[1]).TrimEnd();

    Solve(input.Split(","), PartOne);
  }

  private static bool PartOne(long x)
  {
    var iStr = i.ToString();
    if ((iStr.Length) % 2 != 0)
      return false;
    var halfLen = iStr.Length / 2;
    var left = iStr.Substring(0,halfLen);
    var right = iStr.Substring(halfLen,halfLen);
    return left.Equals(right);
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

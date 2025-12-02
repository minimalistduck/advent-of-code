public static class DayTwoProgram
{
  public static void Main(string[] args)
  {
    var input = File.ReadAllText(args[1]).TrimEnd();

    SolvePartOne(input.Split(","));
  }

  private static void SolvePartOne(string[] items)
  {
    var partOne = 0L;

    foreach (var item in items)
    {
      var split = item.Split("-");
      var low = long.Parse(split[0]);
      var high = long.Parse(split[1]);

      for (var i = low; i <= high; i++)
      {
        var iStr = i.ToString();
        if ((iStr.Length) % 2 != 0)
          continue;
        var halfLen = iStr.Length / 2;
        var left = iStr.Substring(0,halfLen);
        var right = iStr.Substring(halfLen,halfLen);
        if (left.Equals(right))
          partOne += i;
      }
    }
    
    Console.WriteLine(partOne);
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

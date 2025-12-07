public static class DaySixProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartOne(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
    var operations = lines.Last().Split(" ", StringSplitOptions.RemoveEmptyEntries);

    var results = lines.First()
      .Split(" ", StringSplitOptions.RemoveEmptyEntries)
      .Select(long.Parse)
      .ToArray();
    var funcs = operations.Select(op => op switch
      {
        "+" => (x,y) => x+y,
        "*" => (x,y) => x*y,
        _ => (x,y) => x // unused
      }).ToArray();
    for (var ln = 1; ln < lines.Length - 2; ln++)
    {
      var middleLine = lines[ln].Select(long.Parse).ToArray();
      for (var i = 0; i < operations.Length; i++)
      {
        results[i] = funcs[i](results[i], middleLine[i]);
      }
    }
    
    Console.WriteLine(results.Sum());
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

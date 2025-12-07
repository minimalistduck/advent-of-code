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
    var funcs = operations.Select<string,Func<long,long,long>>(op => op switch
      {
        "+" => (long x,long y) => x+y,
        "*" => (long x,long y) => x*y,
        _ => (long x,long y) => x // unused
      }).ToArray();
    for (var ln = 1; ln < lines.Length - 2; ln++)
    {
      var operands = lines[ln].Split(" ", StringSplitOptions.RemoveEmptyEntries)
        .Select(long.Parse).ToArray();
      for (var i = 0; i < operations.Length; i++)
      {
        results[i] = funcs[i](results[i], operands[i]);
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

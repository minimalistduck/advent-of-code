using System.Numerics;

public static class DaySixProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    // 18510974039 is too low
    SolvePartOne(lines);
  }

  private static BigInteger ParseBigInt(string item) =>
    new BigInteger(long.Parse(item));

  private static void SolvePartOne(string[] lines)
  {
    var results = lines.First()
      .Split(" ", StringSplitOptions.RemoveEmptyEntries)
      .Select(ParseBigInt)
      .ToArray();
    Console.WriteLine($"Initial value in first column is {results[0]}");
    
    var operations = lines.Last().Split(" ", StringSplitOptions.RemoveEmptyEntries);
    var funcs = operations.Select<string,Func<BigInteger,BigInteger,BigInteger>>(op => op switch
      {
        "+" => (BigInteger x, BigInteger y) => x+y,
        "*" => (BigInteger x, BigInteger y) => x*y,
        _ => (BigInteger x, BigInteger y) => x // unused
      }).ToArray();
    
    for (var ln = 1; ln < lines.Length - 2; ln++)
    {
      var operands = lines[ln].Split(" ", StringSplitOptions.RemoveEmptyEntries)
        .Select(ParseBigInt).ToArray();
      for (var i = 0; i < operations.Length; i++)
      {
        results[i] = funcs[i](results[i], operands[i]);
        if (i == 0)
          Console.WriteLine($"{operations[i]} {operands[i]} => {results[i]}");
      }
    }

    var partTwo = new BigInteger(0);
    foreach (var result in results)
    {
      partTwo = partTwo + result;
    }
    
    Console.WriteLine(partTwo);
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

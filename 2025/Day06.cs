using System.Numerics;

public static class DaySixProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartOne(lines);
  }

  private static BigInteger ParseBigInt(string item) =>
    new BigInteger(long.Parse(item));

  private static void SolvePartOne(string[] lines)
  {
    var operations = lines.Last().Split(" ", StringSplitOptions.RemoveEmptyEntries);

    var results = lines.First()
      .Split(" ", StringSplitOptions.RemoveEmptyEntries)
      .Select(ParseBigInt)
      .ToArray();
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

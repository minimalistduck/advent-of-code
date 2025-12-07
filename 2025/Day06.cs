using System.Numerics;

public static class DaySixProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartTwo(lines);
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
    
    for (var ln = 1; ln < lines.Length - 1; ln++)
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

  private HashSet<int> SpaceIndexes(string line)
  {
    var result = new HashSet<int>();
    for (int c = 0; c < line.Length; c++)
    {
      if (line[c] == ' ')
        result.Add(c);
    }
    result.Add(-1);
    result.Add(line.Length);
    return result;
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0L;
    
    var spaceIndexSet = SpaceIndexes(lines[0]);
    for (var ln = 1; ln < lines.Length - 1; ln++)
    {
      spaceIndexSet.IntersectWith(SpaceIndexes(lines[ln]));
    }

    var spaceIndexes = spaceIndexSet.ToArray();
    Array.Sort(spaceIndexes);
    Console.WriteLine("Columns where there's a space in all rows:");
    Console.WriteLine(spaceIndexes.Join(","));

    var operations = lines.Last().Split(" ", StringSplitOptions.RemoveEmptyEntries);
    var funcs = operations.Select<string,Func<long,long,long>>(op => op switch
      {
        "+" => (long x, long y) => x+y,
        "*" => (long x, long y) => x*y,
        _ => (long x, long y) => x // unused
      }).ToArray();

    for (var prob = 0; prob < spaceIndexes.Length - 1; prob++)
    {
      var result = operations[prob] switch
      {
          "+" => 0L,
          "*" => 1L,
          _ => 0L // unused
      };
      for (var col = spaceIndexes[prob+1] - 1; col > spaceIndexes[prob]; col--)
      {
        var digits = new List<char>();
        for (var row = 0; row < lines.Length - 1; row++)
        {
          digits.Add(lines[row][col]);
        }
        var operand = long.Parse(new string(digits.ToArray()).Trim());
        result = funcs[prob](result, operand);
      }
      partTwo += result
    }

    Console.WriteLine(partTwo);
  }
}

public static class DayFiveProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    var rangeLines = new List<string>();
    var ingredients = new List<Ingredient>();
    var i = 0;
    for (; !string.IsNullOrEmpty(lines[i]); i++)
    {
      rangeLines.Add(lines[i]);
    }
    i++;
    for (; i < lines.Length; i++)
    {
      ingredients.Add(new Ingredient(long.Parse(lines[i])));
    }

    var ranges = new List<long[]>(rangeLines.Count);
    foreach (var rangeLine in rangeLines)
    {
      var rangeLineParts = rangeLine.Split("-");
      ranges.Add(rangeLineParts.Select(long.Parse).ToArray());
    }

    SolvePartOne(ingredients, ranges.ToArray());
  }

  private static void SolvePartOne(List<Ingredient> ingredients, long[][] ranges)
  {
    var rangeLines = new List<string>();

    foreach (var rangeEnds in ranges)
    {
      foreach (var ing in ingredients)
      {
        if (rangeEnds[0] <= ing.Id && ing.Id <= rangeEnds[1])
          ing.IsFresh = true;
      }
    }

    Console.WriteLine(ingredients.Count(ing => ing.IsFresh));
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

public class Ingredient(long _id)
{
  public long Id => _id;
  public bool IsFresh { get; set; }
}

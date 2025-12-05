public static class DayFiveProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartOne(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
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

    foreach (var rangeLine in rangeLines)
    {
      var rangeLineParts = rangeLine.Split("-");
      var rangeEnds = rangeLineParts.Select(long.Parse).ToArray();

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

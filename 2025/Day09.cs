public static class DayNineProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    //SolvePartOne(lines);
    InvestigatePartTwo(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
    var possibleCorners = lines.Select(Point.FromString).ToArray();

    var pairs = new List<PairOfPoints>();
    for (var i = 0; i < possibleCorners.Length - 1; i++)
    {
      for (var j = i+1; j < possibleCorners.Length; j++)
      {
        pairs.Add(new PairOfPoints(possibleCorners[i], possibleCorners[j]));
      }
    }

    pairs.Sort((a,b) => a.AreaOfRectangle.CompareTo(b.AreaOfRectangle));

    Console.WriteLine(pairs.Last().AreaOfRectangle);
  }

  private static void InvestigatePartTwo(string[] lines)
  {
    var corners = lines.Select(Point.FromString).ToArray();

    var minX = corners.Min(p => p.X);
    var maxX = corners.Max(p => p.X);
    var minY = corners.Min(p => p.Y);
    var maxY = corners.Max(p => p.Y);

    Console.WriteLine($"X: [{minX},{maxX}]  y: [{minY},{maxY}]");
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

public class Point
{
  public readonly long X;
  public readonly long Y;
  
  private Point(long x, long y)
  {
    X = x;
    Y = y;
  }
  
  public static Point FromString(string inputLine)
  {
    var split = inputLine.Split(",").Select(long.Parse).ToArray();
    return new Point(split[0], split[1]);
  }
}

public class PairOfPoints
{
  public readonly Point First;
  public readonly Point Second;
  public readonly long AreaOfRectangle; 

  public PairOfPoints(Point first, Point second)
  {
    First = first;
    Second = second;
    var width = Math.Abs(second.X-first.X) + 1L;
    var height = Math.Abs(second.Y-first.Y) + 1L;
    AreaOfRectangle = width * height;
  }
}

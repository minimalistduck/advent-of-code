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
    var redTiles = lines.Select(Point.FromString).ToArray();

    var greenEdgeTiles = new List<Point>();
    for (var i = 0; i < redTiles.Length - 1; i++)
    {
      var dx = Math.Sign(redTiles[i+1].X - redTiles[i].X);
      var dy = Math.Sign(redTiles[i+1].Y - redTiles[i].Y);

      var greenX = redTiles[i].X;
      var greenY = redTiles[i].Y;

      while (greenX + dx != redTiles[i+1].X || greenY + dy != redTiles[i+1].Y)
      {
        greenX += dx;
        greenY += dy;
        greenEdgeTiles.Add(Point.FromXY(greenX, greenY));
      }
    }

    var minX = greenEdgeTiles.Min(p => p.X);
    var maxX = greenEdgeTiles.Max(p => p.X);
    var minY = greenEdgeTiles.Min(p => p.Y);
    var maxY = greenEdgeTiles.Max(p => p.Y);

    Console.WriteLine($"X~[{minX},{maxX}] Y~[{minY},{maxY}]   {redTiles.Length} red corner tiles   {greenEdgeTiles.Count} green edge tiles");

    var distinctX = new SortedSet<long>();
    var distinctY = new SortedSet<long>();

    distinctX.UnionWith(redTiles.Select(rt => rt.X));
    distinctY.UnionWith(redTiles.Select(rt => rt.Y));

    var widthOfReducedGrid = distinctX.Count * 2 - 1;
    var heightOfReducedGrid = distinctY.Count * 2 - 1;
    Console.WriteLine($"Minimal representative grid would be {widthOfReducedGrid} x {heightOfReducedGrid}");

    Range[] DeriveAxis(int sizeOfAxis, long[] requiredPoints)
    {
      var axis = new Range[sizeOfAxis];
      for (var i = 0; i < requiredPoints.Length; i++)
      {
        axis[i*2] = new Range(requiredPoints[i], requiredPoints[i], i*2);
      }
      for (var j = 1; j < axis.Length; j += 2)
      {
        axis[j] = new Range(axis[j-1].Upper + 1, axis[j+1].Lower - 1, j);
      }
    }
    var xAxis = DeriveAxis(widthOfReducedGrid, distinctX.ToArray());
    var yAxis = DeriveAxis(heightOfReducedGrid, distinctY.ToArray());

    Console.WriteLine("Derived reduced axes");
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

  public static Point FromXY(long x, long y)
  {
    return new Point(x, y);
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

public class Range
{
  private readonly long _lowerInOriginalSpace;
  private readonly long _upperInOriginalSpace;
  private readonly int _indexInReducedSpace;

  public Range(long lower, long upper, int index)
  {
    if (lower > upper)
      throw new ArgumentException("Upper bound of range must be higher or equal to lower bound");
    
    _lowerInOriginalSpace = lower;
    _upperInOriginalSpace = upper;
    _indexInReducedSpace = index;
  }

  public long Lower => _lowerInOriginalSpace;

  public long Upper => _upperInOriginalSpace;

  public int Index => _indexInReducedSpace;

  public Colour Colour { get; set; }
}

public enum Colour
{
  White,
  Red,
  Green
}

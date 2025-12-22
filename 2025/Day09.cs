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

    var distinctX = new SortedSet<int>();
    distinctX.UnionWith(redTiles.Select(rt => rt.X));
    var widthOfReducedGrid = distinctX.Count * 2 - 1;

    var distinctY = new SortedSet<int>();
    distinctY.UnionWith(redTiles.Select(rt => rt.Y));
    var heightOfReducedGrid = distinctY.Count * 2 - 1;
    
    Console.WriteLine($"Minimal representative grid would be {widthOfReducedGrid} x {heightOfReducedGrid}");

    Range[] DeriveAxis(int sizeOfAxis, int[] requiredPoints)
    {
      var axis = new Range[sizeOfAxis];
      for (var i = 0; i < requiredPoints.Length; i++)
      {
        axis[i*2] = new Range(requiredPoints[i], requiredPoints[i], i*2);
      }
      // These interim ranges are allowed to be size 0
      for (var j = 1; j < axis.Length; j += 2)
      {
        axis[j] = new Range(axis[j-1].Upper + 1, axis[j+1].Lower - 1, j);
      }
      return axis;
    }
    var distinctXArr = distinctX.ToArray();
    var xAxis = DeriveAxis(widthOfReducedGrid, distinctXArr);

    var distinctYArr = distinctY.ToArray();
    var yAxis = DeriveAxis(heightOfReducedGrid, distinctYArr);

    var xLookup = new Dictionary<long, int>();
    for (var i = 0; i < distinctXArr.Length; i++)
    {
      xLookup[distinctXArr[i]] = i;
    }

    var yLookup = new Dictionary<long, int>();
    for (var i = 0; i < distinctYArr.Length; i++)
    {
      yLookup[distinctYArr[i]] = i;
    }

    var grid = new Colour[widthOfReducedGrid,heightOfReducedGrid];
    for (var i = 0; i < redTiles.Length; i++)
    {
      var redTile = redTiles[i];
      var x = xLookup[redTile.X];
      var y = yLookup[redTile.Y];
      grid[x,y] = Colour.Red;

      var iNext = (i+1) % redTiles.Length;
      var xNext = xLookup[redTiles[iNext].X];
      var yNext = yLookup[redTiles[iNext].Y];
      
      var dx = Math.Sign(xNext - x);
      var dy = Math.Sign(yNext - y);

      var greenX = x;
      var greenY = y;
      while (greenX + dx != xNext || greenY + dy != yNext)
      {
        greenX += dx;
        greenY += dy;
        grid[greenX,greenY] = Colour.Green;
      }
    }

    var midY = heightOfReducedGrid / 2;
    var possX = 0;
    var inside = false;
    while (!inside || grid[possX,midY] != Colour.White)
    {
      var onEdge = grid[possX,midY] != Colour.White;
      if (onEdge)
        inside = !inside;
      possX++;
    }
    Console.WriteLine($"There is an inside space at {possX},{midY} where we could start filling");
    
    var frontier = new Queue<Point>();
    frontier.Enqueue(Point.FromXY(possX,midY));
    var xOffsets = new [] { 0, 1, 0, -1 };
    var yOffsets = new [] { 1, 0, -1, 0 };
    var fillCount = 0;
    while (frontier.Count > 0)
    {
      Console.WriteLine($"Filling {frontier.Count} point(s) on the frontier");
      var worklist = frontier.ToArray();
      frontier.Clear();
      foreach (var here in worklist)
      {
        grid[here.X, here.Y] = Colour.Green;
        fillCount++;
        for (int i = 0; i < 4; i++)
        {
          var nextX = here.X + xOffsets[i];
          var nextY = here.Y + yOffsets[i];
          if (nextX >= 0 && nextX < widthOfReducedGrid && nextY >= 0 && nextY < heightOfReducedGrid &&
             grid[nextX,nextY] == Colour.White)
          {
            // TODO: we're probably enqueing duplicate
            frontier.Enqueue(Point.FromXY(nextX,nextY));
          }
        }
      }
    }
    Console.WriteLine($"Filled {fillCount} squares");

    //Console.WriteLine("Done red corners and green edges in reduced grid");
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

public class Point
{
  public readonly int X;
  public readonly int Y;
  
  private Point(int x, int y)
  {
    X = x;
    Y = y;
  }
  
  public static Point FromString(string inputLine)
  {
    var split = inputLine.Split(",").Select(int.Parse).ToArray();
    return new Point(split[0], split[1]);
  }

  public static Point FromXY(int x, int y)
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
    var width = Math.Abs(second.X-first.X) + 1;
    var height = Math.Abs(second.Y-first.Y) + 1;
    AreaOfRectangle = (long)width * (long)height;
  }
}

public class Range
{
  private readonly long _lowerInOriginalSpace;
  private readonly long _upperInOriginalSpace;
  private readonly int _indexInReducedSpace;

  public Range(long lower, long upper, int index)
  {
    _lowerInOriginalSpace = lower;
    _upperInOriginalSpace = upper;
    _indexInReducedSpace = index;
  }

  public long Lower => _lowerInOriginalSpace;

  public long Upper => _upperInOriginalSpace;

  public int Index => _indexInReducedSpace;
}

public enum Colour
{
  White,
  Red,
  Green
}

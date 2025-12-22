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

    var xLookup = new Dictionary<int, int>();
    for (var i = 0; i < distinctXArr.Length; i++)
    {
      xLookup[distinctXArr[i]] = i;
    }

    var yLookup = new Dictionary<int, int>();
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
    
    var frontier = new HashSet<System.Drawing.Point>();
    frontier.Add(new System.Drawing.Point(possX,midY));
    var xOffsets = new [] { 0, 1, 0, -1 };
    var yOffsets = new [] { 1, 0, -1, 0 };
    var fillCount = 0;
    while (frontier.Count > 0)
    {
      var worklist = frontier.ToArray();
      frontier.Clear();
      foreach (var here in worklist)
      {
        grid[here.X, here.Y] = Colour.Green;
        fillCount++;
      }
      foreach (var here in worklist)
      {
        for (int i = 0; i < 4; i++)
        {
          var nextX = here.X + xOffsets[i];
          var nextY = here.Y + yOffsets[i];
          if (nextX >= 0 && nextX < widthOfReducedGrid && nextY >= 0 && nextY < heightOfReducedGrid &&
             grid[nextX,nextY] == Colour.White)
          {
            frontier.Add(new System.Drawing.Point(nextX,nextY));
          }
        }
      }
    }
    Console.WriteLine($"Filled {fillCount} squares");

    var pairs = new List<PairOfPoints>();
    for (var i = 0; i < redTiles.Length - 1; i++)
    {
      for (var j = i+1; j < redTiles.Length; j++)
      {
        var xi = xLookup[redTiles[i].X];
        var yi = yLookup[redTiles[i].Y];
        var pi = Point.FromXY(xi,yi);
        var xj = xLookup[redTiles[j].X];
        var yj = yLookup[redTiles[j].Y];
        var pj = Point.FromXY(xj,yj);
        pairs.Add(new PairOfPoints(pi, pj));
      }
    }

    // Now. similar to part 1, we need to know the rectangles with red opposite
    // corners. We need to dismiss any that have a white square inside.
    // We need to find the one with biggest area *when rescaled onto the original axes* 

    foreach (var pair in pairs)
    {
      pair.RescaleArea(xAxis, yAxis);
    }

    pairs.Sort((a,b) => a.AreaOfRectangle.CompareTo(b.AreaOfRectangle));
    pairs.Reverse();

    var solved = false;
    var p = 0;
    while (!solved && p < pairs.Count)
    {
      var possPair = pairs[p];
      var startX = Math.Min(possPair.First.X, possPair.Second.X);
      var finishX = Math.Max(possPair.First.X, possPair.Second.X);
      var startY = Math.Min(possPair.First.Y, possPair.Second.Y);
      var finishY = Math.Max(possPair.First.Y, possPair.Second.Y);
      var stillPoss = true;
      for (var y = startY; y <= finishY && stillPoss; y++)
      {
        for (var x = startX; x <= finishX && stillPoss; x++)
        {
          stillPoss = grid[x,y] != Colour.White;
        }
      }
      if (stillPoss)
      {
        solved = true;
        Console.WriteLine($"Part two: {possPair.AreaOfRectangle}");
      }
    }

    // I am not sure how much efficiency matters in answering whether there is a white
    // tile or not. If it needs improving, I am thinking the white tiles could be sorted 
    // and we could use binary search, but they'd need to be indexed on both axes
    // and we'd need to know whether any of the tiles we'd found on one axis was also
    // found on the other axis.
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
  public long AreaOfRectangle { get; private set; } 

  public PairOfPoints(Point first, Point second)
  {
    First = first;
    Second = second;
    var width = Math.Abs(second.X-first.X) + 1;
    var height = Math.Abs(second.Y-first.Y) + 1;
    AreaOfRectangle = (long)width * (long)height;
  }

  public void RescaleArea(Range[] xRanges, Range[] yRanges)
  {
    var width = Math.Abs(xRanges[Second.X].Lower - xRanges[First.X].Lower) + 1;
    var height = Math.Abs(yRanges[Second.Y].Lower - yRanges[First.Y].Lower) + 1;
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

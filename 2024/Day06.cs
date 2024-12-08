public static class DaySixProgram
{
  public static void Main()
  {
     const string folder = @"D:\David\Coding\AdventOfCode2024\Day6";
     string inputPath = Path.Combine(folder, "input.txt");
     var lines = File.ReadAllLines(inputPath).Select(ln => ln.ToCharArray()).ToArray();
     
     var width = lines[0].Length;
     var height = lines.Length;
     
     Console.WriteLine("Grid is {0} x {1}", width, height);
     
     Guard guard = null;
     for (int y = 0; y < height; y++)
     {
       for (int x = 0; x < width; x++)
       {
         if (lines[y][x] == '^')
         {
           guard = new Guard(x,y);
         }
       }
     }
     
     while (guard.Step(lines, width, height))
     {
     }
     
     var partOne = lines.Sum(ln => ln.Count(c => c == 'X' || c == 'O'));
     Console.WriteLine(partOne);
     
     // I made a hasty guess that if you place an obstruction where there was already an X,
     // you create a loop. That guess was wrong.
     var partTwo = lines.Sum(ln => ln.Count(c => c == 'O'));
     Console.WriteLine(partTwo);
  }
}

// You can define other methods, fields, classes and namespaces here
public class Guard
{
  public Point Position;
  private int DirectionIndex;
  
  public Guard(int startX, int startY)
  {
    Position = new Point(startX, startY);
    DirectionIndex = 0;
  }
  
  // Take the next step (move or turn)
  // and return whether we're still in bounds
  public bool Step(char[][] grid, int width, int height)
  {
    var oldPosition = Position;
    var forwardPosition = Position;
    forwardPosition.Offset(Direction.All[DirectionIndex].Offset);
    if (IsInBounds(forwardPosition, width, height))
    {
      if (grid[forwardPosition.Y][forwardPosition.X] == '#')
      {
        // turn
        DirectionIndex = (DirectionIndex + 1) % Direction.All.Length;
      }
      else
      {
        // move
        Position = forwardPosition;
        if (grid[oldPosition.Y][oldPosition.X] == 'X')
        {
          grid[oldPosition.Y][oldPosition.X] = 'O';
        }
        else
        {
          grid[oldPosition.Y][oldPosition.X] = 'X';
        }
      }
      return true;
    }
    else
    {
      grid[oldPosition.Y][oldPosition.X] = 'X';
      return false;
    }
  }
  
  private bool IsInBounds(Point position, int width, int height)
  {
    return position.X >= 0 &&
      position.X < width &&
      position.Y >= 0 &&
      position.Y < height;
  }
}

public class Direction
{
  public readonly Point Offset;
  
  private Direction(Point offset)
  {
    Offset = offset;
  }
  
  public static Direction Up = new Direction(new Point(0,-1));
  public static Direction Left = new Direction(new Point(1,0));
  public static Direction Down = new Direction(new Point(0,1));
  public static Direction Right = new Direction(new Point(-1,0));
  
  public static Direction[] All = new Direction[] {
    Up, Left, Down, Right
  };
}

public static class DayFourProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();
    var width = lines[0].Length + 2;
    var height = lines.Length + 2;

    var grid = new char[width, height];

    for (int r = 0; r < lines.Length; r++)
    {
      for (int c = 0; c < lines[r].Length; c++)
      {
        grid[c+1,r+1] = lines[r][c];
      }
    }
    for (int r = 0; r < height; r++)
    {
      grid[0,r] = '.';
      grid[width-1,r] = '.';
    }
    for (int c = 0; c < width; c++)
    {
      grid[c,0] = '.';
      grid[c,height-1] = '.';
    }
    
    SolvePartOne(grid, width, height);
  }

  private static void SolvePartOne(char[,] lines, int width, int height)
  {
    var partOne = 0;

    var xOffset = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
    var yOffset = new int[] { -1,-1,-1,  0, 0,  1, 1, 1 };
    for (int x = 1; x < width - 1; x++)
    {
      for (int y = 1; y < height - 1; y++)
      {
        if (grid[x,y] == '@')
        {
          var adjacentCount = 0;
          for (int i = 0; i < xOffset.Length; i++)
          {
            // TODO: increment adjacentCount if bale found
          }
        }
      }
    }

    Console.WriteLine(partOne);
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

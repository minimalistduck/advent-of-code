public static class DaySevenProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    // 1391356900 is too low
    SolvePartTwo(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
    var partOne = 0;
    
    var width = lines[0].Length + 2;
    var height = lines.Length;
    
    var grid = new char[width,height];
    
    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        grid[x,y] = '.';
      }
    }
    
    for (var x = 1; x < width - 1; x++)
    {
      if (lines[0][x-1] == 'S')
        grid[x,0] = '|';
    }
    
    for (var y = 1; y < height; y++)
    {
      for (var x = 1; x < width - 1; x++)
      {
        if (lines[y][x-1] == '^')
        {
          grid[x,y] = '^';
        }
      }
    }
    
    for (var row = 1; row < height; row++)
    {
      var rowAbove = row - 1;
      for (var col = 1; col < width - 1; col++)
      {
        if (grid[col,row] == '^' && grid[col,rowAbove] == '|')
        {
          // assuming there isn't a splitter there too
          grid[col-1,row] = '|';
          grid[col+1,row] = '|';
          partOne++;
        }
        else if (grid[col,rowAbove] == '|')
        {
          grid[col,row] = '|';
        } 
      }
    }

    Console.WriteLine(partOne);
  }

  private static void SolvePartTwo(string[] lines)
  {
    var width = lines[0].Length + 2;
    var height = lines.Length;
    
    var grid = new int[width,height];
    
    for (var x = 0; x < width; x++)
    {
      for (var y = 0; y < height; y++)
      {
        grid[x,y] = 0;
      }
    }
    
    for (var x = 1; x < width - 1; x++)
    {
      if (lines[0][x-1] == 'S')
        grid[x,0] = 1;
    }
    
    for (var y = 1; y < height; y++)
    {
      for (var x = 1; x < width - 1; x++)
      {
        if (lines[y][x-1] == '^')
        {
          grid[x,y] = -1;
        }
      }
    }
    
    for (var row = 1; row < height; row++)
    {
      var rowAbove = row - 1;
      for (var col = 1; col < width - 1; col++)
      {
        if (grid[col,row] == -1 && grid[col,rowAbove] > 0)
        {
          // assuming there isn't a splitter there too
          grid[col-1,row] += grid[col, rowAbove];
          grid[col+1,row] += grid[col, rowAbove];
        }
        else if (grid[col,rowAbove] > 0)
        {
          grid[col,row] += grid[col, rowAbove];
        } 
      }
    }

    var partTwo = 0;
    var lastRow = height - 1;
    for (var col = 1; col < width - 1; col++)
    {
      partTwo += grid[col,lastRow];
    }

    Console.WriteLine(partTwo);
  }
}

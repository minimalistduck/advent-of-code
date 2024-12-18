using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day15Program
{
  public static void Main()
  {
     bool dumpGrid = true;
     const string inputFilePath = @"C:\Users\davidt\Coding\AdventOfCode\2024\Day15-input.txt";
     var lines = File.ReadAllLines(inputFilePath);
     var gridLines = lines.Where(ln => ln.StartsWith("#")).ToArray();
     var width = gridLines[0].Length;
     var height = gridLines.Length;
     
     var grid = new GridCell[width,height];
     
     RobotCell robotCell = null;
     for (int y = 0; y < height; y++)
     {
         for (int x = 0; x < width; x++)
         {
             switch (lines[y][x])
             {
                 case '.':
                    grid[x,y] = new EmptyCell(x,y,grid);
                    break;
                 case 'O':
                    grid[x,y] = new BoxCell(x,y,grid);
                    break;
                 case '@':
                    robotCell = new RobotCell(x,y,grid);
                    grid[x,y] = robotCell;
                    break;
                 case '#':
                    grid[x,y] = new WallCell(x,y);
                    break;
                 default:
                    throw new InvalidOperationException("Unrecognised character in grid");
             }
         }
     }
     
     var moves = string.Join("", lines.Where(ln => !ln.StartsWith("#") && !string.IsNullOrWhiteSpace(ln)));
     foreach (var m in moves)
     {
         robotCell.TryMove(Direction.Get(m));
     }
     
     var result = 0;
     for (int y = 0; y < height; y++)
     {
         for (int x = 0; x < width; x++)
         {
             result += grid[x,y].GpsCoordinate;
         }
     }
     
     // 703370 is too low
     Console.WriteLine("Part one: {0}", result);
     
     if (dumpGrid)
     {
         var reportBuilder = new StringBuilder();
         for (int y = 0; y < height; y++)
         {
             for (int x = 0; x < width; x++)
             {
                 reportBuilder.Append(grid[x,y].ToString());
             }
             reportBuilder.AppendLine();
         }
         Console.WriteLine(reportBuilder);
     }
  }
}

public abstract class GridCell
{
    protected Point _position;
    
    protected GridCell(int x, int y)
    {
        _position = new Point(x,y);
    }
    
    public virtual bool CanEnterGoing(Direction d) =>
        throw new NotSupportedException();
    
    public virtual void BeforeEnterGoing(Direction d) =>
        throw new NotSupportedException();
        
    public virtual int GpsCoordinate => 0;
    
    protected Point Adjacent(Direction d)
    {
        var result = _position;
        result.Offset(d.Offset);
        return result;
    }
}

public class WallCell : GridCell
{
    public WallCell(int x, int y)
        :base(x,y)
    {}
    
    public override bool CanEnterGoing(Direction d) => false;
   
    public override string ToString() => "#";
}

public class BoxCell : GridCell
{
    private readonly GridCell[,] _grid;
    
    public BoxCell(int x, int y, GridCell[,] grid)
        :base(x,y)
    {
        _grid = grid;
    }
    
    public override bool CanEnterGoing(Direction d)
    {
        var adjacentPos = Adjacent(d);
        return _grid[adjacentPos.X, adjacentPos.Y].CanEnterGoing(d);
    }
    
    public override void BeforeEnterGoing(Direction d)
    {
        var oldPosition = _position;
        var adjacentPos = Adjacent(d);
        _grid[adjacentPos.X, adjacentPos.Y].BeforeEnterGoing(d);
        _grid[adjacentPos.X, adjacentPos.Y] = this;
        _position = adjacentPos;
        _grid[oldPosition.X, oldPosition.Y] = null;
    }
    
    public override int GpsCoordinate => 100 * _position.Y + _position.X;

    public override string ToString() => "O";
}

public class EmptyCell : GridCell
{
    private readonly GridCell[,] _grid;
    
    public EmptyCell(int x, int y, GridCell[,] grid)
        :base(x,y)
    {
        _grid = grid;
    }
    
    public override bool CanEnterGoing(Direction d) => true;
    
    public override void BeforeEnterGoing(Direction d)
    {
        // just remove myself from the grid
        _grid[_position.X,_position.Y] = null;
    }
    
    public override string ToString() => ".";
}

public class RobotCell : GridCell
{
    private readonly GridCell[,] _grid;
    
    public RobotCell(int x, int y, GridCell[,] grid)
        :base(x,y)
    {
        _grid = grid;
    }
    
    public void TryMove(Direction d)
    {
        var oldPosition = _position;
        var adjacentPos = Adjacent(d);
        if (_grid[adjacentPos.X, adjacentPos.Y].CanEnterGoing(d))
        {
            _grid[adjacentPos.X, adjacentPos.Y].BeforeEnterGoing(d);
            _grid[adjacentPos.X, adjacentPos.Y] = this;
            _position = adjacentPos;
            _grid[oldPosition.X, oldPosition.Y] = new EmptyCell(oldPosition.X, oldPosition.Y, _grid);
        }
    }

    public override string ToString() => "@";
}

public class Direction
{
    public readonly Point Offset;
    public readonly char Symbol;
    
    private Direction(int x, int y, char symbol)
    {
        Offset = new Point(x, y);
        Symbol = symbol;
    }
    
    public static readonly Direction North = new Direction(0,-1,'^');
    public static readonly Direction East = new Direction(1,0,'>');
    public static readonly Direction South = new Direction(0,1,'v');
    public static readonly Direction West = new Direction(-1,0,'<');
    
    public static Direction TurnClockwise(Direction original)
    {
        switch (original.Symbol)
        {
            case '^':
                return Direction.East;
            case '>':
                return Direction.South;
            case 'v':
                return Direction.West;
            case '<':
                return Direction.North;
            default:
                throw new InvalidOperationException();
        }
    }
    
    public static Direction TurnAnticlockwise(Direction original)
    {
        switch (original.Symbol)
        {
            case '^':
                return Direction.West;
            case '>':
                return Direction.North;
            case 'v':
                return Direction.East;
            case '<':
                return Direction.South;
            default:
                throw new InvalidOperationException();
        }
    }
    
    public static Direction[] All = new Direction[] {
        North, East, South, West
    };
    
    public static Direction Get(char d)
    {
        switch (d)
        {
            case '^':
                return Direction.North;
            case '>':
                return Direction.East;
            case 'v':
                return Direction.South;
            case '<':
                return Direction.West;
            default:
                throw new ArgumentException();
        }
    }
    
    public override string ToString() => Symbol.ToString();
}
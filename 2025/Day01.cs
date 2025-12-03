public static class DayOneProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartTwo(lines);
  }

  private static void SolvePartTwo(string[] lines)
  {
    var dial = new Dial();
  
    foreach (var line in lines)
    {
      var dirn = line[0];
      var distance = int.Parse(line.Substring(1,line.Length-1));

      if (dirn == 'L')
        dial.MoveLeft(distance);
      else
        dial.MoveRight(distance);
    }
    
    Console.WriteLine(dial.TimesPastZero);
  }

  private static void SolvePartOne(string[] lines)
  {
    var partOne = 0;
    var pointingAt = 50;

    foreach (var line in lines)
    {
      var dirn = line[0] == 'R' ? 1 : -1;
      var distance = int.Parse(line.Substring(1,line.Length-1));
      var offset = dirn * distance;
      while (pointingAt + offset < 0) // is this loop needed?
        offset += 100;
      pointingAt = (pointingAt + offset) % 100;
      if (pointingAt == 0)
        partOne++;
    }
    Console.WriteLine(partOne);
  }
}

public class LinkedPoint(string label)
{
  public int VisitCount;
  public LinkedPoint Left;
  public LinkedPoint Right;
  public string Label => label;
}

public class Dial
{
  private readonly LinkedPoint[] _points;
  private LinkedPoint _current;
  
  public Dial()
  {
    _points = Build();
    _current = _points[0];
  }
  
  public void MoveLeft(int distance)
  {
    for (var i = 0; i < distance; i++)
    {
      _current = _current.Left;
      _current.VisitCount++;
    }
  }

  public void MoveRight(int distance)
  {
    for (var i = 0; i < distance; i++)
    {
      _current = _current.Right;
      _current.VisitCount++;
    }
  }
  
  public int TimesPastZero =>
    _points.Single(p => p.Label.Equals("0")).VisitCount;

    private static LinkedPoint[] Build()
    {
      var seq = Enumerable.Range(50,50).Concat(
        Enumerable.Range(0,50)).ToArray();
        
      var resultBuilder = new List<LinkedPoint>();
      
      resultBuilder.Add(new LinkedPoint(seq[0].ToString()));
      
      foreach(var n in seq.Skip(1))
      {
        var prev = resultBuilder.Last();
        var curr = new LinkedPoint(n.ToString());
        prev.Right = curr;
        curr.Left = prev;
        resultBuilder.Add(curr);
      }
      
      var end = resultBuilder.Last();
      var start = resultBuilder.First();
      end.Right = start;
      start.Left = end;
      
      return resultBuilder.ToArray();
    }

}

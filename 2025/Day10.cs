public static class DayTenProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    // 678 is too high
    SolvePartOne(lines);
  }

  private static void SolvePartOne(IEnumerable<string> lines)
  {
    var partOne = 0L;

    foreach (var line in lines)
    {
      //Console.WriteLine("Input: " + line);
      var splitLine = line.Split(" ");
      var targetStr = splitLine[0].Replace("[","").Replace("]","");
      var target = 0u;
      for (var i = 0; i < targetStr.Length; i++)
      {
        // The bits of target will be the reverse of targetStr, but that's what we want
        // - in the rightmost bit (shift 0) we want the leftmost light 
        if (targetStr[i] == '#')
        {
          var newBit = 1u << i;
          //Console.WriteLine($"i={i}, targetStr[i]={targetStr[i]}, newBit={newBit:B}");
          target = target | newBit;
        }
      }
      //Console.WriteLine("Target: " + target.ToString("B"));
      
      var tracker = new ResultTracker(target);
      var keepGoing = true;
      tracker.Solved += (sender, args) => {
        keepGoing = false;
        partOne += args.Iteration;
      };

      var buttons = new List<uint>();
      for (var bs = 1; bs < splitLine.Length - 1; bs++)
      {
        var buttonPositions = splitLine[bs].Replace("(", "").Replace(")", "")
          .Split(",").Select(int.Parse);
        var button = 0u;
        foreach (var bp in buttonPositions)
        {
          var newBit = 1u << bp;
          button = button | newBit;
        }
        //Console.WriteLine("Button: " + button.ToString("B"));
        buttons.Add(button);
      }
      
      do
      {
        var worklist = tracker.NextIteration().ToArray();
        if (worklist.Length == 0)
        {
          keepGoing = false;
        }
        for (var w = 0; w < worklist.Length && keepGoing; w++)
        {
          foreach (var b in buttons)
          {
            var adjacent = worklist[w] ^ b;
            tracker.TrackResult(adjacent);
          }
        }
      } while (keepGoing);
    }

    Console.WriteLine(partOne);
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

class ResultTracker
{
  private uint _target;
  private HashSet<uint> _resultsSeen = new HashSet<uint>();
  private List<HashSet<uint>> _resultsByIteration = new List<HashSet<uint>>();

  public ResultTracker(uint target)
  {
    _target = target;
    _resultsSeen.Add(0u);
    var resultsAtIterationZero = new HashSet<uint>();
    resultsAtIterationZero.Add(0u);
    _resultsByIteration.Add(resultsAtIterationZero);
  }

  // Advances to the next iteration, and returns the worklist
  // to start from when generating the results for the next iteration
  public IEnumerable<uint> NextIteration()
  {
    //Console.WriteLine("Iteration: " + _resultsByIteration.Count);
    var result = _resultsByIteration.Last();
    _resultsByIteration.Add(new HashSet<uint>());
    return result;
  }

  public event EventHandler<SolvedEventArgs> Solved;

  public void TrackResult(uint result)
  {
    // only retain new results
    if (_resultsSeen.Add(result))
    {
      _resultsByIteration.Last().Add(result);
      //Console.WriteLine("Achieved: " + result.ToString("B"));
    }
    if (result == _target)
    {
      Solved(this, new SolvedEventArgs(_resultsByIteration.Count-1));
    }
  }
}

public class SolvedEventArgs : EventArgs
{
  public int Iteration { get; private set; }

  public SolvedEventArgs(int iteration)
  {
    Iteration = iteration;
  }
}

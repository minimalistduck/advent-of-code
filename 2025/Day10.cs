public static class DayTenProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartOne(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
    var partOne = 0L;

    foreach (var line in lines)
    {
      // TODO: parse the target into a uint
      // Is it easier to reverse the target, so that button 0 is the first bit i.e. 2^0 ?

      var tracker = new ResultTracker(target);
      var keepGoing = true;
      tracker.Solved += (sender, args) => {
        keepGoing = false;
        partOne += args.Iteration;
      };

      // TODO: Parse the buttons into uints
      
      do
      {
        var worklist = tracker.NextIteration().ToArray();
        for (var w = 0; w < worklist.Length && keepGoing; w++)
        {
          foreach (var b in buttons)
          {
            var adjacent = worklist[w] ^ b;
            tracker.TrackResult(adjacent);
          }
        }
      } while (keepGoing)
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
    }
    if (result == target)
    {
      Solved(new SolvedEventArgs(_resultsByIteration.Count));
    }
  }
}

public class SolvedEventArgs : EventArgs
{
  private int Iteration { get; private set; }

  public SolvedEventArgs(int iteration)
  {
    Iteration = iteration;
  }
}

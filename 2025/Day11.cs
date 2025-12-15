public static class DayElevenProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartOne(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
    //var partOne = 0;

    var edgeDict = new Dictionary<string, string[]>();
    foreach (var line in lines)
    {
      var firstSplit = line.Split(":");
      var thisNode = firstSplit[0];
      var secondSplit = firstSplit[1].Split(" ").Select(s => s.Trim()).ToArray();
      edgeDict.Add(thisNode, secondSplit);      
    }

    // debug
    foreach (var adj in edgeDict["you"])
    {
      Console.WriteLine($"you -> {adj}");
    }
    //Console.WriteLine(partOne);
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

public class ResultTracker<T>
{
  private T _target;
  private HashSet<T> _resultsSeen = new HashSet<uint>();
  private List<HashSet<T>> _resultsByIteration = new List<HashSet<T>>();

  public ResultTracker(T initial, T target)
  {
    _target = target;
    _resultsSeen.Add(initial);
    var resultsAtIterationZero = new HashSet<T>();
    resultsAtIterationZero.Add(initial);
    _resultsByIteration.Add(resultsAtIterationZero);
  }

  // Advances to the next iteration, and returns the worklist
  // to start from when generating the results for the next iteration
  public IEnumerable<T> NextIteration()
  {
    //Console.WriteLine("Iteration: " + _resultsByIteration.Count);
    var result = _resultsByIteration.Last();
    _resultsByIteration.Add(new HashSet<T>());
    return result;
  }

  public event EventHandler<SolvedEventArgs> Solved;

  public void TrackResult(T result)
  {
    // only retain new results
    if (_resultsSeen.Add(result))
    {
      _resultsByIteration.Last().Add(result);
      //Console.WriteLine("Achieved: " + result.ToString());
    }
    if (result.Equals(_target))
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

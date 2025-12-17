public static class DayElevenProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    var edgeDict = new Dictionary<string, string[]>();
    foreach (var line in lines)
    {
      var firstSplit = line.Split(":");
      var thisNode = firstSplit[0];
      var adjNodes = firstSplit[1].Trim().Split(" ").Select(s => s.Trim()).ToArray();
      edgeDict.Add(thisNode, adjNodes);      
    }
    edgeDict.Add("out", new string[0]);

    //SolvePartOne(edgeDict, "you", "out");
    //SolvePartOne(edgeDict, "svr", "dac"); // diagnostic
    SolvePartTwo(edgeDict, "svr");
  }

  private static void SolvePartOne(Dictionary<string, string[]> edgeDict, string start, string end)
  {
    var partOne = 0L;
    
    var initialQueue = new Queue<string>();
    initialQueue.Enqueue(start);
    var breadcrumb = new Stack<Queue<string>>();
    breadcrumb.Push(initialQueue);

    var done = false;
    while (!done)
    {
      var workQueue = breadcrumb.Peek();
      while (workQueue.Count == 0 && breadcrumb.Count > 0)
      {
        breadcrumb.Pop();
        workQueue = breadcrumb.Peek();
      }
      if (workQueue.Count == 0)
        continue;

      var item = workQueue.Dequeue();
      if (item.Equals(end))
      {
        partOne++;
      }
      else
      {
        var nextItems = edgeDict[item];
        if (nextItems.Length > 0)
        {
          var deeperQueue = new Queue<string>(nextItems);
          breadcrumb.Push(deeperQueue);
        }
      }
      done = breadcrumb.Sum(q => q.Count) == 0;
    }

    Console.WriteLine($"Paths from {start} to {end}: {partOne}");
  }

  private static void SolvePartTwo(Dictionary<string, string[]> edgeDict, string start)
  {
    var partTwo = 0;

    var initialQueue = new Queue<string>();
    initialQueue.Enqueue(start);
    var unsearched = new Stack<Queue<string>>();
    unsearched.Push(initialQueue);
    var path = new Stack<string>();
    path.Push(start);

    var done = false;
    while (!done)
    {
      var workQueue = unsearched.Peek();
      while (workQueue.Count == 0 && unsearched.Count > 0)
      {
        unsearched.Pop();
        path.Pop();
        workQueue = unsearched.Peek();
      }
      if (workQueue.Count == 0)
        continue;

      var item = workQueue.Dequeue();
      var pathSet = new HashSet<string>(path);
      if (pathSet.Contains(item))
      {
        throw new InvalidOperationException("Cycle detected: " +
          string.Join(path.Reverse(), "->") + "->" + item);
      }
      if (item.Equals("out"))
      {
        if (pathSet.Contains("dac") && pathSet.Contains("fft"))
        {
          partTwo++;
        }
      }
      else
      {
        var nextItems = edgeDict[item];
        if (nextItems.Length > 0)
        {
          var deeperQueue = new Queue<string>(nextItems);
          unsearched.Push(deeperQueue);
          path.Push(item);
        }
      }
      done = unsearched.Sum(q => q.Count) == 0;
    }

    Console.WriteLine(partTwo);
  }
}

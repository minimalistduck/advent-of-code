public static class DayElevenProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartOne(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
    var partOne = 0L;
    
    var edgeDict = new Dictionary<string, string[]>();
    foreach (var line in lines)
    {
      var firstSplit = line.Split(":");
      var thisNode = firstSplit[0];
      var adjNodes = firstSplit[1].Trim().Split(" ").Select(s => s.Trim()).ToArray();
      edgeDict.Add(thisNode, adjNodes);      
    }

    var initialQueue = new Queue<string>();
    initialQueue.Enqueue("you");
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
      if (item.Equals("out"))
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

    Console.WriteLine(partOne);
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

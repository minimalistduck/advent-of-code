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

    // investigating the scale of Part Two
    // Paths from dac to out: 3050
    // From fft to out: gave up after 15 min
    // from svr to dac: gave up after 13 min
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
    var nodes = new Dictionary<string, Node>();

    foreach (var nodeName in edgeDict.Keys)
    {
      nodes[nodeName] = new Node(nodeName);
    }

    foreach (var entry in edgeDict)
    {
      var fromNode = nodes[entry.Key];
      foreach (var toName in entry.Value)
      {
        nodes[toName].AddIncomingNode(fromNode);
      }
    }

    nodes[start].PathsInto = 1L;
    foreach (var node in nodes.Values.Where(n => !n.PathsInto.HasValue && !n.HasIncomingNodes))
    {
      node.PathsInto = 0L;
    }

    var targetNode = nodes["out"];
    var pathCount = targetNode.CalculatePathsInto();
    Console.WriteLine($"Total paths (ignoring constraints) from {start} to out: {pathCount}");
  }
}

public class Node
{
  private readonly List<Node> _incomingNodes = new List<Node>();
  private readonly string _name;
  public long? PathsInto = null;
  
  public Node(string name)
  {
    _name = name;
  }

  public void AddIncomingNode(Node other)
  {
    _incomingNodes.Add(other);
  }

  public bool HasIncomingNodes => _incomingNodes.Count > 0;

  public long CalculatePathsInto()
  {
    if (PathsInto.HasValue)
      return PathsInto.Value;

    var result = 0L;
    foreach (var earlierNode in _incomingNodes)
    {
      result += earlierNode.CalculatePathsInto();
    }
    PathsInto = result;
    return result;
  }
}

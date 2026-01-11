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

// Beginning of work for part 2
public class ProblemStep
{
   public readonly int[] Target;
   // in this representation, each button has Size elements
   // which are either 1 or 0
   public readonly int[][] Buttons;
   public int Size => Target.Length;
   
   private ProblemStep(int[] target, int[][] buttons)
   {
      Target = target;
      Buttons = buttons;
   }
   
   // We accept buttons in the format of the input
   public ProblemStep Initial(int[] target, int[][] buttonIndices)
   {
      var buttons = new int[][buttonIndices.Length];
      for (var i = 0; i < buttonIndices.Length; i++)
      {
         buttons[i] = new int[target.Length];
         foreach (var buttonIndex in buttonIndices[i])
         {
            buttons[buttonIndex] = 1;
         }
      }
      return new ProblemStep(target, buttons);
   }
   
   public ProblemStep AfterPressing(int buttonIndex, int pressCount)
   {
      var newTarget = new int[Size];
      for (var i = 0; i < Size; i++)
      {
         newTarget[i] = Target[i] - Buttons[buttonIndex][i] * pressCount;
      }
      return new ProblemStep(newTarget, Buttons);
   }
   
   public ProblemStep Reduced()
   {
      List<int> keepIndexes = new List<int>();
      List<int> dropIndexes = new List<int>();
      for (var oldIndex = 0; oldIndex < Size; oldIndex++)
      {
         if (Target[oldIndex] > 0)
         {
            keepIndexes.Add(oldIndex);
         }
         else
         {
            dropIndexes.Add(oldIndex);
         }
      }
      
      // If we're keeping everything, the reduced problem is identical to this one
      if (keepIndexes.Count == Target.Length)
      {
         return this;
      }
      
      var newTarget = new int[keepIndexes.Count];
      for (var newIndex = 0; newIndex < keepIndexes.Count; newIndex++)
      {
         newTarget[newIndex] = Target[keepIndexes[newIndex]]
      }
      
      var newButtons = new List<int[]>();
      foreach (var oldButton in Buttons)
      {
         var keep = true;
         foreach (var dropIndex in dropIndexes)
         {
            keep = keep && oldButton[dropIndex] == 0;
         }
         
         if (keep)
         {
            var newButton = new int[keepIndexes.Count];
            for (var newIndex = 0; newIndex < keepIndexes.Count; newIndex++)
            {
               newButton[newIndex] = oldButton[keepIndexes[newIndex]];
            }
            newButtons.Add(newButton);
         }
      }

      return new ProblemStep(newTarget, newButtons);
   }
}
/*
Given target: int[] and buttons: int[][]

For indexes i where target[i] == 0
discard buttons b where buttons[b].Contains(i)

This yields a sub-problem where all elements of target are > 0.

Find i where target[i] = Min(target)
Consider buttons b' that contain i
Which button bi leaves the lowest max after it has been pressed target[i] times?
- is this any different from picking the button with most non-zeroes?
- do we need to go to next-max to break ties?
- use total remaining instead of max?

Increment number of presses by target[i]
Solve the sub-problem which remains after bi has been pressed target[i] times

Is it possible to miss any solutions with this algorithm?
Might it lead to a non-minimal solution? an infeasible solution?

presses = 0
{10,11,11,5,10,5}
- no zeros to remove -> no buttons removed
i = 3; target[3] = 5
Buttons containing 3: b0=(0,1,2,3,4), b1=(0,3,4)
After b0*5: {5,6,6,0,5,5}
After b1*5: {5,11,11,0,5,5}
b0 has the lowest max
presses = 5

{5,6,6,0,5,5}
reduce to {5,6,6,5,5} (0,1,2,4->3,5->4) (1,2)
i = 0; target[0] = 5
Buttons containing 0: b0=(0,1,2,3,4)
After b0*5: {0,1,1,0,0}
b0 has the lowest max (it's the only choice)
presses = 10

{0,1,1,0,0}
reduce to {1,1} (1->0,2->1)
i = 0; target[0] = 1
Buttons containing 0: b0=(0,1)
After b0*1: {0,0}
b0 is the only choice
presses = 11

{0,0}
reduce to {}
done
*/

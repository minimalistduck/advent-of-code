using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day24Program
{
  public static void Main()
  {
    const string inputFilePath = @"D:\David\Coding\AdventOfCode\2024\Day24-input.txt";
    var lines = File.ReadAllLines(inputFilePath);
    
    Dictionary<string, bool> initialValues = new Dictionary<string, bool>();
    List<Gate> gates = new List<Gate>();
    bool after = false;
    foreach (var line in lines)
    {
      if (string.IsNullOrWhiteSpace(line))
      {
        after = true;
        continue;
      }
      
      if (!after)
      {
        var splitLine = line.Split(": ");
        initialValues[splitLine[0]] = splitLine[1].Equals("1");
      }
      else
      {
        var gateSpec = line.Split(" ");
        gates.Add(new Gate(gateSpec[0], gateSpec[2], Operation.Get(gateSpec[1]), gateSpec[4]));
      }
    }
    Console.WriteLine("Loaded {0} logic gates and {1} initial states", gates.Count, initialValues.Count);
    
    var valuesAfter = PopulateOutputs(initialValues, gates);

    string GatesBinaryValue(Dictionary<string, bool> wireValues, string prefix)
    {
      var relevantGates = wireValues
        .Where(kvp => kvp.Key.StartsWith(prefix))
        .OrderByDescending(kvp => kvp.Key)
        .Select(kvp => kvp.Value ? '1' : '0')
        .ToArray();
      return new String(relevantGates);
    }
    
    var partOneResultBinary = GatesBinaryValue(valuesAfter, "z");
    Console.WriteLine("Part one: {0}", partOneResultBinary);

    // Shorthand
    ulong partOneResult = (ulong)Convert.ToInt64(partOneResultBinary, 2);
    
    // Longhand
    partOneResult = 0UL;
    ulong one = 1UL;
    for (int i = 0; i < partOneResultBinary.Length; i++)
    {
      if (partOneResultBinary[i] == '1')
      {
        partOneResult |= one << i;
      }
    }
    // 3919321143819 was too low (I needed to reverse significance of bits)
    Console.WriteLine("... which is {0}", partOneResult);
    
    var xBinary = GatesBinaryValue(initialValues, "x");
    var yBinary = GatesBinaryValue(initialValues, "y");
    
    var xNumber = Convert.ToInt64(xBinary, 2);
    var yNumber = Convert.ToInt64(yBinary, 2);
    
    var expectedZNumber = xNumber + yNumber;
    var expectedZBinary = expectedZNumber.ToString("B");
    Console.WriteLine("Should be: {0}", expectedZBinary);
    
    if (partOneResultBinary.Length != expectedZBinary.Length)
    {
      throw new InvalidOperationException("Binary numbers were not the same length");
    }
    
    List<string> badZs = new List<string>();
    for (int d = 0; d < partOneResultBinary.Length; d++)
    {
      if (partOneResultBinary[d] != expectedZBinary[d])
      {
        badZs.Add("z" + d.ToString("00"));
      }
    }
    
    Console.WriteLine("Outputs with the wrong value in this example: {0}", string.Join(",", badZs));
    
    Dictionary<string, Gate> backtrack = gates.ToDictionary(g => g.Output);
    
    HashSet<string> Feeders(string output)
    {
      HashSet<string> backWorklist = new HashSet<string>();
      backWorklist.Add(output);
      HashSet<string> doneList = new HashSet<string>();
      while (backWorklist.Count > 0)
      {
        string outGate = backWorklist.First();
        backWorklist.Remove(outGate);
        doneList.Add(outGate);
        if (backtrack.TryGetValue(outGate, out var gate))
        {
          foreach (var input in gate.Inputs)
          {
            if (!doneList.Contains(input))
            {
              backWorklist.Add(input);
            }
          }
        }
      }
      return doneList;
    }
    
    List<HashSet<string>> feeders = new List<HashSet<string>>();
    foreach (var z in badZs)
    {
      feeders.Add(Feeders(z));
    }
    
    HashSet<string> commonFeeders = new HashSet<string>(feeders[0]);
    for (int f = 1; f < feeders.Count; f++)
    {
      commonFeeders.IntersectWith(feeders[f]);
    }
    // According to the problem definition, it is outputs that have been swapped, not inputs
    commonFeeders.RemoveWhere(f => f.StartsWith("x") || f.StartsWith("y"));
    
    Console.WriteLine("After backtracking, the misplaced outputs that feed into all the incorrect outputs are: {0}",
      string.Join(",", commonFeeders));
    
    // The swapped outputs must have different values, otherwise swapping them would make no difference.
    List<string>[] commonFeedersByValue = new List<string>[2];
    commonFeedersByValue[0] = new List<string>();
    commonFeedersByValue[1] = new List<string>();
    foreach (var commonFeeder in commonFeeders)
    {
      var k = valuesAfter[commonFeeder] ? 0 : 1;
      commonFeedersByValue[k].Add(commonFeeder);
    }
    
    Console.WriteLine("Having value 0: {0}", string.Join(",", commonFeedersByValue[0]));
    Console.WriteLine("Having value 1: {0}", string.Join(",", commonFeedersByValue[1]));
    
    // We've only got two entries in commonFeedersByValue[1], which gives us relatively few
    // combinations to try.
    // Try swapping the first one with each of those in 0, and the second with each of the remaining ones
    for (int z1 = 0; z1 < commonFeedersByValue[0].Count - 1; z1++)
    {
      for (int z2 = z1 + 1; z2 < commonFeedersByValue[0].Count; z2++)
      {
         for (int oa = 0; oa < commonFeedersByValue[1].Count - 1; oa++)
         {
           for (int ob = oa + 1; ob < commonFeedersByValue[1].Count; ob++)
           {
             var firstPair = new string[] { commonFeedersByValue[0][z1], commonFeedersByValue[1][oa] };
             var secondPair = new string[] { commonFeedersByValue[0][z2], commonFeedersByValue[1][ob] };
             Console.WriteLine("Trying with swaps {0}<->{1} and {2}<->{3}",
               firstPair[0], firstPair[1], secondPair[0], secondPair[1]);

             var gatesAfterSwap = new Dictionary<string, Gate>(backtrack);
             var firstPairGates = firstPair.Select(ou => gatesAfterSwap[ou]).ToArray();
             var secondPairGates = secondPair.Select(ou => gatesAfterSwap[ou]).ToArray();
             var firstSwapped = new Gate[] {
               firstPairGates[0].CopyWithOutput(firstPair[1]),
               firstPairGates[1].CopyWithOutput(firstPair[0])
             };
             var secondSwapped = new Gate[] {
               secondPairGates[0].CopyWithOutput(secondPair[1]),
               secondPairGates[1].CopyWithOutput(secondPair[0])
             };
             gatesAfterSwap[firstPair[0]] = firstSwapped[1];
             gatesAfterSwap[firstPair[1]] = firstSwapped[0];
             gatesAfterSwap[secondPair[0]] = secondSwapped[1];
             gatesAfterSwap[secondPair[1]] = secondSwapped[0];
             var modifiedGates = new List<Gate>(gates);
             var i = modifiedGates.IndexOf(firstPairGates[0]);
             modifiedGates[i] = firstSwapped[0];
             i = modifiedGates.IndexOf(firstPairGates[1]);
             modifiedGates[i] = firstSwapped[1];
             i = modifiedGates.IndexOf(secondPairGates[0]);
             modifiedGates[i] = secondSwapped[0];
             i = modifiedGates.IndexOf(secondPairGates[1]);
             modifiedGates[i] = secondSwapped[1];
             valuesAfter = PopulateOutputs(initialValues, modifiedGates);
             var possiblyFixedResult = GatesBinaryValue(valuesAfter, "z");
             if (possiblyFixedResult.Equals(expectedZBinary))
             {
               Console.WriteLine("Achieved expected result by swapping {0}<->{1} and {2}<->{3}",
                 firstPair[0], firstPair[1], secondPair[0], secondPair[1]);
               return;
             }
             
             // same, but with oa/ob reversed
             firstPair = new string[] { commonFeedersByValue[0][z1], commonFeedersByValue[1][ob] };
             secondPair = new string[] { commonFeedersByValue[0][z2], commonFeedersByValue[1][oa] };
             Console.WriteLine("Trying with swaps {0}<->{1} and {2}<->{3}",
               firstPair[0], firstPair[1], secondPair[0], secondPair[1]);
             gatesAfterSwap = new Dictionary<string, Gate>(backtrack);
             firstPairGates = firstPair.Select(ou => gatesAfterSwap[ou]).ToArray();
             secondPairGates = secondPair.Select(ou => gatesAfterSwap[ou]).ToArray();
             firstSwapped = new Gate[] {
               firstPairGates[0].CopyWithOutput(firstPair[1]),
               firstPairGates[1].CopyWithOutput(firstPair[0])
             };
             secondSwapped = new Gate[] {
               secondPairGates[0].CopyWithOutput(secondPair[1]),
               secondPairGates[1].CopyWithOutput(secondPair[0])
             };
             gatesAfterSwap[firstPair[0]] = firstSwapped[1];
             gatesAfterSwap[firstPair[1]] = firstSwapped[0];
             gatesAfterSwap[secondPair[0]] = secondSwapped[1];
             gatesAfterSwap[secondPair[1]] = secondSwapped[0];
             modifiedGates = new List<Gate>(gates);
             i = modifiedGates.IndexOf(firstPairGates[0]);
             modifiedGates[i] = firstSwapped[0];
             i = modifiedGates.IndexOf(firstPairGates[1]);
             modifiedGates[i] = firstSwapped[1];
             i = modifiedGates.IndexOf(secondPairGates[0]);
             modifiedGates[i] = secondSwapped[0];
             i = modifiedGates.IndexOf(secondPairGates[1]);
             modifiedGates[i] = secondSwapped[1];
             valuesAfter = PopulateOutputs(initialValues, modifiedGates);
             possiblyFixedResult = GatesBinaryValue(valuesAfter, "z");
             if (possiblyFixedResult.Equals(expectedZBinary))
             {
               Console.WriteLine("Achieved expected result by swapping {0}<->{1} and {2}<->{3}",
                 firstPair[0], firstPair[1], secondPair[0], secondPair[1]);
               return;
             }
           }
         }
      }
    }
    
    // TODO: We could look at different examples to see which digits come out wrong
    // TODO: It's not guaranteed that the bad outputs are common to all, because it's two pairs.
  }
  
  public static Dictionary<string, bool> PopulateOutputs(Dictionary<string, bool> initialValues, List<Gate> gates)
  {
    Dictionary<string, bool> currentValues = new Dictionary<string, bool>(initialValues);
    List<Gate> worklist = new List<Gate>(gates);
    while (worklist.Count > 0)
    {
      //Console.WriteLine("Testing {0} gates", worklist.Count);
      List<Gate> nextWorklist = new List<Gate>();
      foreach (var gate in worklist)
      {
        if (!gate.TryEvaluate(currentValues))
        {
          nextWorklist.Add(gate);
        }
      }
      worklist = nextWorklist;
    }
    return currentValues;
  }
}

public class Gate
{
  private readonly string[] _connectors = new string[3];
  private readonly Operation _operation;
  
  public Gate(string firstInput, string secondInput, Operation operation, string output)
  {
    _connectors[0] = firstInput;
    _connectors[1] = secondInput;
    _connectors[2] = output;
    _operation = operation;
  }
  
  public bool TryEvaluate(Dictionary<string, bool> knownValues)
  {
    if (knownValues.TryGetValue(_connectors[0], out bool w1) &&
      knownValues.TryGetValue(_connectors[1], out bool w2))
    {
      knownValues.Add(_connectors[2], _operation.On(w1, w2));
      return true;
    }
    return false;
  }
  
  public string Output => _connectors[2];
  
  public IEnumerable<string> Inputs => _connectors.Take(2);
  
  public Gate CopyWithOutput(string otherOutput)
  {
    return new Gate(_connectors[0], _connectors[1], _operation, otherOutput);
  }
}

public class Operation
{
  private readonly string _text;
  private readonly Func<bool, bool, bool> _op;
  
  private Operation(string text, Func<bool, bool, bool> op)
  {
    _text = text;
    _op = op;
  }
  
  private static Operation And = new Operation("AND", (w1, w2) => w1 && w2);
  private static Operation Or = new Operation("OR", (w1, w2) => w1 || w2);
  private static Operation Xor = new Operation("XOR", (w1, w2) => w1 ^ w2);
  
  public static Operation Get(string operationText)
  {
    switch (operationText)
    {
      case "AND":
        return And;
      case "OR":
        return Or;
      case "XOR":
        return Xor;
      default:
        throw new ArgumentException("operationText");
    }
  }
  
  public bool On(bool w1, bool w2)
  {
    return _op(w1, w2);
  }
}
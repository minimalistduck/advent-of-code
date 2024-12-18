using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day09Program
{
  public static void Main()
  {
     //var input = "2333133121414131402";
     const string inputFilePath = @"C:\Users\davidt\Coding\AdventOfCode\2024\Day09-input.txt";
     var input = File.ReadAllText(inputFilePath).Trim();
     
     IDiskLayout partOneDiskLayout = new DiskLayout();
     Solve(input, partOneDiskLayout, "one");
     
     IDiskLayout partTwoDiskLayout = new EfficientDiskLayout();
     Solve(input, partTwoDiskLayout, "two");
  }
  
  private static void Solve(string input, IDiskLayout layout, string partName)
  {    
     var populateActions = new Action<int>[] {
         x => layout.AddContent(x),
         x => layout.AddGap(x)
     };
     
     for (int i = 0; i < input.Length; i++)
     {
         populateActions[i % 2](int.Parse(input[i].ToString()));
     }
     
     //Console.WriteLine(layout);
     layout.Defrag();
     //Console.WriteLine(layout);
     
     Console.WriteLine("Part {1}: {0}", layout.CalculateChecksum(), partName);
  }
}
   
public class Block
{
    public readonly long ContentId;
    
    public Block(long contentId)
    {
        ContentId = contentId;
    }
}   

public interface IDiskLayout
{
    void AddContent(int length);
    void AddGap(int length);
    void Defrag();
    long CalculateChecksum();
}

public class DiskLayout : IDiskLayout
{
    private readonly List<Block> _layout = new List<Block>();
    private long _nextContentId;
    
    public void AddContent(int length)
    {
        for (int j = 0; j < length; j++)
        {
            _layout.Add(new Block(_nextContentId));
        }
        _nextContentId++;
    }
    
    public void AddGap(int length)
    {
        for (int j = 0; j < length; j++)
        {
            _layout.Add(null);
        }
    }
    
    public void Defrag()
    {
        int firstGap = 0;
        int lastContent = _layout.Count - 1;
        while (lastContent > firstGap)
        {
            while (_layout[firstGap] != null)
                firstGap++;
            while (_layout[lastContent] == null)
                lastContent--;
            
            if (lastContent > firstGap)
            {
                _layout[firstGap] = _layout[lastContent];
                _layout[lastContent] = null;
            }
        }
    }
    
    public long CalculateChecksum()
    {
        long result = 0;
        for (int i = 0; i < _layout.Count; i++)
        {
            if (_layout[i] != null)
            {
                result += i * _layout[i].ContentId;
            }
        }
        return result;
    }
    
    public override string ToString()
    {
        var result = new StringBuilder();
        foreach (var b in _layout)
        {
            result.Append(b == null ? "." : b.ContentId.ToString());
        }
        return result.ToString();
    }
}

public class EfficientDiskLayout : IDiskLayout
{
    private readonly List<LayoutEntry> _layout = new List<LayoutEntry>();
    private long _nextContentId;
    
    public void AddContent(int length)
    {
        _layout.AddRange(LayoutEntry.GenerateContent(_nextContentId, length));
        _nextContentId++;
    }
    
    public void AddGap(int length)
    {
        _layout.AddRange(LayoutEntry.GenerateGap(length));
    }
    
    public void Defrag()
    {
        int endCursor = _layout.Count - 1;
        while (endCursor > 0)
        {
            // Make sure we're pointing to content
            while (!_layout[endCursor].ContentId.HasValue)
            {
                endCursor--;
                if (endCursor <= 0)
                {
                    break;
                }
            }
            if (endCursor <= 0)
            {
                break;
            }
            
            // Go to the start of this content block
            var contentId = _layout[endCursor].ContentId.Value;
            while (endCursor >= 0 &&
                !_layout[endCursor].IsGap &&
                _layout[endCursor].ContentId.Value == contentId)
            {
                endCursor--;
            }
            endCursor++;

            var minGapSize = _layout[endCursor].CountRemaining;
            //Console.WriteLine("Looking for the first gap of size {0} for {1}", minGapSize, _layout[endCursor].ContentId.Value);
            
            var gapStart = 0;
            var found = false;
            // If the first suitably sized gap for X is after its current position, we won't move it
            while (!found && gapStart < endCursor)
            {
                if (_layout[gapStart].IsGap && _layout[gapStart].CountRemaining >= minGapSize)
                {
                    found = true;
                }
                else
                {
                    gapStart += _layout[gapStart].CountRemaining;
                }
            }
            if (found)
            {
                var newGap = LayoutEntry.GenerateGap(minGapSize);
                for (int m = 0; m < minGapSize; m++)
                {
                    _layout[gapStart + m] = _layout[endCursor + m];
                    _layout[endCursor + m] = newGap[m];
                }
            }
            endCursor--;
        }
    }
    
    public long CalculateChecksum()
    {
        long result = 0;
        for (int i = 0; i < _layout.Count; i++)
        {
            if (!_layout[i].IsGap)
            {
                result += i * _layout[i].ContentId.Value;
            }
        }
        return result;
    }
}

public class LayoutEntry
{
    public readonly long? ContentId;
    public readonly int CountRemaining; // inclusive of the current one
    public bool IsGap => !ContentId.HasValue;
    
    private LayoutEntry(long? contentId, int countRemaining)
    {
        ContentId = contentId;
        CountRemaining = countRemaining;
    }
    
    public static LayoutEntry[] GenerateContent(long contentId, int count)
    {
        return Generate(contentId, count);
    }
    
    public static LayoutEntry[] GenerateGap(int count)
    {
        return Generate(null, count);
    }
    
    private static LayoutEntry[] Generate(long? contentId, int count)
    {
        var result = new LayoutEntry[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = new LayoutEntry(contentId, count - i);
        }
        return result;
    }
}
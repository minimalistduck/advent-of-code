public static class DayThreeProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();

    SolvePartOne(lines);
  }

  private static void SolvePartOne(string[] lines)
  {
    var partOne = 0;

    foreach (var line in lines)
    {
      var pack = line.Select(d => (int)d - (int)'0').ToArray();
      var firstIndex = PosOfMax(pack, 0, pack.Length - 2);
      var secondIndex = PosOfMax(pack, firstIndex + 1, pack.Length - 1);
      var packJoltage = pack[firstIndex]*10 + pack[secondIndex];
      partOne += packJoltage;
    }
    
    Console.WriteLine(partOne);
  }

  private static int PosOfMax(int[] all, int startIndex, int endIndex)
  {
    var resultIndex = -1;
    var highestSoFar = -1;
    for (var i = startIndex; i <= endIndex; i++)
    {
      if (all[i] > highestSoFar)
      {
        highestSoFar = all[i];
        resultIndex = i;
      }
    }
    return resultIndex;
  }

  private static void SolvePartTwo(string[] lines)
  {
    var partTwo = 0;

    Console.WriteLine(partTwo);
  }
}

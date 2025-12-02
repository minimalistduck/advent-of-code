public static class DayOneProgram
{
  public static void Main(string[] args)
  {
    var lines = File.ReadAllLines(args[1]).ToArray();
    var partOne = 0;
    var pointingAt = 50;

    foreach (var line in lines)
    {
      var dirn = line[0] == 'R' ? 1 : -1;
      var distance = int.Parse(line.Substring(1,line.Length-1));
      var offset = dirn * distance;
      while (pointingAt + offset < 0) // is this loop needed?
        offset += 100;
      pointingAt = (pointingAt + offset) % 100;
      if (pointingAt == 0)
        partOne++;
    }
    Console.WriteLine(partOne);
  }
}

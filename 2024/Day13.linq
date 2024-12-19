<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
</Query>

void Main()
{
   const string folder = @"D:\David\Coding\AdventOfCode2024\Day13";
   string inputPath = Path.Combine(folder, "input.txt");
   var allText =  File.ReadAllText(inputPath);
   
   var allNumbers = Regex.Matches(allText, @"\d+").Select(m => long.Parse(m.Groups[0].Value)).ToArray();
   //Console.WriteLine("Extracted {0} numbers from the input", allNumbers.Length);
   
   var partOneResult = 0L;
   //var partTwoResult = 0L;
   for (int i = 0; i < allNumbers.Length; i += 6)
   {
      partOneResult += GetExpenditurePartOne(
         allNumbers[i], allNumbers[i+1],
         allNumbers[i+2], allNumbers[i+3],
         allNumbers[i+4], allNumbers[i+5]
      );
      //partTwoResult += GetExpenditurePartTwo(
      //   allNumbers[i], allNumbers[i+1],
      //   allNumbers[i+2], allNumbers[i+3],
      //   allNumbers[i+4] + 10000000000000L, allNumbers[i+5] + 10000000000000L
      //);
      double axd = allNumbers[i];
      double ayd = allNumbers[i+1];
      double bxd = allNumbers[i+2];
      double byd = allNumbers[i+3];
      double tx = allNumbers[i+4];
      double ty = allNumbers[i+5];
      
      bool interesting = false;
      for (double a = 0; a < 3 && !interesting; a++)
      for (double b = 0; b < 3 && !interesting; b++)
      {
         if (a == 0.0 && b == 0.0)
            continue;
         var x = a * axd + b * bxd;
         var y = a * ayd + b * byd;
         if (x * 0.95 <= y && y < x && ty < tx)
         {
            Console.WriteLine("Claw {0} looks interesting: {1} {2} {3} {4} ({5}v{6})", i / 6,
               allNumbers[i], allNumbers[i+1], allNumbers[i+2], allNumbers[i+3],
               x, y);
            interesting = true;
         }
         if (x <= y && y < x * 1.05 && tx <= ty)
         {
            Console.WriteLine("Claw {0} looks interesting: {1} {2} {3} {4} ({5}v{6})", i / 6,
               allNumbers[i], allNumbers[i+1], allNumbers[i+2], allNumbers[i+3],
               x, y);
            interesting = true;
         }
      }
   }
   
   Console.WriteLine("Part one: {0}", partOneResult);
   //Console.WriteLine("Part two: {0}", partTwoResult);
}

public long GetExpenditurePartOne(long ax, long ay, long bx, long by, long tx, long ty)
{
   return GetExpenditure(ax, ay, bx, by, tx, ty, 100L, 0L, 100L, 0L);
}

public long GetExpenditurePartTwo(long ax, long ay, long bx, long by, long tx, long ty)
{
   // Suppose we aim to get there using A as much as possible
   long? resultA = null;
   var topAByX = tx / ax;  // either a hit, or adding one would overshoot
   var bottomAByX = topAByX - bx; // if there's no hit in this range, there's no hit at all
   var topBByX = (tx - bottomAByX) / bx; // either a hit, or adding one would overshoot
   var bottomBByX = (tx - topAByX) / bx;
   
   var topAByY = ty / ay;
   var bottomAByY = topAByY - by;
   var topBByY = (ty - bottomAByY) / by;
   var bottomBByY = (ty - topAByY) / by;
   
   //var topA = Math.Min(topAByX, topAByY);
   var topA = Math.Max(topAByX, topAByY);
   //var bottomA = Math.Max(bottomAByX, bottomAByY);
   var bottomA = Math.Min(bottomAByX, bottomAByY);
   //var topB = Math.Min(topBByX, topBByY);
   var topB = Math.Max(topBByX, topBByY);
   //var bottomB = Math.Max(bottomBByX, bottomBByY);
   var bottomB = Math.Min(bottomBByX, bottomBByY);
   if (bottomA > topA)
   {
      Console.WriteLine("No amount of pressing A can achieve both targets");
   }
   else if (bottomB > topB)
   {
      Console.WriteLine("If we press A [{0},{1}] times, no amount of pressing B can achieve both targets",
         bottomA, topA);
   }
   else
   {
      resultA = SeekExpenditure(ax, ay, bx, by, tx, ty, topA, bottomA, topB, bottomB, 3, 1);
      Console.WriteLine("Seeking to use A as much as possible, found {0}", resultA.HasValue ? resultA.Value : "nothing");
   }
   
   // Now suppose we aim to use B as much as possible
   long? resultB = null;
   topBByX = tx / bx;
   bottomBByX = topBByX - ax;
   topAByX = (tx - bottomBByX) / ax;
   bottomAByX = (tx - topBByX) / ax;
   
   topBByY = ty / by;
   bottomBByY = topBByY - ay;
   topAByY = (ty - bottomBByY) / ay;
   bottomAByY = (ty - topBByY) / ay;
   
   //topB = Math.Min(topBByX, topBByY);
   topB = Math.Max(topBByX, topBByY);
   //bottomB = Math.Max(bottomBByX, bottomBByY);
   bottomB = Math.Min(bottomBByX, bottomBByY);
   //topA = Math.Min(topAByX, topAByY);
   topA = Math.Max(topAByX, topAByY);
   //bottomA = Math.Max(bottomAByX, bottomAByY);
   bottomA = Math.Min(bottomAByX, bottomAByY);
   if (bottomB > topB)
   {
      Console.WriteLine("No amount of pressing B can achieve both targets");
   }
   else if (bottomA > topA)
   {
      Console.WriteLine("If we press B [{0},{1}] times, no amount of pressing A can achieve both targets",
         bottomB, topB);
   }
   else
   {
      resultB = SeekExpenditure(bx, by, ax, ay, tx, ty, topB, bottomB, topA, bottomA, 1, 3);
      Console.WriteLine("Seeking to use B as much as possible, found {0}", resultB.HasValue ? resultB.Value : "nothing");
   }
   
   if (resultA.HasValue && resultB.HasValue)
      return Math.Min(resultA.Value, resultB.Value);
   else if (resultA.HasValue)
      return resultA.Value;
   else if (resultB.HasValue)
      return resultB.Value;
   else
      return 0L;
}

public long GetExpenditure(long ax, long ay, long bx, long by, long tx, long ty, long topA, long bottomA, long topB, long bottomB)
{
   // My thinking is:
   // one button out of A and B offers better "value for money"
   // therefore the lowest cost solution will either be the one
   // with smallest a, or the one with smallest b. So we find
   // both and take the minimum

   long? resultA = SeekExpenditure(ax, ay, bx, by, tx, ty, topA, bottomA, topB, bottomB, 3, 1);   
   long? resultB = SeekExpenditure(bx, by, ax, ay, tx, ty, topB, bottomB, topA, bottomA, 1, 3);
   
   // either they both have a value (because if they didn't find different
   // values, they'll both have reached the same one), or neither has a value
   return resultA.HasValue ? Math.Min(resultA.Value, resultB.Value) : 0;
}

private long? SeekExpenditure(long px, long py, long qx, long qy, long tx, long ty,
   long topP, long bottomP, long topQ, long bottomQ,
   long costP, long costQ)
{
   long? result = null;
   long p = topP;
   while (!result.HasValue && p >= bottomP)
   {
      long q = bottomQ;
      while (!result.HasValue &&
         (px*p + qx*q) <= tx &&
         (py*p + qy*q) <= ty)
      {
         if (px*p + qx*q == tx &&
            py*p + qy*q == ty)
         {
            result = costP*p +costQ*q;
         }
         q++;
      }
      p--;
   }
   return result;
}

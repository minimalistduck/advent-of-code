using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayEight
   {
      const string InputFilePath = @"Input-Day08.txt";

      public static void Solve()
      {
         var rawTreeGrid = File.ReadAllLines(InputFilePath);
         //Console.WriteLine(rawTreeGrid[0].Length);
         int width = rawTreeGrid[0].Length;
         int height = rawTreeGrid.Length;
         int[,] treeHeightGrid = new int[width, height];
         for (int r = 0; r < height; r++)
         {
            for (int c = 0; c < width; c++)
            {
               treeHeightGrid[c,r] = (int)(rawTreeGrid[r][c] - '0');
            }
         }
        
         bool[,] treeVisibility = new bool[width, height];
         var topToBottom = Enumerable.Range(0, height);
         var bottomToTop = topToBottom.Reverse();
         var leftToRight = Enumerable.Range(0, width);
         var rightToLeft = leftToRight.Reverse();
         foreach (var r in topToBottom)
         {
            foreach (var horizontalDirection in new [] { leftToRight, rightToLeft })
            {
               int highestSoFar = -1;
               foreach (var c in horizontalDirection)
               {
                  if (highestSoFar < treeHeightGrid[c,r])
                  {
                     treeVisibility[c,r] = true;
                     highestSoFar = treeHeightGrid[c,r];
                  }
               }
            }
         }
         foreach (var c in leftToRight)
         {
            foreach (var verticalDirection in new [] { topToBottom, bottomToTop })
            {
               int highestSoFar = -1;
               foreach (var r in verticalDirection)
               {
                  if (highestSoFar < treeHeightGrid[c,r])
                  {
                     treeVisibility[c,r] = true;
                     highestSoFar = treeHeightGrid[c,r];
                  }
               }
            }
         }
         var visibleTreeCount = 0;
         foreach (var r in topToBottom)
         {
            foreach (var c in leftToRight)
            {
               if (treeVisibility[c,r])
               {
                  visibleTreeCount++;
               }
            }
         }
         Console.WriteLine("Part one: " + visibleTreeCount);
        
         int[,] scenicScore = new int[width,height];
         for (int fromRow = 1; fromRow < height - 1; fromRow++)
         {
            for (int fromColumn = 1; fromColumn < width - 1; fromColumn++)
            {
                int[][] directionDeltas = new int[][] {
                   new [] {0,1}, // down
                   new [] {1,0}, // right
                   new [] {0,-1}, // up
                   new [] {-1,0} // left
                };
                int scenicScoreHere = 1;
                foreach (var delta in directionDeltas)
                {
                   int toRow = fromRow;
                   int toColumn = fromColumn;
                   int distance = 0;
                   do
                   {
                      toColumn += delta[0];
                      toRow += delta[1];
                      distance++;
                   } while (
                      0 < toColumn && toColumn < width - 1 &&
                      0 < toRow && toRow < height - 1 &&
                      treeHeightGrid[toColumn,toRow] < treeHeightGrid[fromColumn,fromRow]
                   );
                   scenicScoreHere *= distance;
               }
               scenicScore[fromColumn,fromRow] = scenicScoreHere;
            }
         }
         var maxScenicScore = -1;
         foreach (var r in topToBottom)
         {
            foreach (var c in leftToRight)
            {
               if (scenicScore[r,c] > maxScenicScore)
               {
                  maxScenicScore = scenicScore[r,c];
                  //Console.WriteLine("Found {0} at {1},{2}", maxScenicScore, r, c);
               }
            }
         }
         Console.WriteLine("Part two: " + maxScenicScore);
      }
   }
}

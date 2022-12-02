namespace DavidDylan.AdventOfCode2022
{
   public static class DayTwoPartTwo
   {
      public static int Answer(string inputFilePath)
      {
         int totalScore = 0;
         foreach (string line in File.ReadAllLines(inputFilePath))
         {
            totalScore += OutcomeScore(line[2]) + ScoreForShapeIChoose(line[0], line[2]);
         }
         return totalScore;
      }
      
      private static int OutcomeScore(char outcome)
      {
         return (int)(outcome - 'X') * 3;
      }
      
      private static int ScoreForShapeIChoose(char theirs, char outcome)
      {
         // shift 4 for a win, 3 for a draw, 2 for a loss
         int shift = (int)(outcome - 'V');
         return (int)((theirs - 'A' + shift) % 3) + 1;
      }
   }
   
   public static class DayTwoPartOne
   {
      public static int Answer(string inputFilePath)
      {
         int totalScore = 0;
         foreach (string line in File.ReadAllLines(inputFilePath))
         {
            totalScore += OutcomeScore(line[0], line [2]) + ShapeScore(line[2]);
         }
         return totalScore;
      }
      
      private static int OutcomeScore(char theirs, char mine)
      {
         char mineInSameNotation = InTheirNotation(mine);
         if (mineInSameNotation == theirs)
            return 3;
         if (mineInSameNotation == WhatBeats(theirs))
            return 6;
         return 0;
      }

      private static char InTheirNotation(char mine)
      {
         return (char)(mine-23);
      }

      private static char WhatBeats(char theirs)
      {
         return (char)(((theirs - 'A' + 1) % 3) + 'A');
      }

      private static int ShapeScore(char mine)
      {
         return (int)(mine - 'W');
      }      
   }
}

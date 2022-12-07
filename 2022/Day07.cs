using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DavidDylan.AdventOfCode2022
{
   public static class DaySeven
   {
      const string InputFilePath = @"Input-Day07.txt";

      [Test]
      public static void Solve()
      {
         var sizes = MeasureDirectorySizes(File.ReadAllLines(InputFilePath));
         SolvePartOne(sizes);
         SolvePartTwo(sizes);
      }

      public static void SolvePartTwo(Dictionary<string, int> sizeByDirectory)
      {
         var spaceFree = 70000000 - sizeByDirectory["/"];
         var extraToFree = 30000000 - spaceFree;
         Console.WriteLine(
            sizeByDirectory.Select(kvp => kvp.Value).Where(v => v >= extraToFree).Min()
         );
      }
      
      public static void SolvePartOne(Dictionary<string, int> sizeByDirectory)
      {
         Console.WriteLine(
            sizeByDirectory.Select(kvp => kvp.Value).Where(v => v <= 100000).Sum()
         );
      }

      public static Dictionary<string, int> MeasureDirectorySizes(IEnumerable<string> commands)
      {
         var currentDir = new Stack<string>();
         var fileSizes = new List<FileSize>();

         foreach (var command in commands)
         {
            if (command.Equals("$ cd /"))
            {
               currentDir.Clear();
            }
            else if (command.Equals("$ cd .."))
            {
               currentDir.Pop();
            }
            else if (command.StartsWith("$ cd "))
            {
               currentDir.Push(command.Substring(5));
            }
            else if (char.IsDigit(command[0]))
            {
               fileSizes.Add(new FileSize(
                  int.Parse(command.Split(" ")[0]),
                  currentDir.Reverse().ToArray()
               ));
            }
         }

         var sizeByDir = new Dictionary<string, int>();
         foreach (var fileSize in fileSizes)
         {
            for (var i = 0; i <= fileSize.DirectorySegments.Length; i++)
            {
               var dir = fileSize.GetSubPath(i);
               sizeByDir.TryGetValue(dir, out int sizeSoFar);
               sizeByDir[dir] = sizeSoFar + fileSize.Size;
            }
         }
         return sizeByDir;
      }
   }

   public class FileSize
   {
      public readonly int Size;
      public readonly string[] DirectorySegments;

      public FileSize(int size, IEnumerable<string> path)
      {
         Size = size;
         DirectorySegments = path.ToArray();
      }

      public string GetPath()
      {
         return GetSubPath(DirectorySegments.Length);
      }

      public string GetSubPath(int depth)
      {
         return "/" + string.Join("/", DirectorySegments.Take(depth));
      }

      public override string ToString()
      {
         return $"{Size} {GetPath()}";
      }
   }
}

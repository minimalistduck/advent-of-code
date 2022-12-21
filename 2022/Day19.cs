using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayNineteen
   {
      public const string InputFilePath = "Input-Day19.txt";
      
      public static Blueprint[] ExampleBlueprints
      {
         get
         {
            return new [] {
               new Blueprint(1, 4, 2, 3, 14, 2, 7),
               new Blueprint(2, 2, 3, 3, 8, 3, 12)
            };
         }
      }
      
      public static Blueprint[] RealBlueprints
      {
         get
         {
            List<Blueprint> blueprints = new List<Blueprint>();
            foreach (var line in File.ReadAllLines(InputFilePath))
            {
               blueprints.Add(new Blueprint(
                  Regex.Matches(line, @"\d+").Select(m => int.Parse(m.Groups[0].Value)).ToArray()
               ));
            }
            return blueprints.ToArray();
         }
      }

      public static void SolvePartOneExample()
      {
         Console.WriteLine(SumQualityLevels(24, ExampleBlueprints));
      }
      
      public static void SolvePartOne()
      {
         Console.WriteLine(SumQualityLevels(24, RealBlueprints));
      }
      
      public static void SolvePartTwoExample()
      {
         Console.WriteLine(ProductOfCrackedGeodes(32, ExampleBlueprints));
      }
      
      public static void SolvePartTwo()
      {
         Console.WriteLine(ProductOfCrackedGeodes(32, RealBlueprints.Take(3).ToArray()));
      }
      
      public static int ProductOfCrackedGeodes(int minutes, params Blueprint[] blueprints)
      {
         var result = 1;
         foreach (var blueprint in blueprints)
         {
            result *= FindMaxCrackedGeodes(minutes, blueprint);
         }
         return result;
      }
      
      public static int SumQualityLevels(int minutes, params Blueprint[] blueprints)
      {
         return blueprints.Sum(b => b.Id * FindMaxCrackedGeodes(minutes, b));
      }
      
      public static int FindMaxCrackedGeodes(int minutes, Blueprint blueprint)
      {
         var stockOptions = new List<Stock>();
         stockOptions.Add(Stock.Initial);
         for (int t = 1; t <= minutes; t++)
         {
            var optionsAfter = new Dictionary<string, Stock>();
            foreach (var optionSoFar in stockOptions)
            {
               var feasibleActions = blueprint.Where(a => optionSoFar.WithAction(a) != null);
               var halfwayOption = optionSoFar.AfterTimestep();
               foreach (var action in feasibleActions)
               {
                  var nextOption = halfwayOption.WithAction(action);
                  optionsAfter[nextOption.ToString()] = nextOption;  // deliberately overwriting equivalent options
               }
            }
            // This part gets me to the correct answer, but I only know it's correct because
            // I submitted it and got a star. It takes too long to do an exhaustive search,
            // but I offer no proof that these shortcuts guarantee a correct answer in general.
            var maxCrackers = optionsAfter.Values.Max(opt => opt.GeodeCrackerCount);
            stockOptions = optionsAfter.Values.Where(opt => 
               // we don't need more ore-miners than we can use the ore from
               opt.OreMinerCount <= blueprint.MaxOrePerMinute &&
               // and we can drop any options that have fallen far behind the front-runners
               opt.GeodeCrackerCount >= maxCrackers - 1
            ).ToList();
            if (stockOptions.Count > 100_000)
               Console.WriteLine("   Exploring {0} distinct options after {1} minute(s)", stockOptions.Count, t);
            //var maxGeodesCrackedSoFar = stockOptions.Max(opt => opt.GeodesCracked);
            //Console.WriteLine(stockOptions.Where(opt => opt.GeodesCracked == maxGeodesCrackedSoFar).Last().ToString());
         }
         var result = stockOptions.Max(opt => opt.GeodesCracked);
         Console.WriteLine("Blueprint {0} can crack {1} geode(s).", blueprint.Id, result);
         return result;
      }
   }

   public class RobotType
   {
      public readonly int OreMined;
      public readonly int ClayHarvested;
      public readonly int ObsidianCollected;
      public readonly int GeodeCracked;
      private readonly string mName;
      
      private RobotType(int oreMined, int clayHarvested, int obsidianCollected, int geodeCracked, char symbol)
      {
         OreMined = oreMined;
         ClayHarvested = clayHarvested;
         ObsidianCollected = obsidianCollected;
         GeodeCracked = geodeCracked;
         mName = symbol.ToString();
      }
      
      public override string ToString()
      {
         return mName;
      }
      
      public static readonly RobotType OreMiner = new RobotType(1, 0, 0, 0, 'R');
      public static readonly RobotType ClayHarvester = new RobotType(0, 1, 0, 0, 'C');
      public static readonly RobotType ObsidianCollector = new RobotType(0, 0, 1, 0, 'B');
      public static readonly RobotType GeodeCracker = new RobotType(0, 0, 0, 1, 'G');
   }

   public class ActionType
   {
      public readonly int OreUsed;
      public readonly int ClayUsed;
      public readonly int ObsidianUsed;
      public readonly RobotType RobotTypeCreated;
      
      public ActionType(RobotType robotTypeCreated, int oreUsed = 0, int clayUsed = 0, int obsidianUsed = 0)
      {
         OreUsed = oreUsed;
         ClayUsed = clayUsed;
         ObsidianUsed = obsidianUsed;
         RobotTypeCreated = robotTypeCreated;
      }
      
      public static readonly ActionType Wait = new ActionType(null);
   }

   public class Blueprint : List<ActionType>
   {
      public readonly int Id;
      public readonly int MaxOrePerMinute;
      
      public Blueprint(int id)
         : base()
      {
         Id = id;
         Add(ActionType.Wait);
      }
      
      public Blueprint(params int[] data)
         : this(data[0])
      {
         Add(new ActionType(RobotType.OreMiner, oreUsed: data[1]));
         Add(new ActionType(RobotType.ClayHarvester, oreUsed: data[2]));
         Add(new ActionType(RobotType.ObsidianCollector, oreUsed: data[3], clayUsed: data[4]));
         Add(new ActionType(RobotType.GeodeCracker, oreUsed: data[5], obsidianUsed: data[6]));
         MaxOrePerMinute = new [] { data[1], data[2], data[3], data[5] }.Max();
      }
   }

   public class Stock
   {
      public readonly int Ore;
      public readonly int Clay;
      public readonly int Obsidian;
      public readonly int GeodesCracked;
      public readonly RobotType[] Robots;
      public readonly int OreMinerCount;
      public readonly int GeodeCrackerCount;
      
      public Stock(int ore, int clay, int obsidian, int geodesCracked, IEnumerable<RobotType> robots)
      {
         Ore = ore;
         Clay = clay;
         Obsidian = obsidian;
         GeodesCracked = geodesCracked;
         Robots = robots.ToArray();
         OreMinerCount = Robots.Where(r => r == RobotType.OreMiner).Count();
         GeodeCrackerCount = Robots.Where(r => r == RobotType.GeodeCracker).Count();
      }
      
      public static readonly Stock Initial = new Stock(0, 0, 0, 0, new [] {RobotType.OreMiner});
      
      // Returns null if this level of stock is insufficient for that action type
      public Stock WithAction(ActionType actionType)
      {
         if (actionType.OreUsed > Ore ||
             actionType.ClayUsed > Clay ||
             actionType.ObsidianUsed > Obsidian)
         {
            return null;
         }
         return new Stock(
            Ore - actionType.OreUsed,
            Clay - actionType.ClayUsed,
            Obsidian - actionType.ObsidianUsed,
            GeodesCracked,
            actionType.RobotTypeCreated == null ? Robots : Robots.Concat(new [] { actionType.RobotTypeCreated } )
         );
      }
      
      public Stock AfterTimestep()
      {
         var ore = Ore;
         var clay = Clay;
         var obsidian = Obsidian;
         var geodes = GeodesCracked;
         foreach (var robot in Robots)
         {
            ore += robot.OreMined;
            clay += robot.ClayHarvested;
            obsidian += robot.ObsidianCollected;
            geodes += robot.GeodeCracked;
         }
         return new Stock(ore, clay, obsidian, geodes, Robots);
      }
      
      // String representation can be used as a key
      public override string ToString()
      {
         var robotPart = Robots.Select(r => r.ToString()).OrderBy(s => s);
         var resourcePart = new [] { Ore.ToString(), Clay.ToString(), Obsidian.ToString(), GeodesCracked.ToString() };
         return string.Join(",", robotPart.Concat(resourcePart));
      }
   }
}

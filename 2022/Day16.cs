using System;
using System.Collections.Generic;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static class DaySixteen
   {
      public static void SolveExample()
      {
         FindMaxRelease(MapBuilder.BuildExampleMap(), 30);
      }
      
      public static void SolvePartOne()
      {
         FindMaxRelease(MapBuilder.BuildRealMap(), 30);
      }
      
      public static void FindMaxRelease(Map map, int minutes)
      {
         var worklist = new List<Path>();
         worklist.Add(new Path(new StartAt(map.StartLocation)));
         for (int t = 1; t <= minutes; t++)
         {
            Console.WriteLine("t={0}. Starting with {1} path(s)", t, worklist.Count);
            var possiblePaths = new List<Path>();
            foreach (var path in worklist)
            {
               if (path.Location.Valve != null && !path.OpenedValves.IsOpen(path.Location.Valve))
               {
                  possiblePaths.Add(path.AndOpenValve(minutes - t));
               }
               
               Location locationToAvoid = null;
               if (path.LastStep is MoveTo)
               {
                  locationToAvoid = path.Steps[path.Steps.Length - 2].Location;
               }
               foreach (var nextLocation in path.LastStep.Location.TunnelsTo.Where(loc => loc != locationToAvoid))
               {
                  possiblePaths.Add(path.AndMoveTo(nextLocation));
               }
            }            
            var shortlistingTimer = Stopwatch.StartNew();
            worklist = ShortlistPossiblePaths(possiblePaths);
            shortlistingTimer.Stop();
            var shortlistingDelta = possiblePaths.Count - worklist.Count;
            Console.WriteLine($"   Shortlisting removed {shortlistingDelta} paths in {shortlistingTimer.Elapsed}");
            Console.WriteLine($"   Payoffs range from {possiblePaths.Max(p => p.Payoff)} down to {possiblePaths.Min(p => p.Payoff)}.");
         }
         Console.WriteLine(worklist.Max(p => p.Payoff));
      }
      
      private static List<Path> ShortlistPossiblePaths(List<Path> longlist)
      {
         // Many of the paths in longlist have reached the same location and opened the same valves.
         // Some of these may share the maximum payoff. As far as the future search goes, these are
         // all equivalent and we only need to keep one.
         var result = new List<Path>();
         var timer = Stopwatch.StartNew();
         foreach (var group in longlist.GroupBy(p => p.OpenedValves.Key))
         {
            foreach (var innerGroup in group.GroupBy(p => p.Location))
            {
               Path pathWithBestPayoffSoFar = innerGroup.First();
               foreach (var item in innerGroup)
               {
                  if (item.Payoff > pathWithBestPayoffSoFar.Payoff)
                  {
                     pathWithBestPayoffSoFar = item;
                  }
               }
               result.Add(pathWithBestPayoffSoFar);
            }
         }
         return result;
      }
   }

   public class Path
   {
      public readonly Step[] Steps;
      public readonly int Payoff;
      public readonly OpenedValves OpenedValves;
      public readonly Step LastStep;
      public Location Location => LastStep.Location;
      
      public Path(StartAt startStep)
      {
         Steps = new Step[] { startStep };
         LastStep = startStep;
         Payoff = 0;
         OpenedValves = OpenedValves.AllClosed;
      }
      
      private Path(Step[] steps, int payoff, OpenedValves openedValves)
      {
         Steps = steps;
         LastStep = steps.Last();
         Payoff = payoff;
         OpenedValves = openedValves;
      }
      
      public Path AndOpenValve(int timeRemaining)
      {
         var currentLocation = Location;
         var resultPayoff = Payoff + currentLocation.Valve.PayoffForOpening(timeRemaining);
         var resultSteps = new Step[Steps.Length + 1];
         Array.Copy(Steps, resultSteps, Steps.Length);
         resultSteps[Steps.Length] = new OpenValve(currentLocation);
         return new Path(resultSteps, resultPayoff, OpenedValves.WithValveOpen(currentLocation.Valve));
      }
      
      public Path AndMoveTo(Location newLocation)
      {
         var resultSteps = new Step[Steps.Length + 1];
         Array.Copy(Steps, resultSteps, Steps.Length);
         resultSteps[Steps.Length] = new MoveTo(newLocation);
         return new Path(resultSteps, Payoff, OpenedValves);
      }
   }

   public abstract class Step
   {
      public readonly Location Location;
      
      protected Step(Location location)
      {
         Location = location;
      }
      
      public override string ToString()
      {
         return Location.Label;
      }
   }

   public class StartAt : Step
   {
      public StartAt(Location location)
         : base(location)
      {}
   }

   public class MoveTo : Step
   {
      public MoveTo(Location location)
         : base(location)
      {}
   }

   public class OpenValve : Step
   {
      public OpenValve(Location location)
         : base(location)
         {}
         
      public override string ToString()
      {
         return "^";
      }
   }

   // Only suitable for problems with 32 or fewer valves
   public class OpenedValves
   {
      public static readonly OpenedValves AllClosed = new OpenedValves(0u);
      private readonly uint mValveStates;
      public uint Key => mValveStates;

      private OpenedValves(uint valveStates)
      {
         mValveStates = valveStates;
      }
      
      public bool IsOpen(Valve v)
      {
         uint tester = 0x1u << v.Id;
         return (tester & mValveStates) > 0;
      }
      
      public OpenedValves WithValveOpen(Valve v)
      {
         uint setter = 0x1u << v.Id;
         return new OpenedValves(mValveStates | setter);
      }
   }

   // Represents a functional valve
   public class Valve
   {
      public readonly string Label;
      public readonly int FlowRate;
      public int Id;
      
      public Valve(string label, int flowRate)
      {
         Label = label;
         FlowRate = flowRate;
      }
      
      public int PayoffForOpening(int timeRemaining)
      {
         return FlowRate * timeRemaining;
      }
   }

   // Represents a location, where there may or may not be a functional valve
   public class Location
   {
      public readonly string Label;
      public readonly string[] ToLabels;
      public readonly Valve Valve;
      public Location[] TunnelsTo;

      public Location(string label, int flowRate, params string[] tunnelsTo)
      {
         Label = label;
         ToLabels = tunnelsTo;
         if (flowRate > 0)
         {
            Valve = new Valve(label, flowRate);
         }
      }
      
      public override string ToString()
      {
         return Label;
      }
   }

   public class Map
   {
      public readonly Location StartLocation;
      
      public Map(Dictionary<string, Location> locations)
      {
         StartLocation = locations["AA"];
         foreach (var location in locations.Values)
         {
            location.TunnelsTo = location.ToLabels.Select(lbl => locations[lbl]).ToArray();
         }
      }
   }

   public class MapBuilder
   {
      private readonly Dictionary<string, Location> mLocations = new Dictionary<string, Location>();
      private int mNextValveId = 0;

      public void AddValve(string label, int flowRate, params string[] tunnelsTo)
      {
         var newLocation = new Location(label, flowRate, tunnelsTo);
         if (newLocation.Valve != null)
         {
            newLocation.Valve.Id = mNextValveId;
            mNextValveId++;
         }
         mLocations.Add(label, newLocation);
      }
      
      public Map Build()
      {
         return new Map(mLocations);
      }
      
      public static Map BuildExampleMap()
      {
         var map = new MapBuilder();
         map.AddValve("AA",0,"DD","II","BB");
         map.AddValve("BB",13,"CC","AA");
         map.AddValve("CC",2,"DD","BB");
         map.AddValve("DD",20,"CC","AA","EE");
         map.AddValve("EE",3,"FF","DD");
         map.AddValve("FF",0,"EE","GG");
         map.AddValve("GG",0,"FF","HH");
         map.AddValve("HH",22,"GG");
         map.AddValve("II",0,"AA","JJ");
         map.AddValve("JJ",21,"II");
         return map.Build();
      }

      public static Map BuildRealMap()
      {
         // Auto-generated from input
         var map = new MapBuilder();
         map.AddValve("QJ",11,"HB","GL");
         map.AddValve("VZ",10,"NE");
         map.AddValve("TX",19,"MG","OQ","HM");
         map.AddValve("ZI",5,"BY","ON","RU","LF","JR");
         map.AddValve("IH",0,"YB","QS");
         map.AddValve("QS",22,"IH");
         map.AddValve("QB",0,"QX","ES");
         map.AddValve("NX",0,"UH","OP");
         map.AddValve("PJ",0,"OC","UH");
         map.AddValve("OR",6,"QH","BH","HB","JD");
         map.AddValve("OC",7,"IZ","JR","TA","ZH","PJ");
         map.AddValve("UC",0,"AA","BY");
         map.AddValve("QX",0,"AA","QB");
         map.AddValve("IZ",0,"OC","SX");
         map.AddValve("AG",13,"NW","GL","SM");
         map.AddValve("ON",0,"MO","ZI");
         map.AddValve("XT",18,"QZ","PG");
         map.AddValve("AX",0,"UH","MO");
         map.AddValve("JD",0,"OR","SM");
         map.AddValve("HM",0,"TX","QH");
         map.AddValve("LF",0,"ZI","UH");
         map.AddValve("QH",0,"OR","HM");
         map.AddValve("RT",21,"PG");
         map.AddValve("NE",0,"VZ","TA");
         map.AddValve("OQ",0,"TX","GE");
         map.AddValve("AA",0,"QZ","UC","OP","QX","EH");
         map.AddValve("UH",17,"PJ","NX","AX","LF");
         map.AddValve("GE",0,"YB","OQ");
         map.AddValve("EH",0,"AA","MO");
         map.AddValve("MG",0,"TX","NW");
         map.AddValve("YB",20,"IH","GE","XG");
         map.AddValve("MO",15,"EH","ON","AX","ZH","CB");
         map.AddValve("JR",0,"ZI","OC");
         map.AddValve("GL",0,"AG","QJ");
         map.AddValve("SM",0,"JD","AG");
         map.AddValve("HB",0,"OR","QJ");
         map.AddValve("TA",0,"OC","NE");
         map.AddValve("PG",0,"RT","XT");
         map.AddValve("XG",0,"CB","YB");
         map.AddValve("ES",9,"QB","FL");
         map.AddValve("BH",0,"RU","OR");
         map.AddValve("FL",0,"SX","ES");
         map.AddValve("CB",0,"MO","XG");
         map.AddValve("QZ",0,"AA","XT");
         map.AddValve("BY",0,"UC","ZI");
         map.AddValve("ZH",0,"MO","OC");
         map.AddValve("OP",0,"NX","AA");
         map.AddValve("NW",0,"MG","AG");
         map.AddValve("RU",0,"ZI","BH");
         map.AddValve("SX",16,"IZ","FL");
         return map.Build();
      }
   }
}

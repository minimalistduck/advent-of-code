using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DavidDylan.AdventOfCode2022
{
   public static class DayTwentyOne
   {
      public static readonly string InputFilePath = "Input-Day21.txt";
      
      public static void SolvePartOne()
      {
         var monkeyTable = SetUpMonkeys(File.ReadAllLines(InputFilePath));
         Console.WriteLine(monkeyTable["root"].Evaluate());
      }
      
      public static void SolvePartTwo()
      {
         var monkeyTable = SetUpMonkeys(File.ReadAllLines(InputFilePath));
         var rootMonkeyChildren = monkeyTable["root"].Children.ToArray();

         monkeyTable["humn"] = Me.Instance;

         // I have established empirically that humn is on the left
         var leftChild = rootMonkeyChildren[0];
         var rightChild = rootMonkeyChildren[1];
         var rightValue = rightChild.Evaluate();
         
         leftChild.ForceValue(rightValue);
         Console.WriteLine(Me.Instance.Value);
         
         // Check the solution
         monkeyTable["humn"] = new Constant(Me.Instance.Value);
         var leftValue = leftChild.Evaluate();
         if (leftValue != rightValue)
            throw new InvalidOperationException($"Expected {rightValue} but got {leftValue}.");
      }
      
      public static void InvestigatePartTwo_Abandoned()
      {
         // Contrast what's going on here with the simplicity of the solution I eventually found :)
         var monkeyTable = SetUpMonkeys(File.ReadAllLines(InputFilePath));
         var rootMonkeyChildren = monkeyTable["root"].Children.ToArray();
         var leftChild = rootMonkeyChildren[0];
         var rightChild = rootMonkeyChildren[1];
         
         var leftValueBefore = leftChild.Evaluate();
         var rightValueBefore = rightChild.Evaluate();
         Console.WriteLine($"Inputs to root are {leftValueBefore} and {rightValueBefore}");
         
         var oldMe = monkeyTable["humn"];
         //var meValueBefore = oldMe.Evaluate();
         // This triggers an exception - when evaluating leftChild.
         //monkeyTable["humn"] = new Constant(-meValueBefore);
         
         monkeyTable["humn"] = Me.Instance;
         //leftChild.Evaluate(); throws as expected
         
         Console.WriteLine($"{leftChild} == {rightChild}");
         
         Func<string, Op> monkeyLookup = m => monkeyTable[m];
         Stack<Op> workingStack = new Stack<Op>();
         Stack<Op> resultStack = null;
         void Recurse(Op parent)
         {
            //Console.WriteLine($"Current parent, {parent.GetType().Name}, has {parent.Children.Count()} children");
            foreach (var child in parent.Children)
            {
               var resolvedChild = child;
               DerefOp childDeref = child as DerefOp;
               if (childDeref != null)
                  resolvedChild = childDeref.Resolve();
               //Console.WriteLine($"Pushed {resolvedChild}");
               workingStack.Push(resolvedChild);
               if (resolvedChild is Me)
                  resultStack = new Stack<Op>(workingStack.Reverse());
               Recurse(resolvedChild);
               var popped = workingStack.Pop();
               //Console.WriteLine($"Popped {popped}");
            }
         }
         Recurse(monkeyTable["root"]);
         if (resultStack == null)
            throw new InvalidOperationException("Failed to find me");
         var meOp = resultStack.Pop();
         if (!(meOp is Me))
            throw new InvalidOperationException("Me was not on top of the stack");

         Console.WriteLine(resultStack.Count);
         Op rootInverseOp = null;
         Op unknownChildOp = meOp;
         while (resultStack.Any())
         {
            var currentOp = resultStack.Pop();
            var children = currentOp.Children.ToArray();
            //if (unknownChildOp == children[0])
            //   rootInverseOp = currentOp.InvertLeft();
            //else
            //   rootInverseOp = currentOp.InvertRight();         
         }
         Console.WriteLine(rootInverseOp.Evaluate());
      }
      
      public static Dictionary<string, Op> SetUpMonkeys(string[] lines)
      {
         var monkeyTable = new Dictionary<string, Op>();
         Func<string, Op> monkeyLookup = m => monkeyTable[m];
         foreach (var line in lines)
         {
            var splitLine = line.Split(":");
            var monkeyName = splitLine[0];
            var possibleBinaryOp = BinaryOp.TryParse(splitLine[1], monkeyLookup);
            monkeyTable[monkeyName] = possibleBinaryOp == null ?
               new Constant(long.Parse(splitLine[1].Trim())) :
               possibleBinaryOp;
         }
         return monkeyTable;
      }
   }

   public abstract class Op
   {
      protected readonly List<Op> mChildren = new List<Op>();
      
      public abstract long Evaluate();
      
      public abstract void ForceValue(long value);
      
      public IEnumerable<Op> Children => mChildren;
   }

   public abstract class BinaryOp : Op
   {
      protected readonly string mSymbol;
      
      protected BinaryOp(Op left, Op right, string symbol)
      {
         mChildren.Add(left);
         mChildren.Add(right);
         mSymbol = symbol;
      }
      
      public Op Left => mChildren[0];
      public Op Right => mChildren[1];
      
      public override void ForceValue(long value)
      {
         long? left = null;
         long? right = null;
         try { left = Left.Evaluate(); } catch {}
         try { right = Right.Evaluate(); } catch {}
         if (left.HasValue)
            ForceRightValue(value, left.Value);
         else
            ForceLeftValue(value, right.Value);
      }
      
      protected abstract void ForceLeftValue(long forcedValue, long rightValue);
      protected abstract void ForceRightValue(long forcedValue, long leftValue);
      
      public override string ToString()
      {
         try
         {
            return Evaluate().ToString();
         }
         catch
         {  
            return "(" + Left.ToString() + mSymbol + Right.ToString() + ")";
         }
      }
      
      public static Op TryParse(string formula, Func<string,Op> symbolLookup)
      {
         Func<Op, Op, BinaryOp> creator = null;
         string[] args = null;

         var tryAdd = formula.Split("+");
         if (tryAdd.Length > 1)
         {
            creator = (l, r) => new AddOp(l, r);
            args = tryAdd;
         }
         
         var trySubtract = formula.Split("-");
         if (trySubtract.Length > 1)
         {
            creator = (l, r) => new SubtractOp(l, r);
            args = trySubtract;
         }
            
         var tryMultiply = formula.Split("*");
         if (tryMultiply.Length > 1)
         {
            creator = (l, r) => new MultiplyOp(l, r);
            args = tryMultiply;
         }
         
         var tryDivide = formula.Split("/");
         if (tryDivide.Length > 1)
         {
            creator = (l, r) => new DivideOp(l, r);
            args = tryDivide;
         }
         
         if (args == null)
            return null;
         
         var derefs = args.Select(a => new DerefOp(a.Trim(), symbolLookup)).ToArray();
         return creator(derefs[0], derefs[1]);
      }
   }

   public class AddOp : BinaryOp
   {
      public AddOp(Op left, Op right)
         : base(left, right, " + ") { }
      
      public override long Evaluate() => Left.Evaluate() + Right.Evaluate();
      protected override void ForceLeftValue(long forcedValue, long rightValue) => ForceOtherValue(forcedValue, rightValue, Left);
      protected override void ForceRightValue(long forcedValue, long leftValue) => ForceOtherValue(forcedValue, leftValue, Right);
      private void ForceOtherValue(long forcedValue, long evaluatedValue, Op forceOp)
      {
         forceOp.ForceValue(forcedValue - evaluatedValue);
      }
   }

   public class SubtractOp : BinaryOp
   {
      public SubtractOp(Op left, Op right)
         : base(left, right, " - ") { }
      
      public override long Evaluate() => Left.Evaluate() - Right.Evaluate();
      protected override void ForceLeftValue(long forcedValue, long rightValue)
      {
         // Left.Value - Right.Value = forcedValue
         Left.ForceValue(forcedValue + rightValue);
      }
      protected override void ForceRightValue(long forcedValue, long leftValue)
      {
         Right.ForceValue(leftValue - forcedValue);
      }
   }

   public class MultiplyOp : BinaryOp
   {
      public MultiplyOp(Op left, Op right)
         : base(left, right, " * ") { }
      
      public override long Evaluate() => Left.Evaluate() * Right.Evaluate();
      protected override void ForceLeftValue(long forcedValue, long rightValue) => ForceOtherValue(forcedValue, rightValue, Left);
      protected override void ForceRightValue(long forcedValue, long leftValue) => ForceOtherValue(forcedValue, leftValue, Right);
      private void ForceOtherValue(long forcedValue, long evaluatedValue, Op forceOp)
      {
         forceOp.ForceValue(forcedValue / evaluatedValue);
      }
   }

   public class DivideOp : BinaryOp
   {
      public DivideOp(Op left, Op right)
         : base(left, right, " / ") { }
      
      public override long Evaluate()
      {
         var left = Left.Evaluate();
         var right = Right.Evaluate();
         if (left % right != 0L)
            throw new ArgumentException($"{left} is not divisible by {right}");
         return left / right;
      }
      
      protected override void ForceLeftValue(long forcedValue, long rightValue)
      {
         // Left.Value / Right.Value = forcedValue
         Left.ForceValue(forcedValue * rightValue);
      }
      protected override void ForceRightValue(long forcedValue, long leftValue)
      {
         Right.ForceValue(leftValue / forcedValue);
      }
   }

   public class DerefOp : Op
   {
      private readonly string mReference;
      private readonly Func<string,Op> mSymbolLookup;
      
      public DerefOp(string reference, Func<string,Op> symbolLookup)
      {
         mReference = reference;
         mSymbolLookup = symbolLookup;
      }
      
      public Op Resolve() => mSymbolLookup(mReference);
      
      public override long Evaluate() => Resolve().Evaluate();

      public override void ForceValue(long value) => Resolve().ForceValue(value);

      public override string ToString()
      {
         return Resolve().ToString();
      }
   }

   public class Constant : Op
   {
      private readonly long mValue;
      
      public Constant(long value)
      {
         mValue = value;
      }
      
      public override long Evaluate() => mValue;
      
      public override void ForceValue(long value) => throw new InvalidOperationException("Cannot force the value of a constant.");
      
      public override string ToString() => mValue.ToString();
   }

   public class Me : Op
   {
      public long Value { get; private set; }
      private Me() { }   // this is true
      public static readonly Me Instance = new Me();  // I wish :)   
      public override long Evaluate() => throw new InvalidOperationException("I don't like being evaluated!");
      public override void ForceValue(long value) { Value = value; }
      public override string ToString() => "ME";
   }
}

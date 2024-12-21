using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DavidDylan.AdventOfCode2024;

public static class Day17Program
{
  private static readonly int[] SourceCode = new int[] { 2,4,1,1,7,5,1,5,4,0,0,3,5,5,3,0 };
  
  public static void Main()
  {
    const int partOneRegisterA = 64196994;

    SolvePartOne(partOneRegisterA);
  }

  public static void SolvePartOne(int registerAValue)
  {
    var computer = InitialiseComputer(registerAValue);
    
    computer.Run();
    
    Console.WriteLine(computer.Executor);
  }

  private static Computer InitialiseComputer(int registerAValue)
  {
    var computer = new Computer();
    computer.SetA(registerAValue);
    
    for (int i = 0; i < sourceCode.Length; i += 2)
    {
      computer.AppendInstruction(SourceCode[i], SourceCode[i+1]);
    }

    return computer;
  }

  public static void SolvePartTwo()
  {
    const int maxToTry = 200;
    var timer = Stopwatch.StartNew();
    for (int a = 0; a <= maxToTry; a++)
    {
      var computer = InitialiseComputer(a);

      computer.Executor.ExpectOutput(SourceCode);
    
      if (computer.Run())
      {
        Console.WriteLine("Solved in {0}", timer.Elapsed);
        Console.WriteLine("Output matched expected when initial value in register A was {0}", a);
      }
    }
    Console.WriteLine("Spent {0} looking for solutions up to {1} but found nothing.", timer.Elapsed, maxToTry);
  }
}

public class Computer
{
  private static readonly Func<int, int[], Operand> LiteralOperandFactory = (i, reg) => new LiteralOperand(i);
  private static readonly Func<int, int[], Operand> ComboOperandFactory = (i, reg) => new ComboOperand(i, reg);
  
  private static readonly Instruction[] Instructions = new Instruction[] {
    new Instruction(ComboOperandFactory, (reg, op, exec) => { reg[0] = reg[0] / (int)Math.Pow(2, op.Evaluate()); }),
    new Instruction(LiteralOperandFactory, (reg, op, exec) => { reg[1] = reg[1] ^ op.Evaluate(); }),
    new Instruction(ComboOperandFactory, (reg, op, exec) => { reg[1] = op.Evaluate() % 8; }),
    new Instruction(LiteralOperandFactory, (reg, op, exec) => { if (reg[0] != 0) { exec.InstructionPointer = op.Evaluate()/2 - 1; } }),
    new Instruction(LiteralOperandFactory, (reg, op, exec) => { reg[1] = reg[1] ^ reg[2]; }),
    new Instruction(ComboOperandFactory, (reg, op, exec) => { exec.Output(op.Evaluate() % 8); }),
    new Instruction(ComboOperandFactory, (reg, op, exec) => { reg[1] = reg[0] / (int)Math.Pow(2, op.Evaluate()); }),
    new Instruction(ComboOperandFactory, (reg, op, exec) => { reg[2] = reg[0] / (int)Math.Pow(2, op.Evaluate()); })
  };
  
  private readonly int[] _registers = new int[3];
  private readonly List<Instruction> _instructions = new List<Instruction>();
  private readonly List<Operand> _operands = new List<Operand>();
  
  public readonly Executor Executor = new Executor();
  
  public void SetA(int v)
  {
    _registers[0] = v;
  }
  
  public void SetB(int v)
  {
    _registers[1] = v;
  }
  
  public void SetC(int v)
  {
    _registers[2] = v;
  }
  
  public void AppendInstruction(int opCode, int operand)
  {
    var instruction = Instructions[opCode];
    _instructions.Add(instruction);
    _operands.Add(instruction.CreateOperand(operand, _registers));
  }
  
  public bool Run()
  {
    var isValid = true;
    while (isValid && Executor.InstructionPointer >= 0 && Executor.InstructionPointer < _instructions.Count)
    {
      _instructions[Executor.InstructionPointer].Execute(
        _registers,
        _operands[Executor.InstructionPointer],
        Executor);
      Executor.InstructionPointer++;
      isValid = Executor.IsValid();
    }
    return isValiid;
  }
}

public class Instruction
{
  private readonly Func<int, int[], Operand> _operandFactory;
  public readonly Action<int[], Operand, Executor> Execute;
  
  public Instruction(Func<int, int[], Operand> operandFactory, Action<int[], Operand, Executor> action)
  {
    _operandFactory = operandFactory;
    Execute = action;
  }
  
  public Operand CreateOperand(int operand, int[] registers)
  {
    return _operandFactory(operand, registers);
  }
}

public abstract class Operand
{
  public abstract int Evaluate();
}

public class LiteralOperand : Operand
{
  private readonly int _value;
  
  public LiteralOperand(int v)
  {
    _value = v;
  }
  
  public override int Evaluate()
  {
    return _value;
  }
}

public class ComboOperand : Operand
{
  private readonly int[] _registers;
  private readonly int _value;
  
  public ComboOperand(int v, int[] registers)
  {
    _registers = registers;
    _value = v;
  }
  
  public override int Evaluate()
  {
    return _value <= 3 ? _value : _registers[_value - 4];
  }
}

public class Executor
{
  public int InstructionPointer;
  private int[] _expectedOutput;
  
  private readonly List<int> _output = new List<int>();
  
  public void Output(int v)
  {
    _output.Add(v);
  }

  public void ExpectOutput(int[] expected)
  {
    _expectedOutput = expected;
  }

  public bool IsValid()
  {
    if (_expectedOutput == null)
      return true; // supports part one
    
    return _output.Count <= _expectedOutput.Length &&
      _expectedOutput.Take(_output.Count).SequenceEqual(_output);
  }
  
  public override string ToString()
  {
    return string.Join(",", _output.Select(o => o.ToString()));
  }
}

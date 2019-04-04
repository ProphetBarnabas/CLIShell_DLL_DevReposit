using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CLIShell.Interpreter;

namespace CLIShell
{
    namespace Interpreter
    {
        public class InterpreterParameters
        {
            /*A private method to find character indices in arguments.*/
            private int[] FindCharacterIndices(char charToFind, string inputStr)
            {
                List<int> indexList = new List<int>();
                for (int i = 0; i < inputStr.Length; i++)
                {
                    if (inputStr[i] == charToFind)
                    {
                        indexList.Add(i);
                    }
                }
                return indexList.ToArray();
            }

            /*Determines if the argument starts with: !(the glob character responsible for negation).*/
            public bool Negate { get; private set; }

            /*Determines if the argument starts with: *(the glob character that defines any character at any length).*/
            public bool FirstCharAsterisk { get; private set; }

            /*Determines if the argument ends with: *(the glob character that defines any character at any length).*/
            public bool LastCharAsterisk { get; private set; }

            /*Determines if the contains : " "(any metacharacter between double quotes are interpreted as normal characters).*/
            public bool DoubleQuote { get; private set; }

            /*The input argument, that is to be interprterd.*/
            public string InputString { get; private set; }

            /*The interpreted argument(all glob characters are removed).*/
            public string InterpretedString { get; private set; }

            /*The default constructor.*/
            public InterpreterParameters(string inputStr)
            {
                InputString = inputStr;
            }

            /*Attempts to interpret the input argument.*/
            public void GetParameters()
            {
                string dummyStr = InputString;

                if (dummyStr.Length > 1)
                {
                    /*Throwing exceptions if the argument contains invalid glob character pattern.*/
                    try
                    {
                        if (dummyStr[0] == '*' && dummyStr[1] == '!')
                        {
                            throw new InterpreterException("Unrecognizable glob character pattern! Argument cannot start with: *!");
                        }
                        if (dummyStr[dummyStr.Length - 1] == '*' && dummyStr[dummyStr.Length - 2] == '!')
                        {
                            throw new InterpreterException("Unrecognizable glob character pattern! Argument cannot end with: !*");
                        }
                        if (dummyStr.Last() == '!')
                        {
                            throw new InterpreterException("Unrecognizable glob character pattern! Argument cannot end with: !");
                        }
                        if (dummyStr.First() == '*' && dummyStr.Last() == '*')
                        {
                            throw new InterpreterException("Unrecognizable glob character pattern! First and last char asterisk coexistence occurred!");
                        }
                        if (FindCharacterIndices('\"', dummyStr).Length == 1)
                        {
                            throw new InterpreterException("Unrecognizable glob character pattern! Missing double quote pair!");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InterpreterException(ex.Message);
                    }

                    /*Interpreting glob character pattern.*/
                    try
                    {
                        if (dummyStr[0] == '!')
                        {
                            Negate = true;
                            dummyStr = dummyStr.Remove(0, 1);
                        }
                        if (dummyStr[0] == '*')
                        {
                            FirstCharAsterisk = true;
                            dummyStr = dummyStr.Remove(0, 1);
                        }
                        if (dummyStr[dummyStr.Length - 1] == '*')
                        {
                            LastCharAsterisk = true;
                            dummyStr = dummyStr.Remove(dummyStr.Length - 1, 1);
                            InterpretedString = dummyStr;
                        }
                        if (FindCharacterIndices('\"', dummyStr).Length == 2)
                        {
                            if (dummyStr[0] == '\"' && dummyStr.Last() == '\"')
                            {
                                DoubleQuote = true;
                                dummyStr = dummyStr.Replace("\"", "");
                            }
                            else
                            {
                                throw new InterpreterException("Invalid double quote location!");
                            }
                        }
                        InterpretedString = dummyStr;
                    }
                    catch (Exception ex)
                    {
                        throw new InterpreterException(ex.Message);
                    }
                }
            }
        }

        public class InterpreterResult
        {
            /*Determines if the compared string matches the interpreted argument.*/
            public bool Result { get; private set; }

            /*The string that will be compared with an interpreted argument.*/
            public string ComparedString { get; private set; }

            /*The default constructor.*/
            public InterpreterResult(string compareStr)
            {
                ComparedString = compareStr;
            }

            /*Attempts to compare the string declared in the default constructor with a specified interpreted argument.*/
            public void GetResult(InterpreterParameters intPtrParams)
            {
                if (intPtrParams.Negate && !intPtrParams.FirstCharAsterisk && !intPtrParams.LastCharAsterisk)
                {
                    if (intPtrParams.InterpretedString != ComparedString)
                    {
                        Result = true;
                    }
                }
                else if (!intPtrParams.Negate && intPtrParams.FirstCharAsterisk && !intPtrParams.LastCharAsterisk)
                {
                    if (ComparedString.EndsWith(intPtrParams.InterpretedString))
                    {
                        Result = true;
                    }
                }
                else if (!intPtrParams.Negate && !intPtrParams.FirstCharAsterisk && intPtrParams.LastCharAsterisk)
                {
                    if (ComparedString.StartsWith(intPtrParams.InterpretedString))
                    {
                        Result = true;
                    }
                }
                else if (intPtrParams.Negate && intPtrParams.FirstCharAsterisk && !intPtrParams.LastCharAsterisk)
                {
                    if (!ComparedString.EndsWith(intPtrParams.InterpretedString))
                    {
                        Result = true;
                    }
                }
                else if (intPtrParams.Negate && !intPtrParams.FirstCharAsterisk && intPtrParams.LastCharAsterisk)
                {
                    if (!ComparedString.StartsWith(intPtrParams.InterpretedString))
                    {
                        Result = true;
                    }
                }
                else if (!intPtrParams.Negate && !intPtrParams.FirstCharAsterisk && !intPtrParams.LastCharAsterisk)
                {
                    if (ComparedString == intPtrParams.InterpretedString)
                    {
                        Result = true;
                    }
                }
                else
                {
                    /*Throws exception if the interpreter fails to recognize glob character pattern.*/
                    throw new InterpreterException("Interpreter failed to recognize glob character pattern!");
                }
            }
        }

        public class InterpreterException : Exception
        {
            public InterpreterException(string message) : base(message) { }
        }
    }

    public class Argument
    {
        /*The full argument that hasn't been interpreted*/
        public string Arg;

        /*The interpreter parameters of the full argument*/
        public InterpreterParameters InterpreterParameters;

        /*Determines if the argument contains any glob characters(", *, !)*/
        public bool ContainsGlobs;
    }

    public class Command
    {
        /*The full command containing the call and all arguments.*/
        public string FullCommand;

        /*The name of the command.*/
        public string Call { get; private set; }

        /*The description of the command*/
        public string Description { get; private set; }

        /*The arguments of the command.*/
        public Argument[] Args;

        /*The required argument count.*/
        public int MinArgCount { get; private set; }

        /*The highest possible argument count.*/
        public int MaxArgCount { get; private set; }

        /*Determines if the command can handle glob characters.*/
        public bool AllowGlobs { get; private set; }

        /*The function is to be executed when the command is called by the user.*/
        public Action Function;

        /*The default constructor.*/
        public Command(string call, int minArgCount, int maxArgCount, bool allowGlobs, string description = null, string fullCommand = null, Action function = null)
        {
            Call = call;
            MinArgCount = minArgCount;
            MaxArgCount = maxArgCount;
            Description = description;
            FullCommand = fullCommand;
            Args = new Argument[MaxArgCount];
            AllowGlobs = allowGlobs;
            Function = function;
        }
    }

    public struct ArgumentManagement
    {
        public Argument GetArgument(string arg)
        {
            Argument newArg = new Argument();
            newArg.Arg = arg;
            if (newArg.Arg.Last() == ' ')
            {
                newArg.Arg = newArg.Arg.Remove(newArg.Arg.Length - 1);
            }
            if (newArg.Arg.StartsWith("!") || newArg.Arg.StartsWith("*") || newArg.Arg.EndsWith("*"))
            {
                newArg.ContainsGlobs = true;
            }
            else if (!newArg.Arg.StartsWith("!") && !newArg.Arg.StartsWith("*") && !newArg.Arg.EndsWith("*"))
            {
                newArg.ContainsGlobs = false;
            }
            newArg.InterpreterParameters = new InterpreterParameters(arg);
            newArg.InterpreterParameters.GetParameters();
            return newArg;
        }

        public Argument[] GetArguments(string[] args)
        {
            List<Argument> argList = new List<Argument>();
            for (int i = 0; i < args.Length; i++)
            {
                Argument newArg = new Argument();
                newArg.Arg = args[i];
                if (newArg.Arg.Last() == ' ')
                {
                    newArg.Arg = newArg.Arg.Remove(newArg.Arg.Length - 1);
                }
                if (newArg.Arg.StartsWith("!") || newArg.Arg.StartsWith("*") || newArg.Arg.EndsWith("*"))
                {
                    newArg.ContainsGlobs = true;
                }
                else if (!newArg.Arg.StartsWith("!") && !newArg.Arg.StartsWith("*") && !newArg.Arg.EndsWith("*"))
                {
                    newArg.ContainsGlobs = false;
                }
                newArg.InterpreterParameters = new InterpreterParameters(args[i]);
                newArg.InterpreterParameters.GetParameters();
                argList.Add(newArg);
            }

            return argList.ToArray();
        }
    }

    public class CommandManagement
    {
        /*Determines if the most recently executed command has been found.*/
        public bool CommandFound { get; private set; }

        /*Contains the call of the last executed command.*/
        public string LastExecutedCall { get; private set; }

        /*Contains all commands.*/
        public CommandPool CommandPool { get; private set; }

        /*The default constructor.*/
        public CommandManagement(CommandPool pool)
        {
            CommandPool = pool;
        }

        /*A private method to find character indices in arguments.*/
        private int[] FindCharacterIndices(char charToFind, string inputStr)
        {
            List<int> indexList = new List<int>();
            for (int i = 0; i < inputStr.Length; i++)
            {
                if (inputStr[i] == charToFind)
                {
                    indexList.Add(i);
                }
            }
            return indexList.ToArray();
        }

        /*Returns the most recently executed command if any.*/
        public Command GetLastExecuted()
        {
            if (CommandFound)
            {
                Command[] cmdArray = CommandPool.GetCommands();
                for (int i = 0; i < cmdArray.Length; i++)
                {
                    if (cmdArray[i].Call == LastExecutedCall)
                    {
                        return cmdArray[i];
                    }
                }
            }
            throw new CommandException("Command could not be found!");
        }

        /*A public method to find a command called by the user in the command pool specified in the constructor.*/
        public Command GetCommandFromPool(string inputStr)
        {
            Command[] cmdArray = CommandPool.GetCommands();
            CommandFound = false;
            string tempArgStr = null;
            string[] argSplit = null;
            List<string> orderedArgs = null;
            List<Argument> argList = null;
            Argument newArg = null;

            /*Searching array for the command by "Call" property*/
            for (int i = 0; i < cmdArray.Length; i++)
            {
                /*Gathering arguments if the command have been found*/
                if (inputStr.StartsWith(cmdArray[i].Call + " ") || inputStr.StartsWith(cmdArray[i].Call.ToUpper() + " ") || inputStr.StartsWith(cmdArray[i].Call.ToLower() + " ") || inputStr == cmdArray[i].Call || inputStr == cmdArray[i].Call.ToUpper() || inputStr == cmdArray[i].Call.ToLower())
                {
                    CommandFound = true;
                    if (cmdArray[i].MaxArgCount > 0)
                    {
                        /*Removing the command Call from the input*/
                        inputStr = inputStr.Remove(0, cmdArray[i].Call.Length);
                        if (inputStr.StartsWith(" "))
                        {
                            inputStr.Remove(0, 1);
                        }

                        /*Separating arguments considering the command's glob character support*/
                        orderedArgs = new List<string>();
                        argSplit = inputStr.Split(' ');
                        if (cmdArray[i].AllowGlobs)
                        {
                            for (int j = 0; j < argSplit.Length; j++)
                            {
                                if (argSplit[j].Length > 1)
                                {
                                    if (argSplit[j][0] == '\"' || (argSplit[j][1] == '\"' && (argSplit[j][0] == '!' || argSplit[j][0] == '*')))
                                    {
                                        for (int k = j; k < argSplit.Length; k++)
                                        {
                                            tempArgStr += argSplit[k];
                                            if (argSplit[j][0] == '\"' || argSplit[j][1] == '\"' && (argSplit[k].Last() != '\"' || (argSplit[k][argSplit[k].Length - 2] != '\"' && argSplit[k].Last() != '*')))
                                            {
                                                tempArgStr += " ";
                                            }
                                            if (argSplit[k].Last() == '\"' || (argSplit[k][argSplit[k].Length - 2] == '\"' && argSplit[k].Last() == '*'))
                                            {
                                                orderedArgs.Add(tempArgStr);
                                                tempArgStr = null;
                                                j = k;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        orderedArgs.Add(argSplit[j]);
                                    }
                                }
                                else
                                {
                                    orderedArgs.Add(argSplit[j]);
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < argSplit.Length; j++)
                            {
                                if (argSplit[j].Length > 1)
                                {
                                    if (argSplit[j][0] == '\"')
                                    {
                                        for (int k = j; k < argSplit.Length; k++)
                                        {
                                            tempArgStr += argSplit[k];
                                            if (argSplit[k].Last() != '\"')
                                            {
                                                tempArgStr += " ";
                                            }
                                            if (argSplit[k].Last() == '\"')
                                            {
                                                tempArgStr.Remove(tempArgStr.Length - 1);
                                                orderedArgs.Add(tempArgStr);
                                                tempArgStr = null;
                                                j = k;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        orderedArgs.Add(argSplit[j]);
                                    }
                                }
                                else
                                {
                                    orderedArgs.Add(argSplit[j]);
                                }
                            }
                        }
                        argList = new List<Argument>();

                        /*Assembling arguments*/
                        for (int j = 0; j < orderedArgs.Count(); j++)
                        {
                            if (orderedArgs[j].Length > 0)
                            {
                                newArg = new Argument();
                                newArg.Arg = orderedArgs[j];
                                if (newArg.Arg.Last() == ' ')
                                {
                                    newArg.Arg = newArg.Arg.Remove(newArg.Arg.Length - 1);
                                }
                                if (newArg.Arg.StartsWith("!") || newArg.Arg.StartsWith("*") || newArg.Arg.EndsWith("*"))
                                {
                                    newArg.ContainsGlobs = true;
                                }
                                else if (!newArg.Arg.StartsWith("!") && !newArg.Arg.StartsWith("*") && !newArg.Arg.EndsWith("*"))
                                {
                                    newArg.ContainsGlobs = false;
                                }
                                newArg.InterpreterParameters = new InterpreterParameters(newArg.Arg);
                                newArg.InterpreterParameters.GetParameters();
                                argList.Add(newArg);
                            }
                        }
                        /*Handling argument count limit overrun*/
                        if (cmdArray[i].MaxArgCount < argList.Count())
                        {
                            throw new CommandException("Too many argument(s)! Max arg count: " + cmdArray[i].MaxArgCount);
                        }
                        else if (cmdArray[i].MinArgCount > argList.Count())
                        {
                            throw new CommandException("Missing argument(s)! Min arg count: " + cmdArray[i].MinArgCount);
                        }
                        else
                        {
                            cmdArray[i].Args = argList.ToArray();
                            return cmdArray[i];
                        }
                    }
                    else
                    {
                        return cmdArray[i];
                    }
                }
            }
            /*If the command cannot be identified by the "Call" property, the method returns an empty command*/
            return null;
        }

        /*Determines if the specified command is called by the user and if it is, returns the command, otherwise returns an empty command.*/
        public Command GetCommand(Command cmd, string inputStr)
        {
            CommandFound = false;
            string tempArgStr = null;
            string[] argSplit = null;
            List<string> orderedArgs = null;
            List<Argument> argList = null;
            Argument newArg = null;

            /*Searching array for the command by "Call" property*/
            /*Gathering arguments if the command have been found*/
            if (inputStr.StartsWith(cmd.Call + " ") || inputStr.StartsWith(cmd.Call.ToUpper() + " ") || inputStr.StartsWith(cmd.Call.ToLower() + " ") || inputStr == cmd.Call || inputStr == cmd.Call.ToUpper() || inputStr == cmd.Call.ToLower())
            {
                CommandFound = true;
                if (cmd.MaxArgCount > 0)
                {
                    /*Removing the command Call from the input*/
                    inputStr = inputStr.Remove(0, cmd.Call.Length);
                    if (inputStr.StartsWith(" "))
                    {
                        inputStr.Remove(0, 1);
                    }

                    /*Separating arguments considering the command's glob character support*/
                    orderedArgs = new List<string>();
                    argSplit = inputStr.Split(' ');
                    if (cmd.AllowGlobs)
                    {
                        for (int j = 0; j < argSplit.Length; j++)
                        {
                            if (argSplit[j].Length > 1)
                            {
                                if (argSplit[j][0] == '\"' || (argSplit[j][1] == '\"' && (argSplit[j][0] == '!' || argSplit[j][0] == '*')))
                                {
                                    for (int k = j; k < argSplit.Length; k++)
                                    {
                                        tempArgStr += argSplit[k];
                                        if (argSplit[j][0] == '\"' || argSplit[j][1] == '\"' && (argSplit[k].Last() != '\"' || (argSplit[k][argSplit[k].Length - 2] != '\"' && argSplit[k].Last() != '*')))
                                        {
                                            tempArgStr += " ";
                                        }
                                        if (argSplit[k].Last() == '\"' || (argSplit[k][argSplit[k].Length - 2] == '\"' && argSplit[k].Last() == '*'))
                                        {
                                            orderedArgs.Add(tempArgStr);
                                            tempArgStr = null;
                                            j = k;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    orderedArgs.Add(argSplit[j]);
                                }
                            }
                            else
                            {
                                orderedArgs.Add(argSplit[j]);
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < argSplit.Length; j++)
                        {
                            if (argSplit[j].Length > 1)
                            {
                                if (argSplit[j][0] == '\"')
                                {
                                    for (int k = j; k < argSplit.Length; k++)
                                    {
                                        tempArgStr += argSplit[k];
                                        if (argSplit[k].Last() != '\"')
                                        {
                                            tempArgStr += " ";
                                        }
                                        if (argSplit[k].Last() == '\"')
                                        {
                                            orderedArgs.Add(tempArgStr);
                                            tempArgStr = null;
                                            j = k;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    orderedArgs.Add(argSplit[j]);
                                }
                            }
                            else
                            {
                                orderedArgs.Add(argSplit[j]);
                            }
                        }
                    }
                    argList = new List<Argument>();

                    /*Assembling arguments*/
                    for (int j = 0; j < orderedArgs.Count(); j++)
                    {
                        if (orderedArgs[j].Length > 0)
                        {
                            newArg = new Argument();
                            newArg.Arg = orderedArgs[j];
                            if (newArg.Arg.Last() == ' ')
                            {
                                newArg.Arg = newArg.Arg.Remove(newArg.Arg.Length - 1);
                            }
                            if (newArg.Arg.StartsWith("!") || newArg.Arg.StartsWith("*") || newArg.Arg.EndsWith("*"))
                            {
                                newArg.ContainsGlobs = true;
                            }
                            else if (!newArg.Arg.StartsWith("!") && !newArg.Arg.StartsWith("*") && !newArg.Arg.EndsWith("*"))
                            {
                                newArg.ContainsGlobs = false;
                            }
                            newArg.InterpreterParameters = new InterpreterParameters(newArg.Arg);
                            newArg.InterpreterParameters.GetParameters();
                            argList.Add(newArg);               
                        }
                    }
                    /*Handling argument count limit overrun*/
                    if (cmd.MaxArgCount < argList.Count())
                    {
                        throw new CommandException("Too many argument(s)! Max arg count: " + cmd.MaxArgCount);
                    }
                    else if (cmd.MinArgCount > argList.Count())
                    {
                        throw new CommandException("Missing argument(s)! Min arg count: " + cmd.MinArgCount);
                    }
                    else
                    {
                        cmd.Args = argList.ToArray();
                        return cmd;
                    }
                }
                else
                {
                    return cmd;
                }
            }
            /*If the command cannot be identified by the "Call" property, the method returns an empty command*/
            return null;
        }

        /*Attempts to execute the specified command.*/
        public void ExecuteCommand(Command cmdToExecute)
        {
            if (cmdToExecute.Call == null)
            {
                throw new CommandException("Command to execute returned null!");
            }
            else if (cmdToExecute.Function != null)
            {
                cmdToExecute.Function();
            }
            else
            {
                throw new CommandException("A command function is not assigned!");
            }
        }
    }

    public class CommandPool
    {
        private List<Command> CMD_LST = new List<Command>();

        public void Add(Command cmdToAdd, Action cmdFunc = null)
        {
            for (int i = 0; i < CMD_LST.Count(); i++)
            {
                if (CMD_LST[i].Call == cmdToAdd.Call)
                {
                    throw new Exception("A command with the same call(name) already exists in the pool!");
                }
            }
            if (cmdFunc != null)
            {
                cmdToAdd.Function = cmdFunc;
            }
            CMD_LST.Add(cmdToAdd);
        }

        public void Remove(string cmdCall)
        {
            bool found = false;
            for (int i = 0; i < CMD_LST.Count(); i++)
            {
                if (CMD_LST[i].Call == cmdCall)
                {
                    found = true;
                    CMD_LST.RemoveAt(i);
                    break;
                }
            }
            if (!found)
            {
                throw new Exception("Command not found!");
            }    
        }

        public void AlterCommand(string cmdCall, int minArgCount, int maxArgCount, bool allowGlobs, string newCall = null, Action function = null, string description = null)
        {
            Command cmdToAlter = null;
            for (int j = 0; j < CMD_LST.Count(); j++)
            {
                if (CMD_LST[j].Call == newCall)
                {
                    throw new Exception("A command with the same call(name) already exists in the pool!");
                }
            }
            for (int i = 0; i < CMD_LST.Count(); i++)
            {
                if (CMD_LST[i].Call == cmdCall)
                {
                    cmdToAlter = CMD_LST[i];
                    string CALL = newCall;
                    int MIN_ARG_CNT = minArgCount;
                    int MAX_ARG_CNT = maxArgCount;
                    bool ALLOW_GLOB = allowGlobs;
                    string DESC = description;
                    Action FUNC = function;
                    if (newCall == null)
                    {
                        CALL = cmdToAlter.Call;
                    }
                    if (description == null)
                    {
                        DESC = cmdToAlter.Description;
                    }
                    if (function == null)
                    {
                        FUNC = cmdToAlter.Function;
                    }
                    CMD_LST[i] = new Command(CALL, MIN_ARG_CNT, MAX_ARG_CNT, ALLOW_GLOB, DESC, null, FUNC);
                    break;
                }
            }
            if (cmdToAlter == null)
            {
                throw new Exception("Command not found!");
            }
        }

        public Command[] GetCommands()
        {
            return CMD_LST.ToArray();
        }
    }

    public class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
    }
}

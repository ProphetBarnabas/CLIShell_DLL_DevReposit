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

            /*Determines if the argument starts and ends with: * *(the glob character that defines any character at any length).*/
            public bool BothEndAsterisk { get; private set; }

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
                        if (FirstCharAsterisk && LastCharAsterisk)
                        {
                            BothEndAsterisk = true;
                            FirstCharAsterisk = false;
                            LastCharAsterisk = false;
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
                else if (!intPtrParams.Negate && !intPtrParams.FirstCharAsterisk && !intPtrParams.LastCharAsterisk && !intPtrParams.BothEndAsterisk)
                {
                    if (ComparedString == intPtrParams.InterpretedString)
                    {
                        Result = true;
                    }
                }
                else if (intPtrParams.Negate && intPtrParams.BothEndAsterisk)
                {
                    if (!ComparedString.Contains(intPtrParams.InterpretedString))
                    {
                        Result = true;
                    }
                }
                else if (!intPtrParams.Negate && intPtrParams.BothEndAsterisk)
                {
                    if (ComparedString.Contains(intPtrParams.InterpretedString))
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
        /*Used in GetCommand() and GetCommandFromPool()*/
        private ArgumentManagement ARG_MGMT;

        /*GetLastExecuted() method return value*/
        private Command LAST_CMD;

        /*Contains all commands.*/
        public CommandPool CommandPool { get; private set; }

        /*The default constructor.*/
        public CommandManagement(CommandPool pool)
        {
            CommandPool = pool;
        }

        /*Returns the most recently executed command if any.*/
        public Command GetLastExecuted()
        {
            if (LAST_CMD != null)
            {
                return LAST_CMD;
            }
            throw new CommandException("Command could not be found!");
        }

        /*A public method to find a command called by the user in the command pool specified in the constructor.*/
        public Command GetCommandFromPool(string inputStr)
        {
            ARG_MGMT = new ArgumentManagement();
            List<Command> cmdArray = CommandPool.GetCommands().ToList();
            List<string> argSplit = null;
            List<Argument> argList = null;

            /*Searching array for the command by "Call" property*/
            Command cmdFound = cmdArray.Find(x => inputStr.StartsWith(x.Call) || inputStr.StartsWith(x.Call.ToUpper()) || inputStr.StartsWith(x.Call.ToLower()));

            /*If the command cannot be identified by the "Call" property, the method returns null*/
            if (cmdFound == null)
            {
                return null;
            }
            if (cmdFound.MaxArgCount > 0)
            {
                /*Removing the command Call from the input*/
                inputStr = inputStr.Remove(0, cmdFound.Call.Length);
                if (inputStr.StartsWith(" "))
                {
                    inputStr.Remove(0, 1);
                }

                /*Separating arguments considering the command's glob character support*/
                argSplit = inputStr.Split(' ').ToList();
                if (cmdFound.AllowGlobs)
                {
                    for (int i = 0; i < argSplit.Count; i++)
                    {
                        if (argSplit[i].StartsWith("!*\"") || argSplit[i].StartsWith("!\"") || argSplit[i].StartsWith("*\"") || argSplit[i].StartsWith("\""))
                        {
                            for (int j = i; j < argSplit.Count; j++)
                            {
                                if (argSplit[j] != argSplit[i])
                                {
                                    argSplit[i] += " " + argSplit[j];
                                    if (argSplit[j].EndsWith("\"") || argSplit[j].EndsWith("\"*"))
                                    {
                                        argSplit[j] = "";
                                        break;
                                    }
                                    else
                                    {
                                        argSplit[j] = "";
                                    }
                                }
                            }
                        }
                    }

                    argSplit.RemoveAll(x => x == "");
                }
                else
                {
                    for (int i = 0; i < argSplit.Count; i++)
                    {
                        if (argSplit[i].StartsWith("\""))
                        {
                            for (int j = i; j < argSplit.Count; j++)
                            {
                                if (argSplit[j] != argSplit[i])
                                {
                                    argSplit[i] += " " + argSplit[j];
                                    if (argSplit[j].EndsWith("\""))
                                    {
                                        argSplit[j] = "";
                                        break;
                                    }
                                    else
                                    {
                                        argSplit[j] = "";
                                    }
                                }
                            }
                        }
                    }

                    argSplit.RemoveAll(x => x == "");
                }

                argList = new List<Argument>();

                /*Assembling arguments*/
                argList = ARG_MGMT.GetArguments(argSplit.ToArray()).ToList();

                /*Handling argument count limit overrun*/
                if (cmdFound.MaxArgCount < argList.Count())
                {
                    throw new CommandException("Too many argument(s)! Max arg count: " + cmdFound.MaxArgCount);
                }
                else if (cmdFound.MinArgCount > argList.Count())
                {
                    throw new CommandException("Missing argument(s)! Min arg count: " + cmdFound.MinArgCount);
                }
                else
                {
                    cmdFound.Args = argList.ToArray();
                    LAST_CMD = cmdFound;
                    return cmdFound;
                }
            }
            else
            {
                return cmdFound;
            }

            /*Gathering arguments if the command has been found*/
        }

        /*Determines if the specified command is called by the user and if it is, returns the command, otherwise returns an empty command.*/
        public Command GetCommand(Command cmd, string inputStr)
        {
            ARG_MGMT = new ArgumentManagement();
            List<string> argSplit = null;
            List<Argument> argList = null;

            /*Gathering arguments if the command have been found*/
            if (inputStr.StartsWith(cmd.Call + " ") || inputStr.StartsWith(cmd.Call.ToUpper() + " ") || inputStr.StartsWith(cmd.Call.ToLower() + " ") || inputStr == cmd.Call || inputStr == cmd.Call.ToUpper() || inputStr == cmd.Call.ToLower())
            {
                if (cmd.MaxArgCount > 0)
                {
                    /*Removing the command Call from the input*/
                    inputStr = inputStr.Remove(0, cmd.Call.Length);
                    if (inputStr.StartsWith(" "))
                    {
                        inputStr.Remove(0, 1);
                    }

                    /*Separating arguments considering the command's glob character support*/
                    argSplit = inputStr.Split(' ').ToList();
                    if (cmd.AllowGlobs)
                    {
                        for (int i = 0; i < argSplit.Count; i++)
                        {
                            if (argSplit[i].StartsWith("!*\"") || argSplit[i].StartsWith("!\"") || argSplit[i].StartsWith("*\"") || argSplit[i].StartsWith("\""))
                            {
                                for (int j = i; j < argSplit.Count; j++)
                                {
                                    if (argSplit[j] != argSplit[i])
                                    {
                                        argSplit[i] += " " + argSplit[j];
                                        if (argSplit[j].EndsWith("\"") || argSplit[j].EndsWith("\"*"))
                                        {
                                            argSplit[j] = "";
                                            break;
                                        }
                                        else
                                        {
                                            argSplit[j] = "";
                                        }
                                    }
                                }
                            }
                        }

                        argSplit.RemoveAll(x => x == "");
                    }
                    else
                    {
                        for (int i = 0; i < argSplit.Count; i++)
                        {
                            if (argSplit[i].StartsWith("\""))
                            {
                                for (int j = i; j < argSplit.Count; j++)
                                {
                                    if (argSplit[j] != argSplit[i])
                                    {
                                        argSplit[i] += " " + argSplit[j];
                                        if (argSplit[j].EndsWith("\""))
                                        {
                                            argSplit[j] = "";
                                            break;
                                        }
                                        else
                                        {
                                            argSplit[j] = "";
                                        }
                                    }
                                }
                            }
                        }

                        argSplit.RemoveAll(x => x == "");
                    }
                    argList = new List<Argument>();

                    /*Assembling arguments*/
                    argList = ARG_MGMT.GetArguments(argSplit.ToArray()).ToList();

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
                        LAST_CMD = cmd;
                        return cmd;
                    }
                }
                else
                {
                    return cmd;
                }
            }
            /*If the command cannot be identified by the "Call" property, the method returns null*/
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
            if (CMD_LST.Find(x => x.Call == cmdToAdd.Call) != null)
            {
                throw new Exception("A command with the same call(name) already exists in the pool!");
            }
            CMD_LST.Add(cmdToAdd);

            if (cmdFunc != null)
            {
                cmdToAdd.Function = cmdFunc;
            }
        }

        public void Remove(string cmdCall)
        {
            try
            {
                CMD_LST.RemoveAt(CMD_LST.FindIndex(x => x.Call == cmdCall));
            }
            catch (Exception)
            {
                throw new Exception("Failed to remove command!");
            }
        }

        public void AlterCommand(string cmdCall, int minArgCount, int maxArgCount, bool allowGlobs, string newCall = null, Action function = null, string description = null)
        {
            string CALL = "";
            int MIN_ARG_CNT = minArgCount;
            int MAX_ARG_CNT = maxArgCount;
            string DESC = "";
            bool ALLOW_GLOB = allowGlobs;
            Action FUNC = null;
            Command cmdToAlter = CMD_LST.Find(x => x.Call == cmdCall);
            if (cmdToAlter == null)
            {
                throw new Exception("Command not found!");
            }
            if (newCall != null && newCall != "")
            {
                CALL = newCall;
                if (CMD_LST.Find(x => x.Call == CALL) != null)
                {
                    throw new Exception("A command with the same name already exists in the pool!");
                }
            }
            if (MIN_ARG_CNT > MAX_ARG_CNT)
            {
                throw new Exception("minArgCount must be less than or equal to maxArgCount");
            }
            if (DESC != null)
            {
                DESC = description;
            }
            if (FUNC != null)
            {
                FUNC = function;
            }
            cmdToAlter = new Command(CALL, MIN_ARG_CNT, MAX_ARG_CNT, ALLOW_GLOB, DESC, null, FUNC);
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
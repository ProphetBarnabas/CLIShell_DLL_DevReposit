using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIShell
{
    public enum ExecutionLevel { User, Administrator };

    public enum CLIMode { Default, Regedit, Text, Any };

    public class Command
    {
        public string Call { get; }

        public CommandArgumentEntry InputArgumentEntry { get; private set; }

        public bool AllowGlobs { get; }

        public ArgumentTable ArgumentTable { get; }

        public ExecutionLevel ExecutionLevel { get; }

        public CLIMode CLIMode { get; }

        public string Description { get; private set; }

        public Func<string> Function { get; private set; }

        public Func<Task<string>> AsyncFunction { get; private set; }

        public Command(string call, ArgumentTable argTable, bool allowGlobs, string description, ExecutionLevel execLevel, CLIMode mode)
        {
            Call = call;
            ArgumentTable = argTable;
            AllowGlobs = allowGlobs;
            ExecutionLevel = execLevel;
            Description = call + " - " + description;
            if (argTable != null)
            {
                Description += "\nSyntax options: \n";
                argTable.ValidSequences.ForEach((x) =>
                {
                    Description += "\t\"" + x.Pattern + "\"" + (x.Description != "" && x.Description != null ? " : \"" + x.Description + " \"" : "") + (x.OrderSensitive ? " (order sensitive)" : "") + "\n";
                });
            }
            CLIMode = mode;
        }

        public Command(string call, ArgumentTable argTable, bool allowGlobs, ExecutionLevel execLevel, CLIMode mode)
        {
            Call = call;
            ArgumentTable = argTable;
            AllowGlobs = allowGlobs;
            ExecutionLevel = execLevel;
            CLIMode = mode;
        }

        public void SetFunction(Func<string> newFunction)
        {
            Function = newFunction;
        }

        public void SetAsyncFunction(Func<Task<string>> newAsyncFunction)
        {
            AsyncFunction = newAsyncFunction;
        }

        public void SetInputArguments(CommandArgumentEntry args)
        {
            InputArgumentEntry = args;
        }
    }

    public class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
    }
}

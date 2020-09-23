using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CLIShell
{
    public class CommandManagement
    {
        public ExecutionLevel ExecutionLevel { get; }

        private Command LAST;

        public CommandManagement(ExecutionLevel execLevel)
        {
            ExecutionLevel = execLevel;
        }

        public Command GetCommand(string input, CommandPool pool)
        {
            Command cmd = pool.Find(x => x.Call.ToLower() == input.Split(' ')[0].ToLower());
            if (cmd == null)
            {
                throw new Exception("Command not found!");
            }
            if (cmd.ArgumentTable == null)
            {
                return cmd;
            }
            input = input == cmd.Call.ToLower() ? "" : input.Remove(0, cmd.Call.Length + 1);
            CommandArgumentEntry args = cmd.ArgumentTable.FindMatchingSequence(input, cmd.AllowGlobs);
            cmd.SetInputArguments(args);
            return cmd;
        }

        public string ExecuteCommand(Command cmd)
        {
            if (cmd.ExecutionLevel == ExecutionLevel.Administrator && ExecutionLevel == ExecutionLevel.User)
            {
                throw new UnauthorizedAccessException("This command requires elevated privileges!");
            }
            else
            {
                LAST = cmd;
                return cmd.Function();
            }
        }

        public async Task<string> ExecuteAsyncCommand(Command cmd)
        {
            if (cmd.ExecutionLevel == ExecutionLevel.Administrator && ExecutionLevel == ExecutionLevel.User)
            {
                throw new UnauthorizedAccessException("This command requires elevated privileges!");
            }
            else
            {
                LAST = cmd;
                return await cmd.AsyncFunction();
            }
        }

        public Command GetLastExecuted()
        {
            return LAST;
        }
    }
}

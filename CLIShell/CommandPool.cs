using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CLIShell
{
    public class CommandPool
    {
        private List<Command> POOL;

        public List<Command> GetPool()
        {
            return POOL;
        }

        public void ClearPool() => POOL.Clear();

        public void Add(Command newCmd) 
        {
            if (!POOL.Exists(x => x.Call == newCmd.Call))
            {
                POOL.Add(newCmd);
            }
        }

        public void AddRange(params Command[] cmdRange) 
        {
            for (int i = 0; i < cmdRange.Length; i++)
            {
                if (!POOL.Exists(x => x.Call == cmdRange[i].Call))
                {
                    POOL.Add(cmdRange[i]);
                }
            }
        } 

        public void Remove(Predicate<Command> cmdToRemove) => POOL.Remove(POOL.Find(cmdToRemove));

        public void RemoveAt(int index) => POOL.RemoveAt(index);

        public void RemoveAll(Predicate<Command> cmdsToRemove) => POOL.RemoveAll(cmdsToRemove);

        public void RemoveRange(int index, int count) => POOL.RemoveRange(index, count);

        public void AlterFunction(Func<string> newFunction, Predicate<Command> cmdToAlter)
        {
            if (POOL.Exists(cmdToAlter))
            {
                POOL[POOL.FindIndex(cmdToAlter)].SetFunction(newFunction);
            }
            else
            {
                throw new CommandPoolException("Command not found!");
            }
        }

        public Command Find(Predicate<Command> cmdToFind)
        {
            return POOL.Find(cmdToFind);
        }

        public List<Command> FindAll(Predicate<Command> predicate)
        {
            return POOL.FindAll(predicate);
        }

        private string LOAD_CMD(string path)
        {
            try
            {
                Assembly dll = Assembly.LoadFile(Path.GetFullPath(path));
                dynamic inst = Activator.CreateInstance(dll.GetType(Path.GetFileNameWithoutExtension(path) + ".Main"));
                POOL.Add((Command)inst.GetCommand());
                return "";
            }
            catch (Exception)
            {
                return "Failed to load command: " + Path.GetFileNameWithoutExtension(path) + "\n";
            }
        }

        public CommandPool(params Command[] commands)
        {
            POOL = new List<Command>();
            if (commands != null)
            {
                POOL.AddRange(commands);
            }
        }

        public CommandPool(string folderPath)
        {
            string error_str = string.Empty;
            POOL = new List<Command>();
            string[] files = Directory.GetFiles(folderPath, "*.dll");
            for (int i = 0; i < files.Length; i++)
            {
                error_str += LOAD_CMD(files[i]);
            }
            if (error_str.Length != 0)
            {
                IOInteractLayer.StandardOutput(null, error_str);
            }
        }
    }

    public class CommandPoolException : Exception
    {
        public CommandPoolException(string message) : base(message) { }
    }
}

// Error handling needs testing

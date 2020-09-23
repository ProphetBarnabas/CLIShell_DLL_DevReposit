using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CLIShell
{
    public class ArgumentTable
    { 
        public List<CommandArgumentEntry> ValidSequences { get; private set; }

        public ArgumentTable()
        {
            ValidSequences = new List<CommandArgumentEntry>();
        }

        public ArgumentTable(List<CommandArgumentEntry> validSequences)
        {
            ValidSequences = validSequences;
        }

        public void Add(CommandArgumentEntry newSequence)
        {
            if (!ValidSequences.Contains(newSequence))
            {
                ValidSequences.Add(newSequence);
            }
        }

        public void Remove(CommandArgumentEntry sequenceToRemove)
        {
            ValidSequences.Remove(sequenceToRemove);
        }

        public void RemoveAll(Predicate<CommandArgumentEntry> predicate)
        {
            ValidSequences.RemoveAll(predicate);
        }

        private class ARGPROPS
        {
            public string CALL;

            public string VALUE;
        }

        public CommandArgumentEntry FindMatchingSequence(string input, bool allowGlobs)
        {
            if (input != null && input != string.Empty)
            {
                input = input.TrimEnd(' ');
                CommandArgumentEntry result = null;
                string[] argSplit = input.Split(' ');
                List<string> stitchedArgs = new List<string>();
                if (allowGlobs)
                {
                    for (int i = 0; i < argSplit.Length; i++)
                    {
                        if (stitchedArgs.Count > 0 && (stitchedArgs.Last().StartsWith("\"") || stitchedArgs.Last().StartsWith("*\"") || stitchedArgs.Last().StartsWith("!*\"") || stitchedArgs.Last().StartsWith("!\"")) && !stitchedArgs.Last().EndsWith("\"") && !stitchedArgs.Last().EndsWith("\"*"))
                        {
                            stitchedArgs[stitchedArgs.Count - 1] += " " + argSplit[i];
                        }
                        else
                        {
                            stitchedArgs.Add(argSplit[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < argSplit.Length; i++)
                    {
                        if (stitchedArgs.Count > 0 && stitchedArgs.Last().StartsWith("\"") && !stitchedArgs.Last().EndsWith("\""))
                        {
                            stitchedArgs[stitchedArgs.Count - 1] += " " + argSplit[i];
                        }
                        else
                        {
                            stitchedArgs.Add(argSplit[i]);
                        }
                    }
                    for (int i = 0; i < stitchedArgs.Count; i++)
                    {
                        stitchedArgs[i] = stitchedArgs[i].Replace("\"", "");
                    }
                }

                string compareStr = "";
                string patternStr = "";
                List<ARGPROPS> props = new List<ARGPROPS>();
                for (int i = 0; i < ValidSequences.Count; i++)
                {
                    props = new List<ARGPROPS>();
                    compareStr = "";
                    patternStr = "";
                    ValidSequences[i].Arguments.ForEach((y) =>
                    {
                        compareStr += y.Call == "" ? "sval " : y.Call + " ";
                        compareStr += y.Type != typeof(object) && y.Call != "" ? "val " : "";
                    });
                    List<string> compareSplit = compareStr.TrimEnd(' ').Split(' ').ToList();
                    List<string> args = stitchedArgs;
                    args.ForEach((y) =>
                    {
                        patternStr += y.StartsWith("-") && !Regex.IsMatch(y.Remove(0, 1), "^[0-9]*$") ? y + " " : patternStr.EndsWith("val ") || patternStr == "" ? "sval " : "val ";
                    });
                    List<string> patternSplit = patternStr.TrimEnd(' ').Split(' ').ToList();
                    if (args.Count == compareSplit.Count)
                    {
                        bool matchFound = true;
                        for (int j = 0; j < compareSplit.Count; j++)
                        {
                            if (ValidSequences[i].OrderSensitive)
                            {
                                if (compareSplit[j] != patternSplit[j])
                                {
                                    matchFound = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (!patternSplit.Contains(compareSplit[j]) && compareSplit[j] != "val" && compareSplit[j] != "sval")
                                {
                                    matchFound = false;
                                    break;
                                }
                            }
                        }
                        if (matchFound)
                        {
                            for (int j = 0; j < compareSplit.Count; j++)
                            {
                                if (ValidSequences[i].OrderSensitive)
                                {
                                    switch (compareSplit[j])
                                    {
                                        case "val":
                                            props.Last().VALUE = args[j];
                                            break;
                                        case "sval":
                                            props.Add(new ARGPROPS() { VALUE = args[j], CALL = ""});
                                            break;
                                        default:
                                            props.Add(new ARGPROPS() { CALL = args[j] });
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (patternSplit[j])
                                    {
                                        case "val":
                                            props.Last().VALUE = args[j];
                                            break;
                                        case "sval":
                                            props.Add(new ARGPROPS() { VALUE = args[j], CALL = ""});
                                            break;
                                        default:
                                            props.Add(new ARGPROPS() { CALL = args[j] });
                                            break;
                                    }
                                }
                            }
                            result = ValidSequences[i];
                            break;
                        }
                    }
                }
                if (result != null)
                {
                    result.Arguments.ForEach((y) =>
                    {
                        y.SetValue(null);
                    });
                    for (int j = 0; j < props.Count; j++)
                    {
                        if (result.Arguments.Exists(y => y.Call == props[j].CALL) && result.Arguments.Find(y => y.Call == props[j].CALL).Type == typeof(bool) && props[j].VALUE == null)
                        {
                            result.Arguments.Find(y => y.Call == props[j].CALL).SetValue(false);
                            continue;
                        }
                        result.Arguments.Find(y => y.Call == props[j].CALL && y.Value == null).SetValue(props[j].VALUE);
                    }
                    return result;
                }
                else
                {
                    throw new Exception("Invalid arguments!");
                }
            }
            else
            {
                return new CommandArgumentEntry();
            }
        }
    }

    public class CommandArgumentEntry
    { 
        public bool OrderSensitive { get; }

        public string Pattern { get; }

        public string Description { get; }

        public List<CommandArgument> Arguments { get; }

        private Type GET_TYPE(string input)
        {
            switch (input)
            {
                case "string":
                    return typeof(string);
                case "int":
                    return typeof(int);
                case "boolean":
                    return typeof(bool);
                case "double":
                    return typeof(double);
                case "float":
                    return typeof(float);
                case "long":
                    return typeof(long);
                case "ulong":
                    return typeof(ulong);
                case "uint":
                    return typeof(uint);
                default:
                    throw new Exception("Unknown type: " + input);
            }
        }

        public CommandArgumentEntry(string pattern, bool orderSensitive)
        {
            Pattern = pattern;
            OrderSensitive = orderSensitive;
            Arguments = new List<CommandArgument>();
            string[] args = pattern.Split(' ');
            string[] current;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("="))
                {
                    current = args[i].Split('=');
                    Arguments.Add(new CommandArgument(GET_TYPE(current[1].Replace("[", "").Replace("]", "")), current[0]));
                }
                else
                {
                    if (args[i].StartsWith("[") && args[i].EndsWith("]"))
                    {
                        Arguments.Add(new CommandArgument(GET_TYPE(args[i].Replace("[", "").Replace("]", "")), ""));
                    }
                    else
                    {
                        Arguments.Add(new CommandArgument(typeof(object), args[i]));
                    }
                }
            }
        }

        public CommandArgumentEntry(string pattern, bool orderSensitive, string description)
        {
            Description = description;
            Pattern = pattern;
            OrderSensitive = orderSensitive;
            Arguments = new List<CommandArgument>();
            string[] args = pattern.Split(' ');
            string[] current;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("="))
                {
                    current = args[i].Split('=');
                    Arguments.Add(new CommandArgument(GET_TYPE(current[1].Replace("[", "").Replace("]", "")), current[0]));
                }
                else
                {
                    if (args[i].StartsWith("[") && args[i].EndsWith("]"))
                    {
                        Arguments.Add(new CommandArgument(GET_TYPE(args[i].Replace("[", "").Replace("]", "")), ""));
                    }
                    else
                    {
                        Arguments.Add(new CommandArgument(typeof(object), args[i]));
                    }
                }
            }
        }

        public CommandArgumentEntry()
        {
            Description = "";
            OrderSensitive = false;
            Pattern = "";
            Arguments = new List<CommandArgument>();
        }
    }
}

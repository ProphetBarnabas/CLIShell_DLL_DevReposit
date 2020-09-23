using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CLIShell
{
    public static class EnvironmentVariables
    {
        private static List<EnvironmentVariable> ENV_VARS = new List<EnvironmentVariable>();

        public static void Add(EnvironmentVariable newVar)
        {
            if (ENV_VARS.Exists(x => x.Name == newVar.Name))
            {
                throw new EnvironmentVariableException("A variable with the given name already exists: " + newVar.Name);
            }
            ENV_VARS.Add(newVar);
        }

        public static void Remove(EnvironmentVariable varToRemove)
        {
            if (varToRemove.VarType == VariableType.Runtime)
            {
                throw new EnvironmentVariableException("Can't remove runtime variable: " + varToRemove.Name);
            }
            if (!ENV_VARS.Exists(x => x.Name == varToRemove.Name))
            {
                throw new EnvironmentVariableException("Variable not found: " + varToRemove.Name);
            }
            ENV_VARS.Remove(varToRemove);
        }

        public static void Clear() => ENV_VARS.Clear();

        public static void RemoveAll(Predicate<EnvironmentVariable> predicate) => ENV_VARS.RemoveAll(predicate);

        public static void AddRange(bool skipExistingItems, params EnvironmentVariable[] newVars)
        {
            for (int i = 0; i < newVars.Length; i++)
            {
                if (ENV_VARS.Exists(x => newVars[i].Name == x.Name) && !skipExistingItems)
                {
                    throw new EnvironmentVariableException("A variable with the given name already exists: " + newVars[i].Name);
                }
                else if (ENV_VARS.Exists(x => newVars[i].Name == x.Name) && skipExistingItems)
                {
                    continue;
                }
                ENV_VARS.Add(newVars[i]);
            }
        }

        public static void SetToDefault(string name)
        {
            if (!ENV_VARS.Exists(x => x.Name == name))
            {
                throw new EnvironmentVariableException("Variable not found: " + name);
            }
            else if (ENV_VARS.Exists(x => x.Name == name && x.VarType == VariableType.Runtime))
            {
                throw new EnvironmentVariableException($"Runtime variable: {name} is read-only.");
            }
            object oldVal = ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)].CurrentValue;
            EnvironmentVariable temp = ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)];
            temp.ResetCurrentValue();
            ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)] = temp;
            CurrentValueChanged(ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)], new ValueChangedEventArgs(ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)].DefaultValue, oldVal));
        }

        public static void SetAllToDefault()
        {
            ENV_VARS.ForEach((x) =>
            {
                if (x.VarType == VariableType.Constant)
                {
                    SetToDefault(x.Name);
                } 
            });
        }

        public static void ChangeCurrentValue(string name, object value, bool raiseEvent = true)
        {
            if (!ENV_VARS.Exists(x => x.Name == name))
            {
                throw new EnvironmentVariableException("Variable not found: " + name);
            }
            else if (ENV_VARS.Exists(x => x.Name == name && x.VarType == VariableType.Runtime))
            {
                throw new EnvironmentVariableException($"Runtime variable: {name} is read-only.");
            }
            object oldVal = ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)].CurrentValue;
            EnvironmentVariable temp = ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)];
            temp.SetCurrentValue(value);
            ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)] = temp;
            if (raiseEvent)
            {
                CurrentValueChanged(ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)], new ValueChangedEventArgs(value, oldVal));
            }
        }

        public static void ChangeDefaultValue(string name, object value, bool raiseEvent = true)
        {
            if (!ENV_VARS.Exists(x => x.Name == name))
            {
                throw new EnvironmentVariableException("Variable not found: " + name);
            }
            else if (ENV_VARS.Exists(x => x.Name == name && x.VarType == VariableType.Runtime))
            {
                throw new EnvironmentVariableException($"Runtime variable: {name} is read-only.");
            }
            object oldVal = ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)].DefaultValue;
            EnvironmentVariable temp = ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)];
            temp.SetDefaultValue(value);
            ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)] = temp;
            if (raiseEvent)
            {
                DefaultValueChanged(ENV_VARS[ENV_VARS.FindIndex(x => x.Name == name)], new ValueChangedEventArgs(value, oldVal));
            }
        }

        public static EnvironmentVariable Find(string name)
        {
            if (!ENV_VARS.Exists(x => x.Name == name))
            {
                throw new EnvironmentVariableException("Variable not found: " + name);
            }

            return ENV_VARS.Find(x => x.Name == name);
        }

        public static EnvironmentVariable Find(Predicate<EnvironmentVariable> predicate)
        {
            return ENV_VARS.Find(predicate);
        }

        public static List<EnvironmentVariable> FindAll(Predicate<EnvironmentVariable> predicate)
        {
            return ENV_VARS.FindAll(predicate);
        }

        public static List<EnvironmentVariable> GetAll()
        {
            return ENV_VARS;
        }

        public static object GetCurrentValue(string name)
        {
            if (!ENV_VARS.Exists(x => x.Name == name))
            {
                throw new EnvironmentVariableException("Variable not found: " + name);
            }
            return Convert.ChangeType(ENV_VARS.Find(x => x.Name == name).CurrentValue, ENV_VARS.Find(x => x.Name == name).ValueType);
        }

        public static object GetDefaultValue(string name)
        {
            if (!ENV_VARS.Exists(x => x.Name == name))
            {
                throw new EnvironmentVariableException("Variable not found: " + name);
            }
            return Convert.ChangeType(ENV_VARS.Find(x => x.Name == name).DefaultValue, ENV_VARS.Find(x => x.Name == name).ValueType);
        }

        public static event CurrentValueChanged CurrentValueChanged;

        public static event DefaultValueChanged DefaultValueChanged;
    }

    public enum VariableType { Constant, Runtime, RuntimeConstant }

    public struct EnvironmentVariable
    {
        public object CurrentValue { get; private set; }

        public object DefaultValue { get; private set; }

        public VariableType VarType { get; }

        public Type ValueType { get; }

        public string Name { get; }

        public EnvironmentVariable(string name, Type type, object defaultVaule, VariableType varType)
        {
            Name = name;
            ValueType = type;
            DefaultValue = defaultVaule;
            CurrentValue = null;
            VarType = varType;
        }

        public EnvironmentVariable(string name, Type type, object defaultValue, object currentValue, VariableType varType)
        {
            Name = name;
            ValueType = type;
            DefaultValue = defaultValue;
            CurrentValue = currentValue;
            VarType = varType;
        }

        public void SetDefaultValue(object value)
        {
            if (VarType == VariableType.Runtime)
            {
                throw new EnvironmentVariableException($"Runtime variable: {Name} is read-only.");
            }
            DefaultValue = Convert.ChangeType(value, ValueType);
        }

        public void SetCurrentValue(object value)
        {
            if (VarType == VariableType.Runtime)
            {
                throw new EnvironmentVariableException($"Runtime variable: {Name} is read-only.");
            }
            CurrentValue = Convert.ChangeType(value, ValueType);
        }

        public void ResetCurrentValue()
        {
            if (VarType == VariableType.Runtime)
            {
                throw new EnvironmentVariableException($"Runtime variable: {Name} is read-only.");
            }
            CurrentValue = DefaultValue;
        }
    }

    public class EnvironmentVariableException : Exception
    {
        public EnvironmentVariableException(string message) : base(message) { }
    }

    public delegate void CurrentValueChanged(object sender, ValueChangedEventArgs e);

    public delegate void DefaultValueChanged(object sender, ValueChangedEventArgs e);

    public class ValueChangedEventArgs : EventArgs
    {
        public object NewValue { get; }

        public object OldValue { get; }

        public ValueChangedEventArgs(object newVal, object oldVal)
        {
            NewValue = newVal;
            OldValue = oldVal;
        }
    }
}

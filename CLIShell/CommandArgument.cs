using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIShell
{
    public class CommandArgument
    {
        public Type Type { get; private set; }

        public string Call { get; }

        public object Value { get; private set; }

        public CommandArgument(Type valueType, string call, object value)
        {
            Type = valueType;
            Value = value;
            Call = call;
        }

        public CommandArgument(Type valueType, string call)
        {
            Type = valueType;
            Value = null;
            Call = call;
        }

        public void SetValue(object newValue)
        {
            if (newValue == null)
            {
                Value = null;
            }
            else
            {
                Value = Convert.ChangeType(newValue, Type);
            }
            
        }

        public void SetValueType(Type newType)
        {
            Type = newType;
            if (Value != null)
            {
                Value = Convert.ChangeType(Value, Type);
            }
        }
    }

}

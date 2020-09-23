using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIShell
{
    public class Interpreter
    {
        private string InputString;

        private string GloblessInput;

        private bool StartAsterisk;

        private bool EndAsterisk;

        private bool BothEndAsterisk;

        private bool StartAny;

        private bool EndAny;

        private bool BothAny;

        private bool Negate;

        public Interpreter(string inputStr)
        {
            InputString = inputStr;
            StartAsterisk = InputString.StartsWith("!\"") || InputString.StartsWith("!*\"") || InputString.StartsWith("\"") || InputString.StartsWith("*\"");
            EndAsterisk = InputString.EndsWith("\"") || InputString.EndsWith("\"*");
            BothEndAsterisk = StartAsterisk && EndAsterisk;
            StartAny = InputString.StartsWith("!*") || InputString.StartsWith("*");
            EndAny = InputString.EndsWith("*");
            BothAny = StartAny && EndAny;
            Negate = InputString.StartsWith("!");
            if ((StartAsterisk && !EndAsterisk) || (!StartAsterisk && EndAsterisk))
            {
                throw new InterpreterException("Incomplete asterisk! Start or end asterisk missing.");
            }
            InputString = Negate ? InputString.Remove(0, 1) : InputString;
            InputString = StartAny ? InputString.Remove(0, 1) : InputString;
            InputString = EndAny ? InputString.Remove(InputString.Length - 1, 1) : InputString;
            if (BothEndAsterisk)
            {
                InputString = InputString.Remove(InputString.IndexOf('\"'), 1);
                InputString = InputString.Remove(InputString.LastIndexOf('\"'), 1);
            }
            GloblessInput = InputString;
            InputString = inputStr;
        }

        public bool GetResult(string compareStr)
        {
            if (StartAny && Negate && !BothAny)
            {
                return !compareStr.EndsWith(GloblessInput);
            }
            else if (EndAny && Negate && !BothAny)
            {
                return !compareStr.StartsWith(GloblessInput);
            }
            else if (StartAny && !BothAny)
            {
                return compareStr.EndsWith(GloblessInput);
            }
            else if (EndAny && !BothAny)
            {
                return compareStr.StartsWith(GloblessInput);
            }
            else if (BothAny && Negate)
            {
                return !compareStr.Contains(GloblessInput);
            }
            else if (BothAny)
            {
                return compareStr.Contains(GloblessInput);
            }
            else
            {
                return GloblessInput == compareStr;
            }
        }

        public class InterpreterException : Exception
        {
            public InterpreterException(string message) : base(message) { }
        }
    }
}

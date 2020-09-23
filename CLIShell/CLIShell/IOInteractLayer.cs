using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CLIShell
{
    public static class IOInteractLayer
    {
        public static event IOInteractStdOutEvent StandardOutputReceived;

        public static event IOInteractStdErrEvent StandardErrorReceived;

        public static void StandardOutput(Command sender, string output)
        {
            StandardOutputReceived(sender, new IOInteractStdOutEventArgs(output));
        }

        public static void StandardError(Command sender, Exception error)
        {
            StandardErrorReceived(sender, new IOInteractStdErrEventArgs(error));
        }
    }

    public delegate void IOInteractStdOutEvent(object sender, IOInteractStdOutEventArgs e);

    public class IOInteractStdOutEventArgs : EventArgs
    {
        public string Output { get; }

        public IOInteractStdOutEventArgs(string output) => Output = output;
    }

    public delegate void IOInteractStdErrEvent(object sender, IOInteractStdErrEventArgs e);

    public class IOInteractStdErrEventArgs
    { 
        public Exception Error { get; }

        public IOInteractStdErrEventArgs(Exception error) => Error = error;
    }

}

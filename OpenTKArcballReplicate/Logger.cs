using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKArcballReplicate
{
    public static class Logger
    {
        public static void WriteLine(string message)
        {
            if (WriteConsole)
            {
                Console.WriteLine(message);
            }
            if (WriteDebug)
            {
                Debug.WriteLine(message);
            }
        }

        public static bool WriteConsole = true;

        public static bool WriteDebug = false;
    }
}

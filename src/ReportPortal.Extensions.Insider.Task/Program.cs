using ReportPortal.Extensions.Insider.Sdk.Instrumentation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReportPortal.Extensions.Insider.Task
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var inst = new AssemblyInstrumentator(args[0]);
            inst.Instrument();
        }
    }
}

using ReportPortal.Extensions.Insider.Sdk.Instrumentation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReportPortal.Extensions.Insider.MsBuild
{
    public class InsiderTask : Microsoft.Build.Utilities.Task
    {
        public override bool Execute()
        {
            var inst = new AssemblyInstrumentator(TargetAssemblyPath);
            inst.Instrument();

            return true;
        }

        public string TargetAssemblyPath { get; set; }
    }
}

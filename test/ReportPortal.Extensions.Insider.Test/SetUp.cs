using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReportPortal.Extensions.Insider.Test
{
    [SetUpFixture]
    public class SetUp
    {
        [OneTimeSetUp]
        public static void SetUpMethod()
        {
            var assemblyToInstrument = TestContext.CurrentContext.TestDirectory + "/ReportPortal.Extensions.Insider.Test.Internal.NetStandard.dll";

            var instrumentator = new Sdk.Instrumentation.AssemblyInstrumentator(assemblyToInstrument);

            instrumentator.Instrument();

            var filePath = TestContext.CurrentContext.TestDirectory + "/ReportPortal.Extensions.Insider.Test.Internal.NetCoreApp.dll";
            if (File.Exists(filePath))
            {
                var assemblyToAppCoreInstrument = filePath;
                var instrumentator2 = new Sdk.Instrumentation.AssemblyInstrumentator(assemblyToAppCoreInstrument);

                instrumentator2.Instrument();
            }

            
        }
    }
}

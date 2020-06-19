using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReportPortal.Extensions.Insider.Sdk.Instrumentation
{
    class AssemblyWriterParametersFactory
    {
        public WriterParameters CreateWriterParameters(string assemblyPath)
        {
            var writerParameters = new WriterParameters();

            var dirPath = Path.GetDirectoryName(assemblyPath);
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            var pdbFilePath = Path.Combine(dirPath, assemblyName + ".pdb");
            if (File.Exists(pdbFilePath))
            {
                writerParameters.WriteSymbols = true;
            }

            return writerParameters;
        }
    }
}

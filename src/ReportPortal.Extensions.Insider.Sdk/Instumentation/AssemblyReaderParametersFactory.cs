using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReportPortal.Extensions.Insider.Sdk.Instrumentation
{
    class AssemblyReaderParametersFactory
    {
        public ReaderParameters CreateReaderParameters(string assemblyPath)
        {
            var resolver = new AssResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));

            var readerParameters = new ReaderParameters()
            {
                ReadWrite = true,
                AssemblyResolver = resolver
            };

            var dirPath = Path.GetDirectoryName(assemblyPath);
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            if (File.Exists(Path.Combine(dirPath, assemblyName + ".pdb")))
            {
                readerParameters.ReadSymbols = true;
            }

            return readerParameters;
        }
    }

    class AssResolver : DefaultAssemblyResolver
    {
        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return base.Resolve(name);
        }
    }
}

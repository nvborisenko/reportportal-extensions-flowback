﻿using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportPortal.Extensions.FlowBack.Task
{
    class AssemblyInstrumentator
    {
        private string _assemblyPath;

        private TaskLoggingHelper _logger;

        public AssemblyInstrumentator(string assemblyPath, TaskLoggingHelper logger)
        {
            _assemblyPath = assemblyPath;
            _logger = logger;
        }

        public void Instrument()
        {
            _logger.LogWarning($"Instrumenting: {_assemblyPath}");

            using (ModuleDefinition module = ModuleDefinition.ReadModule(_assemblyPath, new ReaderParameters { ReadWrite = true, ReadSymbols = true }))
            {
                foreach (var type in module.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.HasCustomAttributes)
                        {
                            if (method.CustomAttributes.Any(ca => ca.AttributeType.FullName == typeof(FlowBackAttribute).FullName))
                            {
                                var processor = method.Body.GetILProcessor();

                                var lastRet = method.Body.Instructions.Last();


                                var i_interceptor_type = module.ImportReference(typeof(Interception.IInterceptor));

                                var beforeInstructions = new List<Instruction>();

                                var varDef = new VariableDefinition(i_interceptor_type);
                                processor.Body.Variables.Add(varDef);

                                // Interception.IInterceptor rp_intercepter = new Interception.MethodInterceptor();
                                beforeInstructions.Add(processor.Create(OpCodes.Newobj, method.Module.ImportReference(typeof(Interception.MethodInterceptor).GetConstructor(new Type[] { }))));
                                beforeInstructions.Add(processor.Create(OpCodes.Stloc, varDef));
                                // rp_intercepter.OnBefore();
                                beforeInstructions.Add(processor.Create(OpCodes.Ldloc, varDef));
                                beforeInstructions.Add(processor.Create(OpCodes.Callvirt, method.Module.ImportReference(typeof(Interception.IInterceptor).GetMethod("OnBefore"))));

                                beforeInstructions.Reverse();

                                foreach (var instruction in beforeInstructions)
                                {
                                    processor.InsertBefore(method.Body.Instructions[0], instruction);
                                }


                                // inner try
                                var handlerInstructions = new List<Instruction>();
                                // Exception exp
                                var exceptionType = module.ImportReference(typeof(Exception));
                                var expVar = new VariableDefinition(exceptionType);
                                method.Body.Variables.Add(expVar);
                                handlerInstructions.Add(processor.Create(OpCodes.Stloc, expVar));
                                handlerInstructions.Add(processor.Create(OpCodes.Ldloc, varDef));
                                handlerInstructions.Add(processor.Create(OpCodes.Ldloc, expVar));
                                handlerInstructions.Add(processor.Create(OpCodes.Callvirt, method.Module.ImportReference(typeof(Interception.IInterceptor).GetMethod("OnException"))));
                                handlerInstructions.Add(processor.Create(OpCodes.Rethrow));

                                var innerTryLeave = processor.Create(OpCodes.Leave_S, lastRet);
                                //handlerInstructions.Add(innerTryLeave);

                                processor.InsertBefore(lastRet, innerTryLeave);

                                foreach (var instruction in handlerInstructions)
                                {
                                    processor.InsertBefore(lastRet, instruction);
                                }

                                //finally try
                                var finallyInstructions = new List<Instruction>();
                                finallyInstructions.Add(processor.Create(OpCodes.Ldloc, varDef));
                                finallyInstructions.Add(processor.Create(OpCodes.Callvirt, method.Module.ImportReference(typeof(Interception.IInterceptor).GetMethod(nameof(Interception.IInterceptor.OnAfter)))));
                                finallyInstructions.Add(processor.Create(OpCodes.Endfinally));

                                var finalLeave = processor.Create(OpCodes.Leave_S, lastRet);
                                processor.InsertBefore(lastRet, finalLeave);

                                foreach (var instruction in finallyInstructions)
                                {
                                    processor.InsertBefore(lastRet, instruction);
                                }

                                var catchHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
                                {
                                    TryStart = processor.Body.Instructions[beforeInstructions.Count],
                                    TryEnd = handlerInstructions.First(),
                                    CatchType = module.ImportReference(typeof(Exception)),
                                    HandlerStart = handlerInstructions.First(),
                                    HandlerEnd = finalLeave
                                };

                                var finallyhandler = new ExceptionHandler(ExceptionHandlerType.Finally)
                                {
                                    TryStart = processor.Body.Instructions[beforeInstructions.Count],
                                    TryEnd = finallyInstructions.First(),
                                    //CatchType = module.ImportReference(typeof(Exception)),
                                    HandlerStart = finallyInstructions.First(),
                                    HandlerEnd = lastRet
                                };

                                processor.Replace(innerTryLeave, processor.Create(OpCodes.Leave_S, finalLeave));


                                method.Body.ExceptionHandlers.Add(catchHandler);
                                method.Body.ExceptionHandlers.Add(finallyhandler);

                                //method.Body.OptimizeMacros();

                            }
                        }
                    }
                }

                module.Write(new WriterParameters { WriteSymbols = true });
            }
        }
    }
}


using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReportPortal.Extensions.Insider.Sdk.Instrumentation
{
    public class AssemblyInstrumentator
    {
        private string _assemblyPath;

        //private TaskLoggingHelper _logger;

        public AssemblyInstrumentator(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
            //_logger = logger;
        }

        public void Instrument()
        {
            //_logger.LogWarning($"Instrumenting: {_assemblyPath}");
            var readerParameters = new AssemblyReaderParametersFactory().CreateReaderParameters(_assemblyPath);
            using (AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(_assemblyPath, readerParameters))
            {
                foreach (var module in assemblyDef.Modules)
                {
                    var netstandard = module.AssemblyReferences.FirstOrDefault(a => a.Name == "netstandard");
                    var systemRuntimeRef = module.AssemblyReferences.FirstOrDefault(a => a.Name == "System.Runtime");
                    var msCorLib = module.AssemblyReferences.FirstOrDefault(a => a.Name == "mscorlib");

                    AssemblyDefinition mmmm = null;
                    if (netstandard != null)
                    {
                        mmmm = module.AssemblyResolver.Resolve(netstandard);
                    } else if (msCorLib != null)
                    {
                        mmmm = module.AssemblyResolver.Resolve(msCorLib);
                    } else if (systemRuntimeRef != null)
                    {
                        mmmm = module.AssemblyResolver.Resolve(systemRuntimeRef);
                    }
                    var exceptionTypeRef = new TypeReference("System", "Exception", mmmm.MainModule, mmmm.MainModule);

                    var b = module.ImportReference(exceptionTypeRef);




                    var allTypes = new List<TypeDefinition>();

                    foreach (var type in module.Types)
                    {
                        allTypes.Add(type);

                        foreach (var nestedType in type.NestedTypes)
                        {
                            allTypes.Add(nestedType);
                        }
                    }

                    foreach (var type in allTypes)
                    {
                        foreach (var method in type.Methods)
                        {
                            if (true && method.HasBody)
                            //if (method.HasCustomAttributes && method.HasBody)
                            {
                                var flowBackAttribute = method.CustomAttributes?.FirstOrDefault(ca => ca.AttributeType.FullName == typeof(InsiderAttribute).FullName);

                                if (true)
                                //if (flowBackAttribute != null)
                                {
                                    var processor = method.Body.GetILProcessor();

                                    var isIgnored = false;

                                    string flowBackLogicalName;
                                    if (method.ReturnType == module.TypeSystem.Void)
                                    {
                                        flowBackLogicalName = $"Calling {type.Name}.**{method.Name}**";
                                    }
                                    else
                                    {
                                        flowBackLogicalName = $"Calling {type.Name}.**{method.Name}** and return *{method.ReturnType.Name}*";
                                    }
                                    if (flowBackAttribute != null)
                                    {
                                        var flowBackAttributeNameFromCtor = flowBackAttribute.ConstructorArguments.FirstOrDefault(a => a.Type == module.TypeSystem.String).Value;
                                        if (flowBackAttributeNameFromCtor != null)
                                        {
                                            flowBackLogicalName = flowBackAttributeNameFromCtor.ToString();
                                        }
                                        else
                                        {
                                            var flowBackNameProperty = flowBackAttribute.Properties.FirstOrDefault(p => p.Name == "Name");

                                            if (flowBackNameProperty.Argument.Value != null)
                                            {
                                                flowBackLogicalName = flowBackNameProperty.Argument.Value.ToString();
                                            }

                                            var flowBackIgnoreProperty = flowBackAttribute.Properties.FirstOrDefault(p => p.Name == "Ignore");
                                            if (flowBackIgnoreProperty.Argument.Value != null)
                                            {
                                                isIgnored = (bool)flowBackIgnoreProperty.Argument.Value;
                                            }
                                        }
                                    }

                                    var isMethodAsync = method.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute).FullName);
                                    var isTypeAsync = type.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).FullName);
                                    // skip it temporary
                                    if (isMethodAsync || isTypeAsync) isIgnored = true;

                                    if (!isIgnored)
                                    {
                                        var firstUserInstruction = processor.Body.Instructions.First();
                                        var lastUserInstrruction = processor.Body.Instructions.Last();

                                        VariableDefinition retValueDef = null;


                                        Instruction lastLdlocInstruction = null;

                                        if (method.ReturnType != module.TypeSystem.Void)
                                        {
                                            retValueDef = new VariableDefinition(method.ReturnType);
                                            processor.Body.Variables.Add(retValueDef);
                                            lastLdlocInstruction = processor.Create(OpCodes.Ldloc, retValueDef);

                                            processor.Append(lastLdlocInstruction);
                                        }
                                        else
                                        {
                                            lastLdlocInstruction = processor.Create(OpCodes.Nop);
                                            processor.Append(lastLdlocInstruction);
                                        }

                                        var retInstruction = processor.Create(OpCodes.Ret);
                                        processor.Append(retInstruction);



                                        // Interception.IInterceptor rp_intercepter = new Interception.MethodInterceptor();

                                        var interceptorTypeDefinition = module.ImportReference(typeof(Interception.MethodInterceptor)).Resolve();
                                        var i_interceptor_type = module.ImportReference(typeof(Interception.IInterceptor)).Resolve();

                                        var beforeInstructions = new List<Instruction>();

                                        var varDef = new VariableDefinition(module.ImportReference(i_interceptor_type));
                                        processor.Body.Variables.Add(varDef);

                                        beforeInstructions.Add(processor.Create(OpCodes.Newobj, module.ImportReference(interceptorTypeDefinition.GetConstructors().First())));
                                        beforeInstructions.Add(processor.Create(OpCodes.Stloc, varDef));
                                        // rp_intercepter.OnBefore();
                                        beforeInstructions.Add(processor.Create(OpCodes.Ldloc, varDef));
                                        beforeInstructions.Add(processor.Create(OpCodes.Ldstr, flowBackLogicalName));
                                        // array of method arguments
                                        if (method.HasParameters)
                                        {
                                            //var dictionaryTypeDef = module.ImportReference(typeof(ParamInfo));

                                            //var methodParamsVarDefinition = new VariableDefinition(dictionaryTypeDef);
                                            //method.Body.Variables.Add(methodParamsVarDefinition);

                                            //beforeInstructions.Add(processor.Create(OpCodes.Ldc_I4, method.Parameters.Count));
                                            //beforeInstructions.Add(processor.Create(OpCodes.Newarr, method.Module.ImportReference(typeof(ParamInfo))));
                                            //beforeInstructions.Add(processor.Create(OpCodes.Stloc, methodParamsVarDefinition));

                                            //foreach (var methodParam in method.Parameters)
                                            //{
                                            //    beforeInstructions.Add(processor.Create(OpCodes.Ldloc, methodParamsVarDefinition));
                                            //    beforeInstructions.Add(processor.Create(OpCodes.Ldc_I4, methodParam.Index));
                                            //    beforeInstructions.Add(processor.Create(OpCodes.Ldstr, methodParam.Name));
                                            //    beforeInstructions.Add(processor.Create(OpCodes.Ldarg, methodParam));
                                            //    if (methodParam.ParameterType.IsValueType || methodParam.ParameterType.IsGenericParameter)
                                            //    {
                                            //        beforeInstructions.Add(processor.Create(OpCodes.Box, methodParam.ParameterType));
                                            //    }
                                            //    beforeInstructions.Add(processor.Create(OpCodes.Newobj, method.Module.ImportReference(typeof(ParamInfo).GetConstructors().First())));
                                            //    beforeInstructions.Add(processor.Create(OpCodes.Stelem_Ref));
                                            //}

                                            //beforeInstructions.Add(processor.Create(OpCodes.Ldloc, methodParamsVarDefinition));

                                            beforeInstructions.Add(processor.Create(OpCodes.Ldnull));
                                        }
                                        else
                                        {
                                            beforeInstructions.Add(processor.Create(OpCodes.Ldnull));
                                        }

                                        beforeInstructions.Add(processor.Create(OpCodes.Callvirt, module.ImportReference(i_interceptor_type.GetMethods().First(m => m.Name == "OnBefore"))));

                                        beforeInstructions.Reverse();

                                        foreach (var instruction in beforeInstructions)
                                        {
                                            processor.InsertBefore(method.Body.Instructions[0], instruction);
                                        }


                                        // inner try
                                        var handlerInstructions = new List<Instruction>();
                                        // Exception exp

                                        TypeDefinition expTypeDefinition;
                                        TypeReference extTypeReference = null;

                                        bool exceptionTypeResolved = false;

                                        //var netstandard = module.AssemblyReferences.FirstOrDefault(a => a.Name == "netstandard");
                                        //var systemRuntimeRef = module.AssemblyReferences.FirstOrDefault(a => a.Name == "System.Runtime");
                                        //var msCorLib = module.AssemblyReferences.FirstOrDefault(a => a.Name == "mscorlib");
                                        //// full net framework
                                        //if (msCorLib != null)
                                        //{
                                        //    var msCorLibDef = module.AssemblyResolver.Resolve(msCorLib);
                                        //    expTypeDefinition = msCorLibDef.MainModule.GetType("System.Exception");
                                        //    extTypeReference = module.ImportReference(expTypeDefinition);

                                        //    exceptionTypeResolved = true;
                                        //}

                                        //// net core app
                                        //if (!exceptionTypeResolved && systemRuntimeRef != null)
                                        //{
                                        //    var systemRuntimeDef = module.AssemblyResolver.Resolve(systemRuntimeRef);

                                        //    var privateCoreLib = systemRuntimeDef.MainModule.AssemblyReferences.FirstOrDefault(a => a.Name == "System.Private.CoreLib");

                                        //    if (privateCoreLib == null) break;

                                        //    var privateCoreLibDef = module.AssemblyResolver.Resolve(privateCoreLib);

                                        //    expTypeDefinition = privateCoreLibDef.MainModule.GetType("System.Exception");
                                        //    extTypeReference = module.ImportReference(expTypeDefinition);

                                        //    exceptionTypeResolved = true;
                                        //}

                                        //// netstandard library
                                        //if (!exceptionTypeResolved && netstandard != null)
                                        //{
                                        //    var netstandardRef = module.AssemblyResolver.Resolve(netstandard);
                                        //    systemRuntimeRef = netstandardRef.MainModule.AssemblyReferences.FirstOrDefault(a => a.Name == "System.Runtime");
                                        //    if (systemRuntimeRef == null) break;
                                        //    var systemRuntimeDef = module.AssemblyResolver.Resolve(systemRuntimeRef);
                                        //    var privateCoreLib = systemRuntimeDef.MainModule.AssemblyReferences.FirstOrDefault(a => a.Name == "System.Private.CoreLib");
                                        //    if (privateCoreLib == null) break;
                                        //    var privateCoreLibDef = module.AssemblyResolver.Resolve(privateCoreLib);

                                        //    expTypeDefinition = privateCoreLibDef.MainModule.GetType("System.Exception");
                                        //    extTypeReference = module.ImportReference(expTypeDefinition);

                                        //    exceptionTypeResolved = true;
                                        //}

                                        //if (!exceptionTypeResolved)
                                        //{
                                        //    throw new Exception("Unable to determine app runtime.");
                                        //}



                                        //var exceptionTypeRef = module.ImportReference(typeof(Exception));
                                        var expVar = new VariableDefinition(b);
                                        method.Body.Variables.Add(expVar);
                                        handlerInstructions.Add(processor.Create(OpCodes.Stloc, expVar));
                                        handlerInstructions.Add(processor.Create(OpCodes.Ldloc, varDef));
                                        handlerInstructions.Add(processor.Create(OpCodes.Ldloc, expVar));
                                        handlerInstructions.Add(processor.Create(OpCodes.Callvirt, module.ImportReference(i_interceptor_type.GetMethods().First(m => m.Name == "OnException"))));
                                        handlerInstructions.Add(processor.Create(OpCodes.Rethrow));

                                        foreach (var instruction in handlerInstructions)
                                        {
                                            processor.InsertBefore(lastLdlocInstruction, instruction);
                                        }

                                        //finally try
                                        var finallyInstructions = new List<Instruction>();
                                        finallyInstructions.Add(processor.Create(OpCodes.Ldloc, varDef));
                                        if (retValueDef != null)
                                        {
                                            finallyInstructions.Add(processor.Create(OpCodes.Ldloc, retValueDef));
                                            if (retValueDef.VariableType.IsValueType || retValueDef.VariableType.IsGenericParameter)
                                            {
                                                finallyInstructions.Add(processor.Create(OpCodes.Box, retValueDef.VariableType));
                                            }

                                            finallyInstructions.Add(processor.Create(OpCodes.Callvirt, module.ImportReference(i_interceptor_type.GetMethods().First(m => m.Name == "OnAfter" && m.HasParameters))));
                                        }
                                        else
                                        {
                                            //finallyInstructions.Add(processor.Create(OpCodes.Ldnull));
                                            finallyInstructions.Add(processor.Create(OpCodes.Callvirt, module.ImportReference(i_interceptor_type.GetMethods().First(m => m.Name == "OnAfter" && !m.HasParameters))));
                                        }


                                        finallyInstructions.Add(processor.Create(OpCodes.Endfinally));

                                        var finalLeave = processor.Create(OpCodes.Leave, lastLdlocInstruction);
                                        processor.InsertBefore(lastLdlocInstruction, finalLeave);

                                        foreach (var instruction in finallyInstructions)
                                        {
                                            processor.InsertBefore(lastLdlocInstruction, instruction);
                                        }

                                        var firstIndex = processor.Body.Instructions.IndexOf(firstUserInstruction);
                                        var lastIndex = processor.Body.Instructions.IndexOf(lastUserInstrruction);
                                        for (int i = firstIndex; i <= lastIndex; i++)
                                        {
                                            var inst = processor.Body.Instructions[i];

                                            if (inst.OpCode.Code == Code.Ret)
                                            {
                                                if (retValueDef != null)
                                                {
                                                    inst.OpCode = OpCodes.Stloc;
                                                    inst.Operand = retValueDef;

                                                    processor.InsertAfter(inst, processor.Create(OpCodes.Leave, finalLeave));

                                                    lastIndex++;
                                                }
                                                else
                                                {
                                                    inst.OpCode = OpCodes.Leave;
                                                    inst.Operand = finalLeave;
                                                }
                                            }
                                        }



                                        var catchHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
                                        {
                                            TryStart = firstUserInstruction,
                                            TryEnd = handlerInstructions.First(),
                                            CatchType = b,
                                            HandlerStart = handlerInstructions.First(),
                                            HandlerEnd = handlerInstructions.Last().Next
                                        };

                                        var finallyhandler = new ExceptionHandler(ExceptionHandlerType.Finally)
                                        {
                                            TryStart = firstUserInstruction,
                                            TryEnd = finallyInstructions.First(),
                                            HandlerStart = finallyInstructions.First(),
                                            HandlerEnd = finallyInstructions.Last().Next
                                        };

                                        method.Body.ExceptionHandlers.Add(catchHandler);
                                        method.Body.ExceptionHandlers.Add(finallyhandler);


                                        method.Body.OptimizeMacros();

                                        if (true)
                                        {
                                            var msssss = module.AssemblyReferences.FirstOrDefault(a => a.Name == "System.Private.CoreLib");
                                            if (msssss != null)
                                            {
                                                module.AssemblyReferences.Remove(msssss);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                }

                var writerParameters = new AssemblyWriterParametersFactory().CreateWriterParameters(_assemblyPath);
                assemblyDef.Write(writerParameters);
            }
        }
    }
}


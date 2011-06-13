using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using WeavingCommon;

namespace Costura
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class AssemblyLoaderImporter
    {
        ModuleReader moduleReader;
        AssemblyResolver assemblyResolver;
        ConstructorInfo instructionConstructorInfo;
        TypeDefinition targetType;
        TypeDefinition sourceType;
        public MethodDefinition AttachMethod { get; set; }

        [ImportingConstructor]
        public AssemblyLoaderImporter(ModuleReader moduleReader, AssemblyResolver assemblyResolver)
        {
            instructionConstructorInfo = typeof(Instruction).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(OpCode), typeof(object) }, null);
            this.moduleReader = moduleReader;
            this.assemblyResolver = assemblyResolver;
        }
        public void Execute()
        {

            var moduleDefinition = GetModuleDefinition();

            sourceType = moduleDefinition.Types.First(x => x.Name == "ILTemplate");

            targetType = new TypeDefinition("Costura", "ILTemplate", sourceType.Attributes, Resolve(sourceType.BaseType));
            moduleReader.Module.Types.Add(targetType);
            CopyMethod(sourceType.Methods.First(x => x.Name == "ResolveAssembly"));

            AttachMethod = CopyMethod(sourceType.Methods.First(x => x.Name == "Attach"));
        }

        private ModuleDefinition GetModuleDefinition()
        {
            var readerParameters = new ReaderParameters
                                       {
                                           AssemblyResolver = assemblyResolver,
                                       };

            var resourceName = GetResourceName();

            using (var resourceStream = typeof (AssemblyLoaderImporter).Assembly.GetManifestResourceStream(resourceName))
            {
                return ModuleDefinition.ReadModule(resourceStream, readerParameters);
            }
        }

        private string GetResourceName()
        {
            var assembly = moduleReader.Module.Assembly;
            if (!assembly.HasCustomAttributes)
            {
                return "Costura.DotNetTemplate.dll";
            }
            var customAttribute = assembly.CustomAttributes.First(x => x.AttributeType.Name == "TargetFrameworkAttribute");

            var contructorArg = (string) customAttribute.ConstructorArguments.First().Value;
            if (contructorArg.Contains("Silverlight"))
            {
                //TargetFramework("Silverlight,Version=v4.0", FrameworkDisplayName="Silverlight 4")]
                return "Costura.SilverlightTemplate.dll";
            }
            return "Costura.DotNetTemplate.dll";
        }

        private TypeReference Resolve(TypeReference baseType)
        {
            var typeDefinition = baseType.Resolve();
            var typeReference = moduleReader.Module.Import(typeDefinition);
            if (baseType is ArrayType)
            {
                return new ArrayType(typeReference);
            }
            return typeReference;
        }


        private MethodDefinition CopyMethod(MethodDefinition templateMethod)
        {
            var newMethod = new MethodDefinition(templateMethod.Name, templateMethod.Attributes, Resolve(templateMethod.ReturnType))
                                {
                                    Body =
                                        {
                                            InitLocals = true
                                        }
                                };

            foreach (var variableDefinition in templateMethod.Body.Variables)
            {
                newMethod.Body.Variables.Add(new VariableDefinition(Resolve(variableDefinition.VariableType)));
            }
            foreach (var parameterDefinition in templateMethod.Parameters)
            {
                newMethod.Parameters.Add(new ParameterDefinition(Resolve(parameterDefinition.ParameterType)));
            }

            CopyInstructions(templateMethod, newMethod);
            CopyExceptionHandlers(templateMethod, newMethod);
            targetType.Methods.Add(newMethod);
            return newMethod;
        }

        private void CopyExceptionHandlers(MethodDefinition templateMethod, MethodDefinition newMethod)
        {
            if (!templateMethod.Body.HasExceptionHandlers)
            {
                return;
            }
            foreach (var exceptionHandler in templateMethod.Body.ExceptionHandlers)
            {
                var handler = new ExceptionHandler(exceptionHandler.HandlerType);
                var templateInstructions = templateMethod.Body.Instructions;
                var targetInstructions = newMethod.Body.Instructions;
                if (exceptionHandler.TryStart != null)
                {
                    handler.TryStart = targetInstructions[templateInstructions.IndexOf(exceptionHandler.TryStart)];
                }
                if (exceptionHandler.TryEnd != null)
                {
                    handler.TryEnd = targetInstructions[templateInstructions.IndexOf(exceptionHandler.TryEnd)];
                }
                if (exceptionHandler.HandlerStart != null)
                {
                    handler.HandlerStart = targetInstructions[templateInstructions.IndexOf(exceptionHandler.HandlerStart)];
                }
                if (exceptionHandler.HandlerEnd != null)
                {
                    handler.HandlerEnd = targetInstructions[templateInstructions.IndexOf(exceptionHandler.HandlerEnd)];
                }
                if (exceptionHandler.FilterStart != null)
                {
                    handler.FilterStart = targetInstructions[templateInstructions.IndexOf(exceptionHandler.FilterStart)];
                }
                if (exceptionHandler.CatchType != null)
                {
                    handler.CatchType = Resolve(exceptionHandler.CatchType);
                }
                newMethod.Body.ExceptionHandlers.Add(handler);
            }
        }

        private void CopyInstructions(MethodDefinition templateMethod, MethodDefinition newMethod)
        {
            foreach (var instruction in templateMethod.Body.Instructions)
            {
                newMethod.Body.Instructions.Add(CloneInstruction(instruction));
            }
        }

        private Instruction CloneInstruction(Instruction instruction)
        {
            var newInstruction = (Instruction)instructionConstructorInfo.Invoke(new[] { instruction.OpCode , instruction.Operand});
            newInstruction.Operand = Import(instruction.Operand);
            return newInstruction;
        }

        object Import(object operand)
        {
            
            if (operand is MethodReference)
            {
                var methodReference = (MethodReference)operand;
                if (methodReference.DeclaringType == sourceType)
                {
                    return targetType.Methods.First(x=>x.Name == methodReference.Name);
                }
            	return moduleReader.Module.Import(methodReference.Resolve());
            }
            if (operand is TypeReference)
            {
                return Resolve((TypeReference)operand);
            }
            return operand;
        }
    }
}
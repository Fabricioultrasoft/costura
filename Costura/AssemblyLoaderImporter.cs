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
        ConstructorInfo instructionConstructorInfo;
        TypeDefinition targetType;
        TypeDefinition sourceType;
        public MethodDefinition AttachMethod { get; set; }

        [ImportingConstructor]
        public AssemblyLoaderImporter(ModuleReader moduleReader)
        {
            instructionConstructorInfo = typeof(Instruction).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(OpCode), typeof(object) }, null);
            this.moduleReader = moduleReader;
        }
        public void Execute()
        {
            var moduleDefinition = ModuleDefinition.ReadModule(typeof (AssemblyLoaderImporter).Assembly.CodeBase.Remove(0, 8));
            sourceType = moduleDefinition.Types.First(x => x.Name == "ILTemplate");
            targetType = new TypeDefinition("Costura", "ILTemplate", sourceType.Attributes, moduleReader.Module.Import(sourceType.BaseType));
            moduleReader.Module.Types.Add(targetType);
            CopyMethod(sourceType.Methods.First(x => x.Name == "OnCurrentDomainOnAssemblyResolve"));
            CopyMethod(sourceType.Methods.First(x => x.Name == "Attach"));

            AttachMethod = targetType.Methods.First(x => x.Name == "Attach");
        }


        private void CopyMethod(MethodDefinition templateMethod)
        {
            var targetModule = targetType.Module;
            var newMethod = new MethodDefinition(templateMethod.Name, templateMethod.Attributes, targetModule.Import(templateMethod.ReturnType))
                                {
                                    Body =
                                        {
                                            InitLocals = true
                                        }
                                };

            foreach (var variableDefinition in templateMethod.Body.Variables)
            {
                newMethod.Body.Variables.Add(new VariableDefinition(targetModule.Import(variableDefinition.VariableType)));
            }
            foreach (var parameterDefinition in templateMethod.Parameters)
            {
                newMethod.Parameters.Add(new ParameterDefinition(targetModule.Import(parameterDefinition.ParameterType)));
            }

            CopyInstructions(templateMethod, newMethod);
            CopyExceptionHandlers(templateMethod, newMethod);
            targetType.Methods.Add(newMethod);
            return;
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
                    handler.CatchType = moduleReader.Module.Import(exceptionHandler.CatchType);
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
            	return moduleReader.Module.Import(methodReference);
            }
            if (operand is TypeReference)
            {
                return moduleReader.Module.Import((TypeReference)operand);
            }
            return operand;
        }
    }
}
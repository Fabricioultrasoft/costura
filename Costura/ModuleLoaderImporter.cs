using System.ComponentModel.Composition;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using WeavingCommon;

namespace Costura
{
	[Export, PartCreationPolicy(CreationPolicy.Shared)]
	public class ModuleLoaderImporter
	{
		ModuleReader moduleReader;
		AssemblyLoaderImporter assemblyLoaderImporter;
		MsCoreReferenceFinder coreReferenceFinder;

		[ImportingConstructor]
		public ModuleLoaderImporter(ModuleReader moduleReader, AssemblyLoaderImporter assemblyLoaderImporter, MsCoreReferenceFinder coreReferenceFinder)
		{
			this.moduleReader = moduleReader;
			this.assemblyLoaderImporter = assemblyLoaderImporter;
			this.coreReferenceFinder = coreReferenceFinder;
		}

		public void Execute()
		{
			const MethodAttributes attributes = MethodAttributes.Static
												| MethodAttributes.SpecialName
												| MethodAttributes.RTSpecialName;
			var cctor = GetCctor(attributes);
			var il = cctor.Body.GetILProcessor();

			//var typeDefinition = moduleReader.Module.Types.First(x => x.Name == "ILTemplate");
			//il.Append(il.Create(OpCodes.Call, typeDefinition.Methods.First(x => x.Name == "Attach")));
			il.Append(il.Create(OpCodes.Call, assemblyLoaderImporter.AttachMethod));
			il.Append(il.Create(OpCodes.Ret));
		}

		private MethodDefinition GetCctor(MethodAttributes attributes)
		{
			var moduleClass = moduleReader.Module.Types.FirstOrDefault(x => x.Name == "<Module>");
			if (moduleClass == null)
			{
				throw new WeavingException("Found no module class!");
			}
			var cctor = moduleClass.Methods.FirstOrDefault(x => x.Name == ".cctor");
			if (cctor == null)
			{
				cctor = new MethodDefinition(".cctor", attributes, coreReferenceFinder.VoidTypeReference);
				moduleClass.Methods.Add(cctor);
			}
			return cctor;
		}
	}
}
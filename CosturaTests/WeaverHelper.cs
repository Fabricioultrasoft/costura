using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Costura;
using Mono.Cecil;
using XDocumentExtensions = WeavingCommon.XDocumentExtensions;


public class WeaverHelper
{
	string projectPath;
	string assemblyPath;
	public Assembly Assembly { get; set; }

	public WeaverHelper(string projectPath)
	{
		this.projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\TestAssemblies", projectPath));

		GetAssemblyPath();


		var newAssembly = assemblyPath.Replace(".dll", "2.dll");
		var pdbFileName = Path.ChangeExtension(assemblyPath, "pdb");
		var newPdbFileName = Path.ChangeExtension(newAssembly, "pdb");
		File.Copy(assemblyPath, newAssembly, true);
		File.Copy(pdbFileName, newPdbFileName, true);
		var myBuildEngine = new StubBuildEngine
		                    	{
		                    		ProjectFileOfTaskNode = this.projectPath
		                    	};
		var referenceCopyLocalPaths = GetCopyLocal().ToList();
		var embedTask = new EmbedTask
		                	{
		                		TargetPath = newAssembly,
		                		BuildEngine = myBuildEngine,
		                		References = GetReferences(),
		                		DeleteReferences = false,
		                		ReferenceCopyLocalPaths = referenceCopyLocalPaths
		                	};

		var execute = embedTask.Execute();
		if (!execute)
		{
			throw embedTask.Exception;
		}
#if (RELEASE)
            foreach (var referenceCopyLocalPath in referenceCopyLocalPaths)
            {
                File.Delete(referenceCopyLocalPath);
            }
#endif
		Assembly = Assembly.LoadFile(newAssembly);
	}

	private void GetAssemblyPath()
	{
		assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), GetOutputPathValue(), GetAssemblyName() + ".dll");
	}

	private string GetAssemblyName()
	{
		var xDocument = XDocument.Load(projectPath);

		return XDocumentExtensions.BuildDescendants(xDocument, "AssemblyName")
			.Select(x => x.Value)
			.First();
	}

	private string GetOutputPathValue()
	{
		var xDocument = XDocument.Load(projectPath);

		var outputPathValue = (from propertyGroup in XDocumentExtensions.BuildDescendants(xDocument, "PropertyGroup")
		                       let condition = ((string) propertyGroup.Attribute("Condition"))
		                       where (condition != null) &&
		                             (condition.Trim() == "'$(Configuration)|$(Platform)' == 'Debug|AnyCPU'")
		                       from outputPath in XDocumentExtensions.BuildDescendants(propertyGroup, "OutputPath")
		                       select outputPath.Value).First();
#if (!DEBUG)
            outputPathValue = outputPathValue.Replace("Debug", "Release");
#endif
		return outputPathValue;
	}

	private string GetReferences()
	{
		var referenceFinder = new ReferenceFinder(assemblyPath, projectPath);
		var builder = new StringBuilder();

		var assemblyNameReferences = ModuleDefinition.ReadModule(assemblyPath).AssemblyReferences;
		foreach (var assemblyNameReference in assemblyNameReferences)
		{
			builder.Append(referenceFinder.Resolve(assemblyNameReference));
			builder.Append(";");
		}
		builder.Append(referenceFinder.Resolve("System"));
		builder.Append(";");
		builder.Append(referenceFinder.Resolve("System.Core"));
		builder.Append(";");
		return builder.ToString();
	}

	private IEnumerable<string> GetCopyLocal()
	{
		var referenceFinder = new ReferenceFinder(assemblyPath, projectPath);

		var assemblyNameReferences = ModuleDefinition.ReadModule(assemblyPath).AssemblyReferences;
		foreach (var assemblyNameReference in assemblyNameReferences)
		{
			if (assemblyNameReference.FullName.Contains("Reference"))
			{
				yield return referenceFinder.Resolve(assemblyNameReference);
			}
		}
	}
}
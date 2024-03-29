﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;


public class ReferenceFinder
{
	List<string> gacPaths;
	public List<string> Directories { get; private set; }

	public ReferenceFinder(string targetPath, string projectPath)
	{
		var versionReader = new VersionReader(projectPath);
		Directories = new List<string>();

		if (versionReader.IsSilverlight)
		{
			if (string.IsNullOrEmpty(versionReader.TargetFrameworkProfile))
			{
				Directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\Silverlight\{1}\", GetProgramFilesPath(), versionReader.FrameworkVersionAsString));
			}
			else
			{
				Directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\Silverlight\{1}\Profile\{2}", GetProgramFilesPath(), versionReader.FrameworkVersionAsString, versionReader.TargetFrameworkProfile));
			}

		}
		else
		{
			if (string.IsNullOrEmpty(versionReader.TargetFrameworkProfile))
			{
				if (versionReader.FrameworkVersionAsNumber == 3.5)
				{
					Directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\v3.5\", GetProgramFilesPath()));
					Directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\v3.0\", GetProgramFilesPath()));
					Directories.Add(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Microsoft.NET\Framework\v2.0.50727\"));
				}
				else
				{
					Directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\.NETFramework\{1}\", GetProgramFilesPath(), versionReader.FrameworkVersionAsString));
				}
			}
			else
			{
				Directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\.NETFramework\{1}\Profile\{2}", GetProgramFilesPath(), versionReader.FrameworkVersionAsString, versionReader.TargetFrameworkProfile));
			}
		}
		Directories.Add(Path.GetDirectoryName(targetPath));

		GetGacPaths();

		//TODO: throw if less than 3.5
	}

	void GetGacPaths()
	{
		gacPaths = GetDefaultWindowsGacPaths().ToList();
	}

	IEnumerable<string> GetDefaultWindowsGacPaths()
	{
		var environmentVariable = Environment.GetEnvironmentVariable("WINDIR");
		if (environmentVariable != null)
		{
			yield return Path.Combine(environmentVariable, "assembly");
			yield return Path.Combine(environmentVariable, Path.Combine("Microsoft.NET", "assembly"));
		}
	}

	public string GetProgramFilesPath()
	{
		var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
		if (programFiles == null)
		{
			return Environment.GetEnvironmentVariable("ProgramFiles");
		}

		return programFiles;
	}


	string SearchDirectory(string name)
	{
		foreach (var directory in Directories)
		{
			var dllFile = Path.Combine(directory, name + ".dll");
			if (File.Exists(dllFile))
			{
				return dllFile;
			}
			var exeFile = Path.Combine(directory, name + ".exe");
			if (File.Exists(exeFile))
			{
				return exeFile;
			}
		}
		return null;
	}


	public string Resolve(AssemblyNameReference assemblyNameReference)
	{
		var file = SearchDirectory(assemblyNameReference.Name);
		if (file == null)
		{
			file = GetAssemblyInGac(assemblyNameReference);
		}
		if (file != null)
		{
			return file;
		}
		throw new FileNotFoundException();
	}

	public string Resolve(string assemblyName)
	{
		var file = SearchDirectory(assemblyName);
		if (file != null)
		{
			return file;
		}
		throw new FileNotFoundException();
	}

	string GetAssemblyInGac(AssemblyNameReference reference)
	{
		if ((reference.PublicKeyToken == null) || (reference.PublicKeyToken.Length == 0))
		{
			return null;
		}
		return GetAssemblyInNetGac(reference);
	}

	string GetAssemblyInNetGac(AssemblyNameReference reference)
	{
		var gacs = new[] {"GAC_MSIL", "GAC_32", "GAC"};
		var prefixes = new[] {string.Empty, "v4.0_"};

		for (var i = 0; i < 2; i++)
		{
			for (var j = 0; j < gacs.Length; j++)
			{
				var gac = Path.Combine(gacPaths[i], gacs[j]);
				var file = GetAssemblyFile(reference, prefixes[i], gac);
				if (Directory.Exists(gac) && File.Exists(file))
				{
					return file;
				}
			}
		}

		return null;
	}


	static string GetAssemblyFile(AssemblyNameReference reference, string prefix, string gac)
	{
		var builder = new StringBuilder();
		builder.Append(prefix);
		builder.Append(reference.Version);
		builder.Append("__");
		for (var i = 0; i < reference.PublicKeyToken.Length; i++)
		{
			builder.Append(reference.PublicKeyToken[i].ToString("x2"));
		}
		return Path.Combine(Path.Combine(Path.Combine(gac, reference.Name), builder.ToString()), reference.Name + ".dll");
	}




}
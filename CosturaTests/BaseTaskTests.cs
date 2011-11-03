using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;

public abstract class BaseTaskTests
{
	string projectPath;
	Assembly assembly;

	protected BaseTaskTests(string projectPath)
	{

#if (!DEBUG)
            projectPath = projectPath.Replace("Debug", "Release");
#endif
		this.projectPath = projectPath;
	}

	[TestFixtureSetUp]
	public void Setup()
	{
		var weaverHelper = new WeaverHelper(projectPath);
		assembly = weaverHelper.Assembly;
	}


	[Test]
	public void Simple()
	{
		var instance = assembly.GetInstance("ClassToTest");
		Assert.AreEqual("Hello", instance.Foo());
	}
	[Test]
	public void ThrowException()
	{
		try
		{
			var instance = assembly.GetInstance("ClassToTest");
			instance.ThrowException();
		}
		catch (Exception exception)
		{
			Debug.WriteLine(exception.StackTrace);
			Assert.IsTrue(exception.StackTrace.Contains("ClassToReference.cs:line"));	
		}
	}

#if(DEBUG)
	[Test]
	public void PeVerify()
	{
		Verifier.Verify(assembly.CodeBase.Remove(0, 8));
	}

	[Test]
	public void EnsureOnly1RefToMscorLib()
	{
		var moduleDefinition = ModuleDefinition.ReadModule(assembly.CodeBase.Remove(0, 8));
		Assert.AreEqual(1, moduleDefinition.AssemblyReferences.Count(x => x.Name == "mscorlib"));
	}
#endif
}
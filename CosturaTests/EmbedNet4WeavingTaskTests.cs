using NUnit.Framework;


[TestFixture]
public class EmbedNet4WeavingTaskTests : BaseTaskTests
{

    public EmbedNet4WeavingTaskTests()
        : base(@"EmbedTestAssemblies\AssemblyToProcess\AssemblyToProcessDotNet4.csproj")
	{
	}

}
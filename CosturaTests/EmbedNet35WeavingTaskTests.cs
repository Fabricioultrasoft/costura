﻿using NUnit.Framework;


[TestFixture]
public class EmbedNet35WeavingTaskTests : BaseTaskTests
{

    public EmbedNet35WeavingTaskTests()
        : base(@"EmbedTestAssemblies\AssemblyToProcess\AssemblyToProcessDotNet35.csproj")
	{
	}

}
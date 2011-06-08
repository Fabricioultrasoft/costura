using NUnit.Framework;

namespace CosturaTests
{
    [TestFixture]
    public class Net4WeavingTaskTests : BaseTaskTests
    {

        public Net4WeavingTaskTests()
            : base(@"AssemblyToProcess\AssemblyToProcessDotNet4.csproj")
        {
        }

    }
}
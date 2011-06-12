using NUnit.Framework;

namespace CosturaTests
{
    [TestFixture]
    public class Net35WeavingTaskTests : BaseTaskTests
    {

        public Net35WeavingTaskTests()
            : base(@"AssemblyToProcess\AssemblyToProcessDotNet35.csproj")
        {
        }

    }
}
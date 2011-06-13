using NUnit.Framework;

namespace CosturaTests
{
    [TestFixture]
    public class Silverlight4WeavingTaskTests : BaseTaskTests
    {

        public Silverlight4WeavingTaskTests()
            : base(@"AssemblyToProcess\AssemblyToProcessSilverlight4.csproj")
        {
        }

    }
}
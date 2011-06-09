﻿using CosturaVSPackage;
using NUnit.Framework;

namespace CosturaVSPackageTests
{
    [TestFixture]
    public class ConfigureWindowModelTests
    {

        [Test]
        public void ValidateTargetPath()
        {
            var model = new ConfigureWindowModel
                            {
                                TargetPath = null,
                                DeriveTargetPathFromBuildEngine = false,
                                ToolsDirectory = "foo,"
                            };
            Assert.IsNotNullOrEmpty(model.GetErrors());
            model.TargetPath = string.Empty;
            Assert.IsNotNullOrEmpty(model.GetErrors());
            model.TargetPath = "a";
            Assert.IsNull(model.GetErrors());
        }
    }
}
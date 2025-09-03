using System;
using Binaryen.NET;
using Xunit;

namespace Binaryen.NET.Tests
{
    public class ModuleTests
    {
        [Fact]
        public void InstantiateModule_ShouldNotBeNull()
        {
            using var module = new Module();
            Assert.NotNull(module);
        }

        [Fact]
        public void ParseModule_FromWAT_ShouldNotBeNull()
        {
            string wat = "(module)";
            using var module = Module.Parse(wat);

            Assert.NotNull(module);
        }

        [Fact]
        public void AddFunctionExport_ShouldNotThrow()
        {
            using var module = new Module();
            module.AddFunctionExport("internalFunc", "externalFunc");
            // Test passes if no exception is thrown
        }

        [Fact]
        public void ToWAT_ShouldReturnNonEmptyString()
        {
            using var module = new Module();
            string wat = module.ToWAT();

            Assert.False(string.IsNullOrEmpty(wat));
            Assert.Contains("(module", wat);
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            using var module = new Module();
            module.Dispose(); // first call
            var ex = Record.Exception(() => module.Dispose()); // second call
            Assert.Null(ex);
        }
    }
}

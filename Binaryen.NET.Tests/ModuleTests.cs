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
        public void AddFunctionExport_ShouldAppearInWAT()
        {
            using var module = new Module();
            module.AddFunctionExport("internalFunc", "externalFunc");

            string wat = module.ToText();

            Assert.False(string.IsNullOrEmpty(wat));

            // Check that the WAT contains the function export
            Assert.Contains("(export \"externalFunc\" (func $internalFunc))", wat);
            Assert.Contains("(func $internalFunc", wat);
        }

        [Fact]
        public void AddFunctionImport_ShouldAppearInWAT()
        {
            using var module = new Module();
            List<BinaryenType> paramTypes = new List<BinaryenType>() { BinaryenType.None };
            List<BinaryenType> resultTypes = new List<BinaryenType>() { BinaryenType.Int32 };

            module.AddFunctionImport(
                internalName: "myFunc",
                externalModuleName: "env",
                externalBaseName: "myFunc",
                paramTypes: paramTypes,
                resultTypes: resultTypes
            );

            string wat = module.ToText();

            Assert.False(string.IsNullOrEmpty(wat));

            // Check that the WAT contains the function import
            Assert.Contains("(import \"env\" \"myFunc\"", wat);
            Assert.Contains("(func $myFunc", wat);
        }

        [Fact]
        public void ToText_ShouldReturnNonEmptyString()
        {
            using var module = new Module();
            string wat = module.ToText();

            Assert.False(string.IsNullOrEmpty(wat));
            Assert.Contains("(module", wat);
        }

        [Fact]
        public void ToBinary_ShouldReturnNonEmptyByteArray()
        {
            using var module = new Module();
            byte[] binary = module.ToBinary();

            Assert.NotNull(binary);
            Assert.NotEmpty(binary);
        }

        [Fact]
        public void ToBinary_CanBeCalledMultipleTimes()
        {
            using var module = new Module();
            byte[] binary1 = module.ToBinary();
            byte[] binary2 = module.ToBinary();

            Assert.NotNull(binary1);
            Assert.NotEmpty(binary1);
            Assert.NotNull(binary2);
            Assert.NotEmpty(binary2);
            Assert.Equal(binary1.Length, binary2.Length); // basic sanity check
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

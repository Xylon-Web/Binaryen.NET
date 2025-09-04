using System;
using System.Collections.Generic;
using Binaryen.NET;
using Xunit;

namespace Binaryen.NET.Tests
{
    public class ModuleTests
    {
        [Fact]
        public void InstantiateModule_ShouldNotBeNull()
        {
            using var module = new BinaryenModule();
            Assert.NotNull(module);
        }

        [Fact]
        public void ParseModule_FromWAT_ShouldNotBeNull()
        {
            string wat = "(module)";
            using var module = BinaryenModule.Parse(wat);
            Assert.NotNull(module);
        }

        [Fact]
        public void AddFunction_ShouldAppearInWAT()
        {
            using var module = new BinaryenModule();

            var body = BinaryenExpression.Nop(module);
            module.AddFunction("myFunc", body);

            string wat = module.ToText();
            Assert.Contains("(func $myFunc", wat);
        }

        [Fact]
        public void AddFunctionExport_ShouldAppearInWAT()
        {
            using var module = new BinaryenModule();

            // Add a dummy function first so it can be exported
            var body = BinaryenExpression.Nop(module);
            module.AddFunction("internalFunc", body);

            module.AddFunctionExport("internalFunc", "externalFunc");
            string wat = module.ToText();

            Assert.False(string.IsNullOrEmpty(wat));
            Assert.Contains("(export \"externalFunc\" (func $internalFunc))", wat);
            Assert.Contains("(func $internalFunc", wat);
        }

        [Fact]
        public void ToText_ShouldReturnNonEmptyString()
        {
            using var module = new BinaryenModule();
            string wat = module.ToText();
            Assert.False(string.IsNullOrEmpty(wat));
            Assert.Contains("(module", wat);
        }

        [Fact]
        public void ToBinary_ShouldReturnNonEmptyByteArray()
        {
            using var module = new BinaryenModule();
            byte[] binary = module.ToBinary();
            Assert.NotNull(binary);
            Assert.NotEmpty(binary);
        }

        [Fact]
        public void ToBinary_CanBeCalledMultipleTimes()
        {
            using var module = new BinaryenModule();
            byte[] binary1 = module.ToBinary();
            byte[] binary2 = module.ToBinary();

            Assert.NotNull(binary1);
            Assert.NotEmpty(binary1);
            Assert.NotNull(binary2);
            Assert.NotEmpty(binary2);
            Assert.Equal(binary1.Length, binary2.Length);
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            using var module = new BinaryenModule();
            module.Dispose(); // first call
            var ex = Record.Exception(() => module.Dispose()); // second call
            Assert.Null(ex);
        }
    }
}

# Binaryen.NET
C# Bindings for the Binaryen WebAssembly toolchain.

## Samples

### Generating IR -> WAT
```CSharp
using (var module = new BinaryenModule())
{
    // Create a simple function body with a single NOP instruction
    var body = BinaryenExpression.Nop(module);

    // Add a function with no parameters, no return type, and no locals
    module.AddFunction(
        name: "TestMethod",
        paramTypes: Array.Empty<BinaryenType>(),
        resultType: BinaryenType.None,
        localTypes: Array.Empty<BinaryenType>(),
        body: body);

    // Emit WAT
    Console.WriteLine(module.ToText());
}
```

Web Assembly Text (WAT) Output:
```WAT
(module
 (type $0 (func))
 (func $TestMethod
  (nop)
 )
)
```


# Binaryen.NET
C# Bindings for the Binaryen WebAssembly toolchain.

## Building
- **If you only changed C# code:** The project automatically uses the pre-built native binaries from `runtimes/`, no manual compilation is needed.
- **If you updated Binaryen itself:** Rebuild Binaryen as a shared library, then place the compiled binaries for each target runtime into the `runtimes/` folder.

You can build the C# project by running:
```bash
dotnet build
```
from the directory containing `Binaryen.NET.sln`.

To build or update the native Binaryen library (`libbinaryen`), follow the instructions in the [Binaryen repository](https://github.com/WebAssembly/binaryen).

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


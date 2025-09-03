using Binaryen.NET;

namespace Binaryen.NET.Tests;

public class ModuleTests
{
    [Fact]
    public void InstantiateModule()
    {
        var module = new Module();
        Assert.NotNull(module);
    }
}

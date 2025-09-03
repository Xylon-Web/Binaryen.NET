using System.Runtime.InteropServices;

namespace Binaryen.NET;

/// <summary>
/// Modules contain lists of functions, imports, exports, function types.
/// </summary>
public class Module : IDisposable
{
    #region Native
    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenModuleCreate();

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenModuleParse(string wat);

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static void BinaryenModuleDispose(IntPtr module);

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static void BinaryenAddFunctionExport(IntPtr module, string internalName, string externalName);

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static UIntPtr BinaryenModuleWriteText(IntPtr module, [Out] byte[] output, UIntPtr outputSize);
    #endregion

    /// <summary> Pointer to the native class. </summary>
    internal IntPtr Handle { get; set; }

    /// <summary> Creates a new instance of <see cref="Module"/>. </summary>
    public Module()
    {
        Handle = BinaryenModuleCreate();
    }

    /// <summary> Creates a new instance of <see cref="Module"/> from the given pointer. </summary>
    internal Module(IntPtr handle) => Handle = handle;

    /// <summary> Creates a new instance of <see cref="Module"/> from the given WAT string. </summary>
    /// <param name="wat"> The WAT string. </param>
    public static Module Parse(string wat) => new Module(BinaryenModuleParse(wat));

    /// <summary> Adds a function export. </summary>
    public void AddFunctionExport(string internalName, string externalName) => BinaryenAddFunctionExport(Handle, internalName, externalName);

    /// <summary> Converts the module to a WAT string. </summary>
    public string ToWAT()
    {
        byte[] buffer = new byte[65536];
        UIntPtr written = BinaryenModuleWriteText(Handle, buffer, (UIntPtr)buffer.Length);

        return System.Text.Encoding.UTF8.GetString(buffer, 0, (int)written);
    }

    public void Dispose()
    {
        if (Handle == IntPtr.Zero)
            return;

        BinaryenModuleDispose(Handle);
        GC.SuppressFinalize(this);

        Handle = IntPtr.Zero;
    }
}

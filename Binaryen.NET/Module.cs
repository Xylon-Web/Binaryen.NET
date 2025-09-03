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

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static void BinaryenAddFunctionImport(
    IntPtr module,
    string internalName,
    string externalModuleName,
    string externalBaseName,
    IntPtr paramTypes,
    IntPtr resultTypes);
    #endregion

    /// <summary> Pointer to the native class. </summary>
    internal IntPtr Handle { get; set; }

    private bool _disposed;

    /// <summary> Creates a new instance of <see cref="Module"/>. </summary>
    public Module()
    {
        Handle = BinaryenModuleCreate();
    }

    /// <summary> Creates a new instance of <see cref="Module"/> from the given pointer. </summary>
    internal Module(IntPtr handle) => Handle = handle;

    ~Module() => Dispose();

    /// <summary>
    /// Creates a new instance of <see cref="Module"/> from the given WAT string.
    /// </summary>
    /// <param name="wat"> The WAT string. </param>
    public static Module Parse(string wat) => new Module(BinaryenModuleParse(wat));

    /// <summary> Adds a function export to the module. </summary>
    public void AddFunctionExport(string internalName, string externalName) => BinaryenAddFunctionExport(Handle, internalName, externalName);

    /// <summary> Adds a function import to the module. </summary>
    public void AddFunctionImport(
        string internalName,
        string externalModuleName,
        string externalBaseName,
        BinaryenType paramTypes,
        BinaryenType resultTypes)
    {
        if (paramTypes == null) 
            throw new ArgumentNullException(nameof(paramTypes));

        if (resultTypes == null)
            throw new ArgumentNullException(nameof(resultTypes));

        BinaryenAddFunctionImport(
            Handle,
            internalName,
            externalModuleName,
            externalBaseName,
            paramTypes.Handle,
            resultTypes.Handle);
    }

    /// <summary> Converts the module to a WAT string. </summary>
    public string ToWAT()
    {
        // TODO: Handle buffer better
        byte[] buffer = new byte[65536];
        UIntPtr written = BinaryenModuleWriteText(Handle, buffer, (UIntPtr)buffer.Length);

        return System.Text.Encoding.UTF8.GetString(buffer, 0, (int)written);
    }

    public void Dispose()
    {
        if (!_disposed && Handle != IntPtr.Zero)
        {
            BinaryenModuleDispose(Handle);
            Handle = IntPtr.Zero;
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

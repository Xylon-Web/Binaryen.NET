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
    private extern static UIntPtr BinaryenModuleWrite(IntPtr module, [Out] byte[] output, UIntPtr outputSize);

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
        if (Handle == IntPtr.Zero)
            throw new ObjectDisposedException(nameof(Module));

        int bufferSize = 1024;
        while (true)
        {
            byte[] buffer = new byte[bufferSize];
            UIntPtr written = BinaryenModuleWriteText(Handle, buffer, (UIntPtr)buffer.Length);

            // If the module fits in the buffer, return the string
            if ((int)written < bufferSize)
                return System.Text.Encoding.UTF8.GetString(buffer, 0, (int)written);

            // Otherwise, double the buffer size and try again
            bufferSize *= 2;

            // Prevent excessively large allocations
            if (bufferSize > 16 * 1024 * 1024) // 16 MB
                throw new InvalidOperationException("Module is too large to serialize to WAT.");
        }
    }


    /// <summary> Serializes the module to its binary form. </summary>
    public byte[] ToBinary()
    {
        if (Handle == IntPtr.Zero)
            throw new ObjectDisposedException(nameof(Module));

        int bufferSize = 1024;
        while (true)
        {
            byte[] buffer = new byte[bufferSize];
            UIntPtr written = BinaryenModuleWrite(Handle, buffer, (UIntPtr)buffer.Length);

            // If the module fits in the buffer, return the bytes
            if ((int)written < bufferSize)
            {
                byte[] result = new byte[(int)written];
                Array.Copy(buffer, result, (int)written);
                return result;
            }

            // Otherwise, double the buffer size and try again
            bufferSize *= 2;

            // Prevent excessively large allocations
            if (bufferSize > 16 * 1024 * 1024) // 16 MB
                throw new InvalidOperationException("Module is too large to serialize to binary.");
        }
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

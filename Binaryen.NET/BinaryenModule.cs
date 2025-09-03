using System.Runtime.InteropServices;

namespace Binaryen.NET;

/// <summary>
/// Modules contain lists of functions, imports, exports, function types.
/// </summary>
public class BinaryenModule : IDisposable
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

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenAddFunction(
    IntPtr module,
    string name,
    IntPtr paramTypes,
    IntPtr resultTypes,
    IntPtr[] varTypes,
    uint numVarTypes,
    IntPtr body);
    #endregion

    /// <summary> Pointer to the native class. </summary>
    internal IntPtr Handle { get; set; }
    private bool _disposed;

    /// <summary> Creates a new instance of <see cref="BinaryenModule"/>. </summary>
    public BinaryenModule() => Handle = BinaryenModuleCreate();

    /// <summary> Creates a new instance of <see cref="BinaryenModule"/> from the given pointer. </summary>
    internal BinaryenModule(IntPtr handle) => Handle = handle;

    /// <summary> Finalizer. </summary>
    ~BinaryenModule() => Dispose();

    /// <summary>
    /// Creates a new instance of <see cref="BinaryenModule"/> from the given WAT string.
    /// </summary>
    /// <param name="wat"> The WAT string. </param>
    public static BinaryenModule Parse(string wat) => new BinaryenModule(BinaryenModuleParse(wat));

    /// <summary> Adds a function export to the module. </summary>
    public void AddFunctionExport(string internalName, string externalName) => BinaryenAddFunctionExport(Handle, internalName, externalName);

    /// <summary> Adds a function import to the module. </summary>
    public void AddFunctionImport(
        string internalName,
        string externalModuleName,
        string externalBaseName,
        IEnumerable<BinaryenType> paramTypes,
        BinaryenType resultType)
    {
        // No types specified, use default types
        if (paramTypes.Count() == 0)
            paramTypes = new BinaryenType[] { BinaryenType.None };

        IntPtr paramTypesHandle = BinaryenType.Combine(paramTypes.ToArray()).Handle;

        BinaryenAddFunctionImport(
            Handle,
            internalName,
            externalModuleName,
            externalBaseName,
            paramTypesHandle,
            resultType.Handle);
    }

    /// <summary> Adds a function to the module. </summary>
    public IntPtr AddFunction(
        string name,
        IEnumerable<BinaryenType> paramTypes,
        BinaryenType resultType,
        IEnumerable<BinaryenType> localTypes,
        BinaryenExpression body)
    {
        if (paramTypes.Count() == 0)
            paramTypes = new[] { BinaryenType.None };
        if (resultType == null)
            resultType = BinaryenType.None;

        IntPtr paramHandle = BinaryenType.Combine(paramTypes.ToArray()).Handle;
        IntPtr resultHandle = resultType.Handle;

        // locals must be passed as raw array of IntPtr
        IntPtr[] localHandles = localTypes.Select(t => t.Handle).ToArray();
        uint numLocals = (uint)localHandles.Length;

        return BinaryenAddFunction(
            Handle,
            name,
            paramHandle,
            resultHandle,
            localHandles,
            numLocals,
            body.Handle);
    }



    /// <summary> Converts the module to a Web Assembly Text (WAT) string. </summary>
    public string ToText()
    {
        if (Handle == IntPtr.Zero)
            throw new ObjectDisposedException(nameof(BinaryenModule));

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


    /// <summary> Converts the module to a Web Assembly (WASM) byte array. </summary>
    public byte[] ToBinary()
    {
        if (Handle == IntPtr.Zero)
            throw new ObjectDisposedException(nameof(BinaryenModule));

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

using System;
using System.Collections.Generic;
using System.Linq;
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
        UIntPtr paramTypes,
        UIntPtr resultTypes);

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenAddFunction(
        IntPtr module,
        string name,
        UIntPtr paramTypes,
        UIntPtr resultTypes,
        UIntPtr[] varTypes,
        uint numVarTypes,
        IntPtr body);
    #endregion

    internal IntPtr Handle { get; private set; }
    private bool _disposed;

    public BinaryenModule() => Handle = BinaryenModuleCreate();
    internal BinaryenModule(IntPtr handle) => Handle = handle;
    ~BinaryenModule() => Dispose();

    public static BinaryenModule Parse(string wat) => new BinaryenModule(BinaryenModuleParse(wat));

    public void AddFunctionExport(string internalName, string externalName) =>
        BinaryenAddFunctionExport(Handle, internalName, externalName);

    public void AddFunctionImport(
        string internalName,
        string externalModuleName,
        string externalBaseName,
        IEnumerable<BinaryenType>? paramTypes = null,
        BinaryenType? resultType = null)
    {
        paramTypes ??= new[] { BinaryenType.None };
        resultType ??= BinaryenType.None;

        // Use BinaryenType.Create to combine multiple param types
        UIntPtr combinedParams = BinaryenType.Create(paramTypes.ToArray()).Handle;
        UIntPtr combinedResult = resultType.Handle;

        BinaryenAddFunctionImport(
            Handle,
            internalName,
            externalModuleName,
            externalBaseName,
            combinedParams,
            combinedResult);
    }

    public IntPtr AddFunction(
        string name,
        BinaryenExpression body,
        IEnumerable<BinaryenType>? paramTypes = null,
        BinaryenType? resultType = null,
        IEnumerable<BinaryenType>? localTypes = null)
    {
        paramTypes ??= Array.Empty<BinaryenType>();
        resultType ??= BinaryenType.None;
        localTypes ??= Array.Empty<BinaryenType>();

        UIntPtr combinedParams = BinaryenType.Create(paramTypes.ToArray()).Handle;
        UIntPtr combinedResult = resultType.Handle;
        UIntPtr[] locals = localTypes.Select(t => t.Handle).ToArray();
        uint numLocals = (uint)locals.Length;

        return BinaryenAddFunction(Handle, name, combinedParams, combinedResult, locals, numLocals, body.Handle);
    }

    public string ToText()
    {
        if (Handle == IntPtr.Zero)
            throw new ObjectDisposedException(nameof(BinaryenModule));

        int bufferSize = 1024;
        while (true)
        {
            byte[] buffer = new byte[bufferSize];
            UIntPtr written = BinaryenModuleWriteText(Handle, buffer, (UIntPtr)buffer.Length);

            if ((int)written < bufferSize)
                return System.Text.Encoding.UTF8.GetString(buffer, 0, (int)written);

            bufferSize *= 2;
            if (bufferSize > 16 * 1024 * 1024)
                throw new InvalidOperationException("Module is too large to serialize to WAT.");
        }
    }

    public byte[] ToBinary()
    {
        if (Handle == IntPtr.Zero)
            throw new ObjectDisposedException(nameof(BinaryenModule));

        int bufferSize = 1024;
        while (true)
        {
            byte[] buffer = new byte[bufferSize];
            UIntPtr written = BinaryenModuleWrite(Handle, buffer, (UIntPtr)buffer.Length);

            if ((int)written < bufferSize)
            {
                byte[] result = new byte[(int)written];
                Array.Copy(buffer, result, (int)written);
                return result;
            }

            bufferSize *= 2;
            if (bufferSize > 16 * 1024 * 1024)
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

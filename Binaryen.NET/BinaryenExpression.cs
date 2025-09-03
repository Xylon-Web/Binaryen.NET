
using System.Runtime.InteropServices;

namespace Binaryen.NET;

/// <summary>
/// A Binaryen expression.
/// </summary>
public class BinaryenExpression
{
    #region Native
    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr BinaryenDrop(IntPtr module, IntPtr value);

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr BinaryenLocalGet(IntPtr module, uint index, IntPtr type);

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr BinaryenLocalSet(IntPtr module, uint index, IntPtr value);

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr BinaryenNop(IntPtr module);


    #endregion

    /// <summary> Pointer to the native class. </summary>
    internal IntPtr Handle { get; }

    /// <summary> Creates a new instance of <see cref="BinaryenType"/>. </summary>
    internal BinaryenExpression(IntPtr handle) => Handle = handle;

    public static BinaryenExpression Drop(BinaryenModule module, BinaryenExpression value)
    {
        var handle = BinaryenDrop(module.Handle, value.Handle);
        return new BinaryenExpression(handle);
    }

    public static BinaryenExpression LocalGet(BinaryenModule module, uint index, BinaryenType type)
    {
        var handle = BinaryenLocalGet(module.Handle, index, type.Handle);
        return new BinaryenExpression(handle);
    }

    public static BinaryenExpression LocalSet(BinaryenModule module, uint index, BinaryenExpression value)
    {
        var handle = BinaryenLocalSet(module.Handle, index, value.Handle);
        return new BinaryenExpression(handle);
    }

    public static BinaryenExpression Nop(BinaryenModule module)
    {
        var handle = BinaryenNop(module.Handle);
        return new BinaryenExpression(handle);
    }

    public static BinaryenExpression Combine(params BinaryenExpression[] values)
    {
        if (values == null || values.Length == 0)
            return null;

        IntPtr combined = values[0].Handle;
        for (int i = 1; i < values.Length; i++)
            combined |= values[i].Handle; // Combine bitmask

        return new BinaryenExpression(combined);
    }
}

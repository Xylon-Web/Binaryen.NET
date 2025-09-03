
using System.Runtime.InteropServices;

namespace Binaryen.NET;

/// <summary>
/// A Binaryen type.
/// </summary>
public class BinaryenType
{
    #region Native
    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenTypeNone();

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenTypeInt32();

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenTypeInt64();

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenTypeFloat32();

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenTypeFloat64();

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenTypeVec128();
    #endregion

    /// <summary> Pointer to the native class. </summary>
    internal IntPtr Handle { get; }

    /// <summary> Creates a new instance of <see cref="BinaryenType"/>. </summary>
    internal BinaryenType(IntPtr handle) => Handle = handle;

    public static BinaryenType None { get; } = new BinaryenType(BinaryenTypeNone());
    public static BinaryenType Int32 { get; } = new BinaryenType(BinaryenTypeInt32());
    public static BinaryenType Int64 { get; } = new BinaryenType(BinaryenTypeInt64());
    public static BinaryenType Float32 { get; } = new BinaryenType(BinaryenTypeFloat32());
    public static BinaryenType Float64 { get; } = new BinaryenType(BinaryenTypeFloat64());
    public static BinaryenType Vec128 { get; } = new BinaryenType(BinaryenTypeVec128());

    /// <summary>
    /// Combines multiple <see cref="BinaryenType"/> into one.
    /// </summary>
    public static BinaryenType Combine(params BinaryenType[] types)
    {
        if (types == null || types.Length == 0)
            return None;

        IntPtr combined = types[0].Handle;
        for (int i = 1; i < types.Length; i++)
            combined |= types[i].Handle; // Combine bitmask

        return new BinaryenType(combined);
    }
}

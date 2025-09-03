using System.Runtime.InteropServices;

namespace Binaryen.NET;

/// <summary>
/// Modules contain lists of functions, imports, exports, function types.
/// </summary>
public class Module
{
    #region Native
    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static IntPtr BinaryenModuleCreate();

    [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
    private extern static void BinaryenModuleDispose(IntPtr module);
    #endregion

    /// <summary> Pointer to the native class. </summary>
    internal IntPtr Handle { get; }

    /// <summary> Creates a new instance of <see cref="Module"/>. </summary>
    public Module()
    {
        Handle = BinaryenModuleCreate();
    }

    /// <summary> Cleans up the native class. </summary>
    ~Module()
    {
        BinaryenModuleDispose(Handle);
    }
}

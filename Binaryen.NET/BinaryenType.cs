using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Binaryen.NET
{
    /// <summary>
    /// A Binaryen type.
    /// </summary>
    public class BinaryenType
    {
        #region Native
        [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr BinaryenTypeCreate([In] UIntPtr[] valueTypes, uint numTypes);

        [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr BinaryenTypeNone();

        [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr BinaryenTypeInt32();

        [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr BinaryenTypeInt64();

        [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr BinaryenTypeFloat32();

        [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr BinaryenTypeFloat64();

        [DllImport(Interop.CPPDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern UIntPtr BinaryenTypeVec128();
        #endregion

        /// <summary> The underlying numeric representation of the type. </summary>
        internal UIntPtr Handle { get; }

        /// <summary> Creates a new instance of <see cref="BinaryenType"/> from a native handle. </summary>
        internal BinaryenType(UIntPtr handle) => Handle = handle;

        /// <summary> Creates a BinaryenType representing multiple types. </summary>
        internal static BinaryenType Create(params BinaryenType[] types)
        {
            if (types == null || types.Length == 0)
                return None;

            UIntPtr[] nativeTypes = types.Select(t => t.Handle).ToArray();
            UIntPtr combined = BinaryenTypeCreate(nativeTypes, (uint)nativeTypes.Length);
            return new BinaryenType(combined);
        }

        public static BinaryenType None { get; } = new BinaryenType(BinaryenTypeNone());
        public static BinaryenType Int32 { get; } = new BinaryenType(BinaryenTypeInt32());
        public static BinaryenType Int64 { get; } = new BinaryenType(BinaryenTypeInt64());
        public static BinaryenType Float32 { get; } = new BinaryenType(BinaryenTypeFloat32());
        public static BinaryenType Float64 { get; } = new BinaryenType(BinaryenTypeFloat64());
        public static BinaryenType Vec128 { get; } = new BinaryenType(BinaryenTypeVec128());
    }
}

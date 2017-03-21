using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace HttpMockSlim.Utils
{
    public static class MemSetArrayExtensions
    {
        private delegate void MemorySetter(IntPtr array, byte value, int count);
        private static readonly MemorySetter _memset;
        static MemSetArrayExtensions()
        {
            _memset = CreateMemset();
        }
        private static MemorySetter CreateMemset()
        {
            var m = new DynamicMethod(
                "memset",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(void),
                new[] { typeof(IntPtr), typeof(byte), typeof(int) },
                typeof(MemSetArrayExtensions),
                false);
            var il = m.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // address
            il.Emit(OpCodes.Ldarg_1); // initialization value
            il.Emit(OpCodes.Ldarg_2); // number of bytes
            il.Emit(OpCodes.Initblk);
            il.Emit(OpCodes.Ret);
            return (MemorySetter)m.CreateDelegate(typeof(MemorySetter));
        }
        /// <summary>
        /// c# version of memset for performance freaks :)
        /// https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.initblk%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        /// </summary>
        public static void Memset(this byte[] array, byte value, int count, int offset = 0)
        {
            GCHandle h = default(GCHandle);
            try
            {
                h = GCHandle.Alloc(array, GCHandleType.Pinned);
                IntPtr addr = h.AddrOfPinnedObject() + offset;
                _memset(addr, value, count);
            }
            finally
            {
                if (h.IsAllocated)
                    h.Free();
            }
        }
    }
}
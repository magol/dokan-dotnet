using System;
using System.CodeDom;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    /// <summary>
    /// This structure is used for copying UNICODE_STRING from the kernel mode driver
    /// into the user mode driver.
    /// https://msdn.microsoft.com/en-us/library/windows/hardware/ff564879(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
    public struct UNICODE_STRING
    {
        public ushort _length;
        public ushort _maximumLength;

        /// <summary>
        /// Pointer to a buffer used to contain a string of wide characters.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public IntPtr _buffer;

        /// <summary>
        /// The length, in bytes, of the string stored in <see cref="Buffer"/>.
        /// </summary>
        //public ushort Length => _length;

        /// <summary>
        /// The length, in bytes, of <see cref="Buffer"/>.
        /// </summary>
        //public ushort MaximumLength => _maximumLength;



        public UNICODE_STRING(string s)
        {
            _length = (ushort) (s.Length * 2);
            _maximumLength = (ushort) (_length + 2);
            _buffer = Marshal.StringToHGlobalUni(s);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_buffer);
            _buffer = IntPtr.Zero;
        }

        public override string ToString()
        {
            return Marshal.PtrToStringUni(_buffer)
                   ?? string.Empty;
        }

        public static explicit operator UNICODE_STRING(string s)
        {
            return new UNICODE_STRING(s);
        }

        public static implicit operator string(UNICODE_STRING s)
        {
            return s.ToString();
        }
    }
}

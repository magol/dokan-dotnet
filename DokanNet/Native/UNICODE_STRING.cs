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
    [StructLayout(LayoutKind.Sequential)]
    public struct UNICODE_STRING : IDisposable
    {
        public ushort Length;
        public ushort MaximumLength;
        private IntPtr buffer;


        public UNICODE_STRING(string s)
        {
            Length = (ushort) (s.Length * 2);
            MaximumLength = (ushort) (Length + 2);
            buffer = Marshal.StringToHGlobalUni(s);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(buffer);
            buffer = IntPtr.Zero;
        }

        public override string ToString()
        {
            return Marshal.PtrToStringUni(buffer)
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

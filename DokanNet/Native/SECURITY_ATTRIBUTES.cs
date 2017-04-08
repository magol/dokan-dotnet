using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace DokanNet.Native
{
    /// <summary>
    /// TODO   remove?
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }
}

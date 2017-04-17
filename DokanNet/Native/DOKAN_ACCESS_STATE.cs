using System;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DOKAN_ACCESS_STATE
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool SecurityEvaluated;
        [MarshalAs(UnmanagedType.I1)]
        public bool GenerateAudit;
        [MarshalAs(UnmanagedType.I1)]
        public bool GenerateOnClose;
        [MarshalAs(UnmanagedType.I1)]
        public bool AuditPrivileges;
        public uint Flags;
        public uint RemainingDesiredAccess;
        public uint PreviouslyGrantedAccess;
        public uint OriginalDesiredAccess;
        public IntPtr SecurityDescriptor;
        public UNICODE_STRING ObjectName;
        public UNICODE_STRING ObjectType;
    }
}

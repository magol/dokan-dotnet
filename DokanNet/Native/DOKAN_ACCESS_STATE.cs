using System;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DOKAN_ACCESS_STATE
    {
        public bool SecurityEvaluated;
        public bool GenerateAudit;
        public bool GenerateOnClose;
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

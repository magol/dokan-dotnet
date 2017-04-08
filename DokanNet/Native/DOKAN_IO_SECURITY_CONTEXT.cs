using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DOKAN_IO_SECURITY_CONTEXT
    {
        public DOKAN_ACCESS_STATE AccessState;
        public uint DesiredAccess;
    }
}

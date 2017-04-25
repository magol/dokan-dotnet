using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    /// <summary>
    /// Dokan mount options used to describe dokan device behaviour
    /// </summary>
    /// <see cref="NativeMethods.DokanMain"/>
    /// <remarks>This is the same structure as <c>PDOKAN_OPTIONS</c> (dokan.h) in the C++ version of Dokan.</remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    internal struct DOKAN_OPTIONS
    {
        private ushort _version;
        private ushort _threadCount;
        private uint _options;
        private ulong _globalContext;
        [MarshalAs(UnmanagedType.LPWStr)]
        private string _mountPoint;
        [MarshalAs(UnmanagedType.LPWStr)]
        private string _uncName;
        private uint _timeout;
        private uint _allocationUnitSize;
        private uint _sectorSize;

        /// <summary>
        /// Number of threads to be used internally by Dokan library. More thread will handle more event at the same time.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ushort ThreadCount
        {
            get => _threadCount;
            set => _threadCount = value;
        }

        /// <summary>
        /// Version of the dokan features requested (version "123" is equal to Dokan version 1.2.3).
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ushort Version
        {
            get => _version;
            set => _version = value;
        }

        /// <summary>
        /// Features enable for the mount. See <see cref="DokanOptions"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public uint Options
        {
            get => _options;
            set => _options = value;
        }

        /// <summary>
        /// FileSystem can store anything here.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public ulong GlobalContext
        {
            get => _globalContext;
            set => _globalContext = value;
        }

        /// <summary>
        /// Mount point.
        /// Can be <c>M:\\</c>(drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string MountPoint
        {
            get => _mountPoint;
            set => _mountPoint = value;
        }

        /// <summary>
        /// UNC name used for network volume.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string UncName
        {
            get => _uncName;
            set => _uncName = value;
        }

        /// <summary>
        /// Max timeout in milliseconds of each request before Dokan give up.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public uint Timeout
        {
            get => _timeout;
            set => _timeout = value;
        }

        /// <summary>
        /// Allocation Unit Size of the volume. This will behave on the file size.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public uint AllocationUnitSize
        {
            get => _allocationUnitSize;
            set => _allocationUnitSize = value;
        }

        /// <summary>
        /// Sector Size of the volume. This will behave on the file size.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public uint SectorSize
        {
            get => _sectorSize;
            set => _sectorSize = value;
        }
    }
}
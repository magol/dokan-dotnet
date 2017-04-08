using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace DokanNet
{
    public struct DokanFileSystemSecurity
    {
        [DllImport("advapi32.dll")]
        private static extern int GetSecurityDescriptorLength(IntPtr pSecurityDescriptor);

        private readonly IntPtr _rawSecurityDescriptor;
        private readonly int _length;

        /// <summary>
        /// Construct DokanFileSystemSecurity.
        /// </summary>
        /// <param name="rawSecurityDescriptor">A <see cref="IntPtr"/> to a raw File System Security Descriptor.</param>
        /// <param name="length">The length of <paramref name="rawSecurityDescriptor"/>.
        /// If no length is given, it use <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa446650/">GetSecurityDescriptorLength</see> to get it.</param>
        public DokanFileSystemSecurity(IntPtr rawSecurityDescriptor, int length = -1)
        {
            _rawSecurityDescriptor = rawSecurityDescriptor;
            _length = length < 0
                ? GetSecurityDescriptorLength(rawSecurityDescriptor)
                : length;
        }

        /// <summary>
        /// Get a <see cref="DirectorySecurity"/>.
        /// </summary>
        /// <returns>A <see cref="DirectorySecurity"/>.</returns>
        [Pure]
        public DirectorySecurity GetAsDirectorySecurity()
        {
            var security = new DirectorySecurity();
            return GetAsFileSystemSecurity(security);
        }

        /// <summary>
        /// Get a <see cref="FileSecurity"/>.
        /// </summary>
        /// <returns>A <see cref="FileSecurity"/>.</returns>
        [Pure]
        public FileSecurity GetAsFileSecurity()
        {
            var security = new FileSecurity();
            return GetAsFileSystemSecurity(security);
        }

        /// <summary>
        /// Inject the raw security descriptor into <paramref name="fileSystemSecurity"/>.
        /// </summary>
        /// <typeparam name="T">A <see cref="FileSystemSecurity"/> of type <see cref="FileSecurity"/> or <see cref="DirectorySecurity"/>.</typeparam>
        /// <param name="fileSystemSecurity">The <see cref="FileSystemSecurity"/> to inject the raw secutiry descriptor into.</param>
        /// <returns>The <paramref name="fileSystemSecurity"/> with injected security descriptor.</returns>
        private T GetAsFileSystemSecurity<T>(T fileSystemSecurity) where T : FileSystemSecurity
        {
            if (_length <= 0 || _rawSecurityDescriptor == IntPtr.Zero)
            {
                return null;
            }

            var buffer = new byte[_length];

            Marshal.Copy(_rawSecurityDescriptor, buffer, 0, _length);

            fileSystemSecurity.SetSecurityDescriptorBinaryForm(buffer);

            return fileSystemSecurity;
        }
    }
}

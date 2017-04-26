using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DokanNet.Native;
using static DokanNet.FormatProviders;

namespace DokanNet
{
    /// <summary>
    /// %Dokan file information on the current operation.
    /// </summary>
    /// <remarks>
    /// This class cannot be instantiated in C#, it is created by the kernel %Dokan driver.
    /// This is the same structure as <c>_DOKAN_FILE_INFO</c> (dokan.h) in the C++ version of Dokan.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public sealed class DokanFileInfo
    {
        private ulong _context;

        /// <summary>
        /// Used internally, never modify.
        /// </summary>
        private readonly ulong dokanContext;

        /// <summary>
        /// A pointer to the <see cref="DOKAN_OPTIONS"/> which was passed to <see cref="DokanNet.Native.NativeMethods.DokanMain"/>.
        /// </summary>
        private readonly IntPtr dokanOptions;

        private readonly uint processId;

        [MarshalAs(UnmanagedType.U1)] private bool _isDirectory;

        [MarshalAs(UnmanagedType.U1)] private bool _deleteOnClose;

        [MarshalAs(UnmanagedType.U1)] private bool _pagingIo;

        [MarshalAs(UnmanagedType.U1)] private bool _synchronousIo;

        [MarshalAs(UnmanagedType.U1)] private bool _noCache;

        [MarshalAs(UnmanagedType.U1)] private bool _writeToEndOfFile;

        /// <summary>
        /// Prevents a default instance of the <see cref="DokanFileInfo"/> class from being created. 
        /// The class is created by the %Dokan kernel driver.
        /// </summary>
        private DokanFileInfo()
        {
        }

        /// <summary>
        /// Gets or sets context that can be used to carry information between operation.
        /// The Context can carry whatever type like <c><see cref="System.IO.FileStream"/></c>, <c>struct</c>, <c>int</c>,
        /// or internal reference that will help the implementation understand the request context of the event.
        /// </summary>
        public object Context
        {
            get => _context != 0 
                ? ((GCHandle)(IntPtr)_context).Target 
                : null;
            set
            {
                if (_context != 0)
                {
                    ((GCHandle)(IntPtr)_context).Free();
                    _context = 0;
                }

                if (value != null)
                {
                    _context = (ulong)(IntPtr)GCHandle.Alloc(value);
                }
            }
        }

        /// <summary>
        /// Process id for the thread that originally requested a given I/O
        /// operation.
        /// </summary>
        public int ProcessId => (int)processId;

        /// <summary>
        /// Gets or sets a value indicating whether it requesting a directory
        /// file. Must be set in <see cref="IDokanOperations.CreateFile"/> if
        /// the file appear to be a folder.
        /// </summary>
        public bool IsDirectory
        {
            get => _isDirectory;
            set => _isDirectory = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the file has to be delete
        /// during the <see cref="IDokanOperations.Cleanup"/> event.
        /// </summary>
        public bool DeleteOnClose
        {
            get => _deleteOnClose;
            set => _deleteOnClose = value;
        }

        /// <summary>
        /// Read or write is paging IO.
        /// </summary>
        public bool PagingIo => _pagingIo;

        /// <summary>
        /// Read or write is synchronous IO.
        /// </summary>
        public bool SynchronousIo => _synchronousIo;

        /// <summary>
        /// Read or write directly from data source without cache.
        /// </summary>
        public bool NoCache => _noCache;

        /// <summary>
        /// If <c>true</c>, write to the current end of file instead 
        /// of using the <c>Offset</c> parameter.
        /// </summary>
        public bool WriteToEndOfFile => _writeToEndOfFile;

        /// <summary>
        /// This method needs to be called in <see cref="IDokanOperations.CreateFile"/>.
        /// </summary>
        /// <returns>An <c><see cref="WindowsIdentity"/></c> with the access token, 
        /// -or- <c>null</c> if the operation was not successful.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",   Justification = "Should not be a property")]
        public WindowsIdentity GetRequestor()
        {
            try
            {
                return new WindowsIdentity(NativeMethods.DokanOpenRequestorToken(this));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extends the time out of the current IO operation in driver.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to extend with.</param>
        /// <returns>If the operation was successful.</returns>
        public bool TryResetTimeout(int milliseconds)
        {
            return NativeMethods.DokanResetTimeout((uint)milliseconds, this);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return
                DokanFormat(
                    $"{{{Context}, {DeleteOnClose}, {IsDirectory}, {NoCache}, {PagingIo}, #{ProcessId}, {SynchronousIo}, {WriteToEndOfFile}}}");
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="other">The object to compare with the current instance. </param>
        /// <returns><c>true</c> if <paramref name="other" /> and this instance are represent the same value; 
        /// otherwise, <c>false</c>. </returns>
        public bool Equals(DokanFileInfo other)
        {
            if (ReferenceEquals(null, other)) return false;

            return
                _context == other._context &&
                dokanContext == other.dokanContext &&
                dokanOptions.Equals(other.dokanOptions) &&
                processId == other.processId &&
                _isDirectory == other._isDirectory &&
                _deleteOnClose == other._deleteOnClose &&
                _pagingIo == other._pagingIo &&
                _synchronousIo == other._synchronousIo &&
                _noCache == other._noCache &&
                _writeToEndOfFile == other._writeToEndOfFile;
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns><c>true</c> if <paramref name="obj" /> and this instance are the same type and represent the same value; 
        /// otherwise, <c>false</c>. </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var o = obj as DokanFileInfo;
            return o != null && Equals(o);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _context.GetHashCode();
                hashCode = (hashCode * 397) ^ dokanContext.GetHashCode();
                hashCode = (hashCode * 397) ^ dokanOptions.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) processId;
                hashCode = (hashCode * 397) ^ _isDirectory.GetHashCode();
                hashCode = (hashCode * 397) ^ _deleteOnClose.GetHashCode();
                hashCode = (hashCode * 397) ^ _pagingIo.GetHashCode();
                hashCode = (hashCode * 397) ^ _synchronousIo.GetHashCode();
                hashCode = (hashCode * 397) ^ _noCache.GetHashCode();
                hashCode = (hashCode * 397) ^ _writeToEndOfFile.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>Indicates whether two objects are equal.</summary>
        /// <param name="left">The first object to compare. </param>
        /// <param name="right">The secound object to compare. </param>
        /// <returns><c>true</c> if <paramref name="left" /> and <paramref name="right" /> are represent the same value; 
        /// otherwise, <c>false</c>. </returns>
        public static bool operator ==(DokanFileInfo left, DokanFileInfo right)
        {
            return Equals(left, right);
        }

        /// <summary>Indicates whether two objects are not equal.</summary>
        /// <param name="left">The first object to compare. </param>
        /// <param name="right">The secound object to compare. </param>
        /// <returns><c>true</c> if <paramref name="left" /> and <paramref name="right" /> are not represent the same value; 
        /// otherwise, <c>false</c>. </returns>
        public static bool operator !=(DokanFileInfo left, DokanFileInfo right)
        {
            return !Equals(left, right);
        }
    }
}
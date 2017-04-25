#if !(NET10 || NET11 || NET20 || NET30 || NET35 || NET40 ) 
#define NET45_OR_GREATER   
#endif

#if NET45_OR_GREATER && !(NET45 ) 
#define NET451_OR_GREATER   
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;

using DokanNet.Logging;
using DokanNet.Native;

using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;



namespace DokanNet
{
    /// <summary>
    /// The dokan operation proxy.
    /// </summary>
    internal sealed class DokanOperationProxy
    {
        #region Delegates

        public delegate NtStatus ZwCreateFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            IntPtr securityContext,
            uint rawDesiredAccess,
            uint rawFileAttributes,
            uint rawShareAccess,
            uint rawCreateDisposition,
            uint rawCreateOptions,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo dokanFileInfo);

        public delegate void CleanupDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate void CloseFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate NtStatus ReadFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), Out] byte[] rawBuffer,
            uint rawBufferLength,
            ref int rawReadLength,
            long rawOffset,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus WriteFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] rawBuffer,
            uint rawNumberOfBytesToWrite,
            ref int rawNumberOfBytesWritten,
            long rawOffset,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus FlushFileBuffersDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetFileInformationDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            ref BY_HANDLE_FILE_INFORMATION handleFileInfo,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo fileInfo);

        public delegate NtStatus FindFilesDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            IntPtr rawFillFindData,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus FindFilesWithPatternDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawSearchPattern,
            IntPtr rawFillFindData, 
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetFileAttributesDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            uint rawAttributes,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetFileTimeDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            ref FILETIME rawCreationTime,
            ref FILETIME rawLastAccessTime,
            ref FILETIME rawLastWriteTime,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus DeleteFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus DeleteDirectoryDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus MoveFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawNewFileName,
            [MarshalAs(UnmanagedType.Bool)] bool rawReplaceIfExisting,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetEndOfFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetAllocationSizeDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus LockFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus UnlockFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetDiskFreeSpaceDelegate(
            ref long rawFreeBytesAvailable, ref long rawTotalNumberOfBytes, ref long rawTotalNumberOfFreeBytes,
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetVolumeInformationDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawVolumeNameBuffer,
            uint rawVolumeNameSize,
            ref uint rawVolumeSerialNumber,
            ref uint rawMaximumComponentLength,
            ref FileSystemFeatures rawFileSystemFlags,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawFileSystemNameBuffer,
            uint rawFileSystemNameSize,
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetFileSecurityDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [In] ref SECURITY_INFORMATION rawRequestedInformation,
            IntPtr rawSecurityDescriptor,
            uint rawSecurityDescriptorLength,
            ref uint rawSecurityDescriptorLengthNeeded,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetFileSecurityDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [In] ref SECURITY_INFORMATION rawSecurityInformation,
            IntPtr rawSecurityDescriptor,
            uint rawSecurityDescriptorLength,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        /// <summary>
        /// Retrieve all FileStreams informations on the file.
        /// This is only called if <see cref="DokanOptions.AltStream"/> is enabled.
        /// </summary>
        /// <remarks>Supported since 0.8.0. 
        /// You must specify the version at <see cref="DOKAN_OPTIONS.Version"/>.</remarks>
        /// <param name="rawFileName">Filename</param>
        /// <param name="rawFillFindData">A <see cref="IntPtr"/> to a <see cref="FILL_FIND_STREAM_DATA"/>.</param>
        /// <param name="rawFileInfo">A <see cref="DokanFileInfo"/>.</param>
        /// <returns></returns>
        public delegate NtStatus FindStreamsDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            IntPtr rawFillFindData,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus MountedDelegate(
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate NtStatus UnmountedDelegate(
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        #endregion Delegates

        private readonly IDokanOperations _operations;

        private readonly ILogger _logger;

        private readonly uint _serialNumber;

        #region Enum masks
        /// <summary>
        /// To be used to mask out the <see cref="FileOptions"/> flags from what is returned 
        /// from <see cref="Native.NativeMethods.DokanMapKernelToUserCreateFileFlags"/>.
        /// </summary>
        private const int FileOptionsMask =
            (int)
            ( FileOptions.Asynchronous          | FileOptions.DeleteOnClose          | FileOptions.Encrypted 
            | FileOptions.None                  | FileOptions.RandomAccess           | FileOptions.SequentialScan 
            | FileOptions.WriteThrough);

        /// <summary>
        /// To be used to mask out the <see cref="FileAttributes"/> flags from what is returned 
        /// from <see cref="Native.NativeMethods.DokanMapKernelToUserCreateFileFlags"/>.
        /// Note that some flags where introduces in .NET Framework 4.5, and is not supported 
        /// in .NET Framework 4. 
        /// </summary>
        private const int FileAttributeMask =
            (int)
             ( FileAttributes.ReadOnly          | FileAttributes.Hidden              | FileAttributes.System
             | FileAttributes.Directory         | FileAttributes.Archive             | FileAttributes.Device
             | FileAttributes.Normal            | FileAttributes.Temporary           | FileAttributes.SparseFile
             | FileAttributes.ReparsePoint      | FileAttributes.Compressed          | FileAttributes.Offline
             | FileAttributes.NotContentIndexed | FileAttributes.Encrypted
#if NET45_OR_GREATER
             | FileAttributes.IntegrityStream   | FileAttributes.NoScrubData
#endif
            );

        /// <summary>
        /// To be used to mask out the <see cref="FileAccess"/> flags.
        /// </summary>
        private const long FileAccessMask =
            (uint)
            ( FileAccess.ReadData               | FileAccess.WriteData               | FileAccess.AppendData 
            | FileAccess.ReadExtendedAttributes | FileAccess.WriteExtendedAttributes | FileAccess.Execute 
            | FileAccess.DeleteChild            | FileAccess.ReadAttributes          | FileAccess.WriteAttributes
            | FileAccess.Delete                 | FileAccess.ReadPermissions         | FileAccess.ChangePermissions
            | FileAccess.SetOwnership           | FileAccess.Synchronize             | FileAccess.AccessSystemSecurity
            | FileAccess.MaximumAllowed         | FileAccess.GenericAll              | FileAccess.GenericExecute
            | FileAccess.GenericWrite           | FileAccess.GenericRead);

        /// <summary>
        /// To be used to mask out the <see cref="FileShare"/> flags.
        /// </summary>
        private const int FileShareMask =
            (int)
            ( FileShare.ReadWrite | FileShare.Delete | FileShare.Inheritable);
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DokanOperationProxy"/> class.
        /// </summary>
        /// <param name="operations">
        /// A <see cref="IDokanOperations"/> that contains the custom implementation of the driver.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> that handle all logging.
        /// </param>
        public DokanOperationProxy(IDokanOperations operations, ILogger logger)
        {
            this._operations = operations;
            this._logger = logger;
            _serialNumber = (uint)operations.GetHashCode();
        }

#pragma warning disable CA1303 //Do not pass literals as localized parameters

        /// <summary>
        /// CreateFile is called each time a request is made on a file system object.
        /// 
        /// In case <see cref="FileMode.OpenOrCreate"/> and
        /// <see cref="FileMode.Create"/> are opening successfully a already
        /// existing file, you have to return <see cref="DokanResult.AlreadyExists"/> instead of <see cref="NtStatus.Success"/>.
        /// 
        /// If the file is a directory, CreateFile is also called.
        /// In this case, CreateFile should return <see cref="NtStatus.Success"/> when that directory
        /// can be opened and <see cref="DokanFileInfo.IsDirectory"/> has to be set to <c>true</c>.
        /// 
        /// <see cref="DokanFileInfo.Context"/> can be used to store data (like <see cref="FileStream"/>)
        /// that can be retrieved in all other request related to the context.
        /// </summary>
        /// <param name="rawFileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="securityContext">SecurityContext, see <a href="https://msdn.microsoft.com/en-us/library/windows/hardware/ff550613(v=vs.85).aspx">IO_SECURITY_CONTEXT structure (MSDN)</a>.</param>
        /// <param name="rawDesiredAccess">Specifies an <a href="https://msdn.microsoft.com/en-us/library/windows/hardware/ff540466(v=vs.85).aspx">ACCESS_MASK (MSDN)</a> value that determines the requested access to the object.</param>
        /// <param name="rawFileAttributes">Specifies one or more FILE_ATTRIBUTE_XXX flags, which represent the file attributes to set if you create or overwrite a file.</param>
        /// <param name="rawShareAccess">Type of share access, which is specified as zero or any combination of <see cref="FileShare"/>.</param>
        /// <param name="rawCreateDisposition">Specifies the action to perform if the file does or does not exist.</param>
        /// <param name="rawCreateOptions">Specifies the options to apply when the driver creates or opens the file.</param>
        /// <param name="rawFileInfo">>An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns>The <see cref="NtStatus"/>.</returns>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/hardware/ff566424(v=vs.85).aspx">ZwCreateFile routine (MSDN)</a>
        /// <see cref="DokanNet.IDokanOperations.CreateFile"/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus ZwCreateFileProxy(
            string rawFileName,
            IntPtr securityContext,
            uint rawDesiredAccess,
            uint rawFileAttributes,
            uint rawShareAccess,
            uint rawCreateDisposition,
            uint rawCreateOptions,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                var fileAttributesAndFlags = 0;
                var creationDisposition = 0;
                NativeMethods.DokanMapKernelToUserCreateFileFlags(
                    rawFileAttributes,
                    rawCreateOptions,
                    rawCreateDisposition,
                    ref fileAttributesAndFlags,
                    ref creationDisposition);

                var fileAttributes = (FileAttributes)(fileAttributesAndFlags & FileAttributeMask);
                var fileOptions    = (FileOptions   )(fileAttributesAndFlags & FileOptionsMask);
                var desiredAccess  = (FileAccess    )(rawDesiredAccess       & FileAccessMask);
                var shareAccess    = (FileShare     )(rawShareAccess         & FileShareMask);

                _logger.Debug("CreateFileProxy : {0}", rawFileName);
                _logger.Debug("\tCreationDisposition\t{0}", (FileMode)creationDisposition);
                _logger.Debug("\tFileAccess\t{0}", (FileAccess)rawDesiredAccess);
                _logger.Debug("\tFileShare\t{0}", (FileShare)rawShareAccess);
                _logger.Debug("\tFileOptions\t{0}", fileOptions);
                _logger.Debug("\tFileAttributes\t{0}", fileAttributes);
                _logger.Debug("\tContext\t{0}", rawFileInfo);
                var result = _operations.CreateFile(
                    rawFileName,
                    desiredAccess,
                    shareAccess,
                    (FileMode)creationDisposition,
                    fileOptions,
                    fileAttributes,
                    rawFileInfo);

                _logger.Debug("CreateFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("CreateFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.Unsuccessful;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public void CleanupProxy(string rawFileName, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("{0} : {1}", nameof(CleanupProxy), rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                _operations.Cleanup(rawFileName, rawFileInfo);

                _logger.Debug("{0} : {1}", nameof(CleanupProxy), rawFileName);
            }
            catch (Exception ex)
            {
                _logger.Error("{0} : {1} Throw : {2}", nameof(CleanupProxy), rawFileName, ex.Message);
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public void CloseFileProxy(string rawFileName, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("{0} : {1}", nameof(CloseFileProxy), rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                _operations.CloseFile(rawFileName, rawFileInfo);

                _logger.Debug("{0} : {1}", nameof(CloseFileProxy), rawFileName);
            }
            catch (Exception ex)
            {
                _logger.Error("{0} : {1} Throw : {2}", nameof(CloseFileProxy), rawFileName, ex.Message);
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus ReadFileProxy(
            string rawFileName,
            byte[] rawBuffer,
            uint rawBufferLength,
            ref int rawReadLength,
            long rawOffset,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("ReadFileProxy : " + rawFileName);
                _logger.Debug("\tBufferLength\t" + rawBufferLength);
                _logger.Debug("\tOffset\t" + rawOffset);
                _logger.Debug("\tContext\t" + rawFileInfo);

                var result = _operations.ReadFile(rawFileName, rawBuffer, out rawReadLength, rawOffset, rawFileInfo);

                _logger.Debug("ReadFileProxy : " + rawFileName + " Return : " + result + " ReadLength : " + rawReadLength);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("ReadFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus WriteFileProxy(
            string rawFileName,
            byte[] rawBuffer,
            uint rawNumberOfBytesToWrite,
            ref int rawNumberOfBytesWritten,
            long rawOffset,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("WriteFileProxy : {0}", rawFileName);
                _logger.Debug("\tNumberOfBytesToWrite\t{0}", rawNumberOfBytesToWrite);
                _logger.Debug("\tOffset\t{0}", rawOffset);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.WriteFile(
                    rawFileName,
                    rawBuffer,
                    out rawNumberOfBytesWritten,
                    rawOffset,
                    rawFileInfo);

                _logger.Debug(
                    "WriteFileProxy : {0} Return : {1} NumberOfBytesWritten : {2}",
                    rawFileName,
                    result,
                    rawNumberOfBytesWritten);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("WriteFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus FlushFileBuffersProxy(string rawFileName, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("FlushFileBuffersProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.FlushFileBuffers(rawFileName, rawFileInfo);

                _logger.Debug("FlushFileBuffersProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("FlushFileBuffersProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus GetFileInformationProxy(
            string rawFileName,
            ref BY_HANDLE_FILE_INFORMATION rawHandleFileInformation,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("GetFileInformationProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.GetFileInformation(rawFileName, out FileInformation fi, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(fi.FileName != null, "FileName must not be null");
                    _logger.Debug("\tFileName\t{0}", fi.FileName);
                    _logger.Debug("\tAttributes\t{0}", fi.Attributes);
                    _logger.Debug("\tCreationTime\t{0}", fi.CreationTime);
                    _logger.Debug("\tLastAccessTime\t{0}", fi.LastAccessTime);
                    _logger.Debug("\tLastWriteTime\t{0}", fi.LastWriteTime);
                    _logger.Debug("\tLength\t{0}", fi.Length);

                    rawHandleFileInformation.FileAttributes = (uint)fi.Attributes /* + FILE_ATTRIBUTE_VIRTUAL*/;

                    var creationTime = ToFileTime(fi.CreationTime);
                    var lastAccessTime = ToFileTime(fi.LastAccessTime);
                    var lastWriteTime = ToFileTime(fi.LastWriteTime);
                    rawHandleFileInformation.CreationTime = creationTime;
                    rawHandleFileInformation.LastAccessTime = lastAccessTime;
                    rawHandleFileInformation.LastWriteTime = lastWriteTime;

                    rawHandleFileInformation.VolumeSerialNumber = _serialNumber;

                    rawHandleFileInformation.FileSize = fi.Length;
                    rawHandleFileInformation.NumberOfLinks = 1;
                    rawHandleFileInformation.FileIndexHigh = 0;
                    rawHandleFileInformation.FileIndexLow = (uint) (fi.FileName?.GetHashCode() ?? 0);
                }

                _logger.Debug("GetFileInformationProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("GetFileInformationProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus FindFilesProxy(string rawFileName, IntPtr rawFillFindData, DokanFileInfo rawFileInfo)
        {
            try
            {

                _logger.Debug("FindFilesProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.FindFiles(rawFileName, out IList<FileInformation> files, rawFileInfo);

                Debug.Assert(files != null, "Files must not be null");
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (var fi in files)
                    {
                        _logger.Debug("\tFileName\t{0}", fi.FileName);
                        _logger.Debug("\t\tAttributes\t{0}", fi.Attributes);
                        _logger.Debug("\t\tCreationTime\t{0}", fi.CreationTime);
                        _logger.Debug("\t\tLastAccessTime\t{0}", fi.LastAccessTime);
                        _logger.Debug("\t\tLastWriteTime\t{0}", fi.LastWriteTime);
                        _logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                    var fill = GetDataFromPointer<FILL_FIND_FILE_DATA>(rawFillFindData);

                    // used a single entry call to speed up the "enumeration" of the list
                    foreach (var t in files)
                    {
                        AddTo(fill, rawFileInfo, t);
                    }
                }

                _logger.Debug("FindFilesProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("FindFilesProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus FindFilesWithPatternProxy(
            string rawFileName,
            string rawSearchPattern,
            IntPtr rawFillFindData,
            DokanFileInfo rawFileInfo)
        {
            try
            {

                _logger.Debug("FindFilesWithPatternProxy : {0}", rawFileName);
                _logger.Debug("\trawSearchPattern\t{0}", rawSearchPattern);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.FindFilesWithPattern(rawFileName, rawSearchPattern, out IList<FileInformation> files, rawFileInfo);

                Debug.Assert(files != null, "Files must not be null");
                if (result == DokanResult.Success && files.Any())
                {
                    foreach (var fi in files)
                    {
                        _logger.Debug("\tFileName\t{0}", fi.FileName);
                        _logger.Debug("\t\tAttributes\t{0}", fi.Attributes);
                        _logger.Debug("\t\tCreationTime\t{0}", fi.CreationTime);
                        _logger.Debug("\t\tLastAccessTime\t{0}", fi.LastAccessTime);
                        _logger.Debug("\t\tLastWriteTime\t{0}", fi.LastWriteTime);
                        _logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                    var fill = GetDataFromPointer<FILL_FIND_FILE_DATA>(rawFillFindData);

                    // used a single entry call to speed up the "enumeration" of the list
                    foreach (var t in files)
                    {
                        AddTo(fill, rawFileInfo, t);
                    }
                }

                _logger.Debug("FindFilesWithPatternProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("FindFilesWithPatternProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        private static void AddTo(FILL_FIND_FILE_DATA fill, DokanFileInfo rawFileInfo, FileInformation fi)
        {
            Debug.Assert(!string.IsNullOrEmpty(fi.FileName), "FileName must not be empty or null");
            var creationTime = ToFileTime(fi.CreationTime);
            var lastAccessTime = ToFileTime(fi.LastAccessTime);
            var lastWriteTime = ToFileTime(fi.LastWriteTime);
            var data = new WIN32_FIND_DATA
            {
                FileAttributes = fi.Attributes,
                CreationTime = creationTime,
                LastAccessTime = lastAccessTime,
                LastWriteTime = lastWriteTime,
                FileSize = fi.Length,
                FileName = fi.FileName
            };
            //ZeroMemory(&data, sizeof(WIN32_FIND_DATAW));

            fill(ref data, rawFileInfo);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus FindStreamsProxy(string rawFileName, IntPtr rawFillFindData, DokanFileInfo rawFileInfo)
        {
            try
            {

                _logger.Debug("FindStreamsProxy: {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.FindStreams(rawFileName, out IList<FileInformation> files, rawFileInfo);

                Debug.Assert(!(result == DokanResult.NotImplemented && files == null));
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (var fi in files)
                    {
                        _logger.Debug("\tFileName\t{0}", fi.FileName);
                        _logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                    var fill = GetDataFromPointer<FILL_FIND_STREAM_DATA>(rawFillFindData);

                    // used a single entry call to speed up the "enumeration" of the list
                    foreach (var t in files)
                    {
                        AddTo(fill, rawFileInfo, t);
                    }
                }

                _logger.Debug("FindStreamsProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("FindStreamsProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        private static T GetDataFromPointer<T>(IntPtr rawDelegate) where T: class 
        {
#if NET451_OR_GREATER
            return Marshal.GetDelegateForFunctionPointer<T>(rawDelegate);
#else
            return Marshal.GetDelegateForFunctionPointer(rawDelegate, typeof(T)) as T;
#endif
        }
        private static void AddTo(FILL_FIND_STREAM_DATA fill, DokanFileInfo rawFileInfo, FileInformation fi)
        {
            Debug.Assert(!string.IsNullOrEmpty(fi.FileName), "FileName must not be empty or null");
            var data = new WIN32_FIND_STREAM_DATA
            {
                StreamSize = fi.Length,
                StreamName = fi.FileName
            };
            //ZeroMemory(&data, sizeof(WIN32_FIND_DATAW));

            fill(ref data, rawFileInfo);
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus SetEndOfFileProxy(string rawFileName, long rawByteOffset, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("SetEndOfFileProxy : {0}", rawFileName);
                _logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetEndOfFile(rawFileName, rawByteOffset, rawFileInfo);

                _logger.Debug("SetEndOfFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetEndOfFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus SetAllocationSizeProxy(string rawFileName, long rawLength, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("SetAllocationSizeProxy : {0}", rawFileName);
                _logger.Debug("\tLength\t{0}", rawLength);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetAllocationSize(rawFileName, rawLength, rawFileInfo);

                _logger.Debug("SetAllocationSizeProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetAllocationSizeProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus SetFileAttributesProxy(string rawFileName, uint rawAttributes, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("SetFileAttributesProxy : {0}", rawFileName);
                _logger.Debug("\tAttributes\t{0}", (FileAttributes)rawAttributes);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetFileAttributes(rawFileName, (FileAttributes)rawAttributes, rawFileInfo);

                _logger.Debug("SetFileAttributesProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetFileAttributesProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus SetFileTimeProxy(
            string rawFileName,
            ref FILETIME rawCreationTime,
            ref FILETIME rawLastAccessTime,
            ref FILETIME rawLastWriteTime,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                var ctime = (rawCreationTime.dwLowDateTime != 0 || rawCreationTime.dwHighDateTime != 0) &&
                            (rawCreationTime.dwLowDateTime != -1 || rawCreationTime.dwHighDateTime != -1)
                    ? DateTime.FromFileTime(((long) rawCreationTime.dwHighDateTime << 32) |
                                            (uint) rawCreationTime.dwLowDateTime)
                    : (DateTime?) null;
                var atime = (rawLastAccessTime.dwLowDateTime != 0 || rawLastAccessTime.dwHighDateTime != 0) &&
                            (rawLastAccessTime.dwLowDateTime != -1 || rawLastAccessTime.dwHighDateTime != -1)
                    ? DateTime.FromFileTime(((long) rawLastAccessTime.dwHighDateTime << 32) |
                                            (uint) rawLastAccessTime.dwLowDateTime)
                    : (DateTime?) null;
                var mtime = (rawLastWriteTime.dwLowDateTime != 0 || rawLastWriteTime.dwHighDateTime != 0) &&
                            (rawLastWriteTime.dwLowDateTime != -1 || rawLastWriteTime.dwHighDateTime != -1)
                    ? DateTime.FromFileTime(((long) rawLastWriteTime.dwHighDateTime << 32) |
                                            (uint) rawLastWriteTime.dwLowDateTime)
                    : (DateTime?) null;

                _logger.Debug("SetFileTimeProxy : {0}", rawFileName);
                _logger.Debug("\tCreateTime\t{0}", ctime);
                _logger.Debug("\tAccessTime\t{0}", atime);
                _logger.Debug("\tWriteTime\t{0}", mtime);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetFileTime(rawFileName, ctime, atime, mtime, rawFileInfo);

                _logger.Debug("SetFileTimeProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetFileTimeProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus DeleteFileProxy(string rawFileName, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("DeleteFileProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.DeleteFile(rawFileName, rawFileInfo);

                _logger.Debug("DeleteFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("DeleteFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus DeleteDirectoryProxy(string rawFileName, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("DeleteDirectoryProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.DeleteDirectory(rawFileName, rawFileInfo);

                _logger.Debug("DeleteDirectoryProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("DeleteDirectoryProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus MoveFileProxy(
            string rawFileName,
            string rawNewFileName,
            bool rawReplaceIfExisting,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("MoveFileProxy : {0}", rawFileName);
                _logger.Debug("\tNewFileName\t{0}", rawNewFileName);
                _logger.Debug("\tReplaceIfExisting\t{0}", rawReplaceIfExisting);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.MoveFile(rawFileName, rawNewFileName, rawReplaceIfExisting, rawFileInfo);

                _logger.Debug("MoveFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("MoveFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus LockFileProxy(string rawFileName, long rawByteOffset, long rawLength, DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("LockFileProxy : {0}", rawFileName);
                _logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                _logger.Debug("\tLength\t{0}", rawLength);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.LockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                _logger.Debug("LockFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("LockFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus UnlockFileProxy(
            string rawFileName,
            long rawByteOffset,
            long rawLength,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("UnlockFileProxy : {0}", rawFileName);
                _logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                _logger.Debug("\tLength\t{0}", rawLength);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.UnlockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                _logger.Debug("UnlockFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("UnlockFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public NtStatus GetDiskFreeSpaceProxy(
            ref long rawFreeBytesAvailable,
            ref long rawTotalNumberOfBytes,
            ref long rawTotalNumberOfFreeBytes,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("GetDiskFreeSpaceProxy:");
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.GetDiskFreeSpace(
                    out rawFreeBytesAvailable,
                    out rawTotalNumberOfBytes,
                    out rawTotalNumberOfFreeBytes,
                    rawFileInfo);

                _logger.Debug("\tFreeBytesAvailable\t{0}", rawFreeBytesAvailable);
                _logger.Debug("\tTotalNumberOfBytes\t{0}", rawTotalNumberOfBytes);
                _logger.Debug("\tTotalNumberOfFreeBytes\t{0}", rawTotalNumberOfFreeBytes);
                _logger.Debug("GetDiskFreeSpaceProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("GetDiskFreeSpaceProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus GetVolumeInformationProxy(
            StringBuilder rawVolumeNameBuffer,
            uint rawVolumeNameSize,
            ref uint rawVolumeSerialNumber,
            ref uint rawMaximumComponentLength,
            ref FileSystemFeatures rawFileSystemFlags,
            StringBuilder rawFileSystemNameBuffer,
            uint rawFileSystemNameSize,
            DokanFileInfo rawFileInfo)
        {
            rawMaximumComponentLength = 256;
            rawVolumeSerialNumber = _serialNumber;
            try
            {
                _logger.Debug("GetVolumeInformationProxy:");
                _logger.Debug("\tContext\t{0}", rawFileInfo);
                var result = _operations.GetVolumeInformation(out string label, out rawFileSystemFlags, out string name, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(!string.IsNullOrEmpty(name), "name must not be null");
                    Debug.Assert(!string.IsNullOrEmpty(label), "Label must not be null");
                    rawVolumeNameBuffer.Append(label);
                    rawFileSystemNameBuffer.Append(name);

                    _logger.Debug("\tVolumeNameBuffer\t{0}", rawVolumeNameBuffer);
                    _logger.Debug("\tFileSystemNameBuffer\t{0}", rawFileSystemNameBuffer);
                    _logger.Debug("\tVolumeSerialNumber\t{0}", rawVolumeSerialNumber);
                    _logger.Debug("\tFileSystemFlags\t{0}", rawFileSystemFlags);
                }

                _logger.Debug("GetVolumeInformationProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("GetVolumeInformationProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus MountedProxy(DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("MountedProxy:");
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.Mounted(rawFileInfo);

                _logger.Debug("MountedProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("MountedProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus UnmountedProxy(DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("UnmountedProxy:");
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.Unmounted(rawFileInfo);

                _logger.Debug("UnmountedProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("UnmountedProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus GetFileSecurityProxy(
            string rawFileName,
            ref SECURITY_INFORMATION rawRequestedInformation,
            IntPtr rawSecurityDescriptor,
            uint rawSecurityDescriptorLength,
            ref uint rawSecurityDescriptorLengthNeeded,
            DokanFileInfo rawFileInfo)
        {
            var sect = AccessControlSections.None;
            if (rawRequestedInformation.HasFlag(SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Owner;
            }
            if (rawRequestedInformation.HasFlag(SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Group;
            }
            if (rawRequestedInformation.HasFlag(SECURITY_INFORMATION.DACL_SECURITY_INFORMATION) ||
                rawRequestedInformation.HasFlag(SECURITY_INFORMATION.PROTECTED_DACL_SECURITY_INFORMATION) ||
                rawRequestedInformation.HasFlag(SECURITY_INFORMATION.UNPROTECTED_DACL_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Access;
            }
            if (rawRequestedInformation.HasFlag(SECURITY_INFORMATION.SACL_SECURITY_INFORMATION) ||
                rawRequestedInformation.HasFlag(SECURITY_INFORMATION.PROTECTED_SACL_SECURITY_INFORMATION) ||
                rawRequestedInformation.HasFlag(SECURITY_INFORMATION.UNPROTECTED_SACL_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Audit;
            }
            try
            {
                _logger.Debug("GetFileSecurityProxy : {0}", rawFileName);
                _logger.Debug("\tFileSystemSecurity\t{0}", sect);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.GetFileSecurity(rawFileName, out FileSystemSecurity sec, sect, rawFileInfo);
                if (result == DokanResult.Success /*&& sec != null*/)
                {
                    Debug.Assert(sec != null, $"{nameof(sec)} must not be null");
                    _logger.Debug("\tFileSystemSecurity Result : {0}", sec);
                    var buffer = sec.GetSecurityDescriptorBinaryForm();
                    rawSecurityDescriptorLengthNeeded = (uint)buffer.Length;
                    if (buffer.Length > rawSecurityDescriptorLength)
                        return DokanResult.BufferOverflow;

                    Marshal.Copy(buffer, 0, rawSecurityDescriptor, buffer.Length);
                }

                _logger.Debug("GetFileSecurityProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("GetFileSecurityProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Explicit Exception handler")]
        public NtStatus SetFileSecurityProxy(
            string rawFileName,
            ref SECURITY_INFORMATION rawSecurityInformation,
            IntPtr rawSecurityDescriptor,
            uint rawSecurityDescriptorLength,
            DokanFileInfo rawFileInfo)
        {
            var sect = AccessControlSections.None;
            if (rawSecurityInformation.HasFlag(SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Owner;
            }
            if (rawSecurityInformation.HasFlag(SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Group;
            }
            if (rawSecurityInformation.HasFlag(SECURITY_INFORMATION.DACL_SECURITY_INFORMATION) ||
                rawSecurityInformation.HasFlag(SECURITY_INFORMATION.PROTECTED_DACL_SECURITY_INFORMATION) ||
                rawSecurityInformation.HasFlag(SECURITY_INFORMATION.UNPROTECTED_DACL_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Access;
            }
            if (rawSecurityInformation.HasFlag(SECURITY_INFORMATION.SACL_SECURITY_INFORMATION) ||
                rawSecurityInformation.HasFlag(SECURITY_INFORMATION.PROTECTED_SACL_SECURITY_INFORMATION) ||
                rawSecurityInformation.HasFlag(SECURITY_INFORMATION.UNPROTECTED_SACL_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Audit;
            }
            var buffer = new byte[rawSecurityDescriptorLength];
            try
            {
                Marshal.Copy(rawSecurityDescriptor, buffer, 0, (int)rawSecurityDescriptorLength);
                var sec = rawFileInfo.IsDirectory ? (FileSystemSecurity)new DirectorySecurity() : new FileSecurity();
                sec.SetSecurityDescriptorBinaryForm(buffer);

                _logger.Debug("SetFileSecurityProxy : {0}", rawFileName);
                _logger.Debug("\tAccessControlSections\t{0}", sect);
                _logger.Debug("\tFileSystemSecurity\t{0}", sec);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetFileSecurity(rawFileName, sec, sect, rawFileInfo);

                _logger.Debug("SetFileSecurityProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetFileSecurityProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

#pragma warning restore CA1303 //Do not pass literals as localized parameters

        /// <summary>
        /// Converts the value of <paramref name="dateTime"/> to a Windows file time of type <see cref="FILETIME"/>.
        /// If <paramref name="dateTime"/> is <c>null</c> or before 12:00 midnight January 1, 1601 C.E. UTC, it returns <c>0</c>.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
        /// <returns>
        /// The value of <paramref name="dateTime"/> expressed as a Windows file time
        /// -or- it returns <c>0</c> if <paramref name="dateTime"/> is before 12:00 midnight January 1, 1601 C.E. UTC or <c>null</c>.
        /// </returns>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa365739(v=vs.85).aspx">WIN32_FILE_ATTRIBUTE_DATA structure (MSDN)</a>
        [Pure]
        private static FILETIME ToFileTime(DateTime? dateTime)
        {
            var fileTimeRaw = dateTime.HasValue &&
                              dateTime.Value >= DateTime.FromFileTime(0)
                ? dateTime.Value.ToFileTime()
                : 0;
            FILETIME fileTime;
            fileTime.dwHighDateTime = (int)(fileTimeRaw >> 32);
            fileTime.dwLowDateTime = (int)(fileTimeRaw & 0xffffffff);
            return fileTime;
        }

#region Nested type: FILL_FIND_FILE_DATA

        /// <summary>
        /// Used to add an entry in <see cref="DokanOperationProxy.FindFilesProxy"/> and <see cref="DokanOperationProxy.FindFilesWithPatternProxy"/>.
        /// </summary>
        /// <param name="rawFindData">A <see cref="WIN32_FIND_DATA"/>.</param>
        /// <param name="rawFileInfo">A <see cref="DokanFileInfo"/>.</param>
        /// <returns><c>1</c> if buffer is full, otherwise <c>0</c> (currently it never returns <c>1</c>)</returns>
        /// <remarks>This is the same delegate as <c>PFillFindData</c> (dokan.h) in the C++ version of Dokan.</remarks>
        private delegate long FILL_FIND_FILE_DATA(
            ref WIN32_FIND_DATA rawFindData, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

#endregion Nested type: FILL_FIND_FILE_DATA

#region Nested type: FILL_FIND_STREAM_DATA

        /// <summary>
        /// Used to add an entry in <see cref="DokanOperationProxy.FindStreamsProxy"/>.
        /// </summary>
        /// <param name="rawFindData">A <see cref="WIN32_FIND_STREAM_DATA"/>.</param>
        /// <param name="rawFileInfo">A <see cref="DokanFileInfo"/>.</param>
        /// <returns><c>1</c> if buffer is full, otherwise <c>0</c> (currently it never returns <c>1</c>)</returns>
        /// <remarks>This is the same delegate as <c>PFillFindStreamData</c> (dokan.h) in the C++ version of Dokan.</remarks>
        private delegate long FILL_FIND_STREAM_DATA(
            ref WIN32_FIND_STREAM_DATA rawFindData, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

#endregion Nested type: FILL_FIND_STREAM_DATA
    }
}
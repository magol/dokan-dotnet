using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DokanNet
{
    /// <summary>
    /// Used to provide file information to %Dokan during operations by
    ///  - <see cref="IDokanOperations.GetFileInformation"/>
    ///  - <see cref="IDokanOperations.FindFiles"/>
    ///  - <see cref="IDokanOperations.FindStreams"/> 
    ///  - <see cref="IDokanOperations.FindFilesWithPattern"/>.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay("{FileName}, {Length}, {CreationTime}, {LastWriteTime}, {LastAccessTime}, {Attributes}")]
    public struct FileInformation : IEquatable<FileInformation>
    {
        /// <summary>
        /// Gets or sets the name of the file or directory.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the <c><see cref="FileAttributes"/></c> for the file or directory.
        /// </summary>
        public FileAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the file or directory.
        /// If equal to <c>null</c>, the value will not be set or the file has no creation time.
        /// </summary>
        public DateTime? CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the last access time of the file or directory.
        /// If equal to <c>null</c>, the value will not be set or the file has no last access time.
        /// </summary>
        public DateTime? LastAccessTime { get; set; }

        /// <summary>
        /// Gets or sets the last write time of the file or directory.
        /// If equal to <c>null</c>, the value will not be set or the file has no last write time.
        /// </summary>
        public DateTime? LastWriteTime { get; set; }

        /// <summary>
        /// Gets or sets the length of the file.
        /// </summary>
        public long Length { get; set; }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="other">The object to compare with the current instance. </param>
        /// <returns><c>true</c> if <paramref name="other" /> and this instance are represent the same value; 
        /// otherwise, <c>false</c>. </returns>
        public bool Equals(FileInformation other)
        {
            return
                string.Equals(FileName, other.FileName) &&
                Attributes == other.Attributes &&
                CreationTime.Equals(other.CreationTime) &&
                LastAccessTime.Equals(other.LastAccessTime) &&
                LastWriteTime.Equals(other.LastWriteTime) &&
                Length == other.Length;
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns><c>true</c> if <paramref name="obj" /> and this instance are the same type and represent the same value; 
        /// otherwise, <c>false</c>. </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FileInformation && Equals((FileInformation) obj);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ CreationTime.GetHashCode();
                hashCode = (hashCode * 397) ^ LastAccessTime.GetHashCode();
                hashCode = (hashCode * 397) ^ LastWriteTime.GetHashCode();
                hashCode = (hashCode * 397) ^ Length.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>Indicates whether two objects are equal.</summary>
        /// <param name="left">The first object to compare. </param>
        /// <param name="right">The secound object to compare. </param>
        /// <returns><c>true</c> if <paramref name="left" /> and <paramref name="right" /> are represent the same value; 
        /// otherwise, <c>false</c>. </returns>
        public static bool operator ==(FileInformation left, FileInformation right)
        {
            return left.Equals(right);
        }

        /// <summary>Indicates whether two objects are not equal.</summary>
        /// <param name="left">The first object to compare. </param>
        /// <param name="right">The secound object to compare. </param>
        /// <returns><c>true</c> if <paramref name="left" /> and <paramref name="right" /> are not represent the same value; 
        /// otherwise, <c>false</c>. </returns>
        public static bool operator !=(FileInformation left, FileInformation right)
        {
            return !left.Equals(right);
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;

namespace DokanNet
{
    /// <summary>
    /// The dokan exception.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "Can only be created internal")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Standard constructors is not needed")]
    [Serializable]
    public class DokanException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DokanException"/> class with a <see cref="Exception.HResult"/>.
        /// </summary>
        /// <param name="status">
        /// The status for <see cref="Exception.HResult"/>.
        /// </param>
        /// <param name="message">
        /// The error message.
        /// </param>
        internal DokanException(int status, string message) : base(message)
        {
            HResult = status;
        }
    }
}
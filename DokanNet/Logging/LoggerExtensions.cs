using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace DokanNet.Logging
{
    /// <summary>
    /// Extension functions to log messages.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Format a log message.
        /// </summary>
        /// <param name="message">Message to format.</param>
        /// <param name="category">Optional category to add to the log message.</param>
        /// <param name="loggerName">Optional log name to at to the log message.</param>
        /// <returns>A formated log message.</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static string FormatMessageForLogging(
            this string message,
            string category = null,
            string loggerName = "")
        {
            return message.FormatMessageForLogging(false, category, loggerName);
        }

        /// <summary>
        /// Format a log message.
        /// </summary>
        /// <param name="message">Message to format.</param>
        /// <param name="addDateTime">If date and time shout be added to the log message.</param>
        /// <param name="category">Optional category to add to the log message.</param>
        /// <param name="loggerName">Optional log name to at to the log message.</param>
        /// <returns>A formated log message.</returns>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static string FormatMessageForLogging(
            this string message,
            bool addDateTime = false,
            string category = null,
            string loggerName = "")
        {
            var stringBuilder = new StringBuilder();
            if (addDateTime)
            {
                stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} - ", DateTime.Now.ToString(CultureInfo.CurrentCulture));
            }

            if (!string.IsNullOrEmpty(loggerName))
            {
                stringBuilder.Append(loggerName);
            }

            if (!string.IsNullOrEmpty(category))
            {
                stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "{0} ", category));
            }

            stringBuilder.Append(message);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Format a <see cref="FileInformation"/> for the log.
        /// </summary>
        /// <param name="fi">The <see cref="FileInformation"/> to format.</param>
        /// <returns>A string representation form the log</returns>
        internal static string FormatForLogging(this FileInformation fi)
        {
            return string.Format(CultureInfo.CurrentCulture,
                @"\t{0}\t{1}
\t\t{2}\t{3}
\t\t{4}\t{5}
\t\t{6}\t{7}
\t\t{8}\t{9}
\t\t{10}\t{11}", nameof(fi.FileName), fi.FileName, nameof(fi.Attributes), fi.Attributes, nameof(fi.CreationTime),
                fi.CreationTime, nameof(fi.LastAccessTime), fi.LastAccessTime, nameof(fi.LastWriteTime),
                fi.LastWriteTime,
                nameof(fi.Length), fi.Length);
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void DebugPairValuesLevel0( this ILogger logger, object value, [CallerMemberName] string memberName = "")
        {
            logger.Debug(string.Format(CultureInfo.CurrentCulture, "{0} : {1}", memberName, value));
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void DebugPairValuesLevel1(this ILogger logger, object value, string memberName)
        {
            logger.Debug(string.Format(CultureInfo.CurrentCulture, "\t{0}\t{1}", memberName, value));
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void DebugPairValuesLevel2(this ILogger logger, object value, string memberName)
        {
            logger.Debug(string.Format(CultureInfo.CurrentCulture
                , "\t\t{0}\t{1}", memberName, value));
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void DebugReturn(this ILogger logger, string value, NtStatus result, [CallerMemberName] string memberName = "")
        {
            logger.Debug(string.Format(CultureInfo.CurrentCulture
                , "{0} : {1} Return : {2}", memberName, value, result));
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void Exception(this ILogger logger, string value, Exception exception, [CallerMemberName] string memberName = "")
        {
            logger.Error(string.Format(CultureInfo.CurrentCulture,
                "{0} : {1} Throw : {2}", memberName, value, exception));
        }
    }
}
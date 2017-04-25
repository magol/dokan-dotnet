using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

namespace DokanNet.Logging
{
    /// <summary>
    /// Write log using OutputDebugString 
    /// </summary>
    /// <remarks>
    /// To see the output in visual studio 
    /// Project + %Properties, Debug tab, check "Enable unmanaged code debugging".
    /// </remarks> 
    public class DebugViewLogger : ILogger
    {
        private readonly string _loggerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugViewLogger"/> class.
        /// </summary>
        public DebugViewLogger()
            :this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugViewLogger"/> class.
        /// </summary>
        /// <param name="loggerName">Optional name to be added to each log line.</param>
        public DebugViewLogger(string loggerName)
        {
            _loggerName = loggerName;
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            WriteMessageToDebugView("debug", message, args);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            WriteMessageToDebugView("info", message, args);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            WriteMessageToDebugView("warn", message, args);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            WriteMessageToDebugView("error", message, args);
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            WriteMessageToDebugView("fatal", message, args);
        }
        /// <summary>
        /// Sends a string to the debugger for display.
        /// </summary>
        /// <param name="message">The string to be displayed.</param>
        [SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api", Justification = "The managed equivalents does not have exactly the same behavior")]
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "The API is only used internal in this class")]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern void OutputDebugString(string message);

        /// <summary>
        /// Format the message and output the string to the debugger.
        /// </summary>
        /// <param name="category">The category to be displayed.</param>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="args">Arguments to the <paramref name="message"/>.</param>
        private void WriteMessageToDebugView(string category, string message, params object[] args)
        {
            if (args?.Length > 0)
                message = string.Format(CultureInfo.CurrentCulture, message, args);
            OutputDebugString(message.FormatMessageForLogging(category, _loggerName));
        }
    }
}
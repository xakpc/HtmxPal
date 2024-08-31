using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    /// <summary>
    /// Logging class for outputting messages to the output window.
    /// </summary>
    internal class Output
    {
        private static bool first_log_message = true;

        /// <summary>
        /// Writes an information message to the output window.
        /// </summary>
        /// <param name="msg">The information message to write.</param>
        [Conditional("DEBUG")]
        public static void WriteInfo(string msg)
        {
            _ = OutputAsync("INFO: " + msg);
        }

        /// <summary>
        /// Writes a warning message to the output window.
        /// </summary>
        /// <param name="msg">The warning message to write.</param>
        public static void WriteWarining(string msg)
        {
            _ = OutputAsync("WARNING: " + msg);
        }

        /// <summary>
        /// Writes an error message to the output window.
        /// </summary>
        /// <param name="msg">The error message to write.</param>
        public static void WriteError(string msg)
        {
            _ = OutputAsync("ERROR: " + msg);
        }

        /// <summary>
        /// Writes a message to the output window asynchronously.
        /// </summary>
        /// <param name="msg">The message to write.</param>
        public static async Task OutputAsync(string msg)
        {
            Contract.Requires(msg != null);

            if (!ThreadHelper.CheckAccess())
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }

            string msg2 = string.Format(CultureInfo.CurrentCulture, "{0}", msg.Trim() + Environment.NewLine);

            if (first_log_message)
            {
                first_log_message = false;

                StringBuilder sb = new StringBuilder();
                sb.Append("Welcome to\n");
                sb.AppendLine(@" _     _");
                sb.AppendLine(@"| |   | |");
                sb.AppendLine(@"| |__ | |_ _ __ ___ __  __");
                sb.AppendLine(@"| '_ \| __| '_ ` _ \\ \/ /");
                sb.AppendLine(@"| | | | |_| | | | | |>  < ");
                sb.AppendLine(@"|_| |_|\__|_| |_| |_/_/\_\");
                sb.Append("----------------------------------\n");
                sb.Append("htmx-pal, the code-completion and quick-info on htmx library\n");
                sb.Append("https://htmx.org\n");
                msg2 = sb.ToString() + msg2;
            }
            IVsOutputWindowPane outputPane = await GetOutputPaneAsync().ConfigureAwait(true);
            if (outputPane == null)
            {
                Debug.Write(msg2);
            }
            else
            {
                outputPane.OutputStringThreadSafe(msg2);
                outputPane.Activate();
            }
        }

        /// <summary>
        /// Gets the output window pane asynchronously.
        /// </summary>
        /// <returns>The output window pane.</returns>
        public static async Task<IVsOutputWindowPane> GetOutputPaneAsync()
        {
            if (!ThreadHelper.CheckAccess())
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }

            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null)
            {
                return null;
            }
            else
            {
                Guid paneGuid = new Guid("F4D93821-D896-4CB4-A05B-8C44F68CD61D");
                outputWindow.CreatePane(paneGuid, "htmx-pal", 1, 0);
                outputWindow.GetPane(paneGuid, out IVsOutputWindowPane pane);
                return pane;
            }
        }
    }
}

using System.Text;

namespace QP8.Infrastructure.Helpers.ProcessHelpers
{
    public class ProcessExecutionSettings
    {
        /// <summary>
        /// Path to the executable file.
        /// </summary>
        public string ProcessExePath;

        /// <summary>
        /// Arguments to pass to the executable file.
        /// </summary>
        public string Arguments;

        /// <summary>
        /// If specified, redirect process standard input and write data to it.
        /// </summary>
        public string StandardInputData;

        /// <summary>
        /// Timeout in ms, after which process will be killed.
        /// Default is 60000ms.
        /// </summary>
        public int ProcessTimeout = 60000;

        /// <summary>
        /// Timeout in ms, waiting before reading standard and error outputs, after process was finished.
        /// Default is 100ms.
        /// </summary>
        public int WaitForOutputTimeout = 100;

        /// <summary>
        /// Encoding which used for standard and error outputs.
        /// Default is Encoding.UTF8.
        /// </summary>
        public Encoding OutputEncoding = Encoding.UTF8;
    }
}

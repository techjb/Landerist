using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace landerist_library.Parse.Listing.MLModel
{
    /// <summary>
    /// https://ernest-bonat.medium.com/using-c-to-run-python-scripts-with-machine-learning-models-a82cff74b027
    /// </summary>
    public class MLSharpPython : IMLSharpPython
    {
        public readonly string filePythonExePath;
        /// <summary>
        /// ML Sharp Python class constructor
        /// </summary>
        /// <param name="exePythonPath">Python EXE file path</param>
        public MLSharpPython(string exePythonPath)
        {
            filePythonExePath = exePythonPath;
        }
        /// <summary>
        /// Execute Python script file
        /// </summary>
        /// <param name="filePythonScript">Python script file and input parameter(s)</param>
        /// <param name="standardError">Output standard error</param>
        /// <returns>Output text result</returns>
        public string ExecutePythonScript(string filePythonScript, out string standardError)
        {
            string outputText = string.Empty;
            standardError = string.Empty;
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo(filePythonExePath)
                    {
                        Arguments = filePythonScript,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    process.Start();
                    outputText = process.StandardOutput.ReadToEnd();
                    outputText = outputText.Replace(Environment.NewLine, string.Empty);
                    standardError = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message;
            }
            return outputText;
        }
    }
}

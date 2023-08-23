using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public class ShellHelper
	{
		public static string ExecuteCommand(string command)
		{
			ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);

			// The following commands are needed to redirect the standard output.
			procStartInfo.RedirectStandardOutput = true; // This means that it will be redirected to the Process.StandardOutput StreamReader.
			procStartInfo.UseShellExecute = false;
			procStartInfo.CreateNoWindow = true; // Do not create the black window.

			Process proc = new Process(); // Now we create a process, assign its ProcessStartInfo and start it

			proc.StartInfo = procStartInfo;
			proc.Start();

			string output = proc.StandardOutput.ReadToEnd(); // Read the output into a string

			return output;
		}
	}
}

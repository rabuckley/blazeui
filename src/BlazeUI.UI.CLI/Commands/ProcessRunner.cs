using System.Diagnostics;

namespace BlazeUI.UI.CLI.Commands;

internal interface IProcessRunner
{
    int Run(string command, string arguments, string workingDirectory);
}

internal sealed class ProcessRunner : IProcessRunner
{
    public int Run(string command, string arguments, string workingDirectory)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
        return process.ExitCode;
    }
}

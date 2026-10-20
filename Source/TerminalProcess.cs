using System.Diagnostics;

public class TerminalProcess : IDisposable
{
    public Action<string>? OutputData;

    private readonly Process _shell;

    public TerminalProcess()
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "/bin/bash",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
        };

        _shell = Process.Start(startInfo)!;

        _shell.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                OutputData?.Invoke(e.Data);
        };

        _shell.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                OutputData?.Invoke(e.Data);
        };

        _shell.BeginErrorReadLine();
        _shell.BeginOutputReadLine();
    }

    public void SendCommand(string command)
    {
        _shell.StandardInput.WriteLine(command);
    }

    public bool HasRunningChildProcess()
    {
        foreach (var dir in Directory.GetDirectories("/proc"))
        {
            if (!int.TryParse(Path.GetFileName(dir), out int pid))
                continue;

            try
            {
                string stat = File.ReadAllText($"{dir}/stat");

                string rest = stat[(stat.LastIndexOf(')') + 2)..];
                string[] parts = rest.Split(' ');

                int ppid = int.Parse(parts[1]);

                if (ppid == _shell.Id)
                    return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public void Dispose()
    {
        if (!_shell.HasExited)
        {
            _shell.Kill();
        }

        _shell.Dispose();
    }
}

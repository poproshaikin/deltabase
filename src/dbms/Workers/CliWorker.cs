using System.Diagnostics;

namespace Workers;

public class CliWorker : IDltWorker
{
    public void DoStartModule(string input)
    {
        Process process = new Process()
        {
            StartInfo =
            {
                FileName = input switch
                {
                    "1" => "./dlt_server_app.exe",
                    "2" => "./sql_exec_app",
                    _ => throw new ArgumentOutOfRangeException()
                },
                Arguments = "-h",
                CreateNoWindow = false,
                RedirectStandardOutput = false,
                RedirectStandardInput = false,
            }
        };

        process.Start();
        process.WaitForExit();
        Console.ReadKey();
    }
}
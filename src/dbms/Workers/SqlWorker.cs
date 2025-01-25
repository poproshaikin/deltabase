using System.Diagnostics;
using Sql.Executing;

namespace Workers;

public class SqlWorker : IDltWorker
{
    public IExecutionResult DoParseAndExecute(string rawQuery, string serverName, string dbName)
    {
        string executable = "./sql_exec_app.exe";

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = executable,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            UseShellExecute = false
        };

        string executionResultString;

        using (Process process = Process.Start(startInfo)!)
        {
            process.StandardInput.WriteLine(rawQuery);
            process.StandardInput.WriteLine(serverName);
            process.StandardInput.WriteLine(dbName);
            
            executionResultString = process.StandardOutput.ReadToEnd();
        }

        return IExecutionResult.Deserialize(executionResultString);
    } 
}
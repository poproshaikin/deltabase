using Server.Core;
using Utils;

namespace Server.App;

class Program
{
    private static string _command;
    private static string _serverName;
    private static string? _port;
    private static string? _password;
    
    static void Main(string[] args)
    {
        DltServer server;

        SetInput(args);

        if (_command == "startserver")
        {
            server = new DltServer(_serverName);
        }
        else if (_command == "createserver")
        {
            server = DltServer.Create(_serverName, ushort.Parse(_port!), _password!);
        }
        else
        {
            return;
        }
        
        server.Start(CancellationToken.None);
    }

    static void SetInput(string[] args)
    {
        bool showHelp = args is ["-h"];
        
        if (showHelp) Console.Write("Enter command: ");
        _command = Console.ReadLine()!;

        if (_command == "startserver")
        {
            if (showHelp) Console.Write("Server name: ");
            _serverName = Console.ReadLine()!;
        }
        else if (_command == "createserver")
        {
            if (showHelp) Console.Write("Server name: ");
            _serverName = Console.ReadLine()!;
            if (showHelp) Console.Write("Port: ");
            _port = Console.ReadLine()!;
            if (showHelp) Console.Write("Password: ");
            _password = Console.ReadLine()!;
        }
    }
}
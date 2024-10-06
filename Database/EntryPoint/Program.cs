using Server.Core;
using Utils;

namespace EntryPoint;

class Program
{
    static void Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string command = args[0];

        if (command == "startserver")
        {
            Logger.Log("executing \"startserver\" command");
            DltServer server = DltServer.Init(args[1]);
            server.Start();
        }
    }
}
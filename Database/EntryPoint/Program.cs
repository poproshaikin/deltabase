using Server.Core;
using Utils;

namespace EntryPoint;

class Program
{
    static void Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string command = args[0];

        if (command == "startserver")  // startserver <serverName>
        {
            Logger.Log("executing \"startserver\" command");
            DltServer server = new(args[1]);
            server.Start();
        }
        else if (command == "createserver") // createserver <serverName> <serverPort> <serverPassword>
        {
            DltServer server = DltServer.Create(args[1], ushort.Parse(args[2]), args[3]);
            server.Start();
        }
    }
}
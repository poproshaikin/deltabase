using System.Diagnostics;
using Workers;

namespace Deltabase.Cli;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Microservices: ");
        Console.WriteLine();
        Console.WriteLine("1 - Server");
        Console.WriteLine("2 - Sql executor");
        Console.WriteLine();
        Console.Write("Choose microservice (number): ");
        string input = Console.ReadLine()!;

        while (input != "1" && input != "2")
        {
            Console.Write("Invalid input. Try again: ");
            input = Console.ReadLine()!;
        }

        new CliWorker().DoStartModule(input);
    }
}
using Data.Definitions;
using Data.Definitions.Schemes;
using Data.Models;
using Data.Operation.IO;
using Utils;

namespace Tessts;

class Program
{
    static async Task Main(string[] args)
    {
        var stream = new FileStream(
            @"D:\Soft Dev\С#\deltabase\bin\dbms\Debug\net9.0\firstServer\db\firstDb\records\Students\Students.0000.record",
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.ReadWrite);
        
        DataDefinitor definitor = new DataDefinitor("firstDb", new FileSystemHelper("firstServer", @"D:\Soft Dev\С#\deltabase\bin\dbms\Debug\net9.0\"));
        TableScheme scheme = definitor.GetTableScheme("Students");
        BinaryDataIO io = new BinaryDataIO(stream);
        PageHeader header = await io.ReadHeaderAsync();
        Console.WriteLine(header.PageId);
        Console.WriteLine(header.RowsCount);
        Console.WriteLine(string.Join(' ', header.FreeRows));
        
        Console.WriteLine((await io.ReadRowAsync(scheme)).ToString());
        Console.WriteLine((await io.ReadRowAsync(scheme)).ToString());
        
        // await io.WriteHeader(new PageHeader()
        // {
        //     PageId = 0,
        //     RowsCount = 2,
        //     FreeRows = []
        // });
        //
        // await io.WriteRowAsync(new PageRow(0, [1, "Stas", 18.1f]), scheme);
        // await io.WriteRowAsync(new PageRow(1, [2, "Bohdan", 17.9f]), scheme);
    }
}
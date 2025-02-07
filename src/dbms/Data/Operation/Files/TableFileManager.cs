using Data.Definitions.Schemes;
using Data.Models;
using Utils;

namespace Data.Operation;

// сделать разные подходы к чтению страниц
// например выдавать переменную IPageReader, у которой будут реализации WholePageReader и ИзбирательноPageReader
internal class TableFileManager
{
    private string _dbName;

    private FileSystemHelper _fsHelper;

    private FileStreamPool _pool;
    
    internal TableFileManager(string dbName, FileSystemHelper fsHelper)
    {
        _dbName = dbName;
        _fsHelper = fsHelper;
        _pool = new FileStreamPool(FileAccess.Read);
    }

    internal TableFileMetadata GetTableConfig(TableScheme scheme)
    {
        
    }

    internal TableFileMetadata[] GetPages(TableScheme scheme)
    {
         
    }

    internal TableFileReader OpenReader(TableFileMetadata metadata) 
    {
        // определять способ чтения по размеру файла или по типу данных
    }

    internal TableFileWriter OpenWriter(TableFileMetadata metadata)
    {
        
    }
}
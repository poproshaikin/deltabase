using Data.Definitions.Schemes;

namespace Data.Operation.IO;

public class BinaryRowIO : BinaryDataIO
{
    public BinaryRowIO(FileStream stream) : base(stream)
    {
    }

    public BinaryRowIO(FileStream stream, TableScheme scheme) : base(stream, scheme)
    {
    }
    
    
}
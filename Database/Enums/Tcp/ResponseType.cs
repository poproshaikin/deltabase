namespace Enums.Tcp;

public enum ResponseType
{
    ConnectedSuccessfully      = 1,
    DatabaseNameIsNotSpecified,
    InvalidPassword,
    Unauthorized,
    Success,
    InternalServerError,
    
    InvalidSql                 = 100,
    InvalidPassedValueType,
    InvalidTableNameLength,
    InvalidTableName,
    InvalidQueryLength,
    
    TableAlreadyExists         = 200,
    
    TableDoesntExist           = 250,
    ServerDoesntExist,
    DatabaseDoesntExist,
    UserDoesntExist,
    
    PassedPkValueIsntUnique    = 300,
}
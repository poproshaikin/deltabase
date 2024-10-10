namespace Enums.Tcp;

public enum TcpResponseType
{
    ConnectedSuccessfully      = 1,
    DatabaseNameIsNotSpecified = 2,
    ServerDoesntExist          = 3,
    DatabaseDoesntExist        = 4,
    UserDoesntExist            = 5,
    InvalidPassword            = 6,
    Unauthorized               = 7,
    InvalidSql                 = 8,
    Success                    = 9,
    InternalServerError        = 10,
    
    PassedPkValueIsntUnique    = 100,
    InvalidPassedValueType     = 101,
    TableAlreadyExists         = 102,
}
namespace Enums.Exceptions;

public enum ErrorType
{
    // TODO переделать в байты
    
    TableDoesntExist = 1,
    InvalidCondition = 2,
    InvalidQueryLength = 3,
    InvalidQuery = 4,
    InvalidPassedColumns = 5,
    MissingPassedColumns = 6,
    InvalidPassedValues = 7,
    ColumnDoesntExist = 8,
    InvalidAssignment = 9,
    InvalidValueType = 10,
    InvalidValuesCount = 11,
    DatabaseDoesntExist = 12,
    InvalidLogicalOperatorInCondition = 13,
    UnsupportedTransportProtocol = 14,
    MissingServerConfigSetting = 15,
}
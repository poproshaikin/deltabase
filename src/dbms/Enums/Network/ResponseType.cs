namespace Enums.Network;

/// <summary>
/// Represents the various response types from a database connection operation.
/// </summary>
public enum ResponseType
{
    // TODO переделать под байты
    
    /// <summary>
    /// Indicates that the connection was established successfully.
    /// This value signifies that the client is now connected to the server.
    /// </summary>
    ConnectedSuccessfully = 1,
    AlreadyConnected,

    /// <summary>
    /// Indicates that the database name was not specified in the connection request.
    /// Clients should ensure a valid database name is provided.
    /// </summary>
    DatabaseNameIsNotSpecified,

    /// <summary>
    /// Indicates that the password provided for authentication is invalid.
    /// Clients should verify their password before retrying.
    /// </summary>
    InvalidPassword,

    /// <summary>
    /// Indicates that the client is unauthorized to perform the requested operation.
    /// This could be due to insufficient permissions.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Indicates a successful operation.
    /// This value may be used in various contexts to signify successful execution.
    /// </summary>
    Success,

    /// <summary>
    /// Indicates an internal server error occurred while processing the request.
    /// Clients should retry the operation later or contact support.
    /// </summary>
    InternalServerError,

    /// <summary>
    /// Indicates that there are no available slots for new connections.
    /// Clients should wait and try connecting again later.
    /// </summary>
    NoAvailableSlots,

    /// <summary>
    /// Indicates that the SQL command provided is invalid.
    /// Clients should check the syntax and validity of the SQL command.
    /// </summary>
    InvalidSql = 100,

    /// <summary>
    /// Indicates that the value type passed is invalid.
    /// Clients should ensure that the correct data types are being used.
    /// </summary>
    InvalidPassedValueType,

    /// <summary>
    /// Indicates that the table name provided is too long.
    /// Clients should ensure that the table name adheres to the length restrictions.
    /// </summary>
    InvalidTableNameLength,

    /// <summary>
    /// Indicates that the table name provided is invalid.
    /// Clients should ensure that the table name follows the required naming conventions.
    /// </summary>
    InvalidTableName,

    /// <summary>
    /// Indicates that the query length exceeds the minimum allowed length.
    /// Clients should ensure that their SQL queries are within permissible limits.
    /// </summary>
    InvalidQueryLength,
    
    InvalidExpression,
    
    InvalidPassedColumns,

    /// <summary>
    /// Indicates that the table already exists in the database.
    /// Clients should choose a different name or check for existing tables before creating.
    /// </summary>
    TableAlreadyExists = 200,

    /// <summary>
    /// Indicates that the specified table does not exist in the database.
    /// Clients should verify the table name before attempting operations on it.
    /// </summary>
    TableDoesntExist = 250,

    /// <summary>
    /// Indicates that the specified server does not exist.
    /// Clients should check the server configuration and availability.
    /// </summary>
    ServerDoesntExist,

    /// <summary>
    /// Indicates that the specified database does not exist.
    /// Clients should ensure that the database name is correct.
    /// </summary>
    DatabaseDoesntExist,

    /// <summary>
    /// Indicates that the specified user does not exist.
    /// Clients should verify the username and ensure it has been created.
    /// </summary>
    UserDoesntExist,

    /// <summary>
    /// Indicates that the primary key value passed is not unique.
    /// Clients should ensure that primary key values are unique before insertion.
    /// </summary>
    PassedPkValueIsntUnique = 300,
    WrongServerAccessed,
    MissingServerConfigSettings,
    AccessGranted,
    Failure,
    InvalidRequest
}
# Interaction

To interact with **Deltabase** you can use [**Tcp**](#tcp) protocol or
[**Database Client Library**](#database-client-library)

----

### Tcp

>***Command structure***  
> >All the Tcp commands are separated into two parts.  
> >
> >*Command part* - Tcp command, such as `~connect`
> >
> >*Argument part* - part containing an argument to the command  
> >
> >*Separator* - two parts are separated using the ' ■' combination,  
> > you can type it using _Alt+255_, _Alt+254_

>`~connect` - command establishing a connection to the database.  
> > Takes connection string as an argument.
> >
> > After a successful connection, it stores the client's IP address and considers  
> > them authorized.  
> >
> > Returns the result of connection attempt.

>`~sql` - commands executing an SQL request.
> > Takes SQL request as an argument.
> > 
> > Returns the result of SQL request or error code.

>`~close`
> > Doesn't need an argument
> > 
> > Takes the client's IP address and removes him from the list of authorized clients  
> > and closes the connection to the database.

---- 

### Abbreviated response codes

> >**1** - Connected successfully
>
> >**2** - Database name isn't specified
> 
> >**3** - Server doesn't exist 
> 
> >**4** - Database doesn't exist
>
> >**5** - User doesn't exist 
>
> >**6** - Invalid password
>
> >**7** - Unauthorized
> 
> >**8** - Invalid SQL
>
> >**9** - Success
>
> >**10** - Internal server error
> 
> >**100** - Passed primary key value isn't unique
> 
----

### Database client library

The library for interacting with the database will primarily be developed in   
C# using prepared classes from the `System.Data.Common` namespace.  
Development will start after completing basic functions in database engine.
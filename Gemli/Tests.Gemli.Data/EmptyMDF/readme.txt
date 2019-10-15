This "EmptyMDF" folder contains an empty MDF! It is used for DbDataProviderTest.cs and
serves no other purpose but to test Gemli.Data database invocations directly using SQL
Server Express.

Microsoft made it really difficult to programmatically create MDFs for SQL Server Express
(for file-attach mode -- CREATE DATABASE with file doesn't work the same way) and they
also made it impossible to begin with to distribute a database with source code over
TFS without either manually creating one or programmatically creating one because once
the .mdf is checked in, it's read-only.

So, this empty MDF exists so that DbDataProviderTest.cs can copy it out of the EmptyMDF
directory, mark it as read/write, reference it in a SQL Server Express connection 
string, so that database tests can be performed against it.

Cheers,
Jon Davis aka "Stimpy"
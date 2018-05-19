Steps to follow:

1. enable CLR for triggers

```sql
EXEC sp_configure 'clr enabled', 1;
GO
    reconfigure
GO
```

2. Change strict security to 0
Modify installation to account for "CLR strict security" in SQL Server 2017
https://github.com/tSQLt-org/tSQLt/issues/25

```sql
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
```
```sql
EXEC sp_configure 'clr strict security', 0;
RECONFIGURE;
```

3. build assembly using visual studio

after you have built the assembly using visual studio (This will give you a dll file), create assembly in SQL server using following command

```sql
alter database epglobal  set trustworthy on;
GO
CREATE ASSEMBLY CLRTrigger
FROM 'path of the dll file'
WITH PERMISSION_SET = ALL
GO 
```


now create the trigger

```sql
CREATE TRIGGER test
ON dbo.customer
FOR INSERT, UPDATE, DELETE
AS
EXTERNAL NAME CLRTrigger.Triggers.SqlTrigger1;
```
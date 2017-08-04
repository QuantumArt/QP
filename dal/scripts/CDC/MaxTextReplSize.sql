EXEC sp_configure 'show advanced options', 1;
EXEC sp_configure 'max text repl size', -1;
RECONFIGURE;
GO

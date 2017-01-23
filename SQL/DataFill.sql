INSERT INTO Authors VALUES ('JAN 25 1990','Eva', 'Hubena')
INSERT INTO Authors VALUES ('FEB 11 2010','Jana', 'Tlusta')
INSERT INTO Authors VALUES ('DEC 12 1687','Jack', 'Ahoj')
INSERT INTO Authors VALUES ('MAR 01 1977','Tomas', 'Green')
INSERT INTO Authors VALUES ('JUL 22 1889','Ivana', 'Red')

DELETE FROM Authors WHERE 1=1

SELECT * FROM Authors


SELECT * FROM information_schema.tables WHERE TABLE_TYPE='BASE TABLE'

SELECT * FROM Authors

DROP TABLE Books
DROP TABLE Authors
DROP TABLE __EFMigrationsHistory	



/* Drop all tables */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP TABLE [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Table: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

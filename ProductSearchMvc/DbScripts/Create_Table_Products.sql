
CREATE TABLE dbo.Products (
	Id int IDENTITY(1,1) NOT NULL,
	ProductCode nvarchar(500) NULL DEFAULT (NULL),
	Description nvarchar(500) NULL DEFAULT (NULL),
	CreatedDate datetime NULL DEFAULT (NULL)
);

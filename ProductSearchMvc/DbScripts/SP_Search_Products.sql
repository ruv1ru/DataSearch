CREATE PROCEDURE SearchProducts

@RecordsPerPage INT,
@PageNo INT,
@KeyWord NVARCHAR(500),
@SortBy NVARCHAR(100)

AS
BEGIN

	IF @RecordsPerPage <= 0 BEGIN
		SET @RecordsPerPage = 10
	END

	IF @PageNo <= 0 BEGIN
		SET @PageNo = 1
	END

	IF LEN(@SortBy) = 0 BEGIN
		SET @SortBy = 'CreatedDate DESC'
	END;

	WITH TABLE1 AS
    (
        SELECT ROW_NUMBER() OVER (ORDER BY @SortBy)  ROW, tbl_Products.* 
        FROM
        (
            SELECT [Id], [ProductCode], [Description], [CreatedDate]
            FROM [dbo].[Products]
            WHERE 1 = 1 AND ([ProductCode] LIKE '%' + @KeyWord + '%' OR [Description] LIKE '%' + @KeyWord + '%') 
        )  tbl_Products
    )

    SELECT (SELECT COUNT(Id) FROM TABLE1)  RECORDCOUNT, TABLE1.*
    FROM TABLE1 WHERE
    ROW BETWEEN CAST(((@PageNo - 1) * @RecordsPerPage + 1) AS NVARCHAR(10)) 
		AND CAST((@PageNo * @RecordsPerPage) AS NVARCHAR(10)) 


END
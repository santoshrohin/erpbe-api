CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_STOCKLEDGER]

			@STL_I_CODE int,
           @STL_DOC_NO int,
           @STL_DOC_NUMBER numeric(18,0),
           @STL_DOC_TYPE varchar(50),
           @STL_DOC_DATE datetime,
           @STL_DOC_QTY float,
           @STL_STORE_TYPE int         

AS

INSERT INTO [STOCK_LEDGER]
           ([STL_I_CODE]
           ,[STL_DOC_NO]
           ,[STL_DOC_NUMBER]
           ,[STL_DOC_TYPE]
           ,[STL_DOC_DATE]
           ,[STL_DOC_QTY]
           ,STL_STORE_TYPE
           )
     VALUES
           (@STL_I_CODE
           ,@STL_DOC_NO
           ,@STL_DOC_NUMBER
           ,@STL_DOC_TYPE
           ,@STL_DOC_DATE
           ,@STL_DOC_QTY
           ,@STL_STORE_TYPE
          )
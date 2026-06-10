CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_GIN_STOCKLEDGER]

	@GL_CH_NO varchar(50),
	@GL_DATE datetime,
	@GL_GIN_TYPE varchar(50),
	@GL_P_CODE int,
	@GL_I_CODE int,
	@GL_CQTY float,
	@GL_CON_QTY float,
	@GL_DOC_ID int,
	@GL_DOC_NO varchar(50),
	@GL_DOC_DATE datetime,
	@GL_DOC_TYPE varchar(50)
	


AS

		Insert into GIN_STOCK_LEDGER
		(
			GL_CH_NO,
			GL_DATE,
			GL_GIN_TYPE,
			GL_P_CODE,
			GL_I_CODE,
			GL_CQTY,
			GL_CON_QTY,
			GL_QTY_TEMP,
			GL_DOC_ID,
			GL_DOC_NO,
			GL_DOC_DATE,
			GL_DOC_TYPE
		
		)
		values
		(
			@GL_CH_NO,
			@GL_DATE,
			@GL_GIN_TYPE,
			@GL_P_CODE,
			@GL_I_CODE,
			@GL_CQTY,
			@GL_CON_QTY,
			0,
			@GL_DOC_ID,
			@GL_DOC_NO,
			@GL_DOC_DATE,
			@GL_DOC_TYPE
			
		)
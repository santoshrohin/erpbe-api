CREATE OR ALTER PROCEDURE SP_ITEM_MASTER_Update


@I_CODE    int,
@I_CM_COMP_ID    int,
@I_CAT_CODE    int,
@I_CODENO    varchar(50),
@I_DRAW_NO    varchar(50),
@I_NAME    varchar(50),
@I_MATERIAL    varchar(50),
@I_SPECIFICATION    varchar(50),
@I_ET_CODE    int,
@I_T_ACCT_S    varchar(50),
@I_T_ACCT_P    varchar(50),
@I_UOM_CODE    int,
@I_ABC_TYPE    varchar(50),
@I_MAX_LEVEL    float,
@I_MIN_LEVEL    float,
@I_REORDER_LEVEL    float,
@I_OP_BAL    float,
@I_OP_BAL_RATE    float,
@I_STORE_LOC    varchar(50),
@I_INV_RATE    float,
@I_RECEIPT_DATE    datetime,
@I_ISSUE_DATE    datetime,
@I_CURRENT_BAL    float,
@I_ACTIVE_IND    bit,
@ES_DELETE    bit,
@MODIFY    bit,
@I_UWEIGHT    float,
@I_UW_UOM_CODE    int,
@I_COST_HEAD varchar(50)

AS

Update ITEM_MASTER

Set
	
	I_CM_COMP_ID = @I_CM_COMP_ID,
	I_CAT_CODE = @I_CAT_CODE,
	I_CODENO = @I_CODENO,
	I_DRAW_NO = @I_DRAW_NO,
	I_NAME = @I_NAME,
	I_MATERIAL = @I_MATERIAL,
	I_SPECIFICATION = @I_SPECIFICATION,
	I_ET_CODE = @I_ET_CODE,
	I_T_ACCT_S = @I_T_ACCT_S,
	I_T_ACCT_P = @I_T_ACCT_P,
	I_UOM_CODE = @I_UOM_CODE,
	I_ABC_TYPE = @I_ABC_TYPE,
	I_MAX_LEVEL = @I_MAX_LEVEL,
	I_MIN_LEVEL = @I_MIN_LEVEL,
	I_REORDER_LEVEL = @I_REORDER_LEVEL,
	I_OP_BAL = @I_OP_BAL,
	I_OP_BAL_RATE = @I_OP_BAL_RATE,
	I_STORE_LOC = @I_STORE_LOC,
	I_INV_RATE = @I_INV_RATE,
	I_RECEIPT_DATE = @I_RECEIPT_DATE,
	I_ISSUE_DATE = @I_ISSUE_DATE,
	I_CURRENT_BAL = @I_CURRENT_BAL,
	I_ACTIVE_IND = @I_ACTIVE_IND,
	ES_DELETE = @ES_DELETE,
	MODIFY = @MODIFY,
	I_UWEIGHT = @I_UWEIGHT,
	I_UW_UOM_CODE = @I_UW_UOM_CODE,
	 I_COST_HEAD = @I_COST_HEAD
Where

I_CODE = @I_CODE
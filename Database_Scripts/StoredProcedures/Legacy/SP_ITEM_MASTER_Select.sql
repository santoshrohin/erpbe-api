CREATE OR ALTER PROCEDURE SP_ITEM_MASTER_Select


@I_CODE int,
@I_CM_COMP_ID int,
@I_CAT_CODE int,
@I_CODENO varchar(50),
@I_DRAW_NO varchar(50),
@I_NAME varchar(50),
@I_MATERIAL varchar(50),
@I_SPECIFICATION varchar(50),
@I_ET_CODE int,
@I_T_ACCT_S varchar(50),
@I_T_ACCT_P varchar(50),
@I_UOM_CODE int,
@I_ABC_TYPE varchar(50),
@I_MAX_LEVEL float,
@I_MIN_LEVEL float,
@I_REORDER_LEVEL float,
@I_OP_BAL float,
@I_OP_BAL_RATE float,
@I_STORE_LOC varchar(50),
@I_INV_RATE float,
@I_RECEIPT_DATE datetime,
@I_ISSUE_DATE datetime,
@I_CURRENT_BAL float,
@I_ACTIVE_IND bit,
@ES_DELETE bit,
@MODIFY bit,
@I_UWEIGHT float,
@I_UW_UOM_CODE int,
@I_COST_HEAD varchar(50)

AS

Select 

I_CODE,
I_CM_COMP_ID,
I_CAT_CODE,
I_CODENO,
I_DRAW_NO,
I_NAME,
I_MATERIAL,
I_SPECIFICATION,
I_ET_CODE,
I_T_ACCT_S,
I_T_ACCT_P,
I_UOM_CODE,
I_ABC_TYPE,
I_MAX_LEVEL,
I_MIN_LEVEL,
I_REORDER_LEVEL,
I_OP_BAL,
I_OP_BAL_RATE,
I_STORE_LOC,
I_INV_RATE,
I_RECEIPT_DATE,
I_ISSUE_DATE,
I_CURRENT_BAL,
I_ACTIVE_IND,
ES_DELETE,
MODIFY,
I_UWEIGHT,
I_UW_UOM_CODE,
I_COST_HEAD

from ITEM_MASTER

where 
( @I_CODE is null or @I_CODE = I_CODE ) and
( @I_CM_COMP_ID is null or @I_CM_COMP_ID = I_CM_COMP_ID ) and
( @I_CAT_CODE is null or @I_CAT_CODE = I_CAT_CODE ) and
( @I_CODENO is null or @I_CODENO = I_CODENO ) and
( @I_DRAW_NO is null or @I_DRAW_NO = I_DRAW_NO ) and
( @I_NAME is null or @I_NAME = I_NAME ) and
( @I_MATERIAL is null or @I_MATERIAL = I_MATERIAL ) and
( @I_SPECIFICATION is null or @I_SPECIFICATION = I_SPECIFICATION ) and
( @I_ET_CODE is null or @I_ET_CODE = I_ET_CODE ) and
( @I_T_ACCT_S is null or @I_T_ACCT_S = I_T_ACCT_S ) and
( @I_T_ACCT_P is null or @I_T_ACCT_P = I_T_ACCT_P ) and
( @I_UOM_CODE is null or @I_UOM_CODE = I_UOM_CODE ) and
( @I_ABC_TYPE is null or @I_ABC_TYPE = I_ABC_TYPE ) and
( @I_MAX_LEVEL is null or @I_MAX_LEVEL = I_MAX_LEVEL ) and
( @I_MIN_LEVEL is null or @I_MIN_LEVEL = I_MIN_LEVEL ) and
( @I_REORDER_LEVEL is null or @I_REORDER_LEVEL = I_REORDER_LEVEL ) and
( @I_OP_BAL is null or @I_OP_BAL = I_OP_BAL ) and
( @I_OP_BAL_RATE is null or @I_OP_BAL_RATE = I_OP_BAL_RATE ) and
( @I_STORE_LOC is null or @I_STORE_LOC = I_STORE_LOC ) and
( @I_INV_RATE is null or @I_INV_RATE = I_INV_RATE ) and
( @I_RECEIPT_DATE is null or @I_RECEIPT_DATE = I_RECEIPT_DATE ) and
( @I_ISSUE_DATE is null or @I_ISSUE_DATE = I_ISSUE_DATE ) and
( @I_CURRENT_BAL is null or @I_CURRENT_BAL = I_CURRENT_BAL ) and
( @I_ACTIVE_IND is null or @I_ACTIVE_IND = I_ACTIVE_IND ) and
( @ES_DELETE is null or @ES_DELETE = ES_DELETE ) and
( @MODIFY is null or @MODIFY = MODIFY ) and
( @I_UWEIGHT is null or @I_UWEIGHT = I_UWEIGHT ) and
( @I_UW_UOM_CODE is null or @I_UW_UOM_CODE = I_UW_UOM_CODE ) and
 @I_COST_HEAD = I_COST_HEAD
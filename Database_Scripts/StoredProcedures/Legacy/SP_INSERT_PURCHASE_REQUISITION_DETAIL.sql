CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_PURCHASE_REQUISITION_DETAIL] 

			
		   
			@PRD_PRM_CODE int,
			@PRD_I_CODE int,
			@PRD_REQ_QTY float,
			@PRD_OLD_QTY float,
			@PRD_ORD_QTY float,
			@PRD_REMARK varchar(100),	
            @PK_CODE int output,
            @ERROR varchar(max) output

AS


Insert into PURCHASE_REQUISION_DETAIL
(
PRD_PRM_CODE,
PRD_I_CODE,
PRD_REQ_QTY,
PRD_OLD_QTY,
PRD_ORD_QTY,
PRD_REMARK
)
values
(
@PRD_PRM_CODE,
@PRD_I_CODE,
@PRD_REQ_QTY,
@PRD_OLD_QTY,
@PRD_ORD_QTY,
@PRD_REMARK
)
select @PK_CODE = SCOPE_IDENTITY()
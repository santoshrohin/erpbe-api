CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_MATERIAL_REQUISITION_DETAIL] --'a',-2134,-2123,2.4,4.4,-21323,123,232,'121asa',out

			
		   
			@MRD_MR_CODE int,
			@MRD_I_CODE int,
			@MRD_REQ_QTY float,
		    @MRD_PROCESS_CODE int,
		    @MRD_STEPS_NO int,
            @PK_CODE int output,
            @ERROR varchar(max) output

AS


Insert into MATERIAL_REQUISITION_DETAIL
(
MRD_MR_CODE,
MRD_I_CODE,
MRD_REQ_QTY,
MRD_PROCESS_CODE,
MRD_STEPS_NO
)
values
(
@MRD_MR_CODE,
@MRD_I_CODE,
@MRD_REQ_QTY,
@MRD_PROCESS_CODE,
@MRD_STEPS_NO
)
select @PK_CODE = SCOPE_IDENTITY()
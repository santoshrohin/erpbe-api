CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_PRODUCTION_TO_STORE_DETAIL] --'a',-2134,-2123,2.4,4.4,-21323,123,232,'121asa',out

			
		   
			@PSD_PS_CODE int,
			@PSD_I_CODE int,
			@PSD_QTY float,
			@PSD_REMARK varchar(100),				
            @PK_CODE int output,
            @ERROR varchar(max) output,
            @PMD_To_STORE int

AS


Insert into PRODUCTION_TO_STORE_DETAIL
(
PSD_PS_CODE,
PSD_I_CODE,
PSD_QTY,
PSD_REMARK,
PMD_To_STORE
)
values
(
@PSD_PS_CODE,
@PSD_I_CODE,
@PSD_QTY,
@PSD_REMARK,
@PMD_To_STORE
)
select @PK_CODE = SCOPE_IDENTITY()
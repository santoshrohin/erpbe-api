-- =============================================
-- Author:		<Simya,Mangesh Kunkule>
-- Create date: <21-Jul-2013>
-- Description:	<Procedure For Insert,Update,Delete BillPassing master>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_MATERIAL_REQUISITION_MASTER] 
(
		@PROCESS nvarchar(20)='',
		@MR_CODE int,
		@MR_COMP_CM_CODE int,
		@MR_DEPT_NAME varchar(100),
		@MR_BATCH_NO varchar(50),
		@MR_DATE datetime,		
		@MR_FORMULA varchar(100),
		@MR_TYPE varchar(50),
		@MR_CPOM_CODE int,
		@MR_I_CODE int,
		@MR_UM_CODE int,
		@MR_BATCH_CODE int,
		@MR_FORMULA_CODE int,
		@PK_CODE numeric(18,0) Output,
		@ERROR varchar(max) Output
		
)
AS
--For Save
	BEGIN
	    BEGIN TRY 
			IF @PROCESS='Insert'	
				--BEGIN-------Insert
				--			IF NOT EXISTS(SELECT MR_BATCH_NO FROM MATERIAL_REQUISITION_MASTER WHERE MR_BATCH_CODE=@MR_BATCH_CODE and ES_DELETE=0)
								BEGIN---Check  Area No
										Insert into MATERIAL_REQUISITION_MASTER
											(MR_COMP_CM_CODE,
											MR_DEPT_NAME,
											MR_BATCH_NO,
											MR_DATE,		
											MR_FORMULA,
											MR_TYPE,
											MR_CPOM_CODE,
											MR_I_CODE,
											MR_UM_CODE,
											MR_BATCH_CODE,
											MR_FORMULA_CODE)
											values
											(@MR_COMP_CM_CODE,
											@MR_DEPT_NAME,
											@MR_BATCH_NO,
											@MR_DATE,		
											@MR_FORMULA,
											@MR_TYPE,
											@MR_CPOM_CODE,
											@MR_I_CODE,
											@MR_UM_CODE,
											@MR_BATCH_CODE,
											@MR_FORMULA_CODE)

										
										IF (@@ROWCOUNT>0)     
										 BEGIN-----------1 
											SET @PK_CODE=SCOPE_IDENTITY()
											SET @ERROR='Record Successfully Saved'																
									     END-------------1	
						          END------Check Description					 				
		                 	--ELSE 
				         --        BEGIN---isexist
						     	 -- SET @PK_CODE=0
							      --SET @ERROR='Inward No. Already Exist'	
				         --   END----isexist
		     --END
		ELSE IF @PROCESS='Update'
			  BEGIN
				  IF NOT EXISTS(SELECT MR_BATCH_NO FROM MATERIAL_REQUISITION_MASTER WHERE MR_CODE<>@MR_CODE and  ES_DELETE=0)
					BEGIN----1
						     Update MATERIAL_REQUISITION_MASTER Set
						      MR_COMP_CM_CODE=@MR_COMP_CM_CODE,
											MR_DEPT_NAME=@MR_DEPT_NAME,
											MR_BATCH_NO=@MR_BATCH_NO,
											MR_DATE=@MR_DATE,		
											MR_FORMULA=@MR_FORMULA,
											MR_TYPE=@MR_TYPE,
											MR_CPOM_CODE=@MR_CPOM_CODE,
											MR_I_CODE=@MR_I_CODE,
											MR_UM_CODE=@MR_UM_CODE,
											MR_BATCH_CODE=@MR_BATCH_CODE,
											MR_FORMULA_CODE=@MR_FORMULA_CODE
	
	                        where MR_CODE=@MR_CODE
								 IF (@@ROWCOUNT>0)     
									  BEGIN-----------1 
										SET @PK_CODE=@MR_CODE
										SET @ERROR='Record Successfully Updated'																
									 END-------------1	
						
					END				
					ELSE 
						BEGIN----Area Name Already Exist
							SET @PK_CODE=0
							SET @ERROR='Batch Already Exist'	
						END----Area Name Already Exist
		     END----1
		
		

end TRY 
		begin catch
			SET @PK_CODE=0
			SET  @ERROR= ERROR_MESSAGE();
		end catch    
END
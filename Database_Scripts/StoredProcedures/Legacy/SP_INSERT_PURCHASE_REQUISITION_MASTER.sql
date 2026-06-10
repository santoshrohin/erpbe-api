-- =============================================
-- Author:		<Simya,Apoorv lele>
-- Create date: <23-Jul-2013>
-- Description:	<Procedure For Insert,Update,Delete Purchase Requisition Master>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_PURCHASE_REQUISITION_MASTER] 
(
		@PROCESS nvarchar(20)='',
		@PRM_CODE int,
		@PRM_CM_COMP_CODE int,
		@PRM_TYPE int,
		@PRM_NO varchar(50),
		@PRM_DATE datetime,		
		@PRM_MR_CODE int,
		@PRM_I_CODE int,
		@PRM_DEPARTMENT varchar(50),
		@PRM_UM_CODE int,
		@PK_CODE numeric(18,0) Output,
		@ERROR varchar(max) Output
		
)
AS
--For Save
	BEGIN
	    BEGIN TRY 
			IF @PROCESS='Insert'	
				BEGIN-------Insert
							IF NOT EXISTS(SELECT PRM_NO FROM PRUCHASE_REQUISITION_MASTER WHERE PRM_NO=@PRM_NO and ES_DELETE=0)
								BEGIN---Check  Area No
										Insert into PRUCHASE_REQUISITION_MASTER
											(PRM_CM_COMP_CODE,
											PRM_TYPE ,
											PRM_NO,
											PRM_DATE,
											PRM_MR_CODE,
											PRM_I_CODE,
											PRM_DEPARTMENT,
											PRM_UM_CODE)
											values
											(@PRM_CM_COMP_CODE,
											@PRM_TYPE,
											@PRM_NO,
											@PRM_DATE,		
											@PRM_MR_CODE,
											@PRM_I_CODE,
											@PRM_DEPARTMENT,
											@PRM_UM_CODE)

										
										IF (@@ROWCOUNT>0)     
										 BEGIN-----------1 
											SET @PK_CODE=SCOPE_IDENTITY()
											SET @ERROR='Record Successfully Saved'																
									     END-------------1	
						          END------Check Description					 				
		                 	ELSE 
				                 BEGIN---isexist
						     	  SET @PK_CODE=0
							      SET @ERROR='Inward No. Already Exist'	
				            END----isexist
		     END
		ELSE IF @PROCESS='Update'
			  BEGIN
				  IF NOT EXISTS(SELECT PRM_NO FROM PRUCHASE_REQUISITION_MASTER WHERE PRM_CODE<>@PRM_CODE and PRM_NO=@PRM_NO and ES_DELETE=0)
					BEGIN----1
						     Update PRUCHASE_REQUISITION_MASTER Set
						      PRM_CM_COMP_CODE=@PRM_CM_COMP_CODE,
							  PRM_TYPE=@PRM_TYPE,
							  PRM_NO=@PRM_NO,
							  PRM_DATE=@PRM_DATE,		
							  PRM_MR_CODE=@PRM_MR_CODE,
							  PRM_I_CODE=@PRM_I_CODE,
							  PRM_DEPARTMENT=@PRM_DEPARTMENT,
							  PRM_UM_CODE=@PRM_UM_CODE
	
	                        where PRM_CODE=@PRM_CODE
								 IF (@@ROWCOUNT>0)     
									  BEGIN-----------1 
										SET @PK_CODE=@PRM_CODE
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
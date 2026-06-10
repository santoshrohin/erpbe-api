CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_PRODUCTION_TO_STORE_MASTER] 
(
		@PROCESS nvarchar(20)='',
		@PS_CODE int,
		@PS_MR_CODE int,
		@PS_CM_COMP_CODE int,
		@PS_GIN_NO int,
		@PS_GIN_DATE datetime,
		@PS_PERSON_NAME varchar(50),		
		@PS_TYPE int,	
		@PS_BATCH_NO int,
		@PS_P_CODE int,	
		@PK_CODE numeric(18,0) Output,
		@ERROR varchar(max) Output,
		@PM_FROM_STORE int
		
)
AS
--For Save
	BEGIN
	    BEGIN TRY 
			IF @PROCESS='Insert'	
				BEGIN-------Insert
							IF NOT EXISTS(SELECT PS_GIN_NO FROM PRODUCTION_TO_STORE_MASTER WHERE PS_GIN_NO=@PS_GIN_NO and ES_DELETE=0 AND PS_CM_COMP_CODE=@PS_CM_COMP_CODE)
								BEGIN---Check  Area No
										Insert into PRODUCTION_TO_STORE_MASTER
											(PS_MR_CODE,
											PS_CM_COMP_CODE,
											PS_GIN_NO,
											PS_GIN_DATE,
											PS_PERSON_NAME,		
											PS_TYPE,
											PS_BATCH_NO,
											PS_P_CODE,
											PM_FROM_STORE)
											
											values
											(@PS_MR_CODE,
											@PS_CM_COMP_CODE,
											@PS_GIN_NO,
											@PS_GIN_DATE,
											@PS_PERSON_NAME,		
											@PS_TYPE,
											@PS_BATCH_NO,
											@PS_P_CODE,
											@PM_FROM_STORE)

										
										IF (@@ROWCOUNT>0)     
										 BEGIN-----------1 
											SET @PK_CODE=SCOPE_IDENTITY()
											SET @ERROR='Record Successfully Saved'																
									     END-------------1	
						          END------Check Description					 				
		                 	ELSE 
				                 BEGIN---isexist
						     	  SET @PK_CODE=0
							      SET @ERROR='Production No Already Exist'	
				            END----isexist
		     END
		ELSE IF @PROCESS='Update'
			  BEGIN
				  IF NOT EXISTS(SELECT PS_GIN_NO FROM PRODUCTION_TO_STORE_MASTER WHERE PS_CODE<>@PS_CODE and PS_GIN_NO=@PS_GIN_NO and ES_DELETE=0 AND PS_CM_COMP_CODE=@PS_CM_COMP_CODE)
					BEGIN----1
						     Update PRODUCTION_TO_STORE_MASTER Set
						      PS_MR_CODE=@PS_MR_CODE,
						      PS_CM_COMP_CODE=@PS_CM_COMP_CODE,
											PS_GIN_NO=@PS_GIN_NO,
											PS_GIN_DATE=@PS_GIN_DATE,
											PS_PERSON_NAME=@PS_PERSON_NAME,		
											PS_TYPE=@PS_TYPE,
											PS_BATCH_NO=@PS_BATCH_NO,
											PS_P_CODE=@PS_P_CODE,
											PM_FROM_STORE=@PM_FROM_STORE
	
	                        where PS_CODE=@PS_CODE
								 IF (@@ROWCOUNT>0)     
									  BEGIN-----------1 
										SET @PK_CODE=@PS_CODE
										SET @ERROR='Record Successfully Updated'																
									 END-------------1	
						
					END				
					ELSE 
						BEGIN----Area Name Already Exist
							SET @PK_CODE=0
							SET @ERROR='Production No Already Exist'	
						END----Area Name Already Exist
		     END----1
		
		

end TRY 
		begin catch
			SET @PK_CODE=0
			SET  @ERROR= ERROR_MESSAGE();
		end catch    
END
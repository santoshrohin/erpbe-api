CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_SERVICE_INWARD_MASTER] --'Insert',-2147483647,1,0,120,'IWIM','12/Jun/2014 12:00:00 AM',-2147483637,120,'12/Jun/2014 12:00:00 AM',10,123,110,150
(
		@PROCESS nvarchar(20)='',
		@SIM_CODE int,
		@SIM_CM_CODE int,
		@SIM_INWARD_TYPE tinyint,
		@SIM_NO numeric,
		@SIM_TYPE varchar(50),
		@SIM_DATE smalldatetime,
		@SIM_P_CODE int,
		@SIM_CHALLAN_NO varchar(50),
		@SIM_CHAL_DATE datetime,
		@SIM_EGP_NO varchar(50),
		@SIM_LR_NO varchar(50),
		@SIM_OCT_NO varchar(50),
		@SIM_VEH_NO varchar(50),		
		@PK_CODE numeric(18,0) Output,
		@ERROR varchar(max) Output,
		
		@SIM_INV_NO varchar(50),
		@SIM_INV_DATE datetime,
		@SIM_TRANSPORATOR_NAME varchar(50)
		
)
AS
--For Save
	BEGIN
	    BEGIN TRY 
			IF @PROCESS='Insert'	
				BEGIN-------Insert
							IF NOT EXISTS(SELECT SIM_NO FROM SERVICE_INWARD_MASTER WHERE SIM_NO=@SIM_NO and ES_DELETE=0 AND SIM_TYPE=@SIM_TYPE and SIM_CM_CODE=@SIM_CM_CODE)
								BEGIN---Check  Area No
										INSERT INTO SERVICE_INWARD_MASTER (SIM_CM_CODE,SIM_INWARD_TYPE,SIM_NO,SIM_TYPE,SIM_DATE,SIM_P_CODE,SIM_CHALLAN_NO,SIM_CHAL_DATE,SIM_EGP_NO,SIM_LR_NO,SIM_OCT_NO,SIM_VEH_NO,SIM_INV_NO,SIM_INV_DATE,SIM_TRANSPORATOR_NAME)VALUES (@SIM_CM_CODE,@SIM_INWARD_TYPE,@SIM_NO,@SIM_TYPE,@SIM_DATE,@SIM_P_CODE,@SIM_CHALLAN_NO,@SIM_CHAL_DATE,@SIM_EGP_NO,@SIM_LR_NO,@SIM_OCT_NO,@SIM_VEH_NO,@SIM_INV_NO,@SIM_INV_DATE,@SIM_TRANSPORATOR_NAME)
										
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
				  IF NOT EXISTS(SELECT SIM_NO FROM SERVICE_INWARD_MASTER WHERE SIM_CODE<>@SIM_CODE and SIM_NO=@SIM_NO and ES_DELETE=0 AND SIM_TYPE=@SIM_TYPE and SIM_CM_CODE=@SIM_CM_CODE)
					BEGIN----1
						     Update SERVICE_INWARD_MASTER set SIM_INWARD_TYPE=@SIM_INWARD_TYPE ,SIM_NO=@SIM_NO ,SIM_TYPE=@SIM_TYPE,SIM_DATE=@SIM_DATE,SIM_P_CODE=@SIM_P_CODE,SIM_CHALLAN_NO=@SIM_CHALLAN_NO,SIM_CHAL_DATE=SIM_CHAL_DATE ,SIM_EGP_NO=@SIM_EGP_NO, SIM_LR_NO=@SIM_LR_NO,SIM_OCT_NO=@SIM_OCT_NO,SIM_VEH_NO=@SIM_VEH_NO,SIM_INV_NO=@SIM_INV_NO,SIM_INV_DATE=@SIM_INV_DATE,SIM_TRANSPORATOR_NAME=@SIM_TRANSPORATOR_NAME where SIM_CODE=@SIM_CODE
                        
								 IF (@@ROWCOUNT>0)     
									  BEGIN-----------1 
										SET @PK_CODE=@SIM_CODE
										SET @ERROR='Record Successfully Updated'																
									 END-------------1	
						
					END				
					ELSE 
						BEGIN----Area Name Already Exist
							SET @PK_CODE=0
							SET @ERROR='Inward Already Exist'	
						END----Area Name Already Exist
		     END----1
		
		

end TRY 
		begin catch
			SET @PK_CODE=0
			SET  @ERROR= ERROR_MESSAGE();
		end catch    
END
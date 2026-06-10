-- =============================================
-- Author:		<Simya,Santosh>
-- Create date: <24-Jun-2013>
-- Description:	<Procedure For Insert,Update,Delete Inwarw masrer>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_INWARD_MASTER] --'Insert',-2147483647,1,0,120,'IWIM','12/Jun/2014 12:00:00 AM',-2147483637,120,'12/Jun/2014 12:00:00 AM',10,123,110,150
(
		@PROCESS nvarchar(20)='',
		@IWM_CODE int,
		@IWM_CM_CODE int,
		@IWM_INWARD_TYPE tinyint,
		@IWM_NO numeric,
		@IWM_TYPE varchar(50),
		@IWM_DATE smalldatetime,
		@IWM_P_CODE int,
		@IWM_CHALLAN_NO varchar(50),
		@IWM_CHAL_DATE datetime,
		@IWM_EGP_NO varchar(50),
		@IWM_LR_NO varchar(50),
		@IWM_OCT_NO varchar(50),
		@IWM_VEH_NO varchar(50),		
		@PK_CODE numeric(18,0) Output,
		@ERROR varchar(max) Output,
		
		@IWM_INV_NO varchar(50),
		@IWM_INV_DATE datetime,
		@IWM_TRANSPORATOR_NAME varchar(50)
		
)
AS
--For Save
	BEGIN
	    BEGIN TRY 
			IF @PROCESS='Insert'	
				BEGIN-------Insert
							IF NOT EXISTS(SELECT IWM_NO FROM INWARD_MASTER WHERE IWM_NO=@IWM_NO and ES_DELETE=0 AND IWM_CM_CODE=@IWM_CM_CODE AND IWM_TYPE=@IWM_TYPE)
								BEGIN---Check  Area No
										INSERT INTO INWARD_MASTER (IWM_CM_CODE,IWM_INWARD_TYPE,IWM_NO,IWM_TYPE,IWM_DATE,IWM_P_CODE,IWM_CHALLAN_NO,IWM_CHAL_DATE,IWM_EGP_NO,IWM_LR_NO,IWM_OCT_NO,IWM_VEH_NO,IWM_INV_NO,IWM_INV_DATE,IWM_TRANSPORATOR_NAME)VALUES (@IWM_CM_CODE,@IWM_INWARD_TYPE,@IWM_NO,@IWM_TYPE,@IWM_DATE,@IWM_P_CODE,@IWM_CHALLAN_NO,@IWM_CHAL_DATE,@IWM_EGP_NO,@IWM_LR_NO,@IWM_OCT_NO,@IWM_VEH_NO,@IWM_INV_NO,@IWM_INV_DATE,@IWM_TRANSPORATOR_NAME)
										
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
				  IF NOT EXISTS(SELECT IWM_NO FROM INWARD_MASTER WHERE IWM_CODE<>@IWM_CODE  AND IWM_CM_CODE=@IWM_CM_CODE and IWM_NO=@IWM_NO and ES_DELETE=0 AND IWM_TYPE=@IWM_TYPE)
					BEGIN----1
						     Update INWARD_MASTER set IWM_INWARD_TYPE=@IWM_INWARD_TYPE ,IWM_NO=@IWM_NO ,IWM_TYPE=@IWM_TYPE,IWM_DATE=@IWM_DATE,IWM_P_CODE=@IWM_P_CODE,IWM_CHALLAN_NO=@IWM_CHALLAN_NO,IWM_CHAL_DATE=IWM_CHAL_DATE ,IWM_EGP_NO=@IWM_EGP_NO, IWM_LR_NO=@IWM_LR_NO,IWM_OCT_NO=@IWM_OCT_NO,IWM_VEH_NO=@IWM_VEH_NO,IWM_INV_NO=@IWM_INV_NO,IWM_INV_DATE=@IWM_INV_DATE,IWM_TRANSPORATOR_NAME=@IWM_TRANSPORATOR_NAME where IWM_CODE=@IWM_CODE
                        
								 IF (@@ROWCOUNT>0)     
									  BEGIN-----------1 
										SET @PK_CODE=@IWM_CODE
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
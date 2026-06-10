CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_BILLPASSING_DETAIL] --'a',-2134,-2123,2.4,4.4,-21323,123,232,'121asa',out

            
            @BPD_BPM_CODE int,
            @BPD_IWM_CODE int,
            @BPD_SPOM_CODE int,
            @BPD_I_CODE int,
            @BPD_RECD_QTY float,
            @BPD_OK_QTY float,
            @BPD_RATE float,
            @BPD_AMT float,    
            @BPD_EXC_AMT float,    
            @BPD_EDU_AMT float,    
            @BPD_HSEDU_AMT float,    
            @BPD_DISC_AMT float,            
           @PK_CODE int output,
            @ERROR varchar(max) output

AS


Insert into BILL_PASSING_DETAIL
(
BPD_BPM_CODE,
BPD_IWM_CODE,
BPD_SPOM_CODE,
BPD_I_CODE,
BPD_RECD_QTY,
BPD_OK_QTY,
BPD_RATE,
BPD_AMT,
BPD_EXC_AMT,
BPD_EDU_AMT,
BPD_HSEDU_AMT,
BPD_DISC_AMT 
)
values
(
@BPD_BPM_CODE,
@BPD_IWM_CODE,
@BPD_SPOM_CODE,
@BPD_I_CODE,
@BPD_RECD_QTY,
@BPD_OK_QTY,
@BPD_RATE,
@BPD_AMT,
@BPD_EXC_AMT,
@BPD_EDU_AMT,
@BPD_HSEDU_AMT,
@BPD_DISC_AMT 
)
select @PK_CODE = SCOPE_IDENTITY()
CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_SERVICE_INWARD_DETAIL] --'a',-2134,-2123,2.4,4.4,-21323,123,232,'121asa',out

@PROCESS varchar(50),
           @SID_SIM_CODE int,
           @SID_I_CODE int,
           @SID_SQTY float,
           @SID_RATE float,
           @SID_CPOM_CODE int,
           @SID_CH_QTY float,
           @SID_REV_QTY float,
           @SID_REMARK varchar(150),
           @SID_UOM_CODE int,
           @PK_CODE int output,
           @ERROR varchar(max) output,
           @SID_BATCH_NO varchar(50),
           @SID_PROCESS_CODE int=0,
           @SID_TUR_QTY float=0,
           @SID_TUR_WEIGHT float=0
AS

INSERT INTO [SERVICE_INWARD_DETAIL]
           ([SID_SIM_CODE]
           ,[SID_I_CODE]
           ,[SID_SQTY]
           ,[SID_RATE]
           ,[SID_CPOM_CODE]
           ,[SID_CH_QTY]
           ,[SID_REV_QTY]
           ,[SID_REMARK]
           ,[SID_UOM_CODE]
           ,[SID_BATCH_NO],[SID_PROCESS_CODE]
           ,[SID_TUR_QTY],[SID_TUR_WEIGHT]
           )
     VALUES
           (@SID_SIM_CODE,
           @SID_I_CODE,
           @SID_SQTY,
           @SID_RATE,
           @SID_CPOM_CODE,
           @SID_CH_QTY,
           @SID_REV_QTY,
           @SID_REMARK,
           @SID_UOM_CODE,
           @SID_BATCH_NO,
           @SID_PROCESS_CODE,
           @SID_TUR_QTY,
           @SID_TUR_WEIGHT
           )
CREATE OR ALTER PROCEDURE [dbo].[SP_INSERT_INWARD_DETAIL] --'a',-2134,-2123,2.4,4.4,-21323,123,232,'121asa',out

@PROCESS varchar(50),
           @IWD_IWM_CODE int,
           @IWD_I_CODE int,
           @IWD_SQTY float,
           @IWD_RATE float,
           @IWD_CPOM_CODE int,
           @IWD_CH_QTY float,
           @IWD_REV_QTY float,
           @IWD_REMARK varchar(150),
           @IWD_UOM_CODE int,
           @PK_CODE int output,
           @ERROR varchar(max) output,
           @IWD_BATCH_NO varchar(50),
           @IWD_PROCESS_CODE int=0,
           @IWD_TUR_QTY float=0,
           @IWD_TUR_WEIGHT float=0
AS

INSERT INTO [INWARD_DETAIL]
           ([IWD_IWM_CODE]
           ,[IWD_I_CODE]
           ,[IWD_SQTY]
           ,[IWD_RATE]
           ,[IWD_CPOM_CODE]
           ,[IWD_CH_QTY]
           ,[IWD_REV_QTY]
           ,[IWD_REMARK]
           ,[IWD_UOM_CODE]
           ,[IWD_BATCH_NO],[IWD_PROCESS_CODE]
           ,[IWD_TUR_QTY],[IWD_TUR_WEIGHT]
           )
     VALUES
           (@IWD_IWM_CODE,
           @IWD_I_CODE,
           @IWD_SQTY,
           @IWD_RATE,
           @IWD_CPOM_CODE,
           @IWD_CH_QTY,
           @IWD_REV_QTY,
           @IWD_REMARK,
           @IWD_UOM_CODE,
           @IWD_BATCH_NO,
           @IWD_PROCESS_CODE,
           @IWD_TUR_QTY,
           @IWD_TUR_WEIGHT
           )
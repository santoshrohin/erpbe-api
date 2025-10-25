CREATE PROCEDURE [dbo].[ERP_GetCustomerPoById]
    @PoCode INT,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT OFF;

    BEGIN TRY
        -- Get Master Record with Customer Name, Project Name, Currency Name
        SELECT 
            m.CPOM_CODE AS PoCode,
            m.CPOM_P_CODE AS CustomerCode,
            p.P_NAME AS CustomerName,
            m.CPOM_PONO AS PoNumber,
            m.CPOM_DOC_NO AS DocumentNumber,
            m.CPOM_TYPE AS PoType,
            m.CPOM_DATE AS PoDate,
            m.CPOM_CR_DAYS AS CreditDays,
            m.CPOM_CM_COMP_ID AS CompanyId,
            m.CPOM_WORK_ODR_NO AS WorkOrderNumber,
            m.MODIFY AS IsLocked,
            m.ES_DELETE AS IsDeleted,
            m.CPOM_PAY_TERM AS PaymentTerms,
            m.CPOM_AUTH_FLG AS IsAuthorized,
            m.CPOM_PO_DATE AS CustomerPoDate,
            m.CPOM_QE_CODE AS QuotationCode,
            m.CPOM_T_NAME AS TaxName,
            m.CPOM_T_PER AS TaxPercentage,
            m.CPOM_T_AMT AS TaxAmount,
            m.CPOM_EXC_PER AS ExcisePercentage,
            m.CPOM_EXC_EDU_PER AS ExciseEducationPercentage,
            m.CPOM_EXC_HEDU_PER AS ExciseHigherEducationPercentage,
            m.CPOM_BASIC_AMT AS BasicAmount,
            m.CPOM_DISCOUNT_PER AS DiscountPercentage,
            m.CPOM_DISCOUNT_AMT AS DiscountAmount,
            m.CPOM_DISCOUNT_REASON AS DiscountReason,
            m.CPOM_DEVIATION_AMT AS DeviationAmount,
            m.CPOM_DEVIATION_REASON AS DeviationReason,
            m.CPOM_PACKING_AMT AS PackingAmount,
            m.CPOM_EXC_AMT AS ExciseAmount,
            m.CPOM_ROUNDING AS RoundingAmount,
            m.CPOM_GRAND_TOT AS GrandTotal,
            m.CPOM_INV_FLAG AS InvoiceGenerated,
            m.CPOM_AM_COUNT AS AmendmentCount,
            m.CPOM_FINAL_DEST AS FinalDestination,
            m.CPOM_PRE_CARR_BY AS PreCarriageBy,
            m.CPOM_PORT_LOAD AS PortOfLoading,
            m.CPOM_PORT_DIS AS PortOfDischarge,
            m.CPOM_PLACE_DEL AS PlaceOfDelivery,
            m.CPOM_BUYER_NAME AS BuyerName,
            m.CPOM_BUYER_ADD AS BuyerAddress,
            m.CPOM_CURR_CODE AS CurrencyCode,
            m.CPOM_AM_DATE AS AmendmentDate,
            m.CPOM_INQ_CODE AS InquiryCode,
            m.CPOM_IS_VERBAL AS IsVerbalOrder,
            m.CPOM_PROJECT_CODE AS ProjectCode,
            m.CPOM_PROJECT_NAME AS ProjectName
        FROM CUSTPO_MASTER m
        INNER JOIN PARTY_MASTER p ON m.CPOM_P_CODE = p.P_CODE
        WHERE 
            m.CPOM_CODE = @PoCode 
            AND m.CPOM_CM_COMP_ID = @CompanyId
            AND m.ES_DELETE = 0;

        -- Get Detail Records with Item Name, UOM Name, Store Name, Currency Name
        SELECT 
            d.CPOD_CPOM_CODE AS PoCode,
            d.CPOD_I_CODE AS ItemCode,
            i.I_NAME AS ItemName,
            d.CPOD_UOM_CODE AS UomCode,
            u.I_UOM_NAME AS UomName,
            d.CPOD_ORD_QTY AS OrderedQuantity,
            d.CPOD_RATE AS Rate,
            d.CPOD_AMT AS Amount,
            d.CPOD_DESC AS Description,
            d.CPOD_CUST_I_CODE AS CustomerItemCode,
            d.CPOD_CUST_I_NAME AS CustomerItemName,
            d.CPOD_STATUS AS Status,
            d.CPOD_DISPACH AS DispatchedQuantity,
            d.CPOD_IS_ORDER AS IsOrder,
            d.CPOD_ST_CODE AS StoreCode,
            d.CPOD_CURR_CODE AS CurrencyCode,
            d.CPOD_WO_QTY AS WorkOrderQuantity,
            d.CPOD_MODNO AS ModificationNumber,
            d.CPOD_MODDATE AS ModificationDate,
            d.CPOD_AMORTRATE AS AmortizationRate,
            d.CPOD_DIEAMORTRATE AS DieAmortizationRate,
            d.CPOD_DISC_PER AS DiscountPercentage,
            d.CPOD_DISC_AMT AS DiscountAmount
        FROM CUSTPO_DETAIL d
        INNER JOIN ITEM_MASTER i ON d.CPOD_I_CODE = i.I_CODE
        INNER JOIN ITEM_UNIT_MASTER u ON d.CPOD_UOM_CODE = u.I_UOM_CODE
        WHERE d.CPOD_CPOM_CODE = @PoCode
        ORDER BY d.CPOD_I_CODE;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO


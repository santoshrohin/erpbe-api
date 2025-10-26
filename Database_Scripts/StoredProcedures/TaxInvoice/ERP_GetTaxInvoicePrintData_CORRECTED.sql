CREATE OR ALTER PROCEDURE [dbo].[ERP_GetTaxInvoicePrintData]
    @InvoiceCode INT,
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- =========================================================================
        -- 1. GET COMPANY INFORMATION
        -- =========================================================================
        SELECT 
            CM_NAME AS CompanyName,
            ISNULL(CM_ADDRESS1, '') + CASE WHEN ISNULL(CM_ADDRESS2, '') <> '' THEN ', ' + CM_ADDRESS2 ELSE '' END + 
            CASE WHEN ISNULL(CM_ADDRESS3, '') <> '' THEN ', ' + CM_ADDRESS3 ELSE '' END AS Address,
            ISNULL(CM_CITY, '') AS City,
            ISNULL(CM_STATE, '') AS State,
            '' AS StateCode, -- Need to add if column exists
            '' AS PinCode,   -- Need to add if column exists
            ISNULL(CM_PHONENO1, '') AS Phone,
            ISNULL(CM_FAXNO, '') AS Fax,
            ISNULL(CM_EMAILID, '') AS Email,
            ISNULL(CM_WEBSITE, '') AS Website,
            ISNULL(CM_GST_NO, '') AS GstNumber,
            ISNULL(CM_PAN_NO, '') AS PanNumber,
            ISNULL(CM_CIN_NO, '') AS CinNumber,
            ISNULL(CM_VAT_TIN_NO, '') AS VatTin,
            ISNULL(CM_CST_NO, '') AS CstTin,
            '' AS EccNumber,
            '' AS IsoNumber,
            '' AS RegistrationNumber,
            NULL AS LogoPath
        FROM COMPANY_MASTER CM
        INNER JOIN INVOICE_MASTER INM ON INM.INM_CM_CODE = CM.CM_CODE
        WHERE CM.CM_ID = @CompanyId 
        AND INM.INM_CODE = @InvoiceCode
        AND ISNULL(CM.CM_DELETE_FLAG, 0) = 0;

        -- =========================================================================
        -- 2. GET INVOICE MASTER INFORMATION
        -- =========================================================================
        SELECT 
            INM_CODE AS InvoiceCode,
            INM_NO AS InvoiceNumber,
            INM_DATE AS InvoiceDate,
            ISNULL(INM_TNO, '') AS ReferenceNumber,  -- This contains the Serial Number like "SUN252605704"
            ISNULL((SELECT TOP 1 CPOM_PONO FROM CUSTPO_MASTER WHERE CPOM_CODE = INM_CPOM_CODE AND ES_DELETE = 0), '') AS PoNumber,
            (SELECT TOP 1 CPOM_PO_DATE FROM CUSTPO_MASTER WHERE CPOM_CODE = INM_CPOM_CODE AND ES_DELETE = 0) AS PoDate,
            ISNULL(INM_TRANSPORT, '') AS TransportName,
            ISNULL(INM_VEH_NO, '') AS VehicleNumber,
            ISNULL(INM_LR_NO, '') AS LrNumber,
            INM_LR_DATE AS LrDate,
            ISNULL(INM_NET_AMT, 0) AS BasicAmount,
            ISNULL(INM_DISC, 0) AS DiscountPercentage,
            ISNULL(INM_DISC_AMT, 0) AS DiscountAmount,
            ISNULL(INM_PACK_AMT, 0) AS PackingAmount,
            ISNULL(INM_FREIGHT, 0) AS FreightAmount,
            ISNULL(INM_INSURANCE, 0) AS InsuranceAmount,
            ISNULL(INM_OTHER_AMT, 0) AS OtherCharges,
            ISNULL(INM_ROUNDING_AMT, 0) AS RoundingAmount,
            ISNULL(INM_G_AMT, 0) AS GrandTotal,
            ISNULL(INM_REMARK, '') AS Remarks,
            ISNULL(INM_TERMSNCONDITIONS, '') AS TermsAndConditions
        FROM INVOICE_MASTER
        INNER JOIN COMPANY_MASTER CM ON CM.CM_CODE = INM_CM_CODE AND CM.CM_ID = @CompanyId AND ISNULL(CM.CM_DELETE_FLAG, 0) = 0
        WHERE INM_CODE = @InvoiceCode 
        AND INVOICE_MASTER.ES_DELETE = 0;

        -- =========================================================================
        -- 3. GET CUSTOMER INFORMATION
        -- =========================================================================
        SELECT 
            P_CODE AS CustomerCode,
            P_NAME AS CustomerName,
            ISNULL(P_ADD1, '') AS BillingAddress,
            ISNULL(P_DELIVERY_ADD, P_ADD1) AS ShippingAddress,
            ISNULL(P_CITY, '') AS City,
            ISNULL(SM.SM_NAME, '') AS State,
            ISNULL(SM.SM_STATE_CODE, '') AS StateCode,
            ISNULL(P_PIN_CODE, '') AS PinCode,
            ISNULL(P_GST_NO, '') AS GstNumber,
            ISNULL(P_PAN, '') AS PanNumber,
            ISNULL(P_CONTACT, '') AS ContactPerson,
            ISNULL(P_PHONE, '') AS Phone,
            ISNULL(P_EMAIL, '') AS Email
        FROM INVOICE_MASTER
        INNER JOIN PARTY_MASTER ON P_CODE = INM_P_CODE
        LEFT JOIN STATE_MASTER SM ON SM.SM_CODE = P_SM_CODE AND SM.ES_DELETE = 0
        WHERE INM_CODE = @InvoiceCode
        AND INVOICE_MASTER.ES_DELETE = 0;

        -- =========================================================================
        -- 4. GET INVOICE LINE ITEMS WITH TAX DETAILS
        -- =========================================================================
        SELECT 
            ROW_NUMBER() OVER (ORDER BY IND_INM_CODE, IND_I_CODE) AS SerialNumber,
            IND_I_CODE AS ItemCode,
            ISNULL(I.I_NAME, '') AS ItemName,
            ISNULL(IND_REMARK, I.I_NAME) AS ItemDescription,
            ISNULL(IND_HSN_CODE, '') AS HsnCode,
            '' AS CustomerItemCode,  -- Add if column exists
            ISNULL(IND_INQTY, 0) AS Quantity,
            ISNULL(U.I_UOM_NAME, 'NOS') AS UnitOfMeasurement,
            ISNULL(IND_RATE, 0) AS Rate,
            ISNULL(IND_AMT, 0) AS Amount,
            ISNULL(IND_NO_PACK, 0) AS NumberOfPackages,
            ISNULL(IND_PACK_DESC, '') AS PackingDescription,
            
            -- Tax Calculations (Percentages are stored, amounts need to be calculated)
            ISNULL(IND_AMT, 0) AS TaxableAmount,
            ISNULL(E_BASIC_CentralT, 0) AS CgstPercentage,
            ROUND(ISNULL(IND_AMT, 0) * ISNULL(E_BASIC_CentralT, 0) / 100, 2) AS CgstAmount,
            ISNULL(E_EDU_CESS_State, 0) AS SgstPercentage,
            ROUND(ISNULL(IND_AMT, 0) * ISNULL(E_EDU_CESS_State, 0) / 100, 2) AS SgstAmount,
            ISNULL(E_H_EDU_Integrated, 0) AS IgstPercentage,
            ROUND(ISNULL(IND_AMT, 0) * ISNULL(E_H_EDU_Integrated, 0) / 100, 2) AS IgstAmount,
            
            -- Total Tax = CGST + SGST + IGST
            ROUND(
                (ISNULL(IND_AMT, 0) * ISNULL(E_BASIC_CentralT, 0) / 100) +
                (ISNULL(IND_AMT, 0) * ISNULL(E_EDU_CESS_State, 0) / 100) +
                (ISNULL(IND_AMT, 0) * ISNULL(E_H_EDU_Integrated, 0) / 100), 2
            ) AS TotalTax,
            
            -- Total Amount = Taxable Amount + Total Tax
            ROUND(ISNULL(IND_AMT, 0) + 
                (ISNULL(IND_AMT, 0) * ISNULL(E_BASIC_CentralT, 0) / 100) +
                (ISNULL(IND_AMT, 0) * ISNULL(E_EDU_CESS_State, 0) / 100) +
                (ISNULL(IND_AMT, 0) * ISNULL(E_H_EDU_Integrated, 0) / 100), 2
            ) AS TotalAmount,
            
            ISNULL(IND_AMORTAMT, 0) AS AmortizationAmount
        FROM INVOICE_DETAIL
        INNER JOIN ITEM_MASTER I ON I.I_CODE = IND_I_CODE
        LEFT JOIN ITEM_UNIT_MASTER U ON U.I_UOM_CODE = ISNULL(IND_UOM_CODE, I.I_UOM_CODE)
        WHERE IND_INM_CODE = @InvoiceCode
        AND INVOICE_DETAIL.ES_DELETE = 0
        ORDER BY IND_INM_CODE, IND_I_CODE;

        -- =========================================================================
        -- 5. GET TAX SUMMARY
        -- =========================================================================
        SELECT 
            SUM(ISNULL(IND_AMT, 0)) AS TotalTaxableAmount,
            SUM(ROUND(ISNULL(IND_AMT, 0) * ISNULL(E_BASIC_CentralT, 0) / 100, 2)) AS TotalCgst,
            SUM(ROUND(ISNULL(IND_AMT, 0) * ISNULL(E_EDU_CESS_State, 0) / 100, 2)) AS TotalSgst,
            SUM(ROUND(ISNULL(IND_AMT, 0) * ISNULL(E_H_EDU_Integrated, 0) / 100, 2)) AS TotalIgst,
            SUM(ROUND(
                (ISNULL(IND_AMT, 0) * ISNULL(E_BASIC_CentralT, 0) / 100) +
                (ISNULL(IND_AMT, 0) * ISNULL(E_EDU_CESS_State, 0) / 100) +
                (ISNULL(IND_AMT, 0) * ISNULL(E_H_EDU_Integrated, 0) / 100), 2
            )) AS TotalGst,
            (SELECT ISNULL(INM_G_AMT, 0) FROM INVOICE_MASTER WHERE INM_CODE = @InvoiceCode) AS TotalAmount,
            '' AS AmountInWords,  -- Will be calculated in C#
            MAX(ISNULL(INM.INM_TAX_TCS, 0)) AS TcsPercentage,
            MAX(ISNULL(INM.INM_TAX_TCS_AMT, 0)) AS TcsAmount
        FROM INVOICE_DETAIL
        INNER JOIN INVOICE_MASTER INM ON INM.INM_CODE = IND_INM_CODE
        WHERE IND_INM_CODE = @InvoiceCode
        AND INVOICE_DETAIL.ES_DELETE = 0;

        -- =========================================================================
        -- 6. GET E-INVOICE INFORMATION (if exists)
        -- =========================================================================
        SELECT 
            ISNULL(IRN, '') AS Irn,
            ISNULL(AckNo, '') AS AcknowledgementNumber,
            AckDate AS AcknowledgementDate,
            ISNULL(EwayBill, '') AS EwayBillNumber,
            NULL AS QrCodeImage,  -- QRCode column might be text or image, handle in C#
            ISNULL(EInvStatus, '') AS EInvoiceStatus
        FROM INVOICE_MASTER
        WHERE INM_CODE = @InvoiceCode
        AND ES_DELETE = 0;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO


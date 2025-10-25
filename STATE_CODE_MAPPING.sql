-- GST State Code Mapping for India
-- Used in Tax Invoice for "State Code" field

CREATE FUNCTION dbo.GetStateCode(@StateName NVARCHAR(100))
RETURNS NVARCHAR(2)
AS
BEGIN
    DECLARE @StateCode NVARCHAR(2)
    
    SET @StateCode = CASE UPPER(LTRIM(RTRIM(@StateName)))
        WHEN 'ANDAMAN AND NICOBAR ISLANDS' THEN '35'
        WHEN 'ANDHRA PRADESH' THEN '37'
        WHEN 'ARUNACHAL PRADESH' THEN '12'
        WHEN 'ASSAM' THEN '18'
        WHEN 'BIHAR' THEN '10'
        WHEN 'CHANDIGARH' THEN '04'
        WHEN 'CHHATTISGARH' THEN '22'
        WHEN 'DADRA AND NAGAR HAVELI' THEN '26'
        WHEN 'DAMAN AND DIU' THEN '25'
        WHEN 'DELHI' THEN '07'
        WHEN 'GOA' THEN '30'
        WHEN 'GUJARAT' THEN '24'
        WHEN 'HARYANA' THEN '06'
        WHEN 'HIMACHAL PRADESH' THEN '02'
        WHEN 'JAMMU AND KASHMIR' THEN '01'
        WHEN 'JHARKHAND' THEN '20'
        WHEN 'KARNATAKA' THEN '29'
        WHEN 'KERALA' THEN '32'
        WHEN 'LADAKH' THEN '38'
        WHEN 'LAKSHADWEEP' THEN '31'
        WHEN 'MADHYA PRADESH' THEN '23'
        WHEN 'MAHARASHTRA' THEN '27'  -- Most common in your data
        WHEN 'MANIPUR' THEN '14'
        WHEN 'MEGHALAYA' THEN '17'
        WHEN 'MIZORAM' THEN '15'
        WHEN 'NAGALAND' THEN '13'
        WHEN 'ODISHA' THEN '21'
        WHEN 'PUDUCHERRY' THEN '34'
        WHEN 'PUNJAB' THEN '03'
        WHEN 'RAJASTHAN' THEN '08'
        WHEN 'SIKKIM' THEN '11'
        WHEN 'TAMIL NADU' THEN '33'
        WHEN 'TELANGANA' THEN '36'
        WHEN 'TRIPURA' THEN '16'
        WHEN 'UTTAR PRADESH' THEN '09'
        WHEN 'UTTARAKHAND' THEN '05'
        WHEN 'WEST BENGAL' THEN '19'
        ELSE ''
    END
    
    RETURN @StateCode
END
GO

-- Example usage:
-- SELECT dbo.GetStateCode('Maharashtra')  -- Returns '27'
-- SELECT dbo.GetStateCode('Delhi')        -- Returns '07'


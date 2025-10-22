-- Create stored procedures for test database
USE ErpBE_Test;
GO

-- SP_GetUserById
CREATE PROCEDURE [dbo].[SP_GetUserById]
    @UserId INT
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    SELECT 
        UM_CODE AS UserId,
        UM_USERNAME AS Username,
        UM_NAME AS Name,
        UM_EMAIL AS Email,
        UM_CM_ID AS CompanyId,
        -2147483641 AS FinancialYearCode, -- Default value since UM_LEVEL is string
        CASE WHEN IS_ACTIVE = 1 THEN 1 ELSE 0 END AS IsActive,
        CASE WHEN UM_IS_ADMIN = 1 THEN 1 ELSE 0 END AS IsAdmin,
        UM_LASTLOGIN_DATETIME AS LastLoginDateTime,
        UM_IP_ADDRESS AS IpAddress
    FROM USER_MASTER
    WHERE UM_CODE = @UserId
      AND (ES_DELETE = 0 OR ES_DELETE IS NULL);
END
GO

-- SP_GetUserByUsername
CREATE PROCEDURE [dbo].[SP_GetUserByUsername]
    @Username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    SELECT 
        UM_CODE AS UserId,
        UM_USERNAME AS Username,
        UM_NAME AS Name,
        UM_EMAIL AS Email,
        UM_CM_ID AS CompanyId,
        -2147483641 AS FinancialYearCode, -- Default value since UM_LEVEL is string
        CASE WHEN IS_ACTIVE = 1 THEN 1 ELSE 0 END AS IsActive,
        CASE WHEN UM_IS_ADMIN = 1 THEN 1 ELSE 0 END AS IsAdmin,
        UM_LASTLOGIN_DATETIME AS LastLoginDateTime,
        UM_IP_ADDRESS AS IpAddress
    FROM USER_MASTER
    WHERE UM_USERNAME = @Username
      AND (ES_DELETE = 0 OR ES_DELETE IS NULL);
END
GO

-- SP_CreateUser
CREATE PROCEDURE [dbo].[SP_CreateUser]
    @Username NVARCHAR(50),
    @Password NVARCHAR(255),
    @Name NVARCHAR(100),
    @Email NVARCHAR(100) = NULL,
    @CompanyId INT,
    @FinancialYearCode INT,
    @IsActive BIT = 1,
    @IsAdmin BIT = 0
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    INSERT INTO USER_MASTER (UM_USERNAME, UM_PASSWORD, UM_NAME, UM_EMAIL, UM_CM_ID, UM_LEVEL, IS_ACTIVE, UM_IS_ADMIN)
    VALUES (@Username, @Password, @Name, @Email, @CompanyId, 'User', @IsActive, @IsAdmin);

    SELECT SCOPE_IDENTITY() AS UserId;
END
GO

-- SP_UpdateUser
CREATE PROCEDURE [dbo].[SP_UpdateUser]
    @UserId INT,
    @Name NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    UPDATE USER_MASTER 
    SET UM_NAME = ISNULL(@Name, UM_NAME),
        UM_EMAIL = ISNULL(@Email, UM_EMAIL),
        IS_ACTIVE = ISNULL(@IsActive, IS_ACTIVE)
    WHERE UM_CODE = @UserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP_GetUserRoles
CREATE PROCEDURE [dbo].[SP_GetUserRoles]
    @UserName NVARCHAR(50),
    @CompanyId NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    SELECT r.RoleName
    FROM USER_MASTER um
    JOIN UserRoles ur ON um.UM_CODE = ur.UserId
    JOIN ROLES r ON ur.RoleId = r.RoleId
    WHERE um.UM_USERNAME = @UserName 
      AND um.UM_CM_ID = CAST(@CompanyId AS INT)
      AND ur.IsActive = 1
      AND r.IsActive = 1;
END
GO

-- SP_GetUnitMasterById
CREATE PROCEDURE [dbo].[SP_GetUnitMasterById]
    @UnitId INT
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    SELECT
        I_UOM_CODE AS Id,
        I_UOM_CM_COMP_ID AS CompanyId,
        I_UOM_NAME AS UnitName,
        I_UOM_DESC AS UnitDescription,
        CASE WHEN ES_DELETE = 0 THEN 1 ELSE 0 END AS IsActive,
        ES_DELETE AS IsDeleted,
        MODIFY AS IsModified,
        ISNULL(AT.CREATED_DATE, GETDATE()) AS CreatedDate,
        AT.MODIFIED_DATE AS ModifiedDate,
        ISNULL(AT.CREATED_BY, 'System') AS CreatedBy,
        AT.MODIFIED_BY AS ModifiedBy
    FROM ITEM_UNIT_MASTER UOM
    LEFT JOIN AUDIT_TRAIL AT ON AT.TABLE_NAME = 'ITEM_UNIT_MASTER' AND AT.RECORD_ID = UOM.I_UOM_CODE
    WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0;
END
GO

-- SP_CreateUnitMaster
CREATE PROCEDURE [dbo].[SP_CreateUnitMaster]
    @UnitName VARCHAR(10),
    @UnitDescription VARCHAR(100) = NULL,
    @CompanyId INT,
    @IsActive BIT = 1,
    @UnitId INT OUTPUT
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    -- Check if unit name already exists for this company
    IF EXISTS (SELECT 1 FROM ITEM_UNIT_MASTER WHERE I_UOM_NAME = @UnitName AND I_UOM_CM_COMP_ID = @CompanyId AND ES_DELETE = 0)
    BEGIN
        RAISERROR('Unit with this name already exists for this company', 16, 1);
        RETURN;
    END
    
    -- Insert new unit
    INSERT INTO ITEM_UNIT_MASTER (
        I_UOM_CM_COMP_ID,
        I_UOM_NAME,
        I_UOM_DESC,
        ES_DELETE,
        MODIFY
    )
    VALUES (
        @CompanyId,
        @UnitName,
        @UnitDescription,
        0, -- ES_DELETE
        0  -- MODIFY
    );
    
    SET @UnitId = SCOPE_IDENTITY();
    SELECT @UnitId AS UnitId;
END
GO

-- SP_UpdateUnitMaster
CREATE PROCEDURE [dbo].[SP_UpdateUnitMaster]
    @UnitId INT,
    @UnitName VARCHAR(10),
    @UnitDescription VARCHAR(100) = NULL,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT OFF; -- Changed for testing
    
    -- Check if unit exists
    IF NOT EXISTS (SELECT 1 FROM ITEM_UNIT_MASTER WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0)
    BEGIN
        RAISERROR('Unit not found', 16, 1);
        RETURN;
    END
    
    -- Check if unit name already exists for another unit in the same company
    IF EXISTS (
        SELECT 1 FROM ITEM_UNIT_MASTER 
        WHERE I_UOM_NAME = @UnitName 
        AND I_UOM_CM_COMP_ID = (SELECT I_UOM_CM_COMP_ID FROM ITEM_UNIT_MASTER WHERE I_UOM_CODE = @UnitId)
        AND I_UOM_CODE != @UnitId 
        AND ES_DELETE = 0
    )
    BEGIN
        RAISERROR('Unit with this name already exists for this company', 16, 1);
        RETURN;
    END
    
    -- Update unit
    UPDATE ITEM_UNIT_MASTER
    SET
        I_UOM_NAME = @UnitName,
        I_UOM_DESC = @UnitDescription,
        MODIFY = 1
    WHERE I_UOM_CODE = @UnitId AND ES_DELETE = 0;
    
    SELECT @@ROWCOUNT;
END
GO

PRINT 'Stored procedures created successfully!';

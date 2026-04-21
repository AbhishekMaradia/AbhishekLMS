-- =============================================
-- Author:      Abhishek
-- Create date: 2026-02-03
-- Description: Inserts or Updates a Module or Permission based on its Code.
--              If Code exists, it updates the Name. If not, it creates a new record.
-- =============================================
CREATE PROCEDURE [dbo].[sp_UpsertEntityName]
    @Type NVARCHAR(20),      -- 'Module' or 'Permission'
    @Code NVARCHAR(100),     -- The unique Code (e.g., 'COURSE_VIEW')
    @NewName NVARCHAR(100),  -- The display Name
    @TenantId INT = NULL     -- Optional TenantId (default NULL/Global)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- =========================================================================================
        -- 1. Handle MODULES
        -- =========================================================================================
        IF @Type = 'Module'
        BEGIN
            IF EXISTS (SELECT 1 FROM Modules WHERE Code = @Code)
            BEGIN
                -- UPDATE existing Module
                UPDATE Modules
                SET Name = @NewName,
                    UpdatedAt = GETDATE()
                WHERE Code = @Code;
                
                PRINT 'Module Updated: ' + @Code + ' -> ' + @NewName;
            END
            ELSE
            BEGIN
                -- INSERT new Module
                INSERT INTO Modules (Code, Name, IsActive, TenantId, CreatedAt)
                VALUES (@Code, @NewName, 1, @TenantId, GETDATE());
                
                PRINT 'Module Created: ' + @Code + ' (' + @NewName + ')';
            END
        END

        -- =========================================================================================
        -- 2. Handle PERMISSIONS
        -- =========================================================================================
        ELSE IF @Type = 'Permission'
        BEGIN
             IF EXISTS (SELECT 1 FROM Permissions WHERE Code = @Code)
            BEGIN
                -- UPDATE existing Permission
                UPDATE Permissions
                SET Name = @NewName,
                    UpdatedAt = GETDATE()
                WHERE Code = @Code;
                
                PRINT 'Permission Updated: ' + @Code + ' -> ' + @NewName;
            END
            ELSE
            BEGIN
                -- INSERT new Permission
                INSERT INTO Permissions (Code, Name, IsActive, TenantId, CreatedAt)
                VALUES (@Code, @NewName, 1, @TenantId, GETDATE());
                
                PRINT 'Permission Created: ' + @Code + ' (' + @NewName + ')';
            END
        END

        -- =========================================================================================
        -- 3. Handle Invalid Type
        -- =========================================================================================
        ELSE
        BEGIN
            PRINT 'Invalid Type. Please specify "Module" or "Permission".';
        END

    END TRY
    BEGIN CATCH
        PRINT 'Error: ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- Example Usage:
-- EXEC sp_UpsertEntityName 'Module', 'NEW_MODULE', 'My New Module', NULL;
-- EXEC sp_UpsertEntityName 'Permission', 'COURSE_VIEW', 'Can View Courses', NULL;





















BEGIN TRANSACTION;





ALTER TABLE LTS.Users ADD CNIC NVARCHAR(13) NULL;
ALTER TABLE LTS.Users ADD IsProfileCompleted BIT NOT NULL DEFAULT 1;
ALTER TABLE LTS.Users ADD MembershipStatus NVARCHAR(20) NOT NULL DEFAULT 'Active';
ALTER TABLE LTS.Users ADD MembershipBlockedReason NVARCHAR(500) NULL;
ALTER TABLE LTS.Users ADD MembershipBlockedAtUtc DATETIME2 NULL;
ALTER TABLE LTS.Users ADD MembershipBlockedByUserID INT NULL;
ALTER TABLE LTS.Users ADD MembershipRemovedReason NVARCHAR(500) NULL;
ALTER TABLE LTS.Users ADD MembershipRemovedAtUtc DATETIME2 NULL;
ALTER TABLE LTS.Users ADD MembershipRemovedByUserID INT NULL;
ALTER TABLE LTS.Users ADD LastFirmID INT NULL;
ALTER TABLE LTS.Users ADD IsAvailable BIT NOT NULL DEFAULT 1;
ALTER TABLE LTS.Users ADD InactiveReason NVARCHAR(500) NULL;
ALTER TABLE LTS.Users ADD InactiveFromUtc DATETIME2 NULL;
ALTER TABLE LTS.Users ADD InactiveUntilUtc DATETIME2 NULL;
GO




CREATE UNIQUE INDEX IX_Users_Phone ON LTS.Users(Phone) WHERE Phone IS NOT NULL AND IsDeleted = 0;
CREATE UNIQUE INDEX IX_Users_CNIC ON LTS.Users(CNIC) WHERE CNIC IS NOT NULL AND IsDeleted = 0;
GO






ALTER TABLE LTS.FirmAdminRequests ALTER COLUMN FirmName NVARCHAR(150) NULL;
ALTER TABLE LTS.FirmAdminRequests ALTER COLUMN FirmCode NVARCHAR(30) NULL;
ALTER TABLE LTS.FirmAdminRequests ALTER COLUMN AdminFullName NVARCHAR(150) NULL;
GO





ALTER TABLE LTS.UserJoinRequests ADD UserID INT NULL;
ALTER TABLE LTS.UserJoinRequests ALTER COLUMN FullName NVARCHAR(150) NULL;
ALTER TABLE LTS.UserJoinRequests ALTER COLUMN Email NVARCHAR(150) NULL;
ALTER TABLE LTS.UserJoinRequests ALTER COLUMN PasswordHash NVARCHAR(255) NULL;
GO

ALTER TABLE LTS.UserJoinRequests
    ADD CONSTRAINT FK_UserJoinRequests_User FOREIGN KEY (UserID) REFERENCES LTS.Users(UserID);
CREATE INDEX IX_UserJoinRequests_UserID ON LTS.UserJoinRequests(UserID);
GO





CREATE TABLE LTS.FirmMembershipEvents
(
    EventID             BIGINT IDENTITY(1,1) PRIMARY KEY,
    FirmID              INT NOT NULL,
    UserID              INT NOT NULL,
    ActionType          NVARCHAR(30) NOT NULL,
    Reason              NVARCHAR(500) NULL,
    PerformedByUserID   INT NULL,
    PerformedAtUtc      DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_FirmMembershipEvents_Firm FOREIGN KEY (FirmID) REFERENCES LTS.Firms(FirmID),
    CONSTRAINT FK_FirmMembershipEvents_User FOREIGN KEY (UserID) REFERENCES LTS.Users(UserID)
);
CREATE INDEX IX_FirmMembershipEvents_UserID_FirmID ON LTS.FirmMembershipEvents(UserID, FirmID);
GO










INSERT INTO LTS.Permissions (PermissionID, PermissionName, Description) VALUES
    (209, 'BlockFirmUser', 'Block a firm user'),
    (210, 'UnblockFirmUser', 'Unblock a previously blocked firm user'),
    (211, 'RemoveFirmUser', 'Remove a user from the firm'),
    (212, 'ChangeFirmUserRole', 'Change a firm user''s role'),
    (213, 'ManageFirmAdminAvailability', 'Set own Active/Inactive availability (Firm Admin)'),
    (214, 'ViewFirmAdminAvailability', 'View Firm Admin''s availability status');
GO


INSERT INTO LTS.RolePermissions (RoleID, PermissionID)
SELECT r.RoleID, p.PermissionID
FROM LTS.Roles r
CROSS JOIN LTS.Permissions p
WHERE r.RoleName = 'FirmAdmin' AND p.PermissionID IN (209, 210, 211, 212, 213, 214);


INSERT INTO LTS.RolePermissions (RoleID, PermissionID)
SELECT r.RoleID, 214
FROM LTS.Roles r
WHERE r.RoleName IN ('Partner', 'AssociateLawyer', 'Moharrir', 'InternParalegal');
GO

COMMIT TRANSACTION;
GO



IF NOT EXISTS (SELECT 1 FROM LTS.NotificationTypes WHERE NotificationTypeID = 9)
BEGIN
    INSERT INTO LTS.NotificationTypes (NotificationTypeID, TypeName, Description, IsEmail, IsSMS, IsInApp, IsActive)
    VALUES (9, 'FirmMembershipChange', 'Sent to a firm user when they are blocked, unblocked, or removed from their firm', 0, 0, 1, 1);
END
GO

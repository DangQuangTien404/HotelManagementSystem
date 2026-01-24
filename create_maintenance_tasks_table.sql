-- Script to create MaintenanceTasks table
-- Run this script directly in SQL Server Management Studio or Azure Data Studio

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaintenanceTasks]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MaintenanceTasks] (
        [Id] int NOT NULL IDENTITY(1,1),
        [RoomId] int NOT NULL,
        [AssignedTo] int NULL,
        [Priority] nvarchar(max) NOT NULL,
        [Deadline] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        [ApprovedBy] int NULL,
        CONSTRAINT [PK_MaintenanceTasks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MaintenanceTasks_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MaintenanceTasks_Users_ApprovedBy] FOREIGN KEY ([ApprovedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MaintenanceTasks_Users_AssignedTo] FOREIGN KEY ([AssignedTo]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_MaintenanceTasks_ApprovedBy] ON [dbo].[MaintenanceTasks] ([ApprovedBy]);
    CREATE INDEX [IX_MaintenanceTasks_AssignedTo] ON [dbo].[MaintenanceTasks] ([AssignedTo]);
    CREATE INDEX [IX_MaintenanceTasks_RoomId] ON [dbo].[MaintenanceTasks] ([RoomId]);

    -- Mark migration as applied
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260124110225_AddMaintenanceTask')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES ('20260124110225_AddMaintenanceTask', '8.0.0');
    END
END
GO


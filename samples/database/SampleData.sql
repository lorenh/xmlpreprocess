/*
Script created by SQL Data Compare version 7.1.0.230 from Red Gate Software Ltd at 6/5/2012 1:43:14 PM

Run this script on (local).ConfigSettings

Note that this script will carry out all DELETE commands for all tables first, then all the UPDATES and then all the INSERTS
It will disable foreign key constraints at the beginning of the script, and re-enable them at the end
*/
SET NUMERIC_ROUNDABORT OFF
GO
SET XACT_ABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
GO
SET DATEFORMAT YMD
GO
-- Pointer used for text / image updates. This might not be needed, but is declared here just in case
DECLARE @pv binary(16)

BEGIN TRANSACTION

-- Drop constraints from [dbo].[ConfigSettingValues]
ALTER TABLE [dbo].[ConfigSettingValues] DROP CONSTRAINT [FK_ConfigSettingValues_ConfigEnvironments]
ALTER TABLE [dbo].[ConfigSettingValues] DROP CONSTRAINT [FK_ConfigSettingValues_ConfigSettingNames]

-- Add 6 rows to [dbo].[ConfigEnvironments]
SET IDENTITY_INSERT [dbo].[ConfigEnvironments] ON
INSERT INTO [dbo].[ConfigEnvironments] ([EnvironmentID], [EnvironmentName], [ViewOrder], [IsDefault]) VALUES (1, N'Default', 0, 1)
INSERT INTO [dbo].[ConfigEnvironments] ([EnvironmentID], [EnvironmentName], [ViewOrder], [IsDefault]) VALUES (2, N'Local', 10, 0)
INSERT INTO [dbo].[ConfigEnvironments] ([EnvironmentID], [EnvironmentName], [ViewOrder], [IsDefault]) VALUES (3, N'Development', 20, 0)
INSERT INTO [dbo].[ConfigEnvironments] ([EnvironmentID], [EnvironmentName], [ViewOrder], [IsDefault]) VALUES (4, N'Test', 30, 0)
INSERT INTO [dbo].[ConfigEnvironments] ([EnvironmentID], [EnvironmentName], [ViewOrder], [IsDefault]) VALUES (5, N'Integration', 40, 0)
INSERT INTO [dbo].[ConfigEnvironments] ([EnvironmentID], [EnvironmentName], [ViewOrder], [IsDefault]) VALUES (6, N'Production', 50, 0)
SET IDENTITY_INSERT [dbo].[ConfigEnvironments] OFF

-- Add 5 rows to [dbo].[ConfigSettingNames]
SET IDENTITY_INSERT [dbo].[ConfigSettingNames] ON
INSERT INTO [dbo].[ConfigSettingNames] ([SettingID], [SettingName], [ViewOrder], [SettingType], [Comment]) VALUES (1, N'Setting1', 10, NULL, NULL)
INSERT INTO [dbo].[ConfigSettingNames] ([SettingID], [SettingName], [ViewOrder], [SettingType], [Comment]) VALUES (2, N'Setting2', 20, NULL, NULL)
INSERT INTO [dbo].[ConfigSettingNames] ([SettingID], [SettingName], [ViewOrder], [SettingType], [Comment]) VALUES (3, N'Setting3', 30, NULL, NULL)
INSERT INTO [dbo].[ConfigSettingNames] ([SettingID], [SettingName], [ViewOrder], [SettingType], [Comment]) VALUES (4, N'Setting4', 40, NULL, NULL)
INSERT INTO [dbo].[ConfigSettingNames] ([SettingID], [SettingName], [ViewOrder], [SettingType], [Comment]) VALUES (5, N'Setting5', 50, NULL, NULL)
SET IDENTITY_INSERT [dbo].[ConfigSettingNames] OFF

-- Add 8 rows to [dbo].[ConfigSettingValues]
INSERT INTO [dbo].[ConfigSettingValues] ([SettingID], [EnvironmentID], [SettingValue]) VALUES (1, 1, N'setting1default')
INSERT INTO [dbo].[ConfigSettingValues] ([SettingID], [EnvironmentID], [SettingValue]) VALUES (1, 3, N'Setting1_3')
INSERT INTO [dbo].[ConfigSettingValues] ([SettingID], [EnvironmentID], [SettingValue]) VALUES (1, 4, N'Setting1_4')
INSERT INTO [dbo].[ConfigSettingValues] ([SettingID], [EnvironmentID], [SettingValue]) VALUES (1, 5, N'Setting1_5')
INSERT INTO [dbo].[ConfigSettingValues] ([SettingID], [EnvironmentID], [SettingValue]) VALUES (1, 6, N'Setting1_6')
INSERT INTO [dbo].[ConfigSettingValues] ([SettingID], [EnvironmentID], [SettingValue]) VALUES (2, 1, N'Setting2_1')
INSERT INTO [dbo].[ConfigSettingValues] ([SettingID], [EnvironmentID], [SettingValue]) VALUES (2, 2, N'Setting2_2')
INSERT INTO [dbo].[ConfigSettingValues] ([SettingID], [EnvironmentID], [SettingValue]) VALUES (2, 3, N'Setting2_3')

-- Add constraints to [dbo].[ConfigSettingValues]
ALTER TABLE [dbo].[ConfigSettingValues] ADD CONSTRAINT [FK_ConfigSettingValues_ConfigEnvironments] FOREIGN KEY ([EnvironmentID]) REFERENCES [dbo].[ConfigEnvironments] ([EnvironmentID])
ALTER TABLE [dbo].[ConfigSettingValues] ADD CONSTRAINT [FK_ConfigSettingValues_ConfigSettingNames] FOREIGN KEY ([SettingID]) REFERENCES [dbo].[ConfigSettingNames] ([SettingID])

COMMIT TRANSACTION
GO

-- Reseed identity on [dbo].[ConfigSettingNames]
DBCC CHECKIDENT('[dbo].[ConfigSettingNames]', RESEED, 5)
GO

-- Reseed identity on [dbo].[ConfigEnvironments]
DBCC CHECKIDENT('[dbo].[ConfigEnvironments]', RESEED, 6)
GO
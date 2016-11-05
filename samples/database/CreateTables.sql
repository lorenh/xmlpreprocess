/****** Object:  Table [dbo].[ConfigSettingNames]    Script Date: 06/04/2012 18:12:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConfigSettingNames](
	[SettingID] [int] IDENTITY(1,1) NOT NULL,
	[SettingName] [nvarchar](255) NOT NULL,
	[ViewOrder] [int] NULL,
	[SettingType] [int] NULL,
	[Comment] [nvarchar](max) NULL,
 CONSTRAINT [PK_ConfigSettingNames] PRIMARY KEY CLUSTERED 
(
	[SettingID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ConfigEnvironments]    Script Date: 06/04/2012 18:12:01 ******/
CREATE TABLE [dbo].[ConfigEnvironments](
	[EnvironmentID] [int] IDENTITY(1,1) NOT NULL,
	[EnvironmentName] [nvarchar](255) NOT NULL,
	[ViewOrder] [int] NULL,
	[IsDefault] [bit] NULL,
 CONSTRAINT [PK_ConfigEnvironments] PRIMARY KEY CLUSTERED 
(
	[EnvironmentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ConfigSettingValues]    Script Date: 06/04/2012 18:12:01 ******/
CREATE TABLE [dbo].[ConfigSettingValues](
	[SettingID] [int] NOT NULL,
	[EnvironmentID] [int] NOT NULL,
	[SettingValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_ConfigSettingValues] PRIMARY KEY CLUSTERED 
(
	[SettingID] ASC,
	[EnvironmentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  ForeignKey [FK_ConfigSettingValues_ConfigEnvironments]    Script Date: 06/04/2012 18:12:01 ******/
ALTER TABLE [dbo].[ConfigSettingValues]  WITH CHECK ADD  CONSTRAINT [FK_ConfigSettingValues_ConfigEnvironments] FOREIGN KEY([EnvironmentID])
REFERENCES [dbo].[ConfigEnvironments] ([EnvironmentID])
GO
ALTER TABLE [dbo].[ConfigSettingValues] CHECK CONSTRAINT [FK_ConfigSettingValues_ConfigEnvironments]
GO

/****** Object:  ForeignKey [FK_ConfigSettingValues_ConfigSettingNames]    Script Date: 06/04/2012 18:12:01 ******/
ALTER TABLE [dbo].[ConfigSettingValues]  WITH CHECK ADD  CONSTRAINT [FK_ConfigSettingValues_ConfigSettingNames] FOREIGN KEY([SettingID])
REFERENCES [dbo].[ConfigSettingNames] ([SettingID])
GO
ALTER TABLE [dbo].[ConfigSettingValues] CHECK CONSTRAINT [FK_ConfigSettingValues_ConfigSettingNames]
GO

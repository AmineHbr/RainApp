IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'RainDb')
BEGIN
    CREATE DATABASE RainDb;
   
END
GO

USE [RainDb]
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ExecutionLogs')
BEGIN
    CREATE TABLE [dbo].[ExecutionLogs](
        [Id] [uniqueidentifier] NOT NULL,
        [ExecutionDate] [datetime2](7) NULL,
        [Type] [nvarchar](20) NOT NULL,
        [Params] [nvarchar](max) NOT NULL,
        [Status] [nvarchar](20) NOT NULL,
        CONSTRAINT [PK_ExecutionLogs] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
Go

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StockData')
BEGIN
    CREATE TABLE [dbo].[StockData](
        [SerieDate] [datetime] NOT NULL,
        [Symbole] [varchar](25) NULL,
        [TimeZone] [varchar](50) NULL,
        [Open] [decimal](25, 2) NULL,
        [Close] [decimal](25, 2) NULL,
        [High] [decimal](25, 2) NULL,
        [Low] [decimal](25, 2) NULL,
        [Volume] [decimal](25, 2) NULL,
        PRIMARY KEY CLUSTERED 
        (
            [SerieDate] ASC
        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
Go
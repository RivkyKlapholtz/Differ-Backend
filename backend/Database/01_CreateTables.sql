-- Create Jobs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Jobs')
BEGIN
    CREATE TABLE Jobs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        StartTime DATETIME2 NOT NULL,
        EndTime DATETIME2 NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Running',
        TotalRequestsProcessed INT NOT NULL DEFAULT 0,
        DiffsFound INT NOT NULL DEFAULT 0,
        ErrorMessage NVARCHAR(MAX) NULL
    );
END
GO

-- Create Diffs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Diffs')
BEGIN
    CREATE TABLE Diffs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        JobId INT NOT NULL,
        SourceRequest NVARCHAR(MAX) NOT NULL,
        TargetRequest NVARCHAR(MAX) NOT NULL,
        NormalizedSourceResponse NVARCHAR(MAX) NOT NULL,
        NormalizedTargetResponse NVARCHAR(MAX) NOT NULL,
        SourceCompleteResponse NVARCHAR(MAX) NOT NULL,
        TargetCompleteResponse NVARCHAR(MAX) NOT NULL,
        DiffType NVARCHAR(100) NOT NULL,
        Endpoint NVARCHAR(500) NOT NULL,
        Method NVARCHAR(10) NOT NULL,
        Timestamp DATETIME2 NOT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsChecked BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Diffs_Jobs FOREIGN KEY (JobId) REFERENCES Jobs(Id)
    );
END
GO

-- Create indexes
CREATE INDEX IX_Diffs_JobId ON Diffs(JobId);
CREATE INDEX IX_Diffs_IsDeleted ON Diffs(IsDeleted);
CREATE INDEX IX_Diffs_Timestamp ON Diffs(Timestamp DESC);
CREATE INDEX IX_Diffs_DiffType ON Diffs(DiffType);
GO

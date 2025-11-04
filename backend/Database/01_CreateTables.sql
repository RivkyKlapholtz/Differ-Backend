-- Create Jobs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Jobs')
BEGIN
    CREATE TABLE Jobs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        StartTime DATETIME2 NOT NULL,
        EndTime DATETIME2 NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Running',
        TotalDiffs INT NOT NULL DEFAULT 0,
        FailedDiffs INT NOT NULL DEFAULT 0,
        SucceededDiffs INT NOT NULL DEFAULT 0
    );
END
GO

-- Create Diffs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Diffs')
BEGIN
    CREATE TABLE Diffs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        JobId INT NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Endpoint NVARCHAR(500) NOT NULL,
        Method NVARCHAR(10) NOT NULL,
        ProductionResponse NVARCHAR(MAX) NOT NULL,
        IntegrationResponse NVARCHAR(MAX) NOT NULL,
        ProductionCurl NVARCHAR(MAX) NOT NULL,
        IntegrationCurl NVARCHAR(MAX) NOT NULL,
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
GO

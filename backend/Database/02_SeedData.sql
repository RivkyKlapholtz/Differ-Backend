-- Seed sample job
INSERT INTO Jobs (Name, StartTime, EndTime, Status, TotalDiffs, FailedDiffs, SucceededDiffs)
VALUES 
    ('Initial API Comparison', GETUTCDATE(), GETUTCDATE(), 'Completed', 5, 3, 2);

DECLARE @JobId INT = SCOPE_IDENTITY();

-- Seed sample diffs
INSERT INTO Diffs (JobId, Category, Endpoint, Method, ProductionResponse, IntegrationResponse, ProductionCurl, IntegrationCurl, Timestamp, IsDeleted, IsChecked)
VALUES 
    (@JobId, 'JSON Response', '/api/users', 'GET', 
     '{"users": [{"id": 1, "name": "John"}]}', 
     '{"users": [{"id": 1, "name": "Jane"}]}',
     'curl -X GET ''https://api.production.com/api/users''',
     'curl -X GET ''https://api.integration.com/api/users''',
     GETUTCDATE(), 0, 0),
    
    (@JobId, 'Status Code', '/api/products', 'GET',
     '200', '404',
     'curl -X GET ''https://api.production.com/api/products''',
     'curl -X GET ''https://api.integration.com/api/products''',
     GETUTCDATE(), 0, 0),
    
    (@JobId, 'Headers', '/api/orders', 'GET',
     'Content-Type: application/json', 'Content-Type: text/plain',
     'curl -X GET ''https://api.production.com/api/orders''',
     'curl -X GET ''https://api.integration.com/api/orders''',
     GETUTCDATE(), 0, 0);
GO

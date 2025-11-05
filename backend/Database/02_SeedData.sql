-- Seed sample jobs (Hangfire executions)
INSERT INTO Jobs (Name, StartTime, EndTime, Status, TotalRequestsProcessed, DiffsFound)
VALUES 
    ('API Comparison - User Service', DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(HOUR, -2, GETUTCDATE()), 'Completed', 15, 3),
    ('API Comparison - Order Service', DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(HOUR, -1, GETUTCDATE()), 'Completed', 8, 2),
    ('API Comparison - Product Service', DATEADD(MINUTE, -30, GETUTCDATE()), DATEADD(MINUTE, -30, GETUTCDATE()), 'Completed', 12, 0);

DECLARE @JobId1 INT = (SELECT Id FROM Jobs WHERE Name = 'API Comparison - User Service');
DECLARE @JobId2 INT = (SELECT Id FROM Jobs WHERE Name = 'API Comparison - Order Service');

-- Seed realistic diffs with complete request/response data
INSERT INTO Diffs (
    JobId, 
    Category, 
    Endpoint, 
    Method, 
    SourceRequest,
    TargetRequest,
    NormalizedSourceResponse,
    NormalizedTargetResponse,
    SourceCompleteResponse,
    TargetCompleteResponse,
    Timestamp, 
    IsDeleted, 
    IsChecked
)
VALUES 
    -- JSON Response Diff: Different user data
    (@JobId1, 'JSON Response', '/api/users/123', 'GET',
     '{"headers":{"Authorization":"Bearer token123"},"body":null}',
     '{"headers":{"Authorization":"Bearer token123"},"body":null}',
     '{"id":123,"name":"John Doe","email":"john@example.com","role":"admin","lastLogin":"2025-01-05T10:30:00Z"}',
     '{"id":123,"name":"John Doe","email":"john@example.com","role":"user","lastLogin":"2025-01-05T10:30:00Z"}',
     '{"statusCode":200,"headers":{"Content-Type":"application/json"},"body":{"id":123,"name":"John Doe","email":"john@example.com","role":"admin","lastLogin":"2025-01-05T10:30:00Z"}}',
     '{"statusCode":200,"headers":{"Content-Type":"application/json"},"body":{"id":123,"name":"John Doe","email":"john@example.com","role":"user","lastLogin":"2025-01-05T10:30:00Z"}}',
     DATEADD(HOUR, -2, GETUTCDATE()), 0, 0),
    
    -- Status Code Diff: 200 vs 404
    (@JobId1, 'Status Code', '/api/products/999', 'GET',
     '{"headers":{"Authorization":"Bearer token123"},"body":null}',
     '{"headers":{"Authorization":"Bearer token123"},"body":null}',
     '{"products":[{"id":999,"name":"Legacy Product","price":99.99}]}',
     '{"error":"Product not found","code":"PRODUCT_NOT_FOUND"}',
     '{"statusCode":200,"headers":{"Content-Type":"application/json"},"body":{"products":[{"id":999,"name":"Legacy Product","price":99.99}]}}',
     '{"statusCode":404,"headers":{"Content-Type":"application/json"},"body":{"error":"Product not found","code":"PRODUCT_NOT_FOUND"}}',
     DATEADD(HOUR, -2, GETUTCDATE()), 0, 0),
    
    -- JSON Response Diff: Missing field in target
    (@JobId1, 'JSON Response', '/api/orders/456', 'GET',
     '{"headers":{"Authorization":"Bearer token123"},"body":null}',
     '{"headers":{"Authorization":"Bearer token123"},"body":null}',
     '{"orderId":456,"customerId":789,"items":[{"productId":1,"quantity":2}],"total":199.98,"discount":20.00,"finalAmount":179.98}',
     '{"orderId":456,"customerId":789,"items":[{"productId":1,"quantity":2}],"total":199.98,"finalAmount":179.98}',
     '{"statusCode":200,"headers":{"Content-Type":"application/json"},"body":{"orderId":456,"customerId":789,"items":[{"productId":1,"quantity":2}],"total":199.98,"discount":20.00,"finalAmount":179.98}}',
     '{"statusCode":200,"headers":{"Content-Type":"application/json"},"body":{"orderId":456,"customerId":789,"items":[{"productId":1,"quantity":2}],"total":199.98,"finalAmount":179.98}}',
     DATEADD(HOUR, -2, GETUTCDATE()), 0, 1),
    
    -- JSON Response Diff: Array length mismatch
    (@JobId2, 'JSON Response', '/api/categories', 'GET',
     '{"headers":{"Authorization":"Bearer token123"},"body":null}',
     '{"headers":{"Authorization":"Bearer token123"},"body":null}',
     '{"categories":[{"id":1,"name":"Electronics"},{"id":2,"name":"Books"},{"id":3,"name":"Clothing"}]}',
     '{"categories":[{"id":1,"name":"Electronics"},{"id":2,"name":"Books"}]}',
     '{"statusCode":200,"headers":{"Content-Type":"application/json"},"body":{"categories":[{"id":1,"name":"Electronics"},{"id":2,"name":"Books"},{"id":3,"name":"Clothing"}]}}',
     '{"statusCode":200,"headers":{"Content-Type":"application/json"},"body":{"categories":[{"id":1,"name":"Electronics"},{"id":2,"name":"Books"}]}}',
     DATEADD(HOUR, -1, GETUTCDATE()), 0, 0),
    
    -- Status Code Diff: 200 vs 500
    (@JobId2, 'Status Code', '/api/payments/process', 'POST',
     '{"headers":{"Authorization":"Bearer token123","Content-Type":"application/json"},"body":{"amount":150.00,"currency":"USD","cardToken":"tok_123"}}',
     '{"headers":{"Authorization":"Bearer token123","Content-Type":"application/json"},"body":{"amount":150.00,"currency":"USD","cardToken":"tok_123"}}',
     '{"transactionId":"txn_789","status":"success","amount":150.00}',
     '{"error":"Internal server error","message":"Payment gateway timeout"}',
     '{"statusCode":200,"headers":{"Content-Type":"application/json"},"body":{"transactionId":"txn_789","status":"success","amount":150.00}}',
     '{"statusCode":500,"headers":{"Content-Type":"application/json"},"body":{"error":"Internal server error","message":"Payment gateway timeout"}}',
     DATEADD(HOUR, -1, GETUTCDATE()), 0, 0);

GO

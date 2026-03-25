# Function Lookup Fix

## Problem

Test case executions were failing with error: `Function with ID '6979dcc5e97f05829eb72c72' not found`

The validation endpoint showed the function ID exists, but execution couldn't find it.

## Root Cause

MongoDB ObjectId comparison was not working correctly. The `FunctionDefinition.Id` property is marked with `[BsonRepresentation(BsonType.ObjectId)]`, but string comparison in LINQ queries wasn't matching ObjectIds properly.

## Solution

Enhanced `FunctionDefinitionRepository.GetByIdAsync` to handle ObjectId lookups:

### Before
```csharp
public async Task<FunctionDefinition?> GetByIdAsync(string id)
{
    return await _functionDefinitions.Find(x => x.Id == id).FirstOrDefaultAsync();
}
```

### After
```csharp
public async Task<FunctionDefinition?> GetByIdAsync(string id)
{
    try
    {
        // Try direct string comparison first
        var result = await _functionDefinitions.Find(x => x.Id == id).FirstOrDefaultAsync();
        
        if (result == null)
        {
            // If not found, try converting to ObjectId and using filter builder
            if (ObjectId.TryParse(id, out var objectId))
            {
                var filter = Builders<FunctionDefinition>.Filter.Eq("_id", objectId);
                result = await _functionDefinitions.Find(filter).FirstOrDefaultAsync();
            }
        }
        
        if (result == null)
        {
            // Log all available IDs for debugging
            var allFunctions = await _functionDefinitions.Find(_ => true)
                .Project(f => new { f.Id, f.FunctionId, f.Name })
                .ToListAsync();
            Console.WriteLine($"[FunctionRepository] Function with Id '{id}' not found.");
            Console.WriteLine("Available functions:");
            foreach (var func in allFunctions)
            {
                Console.WriteLine($"  - Id: {func.Id}, FunctionId: {func.FunctionId}, Name: {func.Name}");
            }
        }
        else
        {
            Console.WriteLine($"[FunctionRepository] Successfully found: {result.Name} (Id: {result.Id})");
        }
        
        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[FunctionRepository] Error in GetByIdAsync: {ex.Message}");
        throw;
    }
}
```

## Changes Made

1. **Added MongoDB.Bson import** in `FunctionDefinitionRepository.cs`
2. **Two-tier lookup strategy**:
   - First: Try direct string comparison (for backward compatibility)
   - Second: Parse to ObjectId and use filter builder (proper MongoDB way)
3. **Enhanced logging**:
   - Success message when function found
   - Detailed list of all available functions when lookup fails
   - Error handling with exception logging
4. **Created ValidationController** with endpoints:
   - `GET /api/validation/testcases` - Validates all test cases for invalid function IDs
   - `GET /api/validation/functions` - Lists all function MongoDB IDs

## Verification

### 1. Validate Test Cases
```bash
curl http://localhost:5277/api/validation/testcases
```

Expected response:
```json
{
  "totalTestCases": 1,
  "invalidTestCasesCount": 0,
  "validFunctionIds": [
    "6979dcc5e97f05829eb72c6e",
    "6979dcc5e97f05829eb72c6f",
    "6979dcc5e97f05829eb72c72",
    ...
  ],
  "invalidTestCases": []
}
```

### 2. Get Function IDs
```bash
curl http://localhost:5277/api/validation/functions
```

Expected response:
```json
[
  {
    "mongoId": "6979dcc5e97f05829eb72c72",
    "functionId": "query-instrument-id",
    "name": "Query Instrument ID",
    "type": "SCPI"
  },
  ...
]
```

### 3. Execute Test Case
```bash
curl -X POST http://localhost:5277/api/testexecution/697b12d78954fe958b5ccd53
```

Check console output for:
- `[FunctionRepository] Successfully found: Query Instrument ID (Id: 6979dcc5e97f05829eb72c72)`
- `[TestCaseExecutor] Looking up function with ID: 6979dcc5e97f05829eb72c72`
- `[TestCaseExecutor] Found function: Query Instrument ID (FunctionId: query-instrument-id)`

## Technical Details

### Why This Works

MongoDB stores `_id` as ObjectId(Binary), not as string. When querying:
- **Direct LINQ** (`x.Id == id`): May not work due to type mismatch
- **Filter Builder**: Explicitly tells MongoDB to match ObjectId field with ObjectId value

### FunctionDefinitionId Mapping

- **JavaScript**: `operation.FunctionDefinitionId = functionData.Id`  (MongoDB document _id)
- **C# Model**: `FunctionDefinition.Id` property annotated with `[BsonId]` and `[BsonRepresentation(BsonType.ObjectId)]`
- **Database**: `_id` field stores ObjectId("6979dcc5e97f05829eb72c72")
- **Execution**: Lookup uses this ObjectId to find the function

## Key Files Modified

1. **Repositories/FunctionDefinitionRepository.cs** - Enhanced GetByIdAsync with ObjectId handling
2. **Controllers/ValidationController.cs** - NEW - Validation endpoints
3. **Controllers/FunctionsController.cs** - Added `/api/functions/ids` endpoint
4. **Executors/TestCaseExecutor.cs** - Added detailed logging around function lookup

## Next Steps

If you still see "Function with ID 'X' not found" errors:

1. **Check console logs** - Look for the diagnostic output showing available IDs
2. **Run validation** - `GET /api/validation/testcases` to see which test cases have invalid IDs
3. **Delete old test cases** - If they have IDs from before database reseed
4. **Create new test cases** - Using the function selection modal to get current IDs

## Prevention

To avoid this issue in the future:

1. Don't manually edit function IDs in test cases
2. Use the function selection modal which correctly maps MongoDB `Id` property
3. After reseeding database, delete and recreate test cases
4. Run validation endpoint before executing test cases

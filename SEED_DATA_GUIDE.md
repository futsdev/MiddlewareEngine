# Seed Data Usage Guide

## Overview
The MiddlewareEngine includes 11 pre-configured dummy functions that demonstrate all three execution types.

## Seed Data Includes

### REST API Functions (3)
1. **weather-by-city** - Fetch weather data from OpenWeatherMap API
2. **get-user-info** - Get random user data from JSONPlaceholder
3. **create-post** - Create a post via JSONPlaceholder API

### SCPI Command Functions (3)
4. **measure-voltage-ch1** - Measure voltage from oscilloscope
5. **set-frequency** - Set signal generator frequency
6. **query-instrument-id** - Query instrument identification

### SDK Method Functions (5)
7. **math-max** - Calculate maximum of two numbers (System.Math.Max)
8. **math-min** - Calculate minimum of two numbers (System.Math.Min)
9. **math-round** - Round a number to specified digits (System.Math.Round)
10. **string-concat** - Concatenate two strings (System.String.Concat)
11. **datetime-now** - Get current date and time (System.DateTime.Now)

## How to Use Seed Data

### Option 1: Manual Seeding via API

1. **Start the application**:
   ```bash
   dotnet run
   ```

2. **Seed the database** (only seeds if database is empty):
   ```bash
   POST https://localhost:5001/api/seed
   ```

3. **Check function count**:
   ```bash
   GET https://localhost:5001/api/seed/count
   ```

### Option 2: Using the .http file

Open `MiddlewareEngine.http` in VS Code and click "Send Request" on:
```http
POST {{baseUrl}}/api/seed
```

## Testing Seeded Functions

### Test SDK Functions (Work immediately, no external dependencies)

**Calculate Maximum:**
```http
POST {{baseUrl}}/api/execute
Content-Type: application/json

{
  "function_id": "math-max",
  "parameters": {
    "val1": 45.5,
    "val2": 33.2
  }
}
```

**Round Number:**
```http
POST {{baseUrl}}/api/execute
Content-Type: application/json

{
  "function_id": "math-round",
  "parameters": {
    "d": 3.14159,
    "digits": 2
  }
}
```

**Concatenate Strings:**
```http
POST {{baseUrl}}/api/execute
Content-Type: application/json

{
  "function_id": "string-concat",
  "parameters": {
    "str0": "Hello, ",
    "str1": "World!"
  }
}
```

**Get Current DateTime (no parameters):**
```http
POST {{baseUrl}}/api/execute
Content-Type: application/json

{
  "function_id": "datetime-now"
}
```

### Test REST API Functions

**Get Random Users (no API key required):**
```http
POST {{baseUrl}}/api/execute
Content-Type: application/json

{
  "function_id": "get-user-info"
}
```

**Get Weather (requires OpenWeatherMap API key):**
```http
POST {{baseUrl}}/api/execute
Content-Type: application/json

{
  "function_id": "weather-by-city",
  "parameters": {
    "q": "London",
    "appid": "your_api_key_here",
    "units": "metric"
  }
}
```

### Test SCPI Functions

SCPI functions require actual instruments connected. The implementation includes simulation mode for testing.

## Expected Responses

### Successful SDK Execution:
```json
{
  "success": true,
  "result": 45.5,
  "errorMessage": null,
  "executedAt": "2026-01-28T...",
  "functionId": "math-max"
}
```

### Successful REST API Execution:
```json
{
  "success": true,
  "result": "[{\"id\":1,\"name\":\"Leanne Graham\",...}]",
  "errorMessage": null,
  "executedAt": "2026-01-28T...",
  "functionId": "get-user-info"
}
```

### Error Response:
```json
{
  "success": false,
  "result": null,
  "errorMessage": "Function with ID 'invalid-id' not found",
  "executedAt": "2026-01-28T...",
  "functionId": "invalid-id"
}
```

## Modifying Seed Data

Edit `Data/SeedData.json` to add/modify functions. The file uses standard JSON format matching the FunctionDefinition model.

After modifying:
1. Delete existing functions from MongoDB
2. Restart the application
3. Call the seed endpoint again

## Automatic Seeding on Startup

The seeder checks if functions exist before seeding, so it won't duplicate data. To force reseed:
1. Delete all functions via API or MongoDB
2. Call the seed endpoint

## Notes

- Seed data is loaded from `Data/SeedData.json`
- Seeding only occurs if the database is empty
- SDK functions using System.Math work immediately without setup
- REST API functions require internet connectivity
- SCPI functions require connected instruments (or use simulation mode)

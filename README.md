# MiddlewareEngine

A dynamic function execution engine built with ASP.NET Core 9.0 and MongoDB that allows you to define, store, and execute functions mapped to REST APIs, SCPI commands, SDK methods, SSH commands, or any custom execution type. **Fully extensible** - add new execution types without modifying code!

## 🎯 Features

- **Dynamic Execution Types**: No hardcoded types - ExecutionType is a string, not an enum
- **Built-in Support**: REST API, SCPI Commands, SDK Methods, SSH (extensible to any type)
- **Custom Types**: Add your own execution types through the UI or API
- **DLL Upload**: Upload custom .NET assemblies and call methods via reflection
- **Beautiful UI**: Razor Pages interface with gradient theme and responsive design
- **Full CRUD**: Create, Read, Update, Delete function definitions
- **Parameter Support**: Any parameter types (string, int, double, bool, custom)
- **Assembly Management**: Browse system assemblies, upload custom DLLs
- **Seed Data**: 11 sample functions to get started quickly
- **API Documentation**: Swagger UI with complete API reference
- **MongoDB Backend**: Flexible document storage with dynamic schema

## 🚀 Quick Start

### Prerequisites

- .NET 9.0 SDK or later
- MongoDB 4.0 or later

### 1. Start MongoDB

```bash
mongod
```

### 2. Run the Application

```bash
dotnet build
dotnet run
```

### 3. Access the Application

- **UI**: https://localhost:5001
- **Swagger**: https://localhost:5001/swagger  
- **API**: https://localhost:5001/api

### 4. Load Sample Data

Click the **"Seed Data"** button on the home page, or:

```bash
curl -X POST https://localhost:5001/api/seed
```

The application will start on `https://localhost:5001` (or the port configured in launchSettings.json).

### 4. Access Swagger UI

Open your browser and navigate to:
```
https://localhost:5001/swagger
```

## API Endpoints

### Function Management

- `GET /api/functions` - Get all function definitions
- `GET /api/functions/active` - Get all active function definitions
- `GET /api/functions/{id}` - Get function by MongoDB ID
- `GET /api/functions/by-function-id/{functionId}` - Get function by function_id
- `POST /api/functions` - Create a new function definition
- `PUT /api/functions/{id}` - Update an existing function definition
- `DELETE /api/functions/{id}` - Delete a function definition

### Function Execution

- `POST /api/execute` - Execute a function by function_id
- `POST /api/execute/{functionId}` - Execute a function with parameters in the body

## Function Definition Schema

```json
{
  "function_id": "unique-function-id",
  "name": "Function Name",
  "description": "Function description",
  "execution_type": "RestApi | ScpiCommand | SdkMethod",
  "execution_config": {
    // Configuration specific to execution type
  },
  "parameters": [
    {
      "name": "param1",
      "type": "string",
      "required": true,
      "description": "Parameter description"
    }
  ],
  "is_active": true
}
```

## Examples

### 1. Create REST API Function

```json
POST /api/functions
{
  "function_id": "get-weather",
  "name": "Get Weather Data",
  "description": "Fetches weather data from external API",
  "execution_type": "RestApi",
  "execution_config": {
    "url": "https://api.weather.com/data",
    "http_method": "GET",
    "headers": {
      "Authorization": "Bearer YOUR_TOKEN"
    },
    "timeout": 30
  },
  "parameters": [
    {
      "name": "city",
      "type": "string",
      "required": true,
      "description": "City name"
    }
  ],
  "is_active": true
}
```

### 2. Create SCPI Command Function

```json
POST /api/functions
{
  "function_id": "measure-voltage",
  "name": "Measure Voltage",
  "description": "Measures voltage from oscilloscope",
  "execution_type": "ScpiCommand",
  "execution_config": {
    "scpi_command": "MEAS:VOLT? {channel}",
    "connection_string": "TCPIP::192.168.1.100::INSTR"
  },
  "parameters": [
    {
      "name": "channel",
      "type": "string",
      "required": true,
      "description": "Channel number (e.g., CH1)"
    }
  ],
  "is_active": true
}
```

### 3. Create SDK Method Function

```json
POST /api/functions
{
  "function_id": "calculate-sum",
  "name": "Calculate Sum",
  "description": "Calculates sum of two numbers",
  "execution_type": "SdkMethod",
  "execution_config": {
    "assembly_name": "MyMathLibrary",
    "class_name": "MyMathLibrary.Calculator",
    "method_name": "Add"
  },
  "parameters": [
    {
      "name": "a",
      "type": "int",
      "required": true
    },
    {
      "name": "b",
      "type": "int",
      "required": true
    }
  ],
  "is_active": true
}
```

### 4. Execute a Function

```json
POST /api/execute
{
  "function_id": "get-weather",
  "parameters": {
    "city": "New York"
  }
}
```

**Response:**
```json
{
  "success": true,
  "result": "{ ... weather data ... }",
  "errorMessage": null,
  "executedAt": "2026-01-28T10:30:00Z",
  "functionId": "get-weather"
}
```

## Project Structure

```
MiddlewareEngine/
├── Configuration/
│   └── MongoDbSettings.cs          # MongoDB configuration
├── Controllers/
│   ├── ExecuteController.cs        # Function execution endpoints
│   └── FunctionsController.cs      # Function management endpoints
├── Executors/
│   ├── ExecutionEngine.cs          # Main execution orchestrator
│   ├── IFunctionExecutor.cs        # Executor interface
│   ├── RestApiExecutor.cs          # REST API execution
│   ├── ScpiExecutor.cs             # SCPI command execution
│   └── SdkMethodExecutor.cs        # SDK method execution
├── Models/
│   ├── FunctionDefinition.cs       # Function definition model
│   └── FunctionExecution.cs        # Execution request/response models
├── Repositories/
│   └── FunctionDefinitionRepository.cs # MongoDB data access
├── Services/
│   └── FunctionDefinitionService.cs    # Business logic layer
├── appsettings.json                # Application configuration
└── Program.cs                      # Application entry point
```

## Configuration Options

### MongoDB Settings

- `ConnectionString`: MongoDB connection string
- `DatabaseName`: Name of the database
- `FunctionDefinitionsCollectionName`: Name of the collection for function definitions

### REST API Executor

- `url`: Target API URL
- `http_method`: HTTP method (GET, POST, PUT, DELETE)
- `headers`: Custom HTTP headers (optional)
- `timeout`: Request timeout in seconds (optional)

### SCPI Executor

- `scpi_command`: SCPI command with parameter placeholders (e.g., `MEAS:VOLT? {channel}`)
- `connection_string`: Instrument connection string

### SDK Method Executor

- `assembly_name`: Name of the .NET assembly
- `class_name`: Fully qualified class name
- `method_name`: Method name to invoke

## Error Handling

All API responses follow a consistent format:

**Success Response:**
```json
{
  "success": true,
  "result": { ... },
  "errorMessage": null,
  "executedAt": "2026-01-28T10:30:00Z",
  "functionId": "function-id"
}
```

**Error Response:**
```json
{
  "success": false,
  "result": null,
  "errorMessage": "Error details",
  "executedAt": "2026-01-28T10:30:00Z",
  "functionId": "function-id"
}
```

## Development

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Publishing

```bash
dotnet publish -c Release -o ./publish
```

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

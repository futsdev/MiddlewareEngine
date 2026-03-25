# MiddlewareEngine - Features Summary

## Overview
ASP.NET Core 9.0 MiddlewareEngine with MongoDB backend for dynamic function execution. The system is fully generic and extensible, supporting any execution type without code changes.

## 🎯 Core Features

### 1. **Dynamic Execution Types**
- **No Hardcoded Types**: ExecutionType is now a `string` instead of enum
- **Built-in Support**: RestApi, ScpiCommand, SdkMethod, Ssh
- **Custom Types**: Add any custom execution type through the UI
- **Extensible Configuration**: CustomFields dictionary for type-specific config

```csharp
// ExecutionType in FunctionDefinition.cs
[BsonElement("execution_type")]
[JsonPropertyName("execution_type")]
public string ExecutionType { get; set; } = "RestApi";

// CustomFields for extensibility
[BsonElement("custom_fields")]
[JsonPropertyName("custom_fields")]
public Dictionary<string, object>? CustomFields { get; set; }
```

### 2. **Assembly Management (DLL Upload)**
- Upload custom .NET assemblies via UI or API
- Browse available assemblies (system + custom)
- Reflect on classes and methods within assemblies
- Stored in `CustomAssemblies/` folder

**API Endpoints:**
- `GET /api/assemblies` - List all available assemblies
- `GET /api/assemblies/{name}/classes` - Get classes in assembly
- `GET /api/assemblies/{name}/classes/{class}/methods` - Get methods with parameters
- `POST /api/assemblies/upload` - Upload new DLL

**Service:** `IAssemblyManager` / `AssemblyManager`

### 3. **CRUD Operations**

#### Create Function
- Dynamic execution type selection (dropdown with built-in + custom)
- Type-specific configuration sections
- Parameter builder with any type support
- JSON-based custom configuration

#### Edit Function
- Full edit capability with type switching
- Dynamic config sections that show/hide based on type
- Load assemblies button for SDK methods
- Update parameters dynamically

#### Delete Function
- JavaScript fetch API to DELETE endpoint
- Confirmation dialog before deletion

#### Execute Function
- Auto-generated parameter forms based on function definition
- Support for string, int, double, bool parameters
- Real-time execution with results display

### 4. **Execution Types**

#### REST API
```json
{
  "execution_type": "RestApi",
  "execution_config": {
    "url": "https://api.example.com/endpoint",
    "http_method": "GET",
    "headers": {"Authorization": "Bearer token"},
    "timeout": 30
  }
}
```

#### SCPI Command
```json
{
  "execution_type": "ScpiCommand",
  "execution_config": {
    "scpi_command": "MEAS:VOLT? {channel}",
    "connection_string": "TCPIP::192.168.1.100::5025::SOCKET"
  }
}
```

#### SDK Method
```json
{
  "execution_type": "SdkMethod",
  "execution_config": {
    "assembly_name": "System",
    "class_name": "System.Math",
    "method_name": "Sqrt"
  }
}
```

#### SSH Command (New)
```json
{
  "execution_type": "Ssh",
  "execution_config": {
    "custom_fields": {
      "host": "192.168.1.100",
      "port": 22,
      "username": "user",
      "password": "pass",
      "command": "ls -la"
    }
  }
}
```

#### Custom Type (Your Own)
```json
{
  "execution_type": "MyCustomType",
  "execution_config": {
    "custom_fields": {
      "key1": "value1",
      "key2": "value2"
    }
  }
}
```

### 5. **Beautiful UI**
- Purple gradient theme with Bootstrap 5.3
- Responsive cards layout
- Icon-rich interface with Bootstrap Icons
- Real-time parameter forms
- Type-specific badges and colors

#### Pages:
- **Index** ([/](/)): List all functions with cards, execute/edit/delete buttons
- **Create** ([/Create](/Create)): Dynamic function creation with type selection
- **Edit** ([/Edit](/Edit)): Full edit capability with type switching
- **Execute** ([/Execute](/Execute)): Execute functions with parameter forms

### 6. **MongoDB Integration**
- Document-based storage with flexible schema
- Repository pattern for data access
- JSON serialization with snake_case support
- Seed data loader for sample functions

**Connection String:**
```json
"MongoDbSettings": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "MiddlewareEngineDB",
  "CollectionName": "FunctionDefinitions"
}
```

### 7. **API Endpoints**

#### Functions API
- `GET /api/functions` - Get all functions
- `GET /api/functions/{id}` - Get function by ID
- `POST /api/functions` - Create new function
- `PUT /api/functions/{id}` - Update function
- `DELETE /api/functions/{id}` - Delete function

#### Execution API
- `POST /api/execute/{functionId}` - Execute function by ID

#### Seed API
- `POST /api/seed` - Seed database with sample data

#### Assemblies API
- `GET /api/assemblies` - List assemblies
- `GET /api/assemblies/{name}/classes` - Get classes
- `GET /api/assemblies/{name}/classes/{class}/methods` - Get methods
- `POST /api/assemblies/upload` - Upload DLL

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- MongoDB Server
- Visual Studio 2022 or VS Code

### Running the Application

1. **Start MongoDB:**
   ```bash
   mongod
   ```

2. **Build and Run:**
   ```bash
   dotnet build
   dotnet run
   ```

3. **Access the Application:**
   - UI: https://localhost:5001
   - Swagger: https://localhost:5001/swagger
   - API: https://localhost:5001/api

4. **Seed Sample Data:**
   - Via UI: Click "Seed Data" button on Index page
   - Via API: `POST https://localhost:5001/api/seed`

## 📊 Sample Functions

The seed data includes 11 sample functions:

1. **get-weather** (REST API) - OpenWeatherMap API
2. **get-user-info** (REST API) - JSONPlaceholder users
3. **create-post** (REST API) - JSONPlaceholder POST
4. **measure-voltage** (SCPI) - Multimeter voltage measurement
5. **set-frequency** (SCPI) - Signal generator frequency
6. **math-sqrt** (SDK) - System.Math.Sqrt
7. **math-pow** (SDK) - System.Math.Pow
8. **math-max** (SDK) - System.Math.Max
9. **string-concat** (SDK) - String.Concat
10. **datetime-now** (SDK) - DateTime.Now
11. **random-number** (SDK) - Random.Next

## 🔧 Architecture

### Layers

1. **Models** - Domain entities and data transfer objects
2. **Repositories** - Data access layer (MongoDB)
3. **Services** - Business logic and orchestration
4. **Executors** - Function execution handlers
5. **Controllers** - API endpoints
6. **Pages** - Razor Pages UI

### Key Components

- **ExecutionEngine** - Routes function execution to appropriate executor
- **RestApiExecutor** - HTTP client for REST API calls
- **ScpiExecutor** - SCPI command handler (simulated)
- **SdkMethodExecutor** - Reflection-based method invocation
- **AssemblyManager** - DLL upload and assembly reflection
- **DataSeeder** - JSON-based seed data loader

## 🎨 UI Features

### Index Page
- Grid of function cards with type badges
- Quick execute button on each card
- Edit and delete actions
- Color-coded type indicators
- Search and filter capabilities

### Create Page
- Execution type dropdown (RestApi, ScpiCommand, SdkMethod, Ssh, Custom)
- Dynamic configuration sections that show/hide
- Custom type name input for extensibility
- Parameter builder with add/remove
- Load assemblies button for SDK methods

### Edit Page
- Same dynamic behavior as Create page
- Pre-populated with existing function data
- Type switching capability
- Update confirmation

### Execute Page
- Function information display
- Auto-generated parameter forms based on definition
- Support for required/optional parameters
- Real-time execution results
- JSON response viewer

## 🔐 Security Considerations

⚠️ **Important**: This is a development/demo application. For production use, consider:

1. **Authentication & Authorization** - Add user authentication
2. **Input Validation** - Validate all user inputs
3. **Assembly Upload** - Restrict allowed assemblies
4. **SQL Injection** - Use parameterized queries (already using MongoDB)
5. **CORS** - Configure CORS policies
6. **HTTPS** - Enforce HTTPS in production
7. **Rate Limiting** - Add API rate limiting
8. **Logging** - Implement comprehensive logging
9. **Error Handling** - Don't expose stack traces to users

## 📝 Extensibility Guide

### Adding a New Execution Type

1. **Choose a Type Name** (e.g., "Mqtt", "Kafka", "GraphQL")

2. **Create Executor Class:**
   ```csharp
   public class MqttExecutor : IFunctionExecutor
   {
       public async Task<object> ExecuteAsync(FunctionDefinition function, Dictionary<string, object> parameters)
       {
           // Your MQTT logic here
           var host = function.ExecutionConfig.CustomFields["host"]?.ToString();
           var topic = function.ExecutionConfig.CustomFields["topic"]?.ToString();
           // Publish to MQTT broker
           return result;
       }
   }
   ```

3. **Register in Program.cs:**
   ```csharp
   builder.Services.AddScoped<MqttExecutor>();
   ```

4. **Add Case in ExecutionEngine:**
   ```csharp
   case "Mqtt":
       var mqttExecutor = serviceProvider.GetRequiredService<MqttExecutor>();
       return await mqttExecutor.ExecuteAsync(function, parameters);
   ```

5. **Add UI Config Section** (Optional - can use Custom config):
   ```html
   <div id="mqttConfig" class="config-section">
       <input type="text" id="mqttHost" placeholder="MQTT Broker Host" />
       <input type="text" id="mqttTopic" placeholder="Topic" />
   </div>
   ```

6. **Create Function** via UI with your new type!

### No Code Changes Required

For simple custom types, you can:
1. Select "Custom" execution type in UI
2. Enter your custom type name (e.g., "MySpecialType")
3. Provide JSON configuration in CustomFields
4. Store in database
5. Implement executor later when needed

## 🐛 Troubleshooting

### MongoDB Connection Issues
- Check if MongoDB is running: `mongosh`
- Verify connection string in `appsettings.json`

### Assembly Loading Issues
- Ensure DLL is compiled for .NET 9.0 or compatible
- Check CustomAssemblies folder exists and has write permissions
- Verify assembly name is correct (case-sensitive)

### Parameter Type Conversion
- SdkMethodExecutor supports: string, int, double, bool
- Add custom converters in SdkMethodExecutor for complex types

### ReflectionTypeLoadException
- Already resolved by using Swashbuckle 6.5.0
- If occurs again, check package versions

## 📚 Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [MongoDB .NET Driver](https://mongodb.github.io/mongo-csharp-driver/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/5.3/)
- [Swagger/OpenAPI](https://swagger.io/)

## 🎉 What's Next?

Potential enhancements:
- [ ] SSH executor implementation with SSH.NET
- [ ] WebSocket support for real-time execution
- [ ] Execution history and logging
- [ ] Function versioning
- [ ] Scheduled/cron execution
- [ ] Function chaining (call one function from another)
- [ ] Authentication with JWT
- [ ] Role-based access control
- [ ] Import/export function definitions
- [ ] Docker containerization
- [ ] GraphQL API
- [ ] Webhooks for function completion
- [ ] Metrics and monitoring

---

**Created:** 2024  
**Version:** 1.0  
**Framework:** ASP.NET Core 9.0  
**Database:** MongoDB  
**License:** MIT (or your choice)

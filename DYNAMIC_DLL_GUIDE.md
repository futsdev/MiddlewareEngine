# Dynamic DLL Loading & Execution Guide

This guide demonstrates how to upload custom .NET DLLs and execute their methods through the MiddlewareEngine.

## ✅ Yes, Dynamic DLL Execution Works!

The MiddlewareEngine **fully supports**:
- ✅ Uploading custom .NET DLLs via API
- ✅ Loading assemblies at runtime
- ✅ Browsing classes and methods in uploaded DLLs
- ✅ Executing methods from custom DLLs
- ✅ Passing parameters to DLL methods
- ✅ Handling return values from DLL methods

## How It Works

### 1. AssemblyManager Service
The `IAssemblyManager` service handles:
- Loading assemblies from GAC (Global Assembly Cache)
- Loading custom uploaded DLLs from `CustomAssemblies/` folder
- Reflecting on assembly types, classes, and methods
- Providing metadata about available assemblies

### 2. SdkMethodExecutor Integration
The `SdkMethodExecutor` uses `IAssemblyManager` to:
1. Load the specified assembly (system or custom)
2. Find the target class
3. Find the target method
4. Convert parameters to correct types
5. Invoke the method (static or instance)
6. Return the result

## Step-by-Step Example

### Example 1: Create and Upload a Custom Calculator DLL

#### Step 1: Create Your Custom DLL

Create a new class library project:

```bash
# Create a new class library
dotnet new classlib -n MyCustomCalculator
cd MyCustomCalculator
```

**MyCustomCalculator.cs:**
```csharp
namespace MyCustomCalculator
{
    public class AdvancedCalculator
    {
        // Static method - no instance needed
        public static double CalculateCircleArea(double radius)
        {
            return Math.PI * radius * radius;
        }
        
        // Instance method
        public double CalculateCompoundInterest(double principal, double rate, int years)
        {
            return principal * Math.Pow(1 + rate / 100, years);
        }
        
        // Method with multiple parameters
        public static string FormatCurrency(double amount, string currencySymbol)
        {
            return $"{currencySymbol}{amount:N2}";
        }
        
        // Method returning custom object
        public static CalculationResult Calculate(double a, double b, string operation)
        {
            return operation switch
            {
                "add" => new CalculationResult { Result = a + b, Operation = operation },
                "subtract" => new CalculationResult { Result = a - b, Operation = operation },
                "multiply" => new CalculationResult { Result = a * b, Operation = operation },
                "divide" => new CalculationResult { Result = a / b, Operation = operation },
                _ => throw new ArgumentException("Invalid operation")
            };
        }
    }
    
    public class CalculationResult
    {
        public double Result { get; set; }
        public string Operation { get; set; }
    }
}
```

#### Step 2: Build Your DLL

```bash
# Build in Release mode
dotnet build -c Release

# Your DLL will be at: bin/Release/net9.0/MyCustomCalculator.dll
```

#### Step 3: Upload DLL via API

Using PowerShell:
```powershell
$dllPath = "bin\Release\net9.0\MyCustomCalculator.dll"
$uri = "https://localhost:5001/api/assemblies/upload"

$form = @{
    file = Get-Item -Path $dllPath
}

Invoke-RestMethod -Uri $uri -Method Post -Form $form -SkipCertificateCheck
```

Using curl:
```bash
curl -X POST https://localhost:5001/api/assemblies/upload \
  -F "file=@bin/Release/net9.0/MyCustomCalculator.dll" \
  -k
```

Using C#:
```csharp
using var client = new HttpClient();
using var content = new MultipartFormDataContent();
using var fileStream = File.OpenRead("MyCustomCalculator.dll");
using var fileContent = new StreamContent(fileStream);

fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
content.Add(fileContent, "file", "MyCustomCalculator.dll");

var response = await client.PostAsync(
    "https://localhost:5001/api/assemblies/upload", 
    content
);
```

#### Step 4: Verify Upload

Check available assemblies:
```bash
curl https://localhost:5001/api/assemblies -k
```

Response should include:
```json
[
  "System",
  "System.Math",
  "MyCustomCalculator",  // ← Your uploaded DLL
  ...
]
```

Browse classes in your DLL:
```bash
curl https://localhost:5001/api/assemblies/MyCustomCalculator/classes -k
```

Response:
```json
[
  "MyCustomCalculator.AdvancedCalculator",
  "MyCustomCalculator.CalculationResult"
]
```

Browse methods:
```bash
curl "https://localhost:5001/api/assemblies/MyCustomCalculator/classes/MyCustomCalculator.AdvancedCalculator/methods" -k
```

Response:
```json
[
  {
    "name": "CalculateCircleArea",
    "isStatic": true,
    "returnType": "System.Double",
    "parameters": [
      {
        "name": "radius",
        "type": "System.Double",
        "hasDefaultValue": false
      }
    ]
  },
  {
    "name": "CalculateCompoundInterest",
    "isStatic": false,
    "returnType": "System.Double",
    "parameters": [...]
  },
  ...
]
```

#### Step 5: Create Function Definition

**Via API:**
```bash
curl -X POST https://localhost:5001/api/functions \
  -H "Content-Type: application/json" \
  -d '{
    "function_id": "calculate-circle-area",
    "name": "Calculate Circle Area",
    "description": "Calculate area of a circle using custom DLL",
    "execution_type": "SdkMethod",
    "execution_config": {
      "type": "SdkMethod",
      "assembly_name": "MyCustomCalculator",
      "class_name": "MyCustomCalculator.AdvancedCalculator",
      "method_name": "CalculateCircleArea"
    },
    "parameters": [
      {
        "name": "radius",
        "type": "double",
        "required": true,
        "description": "Radius of the circle"
      }
    ],
    "is_active": true
  }' \
  -k
```

**Via UI:**
1. Go to **Create** page
2. Enter function details:
   - Function ID: `calculate-circle-area`
   - Name: `Calculate Circle Area`
3. Select **SDK Method** from execution type
4. Click **Load Available Assemblies** button
5. Enter configuration:
   - Assembly Name: `MyCustomCalculator` (will autocomplete)
   - Class Name: `MyCustomCalculator.AdvancedCalculator`
   - Method Name: `CalculateCircleArea`
6. Add parameter:
   - Name: `radius`
   - Type: `double`
   - Required: ✓
7. Click **Create Function**

#### Step 6: Execute Your Custom DLL Method

**Via API:**
```bash
curl -X POST https://localhost:5001/api/execute/calculate-circle-area \
  -H "Content-Type: application/json" \
  -d '{
    "radius": 5.0
  }' \
  -k
```

Response:
```json
{
  "success": true,
  "result": 78.53981633974483,
  "executionTime": "00:00:00.0234567"
}
```

**Via UI:**
1. Go to **Execute** page
2. Select `calculate-circle-area` function
3. Enter parameter:
   - radius: `5.0`
4. Click **Execute**
5. See result: `78.54`

## Example 2: Using Instance Methods

For non-static methods, the executor automatically creates an instance:

**Function Definition:**
```json
{
  "function_id": "compound-interest",
  "execution_type": "SdkMethod",
  "execution_config": {
    "assembly_name": "MyCustomCalculator",
    "class_name": "MyCustomCalculator.AdvancedCalculator",
    "method_name": "CalculateCompoundInterest"
  },
  "parameters": [
    {"name": "principal", "type": "double", "required": true},
    {"name": "rate", "type": "double", "required": true},
    {"name": "years", "type": "int", "required": true}
  ]
}
```

**Execution:**
```bash
curl -X POST https://localhost:5001/api/execute/compound-interest \
  -H "Content-Type: application/json" \
  -d '{"principal": 1000, "rate": 5, "years": 10}' \
  -k
```

Response:
```json
{
  "success": true,
  "result": 1628.89
}
```

## Example 3: Complex Return Types

When your method returns a custom object:

**Method:**
```csharp
public static CalculationResult Calculate(double a, double b, string operation)
{
    return new CalculationResult { Result = a + b, Operation = operation };
}
```

**Execution:**
```bash
curl -X POST https://localhost:5001/api/execute/calculate-operation \
  -H "Content-Type: application/json" \
  -d '{"a": 10, "b": 5, "operation": "add"}' \
  -k
```

Response:
```json
{
  "success": true,
  "result": {
    "result": 15.0,
    "operation": "add"
  }
}
```

## Example 4: Async Methods Support

If your DLL method is async:

```csharp
public static async Task<string> FetchDataAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}
```

The executor automatically handles async methods and awaits the result!

## Assembly Loading Strategy

The `AssemblyManager.LoadAssembly()` method tries multiple approaches:

1. **Try Assembly.Load()** - Loads from GAC (system assemblies like `System`, `System.Math`)
2. **Try LoadFrom(CustomAssemblies/)** - Loads from uploaded custom DLLs
3. **Try LoadFrom(BaseDirectory)** - Loads from application folder

This means you can use:
- ✅ System assemblies (no upload needed)
- ✅ Custom uploaded assemblies
- ✅ Assemblies in the app directory

## Supported Parameter Types

The `SdkMethodExecutor` automatically converts parameters:

- ✅ `string` - Text values
- ✅ `int`, `long`, `short`, `byte` - Integer numbers
- ✅ `double`, `float`, `decimal` - Floating-point numbers
- ✅ `bool` - Boolean (true/false)
- ✅ `DateTime` - Date and time
- ✅ `Guid` - Unique identifiers
- ✅ `Enum` - Enumeration values
- ✅ Arrays (with custom conversion)
- ✅ Nullable types (`int?`, `double?`, etc.)

For complex types, consider:
- Passing JSON strings and deserializing in your method
- Using multiple simple parameters
- Returning complex objects (automatically serialized to JSON)

## Best Practices

### 1. DLL Design
```csharp
// ✅ Good: Static methods for simple operations
public static double Calculate(double a, double b) { ... }

// ✅ Good: Instance methods with parameterless constructor
public class MyService 
{
    public MyService() { } // Default constructor
    public string Process(string input) { ... }
}

// ❌ Avoid: Constructor with required parameters (executor can't provide them)
public class MyService 
{
    public MyService(IConfiguration config) { } // Won't work!
}
```

### 2. Error Handling in DLL
```csharp
public static double SafeDivide(double a, double b)
{
    if (b == 0)
        throw new ArgumentException("Cannot divide by zero");
    return a / b;
}
```

The exception will be caught and returned in the API response:
```json
{
  "success": false,
  "errorMessage": "Cannot divide by zero"
}
```

### 3. Logging in DLL
Your DLL can't access the middleware logger, so use `Console.WriteLine`:
```csharp
public static void ProcessData(string data)
{
    Console.WriteLine($"Processing: {data}");
    // Will appear in middleware console output
}
```

### 4. Dependencies
If your DLL has dependencies, upload them too:
```bash
# Upload main DLL
curl -X POST https://localhost:5001/api/assemblies/upload -F "file=@MyLibrary.dll" -k

# Upload dependency
curl -X POST https://localhost:5001/api/assemblies/upload -F "file=@Dependency.dll" -k
```

## Troubleshooting

### Issue: "Assembly not found"
**Solution:** 
- Verify assembly name (case-sensitive)
- Check if uploaded: `GET /api/assemblies`
- Reupload if needed

### Issue: "Type not found in assembly"
**Solution:**
- Use fully qualified name: `MyNamespace.MyClass`
- Check with: `GET /api/assemblies/{name}/classes`
- Ensure class is `public`

### Issue: "Method not found"
**Solution:**
- Method must be `public`
- Check spelling (case-sensitive)
- Use: `GET /api/assemblies/{name}/classes/{class}/methods`

### Issue: "Cannot convert parameter"
**Solution:**
- Check parameter types match
- Use supported types (string, int, double, bool)
- Consider JSON strings for complex types

### Issue: "File not found" when loading DLL
**Solution:**
- Check `CustomAssemblies/` folder exists
- Verify file permissions
- Look at logs for details

## Real-World Examples

### Example: External Library Integration

Upload and use popular libraries:

```bash
# Upload Newtonsoft.Json
curl -X POST https://localhost:5001/api/assemblies/upload \
  -F "file=@Newtonsoft.Json.dll" -k

# Create function to parse JSON
{
  "function_id": "parse-json",
  "execution_type": "SdkMethod",
  "execution_config": {
    "assembly_name": "Newtonsoft.Json",
    "class_name": "Newtonsoft.Json.JsonConvert",
    "method_name": "DeserializeObject"
  },
  "parameters": [
    {"name": "value", "type": "string", "required": true}
  ]
}
```

### Example: Database Operations

```csharp
// MyDatabaseHelper.dll
public class DatabaseHelper
{
    public static async Task<List<User>> GetUsersAsync(string connectionString)
    {
        // Your database logic
    }
}
```

Upload and execute:
```bash
curl -X POST https://localhost:5001/api/execute/get-users \
  -d '{"connectionString": "Server=..."}' \
  -k
```

### Example: File Processing

```csharp
// MyFileProcessor.dll
public class FileProcessor
{
    public static string ProcessCsv(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        // Process and return results
        return JsonSerializer.Serialize(results);
    }
}
```

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                     Client Request                       │
│        POST /api/execute/my-custom-function              │
│        { "radius": 5.0 }                                 │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              ExecutionEngine                             │
│  • Validates function exists                             │
│  • Routes to SdkMethodExecutor                           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│            SdkMethodExecutor                             │
│  • Gets AssemblyManager                                  │
│  • Loads assembly (system or custom)                     │
│  • Finds class and method via reflection                 │
│  • Converts parameters                                   │
│  • Invokes method                                        │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│             AssemblyManager                              │
│  • LoadAssembly("MyCustomCalculator")                    │
│  • Tries: GAC → CustomAssemblies/ → BaseDirectory       │
│  • Returns Assembly instance                             │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│          CustomAssemblies Folder                         │
│  • MyCustomCalculator.dll      ← Your uploaded DLL      │
│  • AnotherLibrary.dll                                    │
│  • ThirdParty.dll                                        │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│           Your Custom DLL Method                         │
│  MyCustomCalculator.AdvancedCalculator                   │
│    .CalculateCircleArea(5.0)                             │
│  → Returns: 78.54                                        │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              Response to Client                          │
│  {                                                       │
│    "success": true,                                      │
│    "result": 78.53981633974483                           │
│  }                                                       │
└─────────────────────────────────────────────────────────┘
```

## Summary

**Yes, dynamic DLL loading and execution fully works!**

✅ **Upload** custom DLLs via API  
✅ **Browse** classes and methods  
✅ **Create** function definitions  
✅ **Execute** methods with parameters  
✅ **Get** results back as JSON  

The system handles:
- Assembly loading (system + custom)
- Type resolution
- Method invocation (static + instance)
- Parameter conversion
- Return value serialization
- Error handling
- Async methods

You can literally upload **any .NET DLL** and execute its public methods through the MiddlewareEngine! 🚀

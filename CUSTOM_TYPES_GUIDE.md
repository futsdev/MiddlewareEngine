# Adding Custom Execution Types

This guide shows how to add new execution types to the MiddlewareEngine without modifying existing code.

## Option 1: Use "Custom" Type (No Code Required)

For simple custom types that don't need special executor logic, you can use the generic "Custom" option:

1. **Go to Create Page** in the UI
2. **Select "Custom"** from the execution type dropdown
3. **Enter your custom type name** (e.g., "Mqtt", "Kafka", "GraphQL")
4. **Provide JSON configuration:**
   ```json
   {
     "broker": "mqtt.example.com",
     "port": 1883,
     "topic": "sensors/temperature"
   }
   ```
5. **Save the function** - it will be stored with your custom type name

The configuration will be available in `CustomFields` dictionary when executing.

## Option 2: Implement Custom Executor (With Code)

For advanced functionality with custom execution logic:

### Step 1: Create Executor Interface

All executors must implement `IFunctionExecutor`:

```csharp
public interface IFunctionExecutor
{
    Task<object> ExecuteAsync(FunctionDefinition function, Dictionary<string, object> parameters);
}
```

### Step 2: Create Your Executor Class

Create a new file `Executors/YourTypeExecutor.cs`:

```csharp
using MiddlewareEngine.Models;

namespace MiddlewareEngine.Executors
{
    public class MqttExecutor : IFunctionExecutor
    {
        private readonly ILogger<MqttExecutor> _logger;
        
        public MqttExecutor(ILogger<MqttExecutor> logger)
        {
            _logger = logger;
        }
        
        public async Task<object> ExecuteAsync(
            FunctionDefinition function, 
            Dictionary<string, object> parameters)
        {
            try
            {
                // Get configuration from CustomFields
                var config = function.ExecutionConfig.CustomFields;
                var broker = config["broker"]?.ToString();
                var port = Convert.ToInt32(config["port"]);
                var topic = config["topic"]?.ToString();
                
                // Your MQTT logic here
                _logger.LogInformation($"Publishing to MQTT: {broker}:{port}/{topic}");
                
                // Example: Connect to MQTT broker and publish
                // var factory = new MqttFactory();
                // var client = factory.CreateMqttClient();
                // await client.ConnectAsync(...);
                // await client.PublishAsync(...);
                
                return new 
                {
                    success = true,
                    message = $"Published to {topic}",
                    timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT execution failed");
                throw new InvalidOperationException($"Failed to execute MQTT: {ex.Message}", ex);
            }
        }
    }
}
```

### Step 3: Register Your Executor

In `Program.cs`, add your executor to the DI container:

```csharp
// Add executors
builder.Services.AddScoped<RestApiExecutor>();
builder.Services.AddScoped<ScpiExecutor>();
builder.Services.AddScoped<SdkMethodExecutor>();
builder.Services.AddScoped<MqttExecutor>();  // ← Add your executor
builder.Services.AddScoped<IExecutionEngine, ExecutionEngine>();
```

### Step 4: Add Case in ExecutionEngine

In `Executors/ExecutionEngine.cs`, add your case:

```csharp
public async Task<object> ExecuteAsync(string functionId, Dictionary<string, object> parameters)
{
    // ... existing code ...
    
    return function.ExecutionType switch
    {
        "RestApi" => await ExecuteRestApi(function, parameters),
        "ScpiCommand" => await ExecuteScpi(function, parameters),
        "SdkMethod" => await ExecuteSdkMethod(function, parameters),
        "Mqtt" => await ExecuteMqtt(function, parameters),  // ← Add your case
        _ => throw new InvalidOperationException($"Unsupported execution type: {function.ExecutionType}")
    };
}

// Add your method
private async Task<object> ExecuteMqtt(FunctionDefinition function, Dictionary<string, object> parameters)
{
    var executor = _serviceProvider.GetRequiredService<MqttExecutor>();
    return await executor.ExecuteAsync(function, parameters);
}
```

### Step 5: Add UI Config Section (Optional)

If you want a dedicated UI section, edit `Pages/Create.cshtml` and `Pages/Edit.cshtml`:

```html
<!-- In the execution type dropdown -->
<select class="form-select" id="executionTypeSelect">
    <option value="">-- Select Type --</option>
    <option value="RestApi">REST API</option>
    <option value="ScpiCommand">SCPI Command</option>
    <option value="SdkMethod">SDK Method</option>
    <option value="Ssh">SSH Command</option>
    <option value="Mqtt">MQTT Publish</option>  <!-- Add your type -->
    <option value="Custom">Custom</option>
</select>

<!-- Add config section -->
<div id="mqttConfig" class="col-md-12 config-section" style="display: none;">
    <div class="card">
        <div class="card-body">
            <h5 class="card-title">
                <i class="bi bi-wifi me-2"></i>
                MQTT Configuration
            </h5>
            
            <div class="mb-3">
                <label for="mqttBroker" class="form-label">MQTT Broker</label>
                <input type="text" class="form-control" id="mqttBroker" placeholder="mqtt.example.com">
            </div>
            
            <div class="mb-3">
                <label for="mqttPort" class="form-label">Port</label>
                <input type="number" class="form-control" id="mqttPort" value="1883">
            </div>
            
            <div class="mb-3">
                <label for="mqttTopic" class="form-label">Topic</label>
                <input type="text" class="form-control" id="mqttTopic" placeholder="sensors/temperature">
            </div>
        </div>
    </div>
</div>
```

Update the JavaScript section mapping:

```javascript
const configMap = {
    'RestApi': 'restApiConfig',
    'ScpiCommand': 'scpiConfig',
    'SdkMethod': 'sdkConfig',
    'Ssh': 'sshConfig',
    'Mqtt': 'mqttConfig',  // ← Add your mapping
    'Custom': 'customConfig'
};
```

And handle the config in form submission:

```javascript
else if (executionType === 'Mqtt') {
    executionConfig.custom_fields = {
        broker: document.getElementById('mqttBroker').value,
        port: parseInt(document.getElementById('mqttPort').value),
        topic: document.getElementById('mqttTopic').value
    };
}
```

### Step 6: Test Your Implementation

1. **Build and run:**
   ```bash
   dotnet build
   dotnet run
   ```

2. **Create a function** with your new type via UI

3. **Execute it** from the Execute page

## Real-World Examples

### SSH Executor

```csharp
public class SshExecutor : IFunctionExecutor
{
    public async Task<object> ExecuteAsync(FunctionDefinition function, Dictionary<string, object> parameters)
    {
        var config = function.ExecutionConfig.CustomFields;
        var host = config["host"]?.ToString();
        var port = Convert.ToInt32(config["port"] ?? 22);
        var username = config["username"]?.ToString();
        var password = config["password"]?.ToString();
        var command = config["command"]?.ToString();
        
        // Use SSH.NET or similar library
        // using var client = new SshClient(host, port, username, password);
        // client.Connect();
        // var result = client.RunCommand(command);
        // client.Disconnect();
        
        return new { output = "Command executed", exitCode = 0 };
    }
}
```

### GraphQL Executor

```csharp
public class GraphQlExecutor : IFunctionExecutor
{
    private readonly HttpClient _httpClient;
    
    public GraphQlExecutor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<object> ExecuteAsync(FunctionDefinition function, Dictionary<string, object> parameters)
    {
        var config = function.ExecutionConfig.CustomFields;
        var endpoint = config["endpoint"]?.ToString();
        var query = config["query"]?.ToString();
        
        var request = new 
        {
            query = query,
            variables = parameters
        };
        
        var response = await _httpClient.PostAsJsonAsync(endpoint, request);
        return await response.Content.ReadFromJsonAsync<object>();
    }
}
```

### Kafka Producer Executor

```csharp
public class KafkaExecutor : IFunctionExecutor
{
    public async Task<object> ExecuteAsync(FunctionDefinition function, Dictionary<string, object> parameters)
    {
        var config = function.ExecutionConfig.CustomFields;
        var bootstrapServers = config["bootstrap_servers"]?.ToString();
        var topic = config["topic"]?.ToString();
        
        // Use Confluent.Kafka
        // var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };
        // using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        // var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = ... });
        
        return new { topic = topic, success = true };
    }
}
```

## Package Requirements

Add NuGet packages as needed for your executor:

```bash
# SSH
dotnet add package SSH.NET

# MQTT
dotnet add package MQTTnet

# Kafka
dotnet add package Confluent.Kafka

# gRPC
dotnet add package Grpc.Net.Client

# GraphQL
dotnet add package GraphQL.Client
```

## Best Practices

1. **Error Handling**: Always wrap executor logic in try-catch
2. **Logging**: Use ILogger for diagnostics
3. **Validation**: Validate configuration in executor constructor
4. **Async**: Use async/await for all I/O operations
5. **Dispose**: Implement IDisposable if holding resources
6. **Testing**: Write unit tests for your executor
7. **Documentation**: Add XML comments to public methods

## Configuration Schema

Define a clear schema for your CustomFields:

```csharp
// Example: MQTT Configuration Schema
public class MqttConfig
{
    public string Broker { get; set; }
    public int Port { get; set; } = 1883;
    public string Topic { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int QoS { get; set; } = 0;
}

// In your executor:
var mqttConfig = JsonSerializer.Deserialize<MqttConfig>(
    JsonSerializer.Serialize(function.ExecutionConfig.CustomFields)
);
```

## Testing

Create unit tests for your executor:

```csharp
[Fact]
public async Task ExecuteAsync_ValidConfig_ReturnsSuccess()
{
    // Arrange
    var executor = new MqttExecutor(Mock.Of<ILogger<MqttExecutor>>());
    var function = new FunctionDefinition
    {
        ExecutionType = "Mqtt",
        ExecutionConfig = new ExecutionConfig
        {
            CustomFields = new Dictionary<string, object>
            {
                ["broker"] = "mqtt.example.com",
                ["port"] = 1883,
                ["topic"] = "test/topic"
            }
        }
    };
    var parameters = new Dictionary<string, object>();
    
    // Act
    var result = await executor.ExecuteAsync(function, parameters);
    
    // Assert
    Assert.NotNull(result);
}
```

## Summary

The MiddlewareEngine is designed to be fully extensible:

- **Quick & Simple**: Use "Custom" type with JSON config (no code)
- **Full Control**: Implement IFunctionExecutor for custom logic
- **UI Integration**: Add dedicated config sections (optional)
- **Type Safety**: Define configuration schemas
- **Testable**: Write unit tests for your executors

The system automatically handles:
- ✅ Parameter validation
- ✅ Error handling
- ✅ Logging
- ✅ Serialization/deserialization
- ✅ API endpoints
- ✅ UI display

You just focus on the execution logic!

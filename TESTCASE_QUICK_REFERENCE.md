# Test Case Workflow - Quick Reference

## 🎯 Access Points

### Web UI
- **Test Case Builder**: http://localhost:5000/TestCaseBuilder
- **Navigation**: Click "Test Cases" in the top menu

### API Endpoints

#### Test Case Management
```bash
# List all test cases
GET /api/testcases

# Filter by tag
GET /api/testcases?tag=smoke-test

# Filter by status
GET /api/testcases?status=Active

# Get specific test case
GET /api/testcases/{id}

# Create test case
POST /api/testcases
Content-Type: application/json
{...test case definition...}

# Update test case
PUT /api/testcases/{id}

# Delete test case
DELETE /api/testcases/{id}

# Duplicate test case
POST /api/testcases/{id}/duplicate

# Get execution history
GET /api/testcases/{id}/executions
```

#### Test Execution
```bash
# Execute test case by ID
POST /api/testexecution/{testCaseId}

# Execute inline (without saving)
POST /api/testexecution/execute
Content-Type: application/json
{...test case definition...}

# Get execution result
GET /api/testexecution/{executionId}

# Get recent executions
GET /api/testcases/executions/recent?limit=10
```

#### Seeding
```bash
# Seed sample test cases
POST /api/seed/testcases
```

## 🏗️ Test Case Structure

```json
{
  "name": "My Test Case",
  "description": "Description here",
  "tags": ["tag1", "tag2"],
  "priority": "High",
  "status": "Active",
  
  "setupOperations": [
    {
      "name": "Setup Operation 1",
      "operationType": "RestApi",
      "operationDetails": {...},
      "timeoutSeconds": 30,
      "order": 1
    }
  ],
  
  "steps": [
    {
      "name": "Step 1",
      "description": "Step description",
      "order": 1,
      "continueOnFailure": false,
      
      "actions": [
        {
          "name": "Action 1",
          "description": "Action description",
          "order": 1,
          "delayBeforeMs": 0,
          "delayAfterMs": 0,
          "continueOnFailure": false,
          
          "preConditions": [
            {
              "name": "Pre-condition check",
              "operationType": "RestApi",
              "operationDetails": {...},
              "order": 1
            }
          ],
          
          "operations": [
            {
              "name": "Main operation",
              "operationType": "Scpi",
              "operationDetails": {...},
              "expectedResult": "expected value",
              "timeoutSeconds": 30,
              "order": 1
            }
          ],
          
          "postConditions": [
            {
              "name": "Post-condition validation",
              "operationType": "SdkMethod",
              "operationDetails": {...},
              "order": 1
            }
          ]
        }
      ]
    }
  ],
  
  "teardownOperations": [
    {
      "name": "Cleanup",
      "operationType": "Scpi",
      "operationDetails": {...},
      "order": 1
    }
  ]
}
```

## 🔧 Operation Types

### REST API
```json
{
  "operationType": "RestApi",
  "operationDetails": {
    "url": "http://api.example.com/endpoint",
    "method": "POST",
    "body": "{\"key\": \"value\"}"
  }
}
```

### SCPI
```json
{
  "operationType": "Scpi",
  "operationDetails": {
    "ipAddress": "192.168.1.100",
    "port": 5025,
    "command": "*IDN?"
  }
}
```

### SDK Method
```json
{
  "operationType": "SdkMethod",
  "operationDetails": {
    "assemblyName": "CustomAssemblies/MyLib.dll",
    "className": "MyNamespace.MyClass",
    "methodName": "MyMethod"
  },
  "parameters": {
    "param1": "value1"
  }
}
```

### SSH
```json
{
  "operationType": "Ssh",
  "operationDetails": {
    "host": "server.example.com",
    "username": "admin",
    "command": "ls -la"
  }
}
```

## 🎨 Drag & Drop UI Guide

### Toolbox Items
- **📋 Test Step** - Container for actions
- **⚡ Action** - Container for operations with pre/post conditions
- **🌐 REST API** - HTTP operation
- **📡 SCPI** - Device command
- **📚 SDK Method** - .NET method invocation
- **🖥️ SSH Command** - Remote shell command

### Drop Zones
1. **Setup Operations** - Run once before all steps
2. **Test Steps** - Main test workflow
   - Within each step, you can add **Actions**
   - Within each action, you can add to:
     - **Pre-Conditions** - Validate before
     - **Operations** - Main logic
     - **Post-Conditions** - Validate after
3. **Teardown Operations** - Run once after all steps

### Workflow
1. Drag "Test Step" to Steps zone
2. Drag "Action" into the step
3. Drag operations into:
   - Pre-Conditions (optional)
   - Operations (required)
   - Post-Conditions (optional)
4. Click ✏️ to edit details
5. Click 💾 Save
6. Click ▶️ Execute

## 📊 Execution Flow

```
1. Execute Setup Operations (in order)
   ↓ (if any fails, stop)
   
2. Execute Steps (in order)
   For each Step:
     Execute Actions (in order)
       For each Action:
         - Wait DelayBeforeMs
         - Execute Pre-Conditions
         - If Pre-Conditions pass:
           - Execute Operations
         - Execute Post-Conditions
         - Wait DelayAfterMs
     (stop if action fails and continueOnFailure = false)
   (stop if step fails and continueOnFailure = false)
   
3. Execute Teardown Operations (always runs)
```

## ✅ Quick Start

### 1. Start Application
```powershell
dotnet run
```

### 2. Seed Sample Data
```powershell
curl -X POST http://localhost:5000/api/seed/testcases
```

### 3. View Sample Test Cases
```powershell
curl http://localhost:5000/api/testcases
```

### 4. Execute a Test Case
```powershell
curl -X POST http://localhost:5000/api/testexecution/{testCaseId}
```

### 5. Access UI
Open: http://localhost:5000/TestCaseBuilder

## 🎓 Best Practices

1. **Use Pre-Conditions** to verify prerequisites
2. **Use Post-Conditions** to validate results
3. **Set Timeouts** appropriately (network: 10-30s, devices: 5-15s)
4. **Add Delays** when devices need processing time
5. **Use Expected Results** for validation
6. **Tag Test Cases** for organization ("smoke", "regression")
7. **Set Continue-On-Failure** for non-critical operations
8. **Always Include Teardown** to cleanup

## 🔍 Troubleshooting

### Test Case Won't Save
- Check MongoDB connection
- Verify all required fields
- Check browser console

### Operation Fails
- Verify URLs/IPs/ports
- Check timeout settings
- Review operation details

### UI Not Responding
- Clear browser cache
- Check JavaScript console
- Refresh page

## 📚 Documentation

- **[TESTCASE_WORKFLOW_GUIDE.md](TESTCASE_WORKFLOW_GUIDE.md)** - Complete guide
- **[TESTCASE_IMPLEMENTATION.md](TESTCASE_IMPLEMENTATION.md)** - Implementation details
- **[README.md](README.md)** - Main documentation

## 💡 Examples

Sample test cases are in [Data/SeedTestCases.json](Data/SeedTestCases.json):
1. Device Configuration Test - Multi-step with all operation types
2. Simple REST API Test - Basic CRUD operations

Load them with: `POST /api/seed/testcases`

---

**Need Help?** Check the full [TESTCASE_WORKFLOW_GUIDE.md](TESTCASE_WORKFLOW_GUIDE.md) for detailed information.

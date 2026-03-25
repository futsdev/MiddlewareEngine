# Test Case Workflow Guide

## Overview
The MiddlewareEngine now supports comprehensive test case creation with a hierarchical workflow structure. You can create test cases with multiple steps, where each step contains multiple actions, and each action contains multiple operations (REST API, SCPI, SDK, SSH). Additionally, actions can have pre and post conditions.

## Test Case Hierarchy

```
TestCase
├── Setup Operations (global)
├── Steps
│   └── Actions
│       ├── Pre-Conditions (operations)
│       ├── Operations
│       └── Post-Conditions (operations)
└── Teardown Operations (global)
```

### 1. **Test Case** (Top Level)
- **Name**: Descriptive name for the test case
- **Description**: Detailed description of what the test validates
- **Tags**: Categorization (e.g., "smoke-test", "integration", "device")
- **Priority**: High, Medium, or Low
- **Status**: Draft, Active, or Deprecated
- **Setup Operations**: Run once before all steps
- **Teardown Operations**: Run once after all steps (always executed)

### 2. **Steps**
- **Name**: Step description
- **Description**: Details about the step
- **Order**: Execution sequence
- **Continue on Failure**: Whether to continue to next step if this fails
- **Actions**: List of actions to perform

### 3. **Actions**
- **Name**: Action description
- **Description**: Details about the action
- **Order**: Execution sequence within the step
- **Delay Before**: Milliseconds to wait before executing
- **Delay After**: Milliseconds to wait after executing
- **Continue on Failure**: Whether to continue to next action if this fails
- **Pre-Conditions**: Operations that must succeed before main operations
- **Operations**: Main operations to perform
- **Post-Conditions**: Validation operations after main operations

### 4. **Operations**
Operations are the actual executable tasks. Supported types:

#### REST API Operation
```json
{
  "name": "Call Health Check",
  "operationType": "RestApi",
  "operationDetails": {
    "url": "http://localhost:5000/api/health",
    "method": "GET"
  },
  "expectedResult": "healthy",
  "timeoutSeconds": 10
}
```

#### SCPI Operation
```json
{
  "name": "Query Device ID",
  "operationType": "Scpi",
  "operationDetails": {
    "ipAddress": "192.168.1.100",
    "port": 5025,
    "command": "*IDN?"
  },
  "expectedResult": "Keysight",
  "timeoutSeconds": 10
}
```

#### SDK Method Operation
```json
{
  "name": "Get Device Status",
  "operationType": "SdkMethod",
  "operationDetails": {
    "assemblyPath": "CustomAssemblies/DeviceSDK.dll",
    "typeName": "DeviceSDK.DeviceController",
    "methodName": "GetStatus"
  },
  "parameters": {
    "deviceId": "device-001"
  },
  "timeoutSeconds": 15
}
```

#### SSH Operation
```json
{
  "name": "Check System Status",
  "operationType": "Ssh",
  "operationDetails": {
    "host": "server.example.com",
    "username": "admin",
    "command": "systemctl status myservice"
  },
  "expectedResult": "active (running)",
  "timeoutSeconds": 20
}
```

## Using the Drag & Drop UI

### Access the Builder
Navigate to: `http://localhost:5000/TestCaseBuilder`

### Creating a Test Case

1. **Basic Information**
   - Fill in the test case name, description, tags, and priority at the top

2. **Add Steps**
   - Drag "Test Step" from the toolbox to the "Test Steps" zone
   - Click the edit (✏️) button to customize the step name and description

3. **Add Actions to Steps**
   - Drag "Action" from the toolbox into a specific step
   - Edit the action to set delays and properties

4. **Add Operations**
   - Drag operation types (REST API, SCPI, SDK, SSH) into:
     - Setup zone (runs before all steps)
     - Pre-Conditions section of an action
     - Operations section of an action
     - Post-Conditions section of an action
     - Teardown zone (runs after all steps)
   - Click edit to configure operation details

5. **Save & Execute**
   - Click **💾 Save** to persist the test case to MongoDB
   - Click **▶️ Execute** to run the test case immediately

### UI Features

- **Drag & Drop**: Intuitive building of test hierarchy
- **Collapsible Sections**: Click (−) to collapse/expand elements
- **Live Editing**: Edit any element in place
- **Visual Organization**: Color-coded elements (Steps: blue, Actions: purple, Operations: red)
- **Reordering**: Drag elements to reorder execution sequence

## API Endpoints

### Test Case Management

#### Get All Test Cases
```http
GET /api/testcases
GET /api/testcases?tag=smoke-test
GET /api/testcases?status=Active
```

#### Get Test Case by ID
```http
GET /api/testcases/{id}
```

#### Create Test Case
```http
POST /api/testcases
Content-Type: application/json

{
  "name": "My Test Case",
  "description": "Test description",
  "tags": ["api", "integration"],
  "priority": "High",
  "steps": [...],
  "setupOperations": [...],
  "teardownOperations": [...]
}
```

#### Update Test Case
```http
PUT /api/testcases/{id}
Content-Type: application/json
```

#### Delete Test Case
```http
DELETE /api/testcases/{id}
```

#### Duplicate Test Case
```http
POST /api/testcases/{id}/duplicate
```

#### Get Execution History
```http
GET /api/testcases/{id}/executions
```

### Test Execution

#### Execute Test Case by ID
```http
POST /api/testexecution/{testCaseId}
```

#### Execute Inline Test Case
```http
POST /api/testexecution/execute
Content-Type: application/json

{
  "name": "Inline Test",
  "steps": [...]
}
```

#### Get Execution Result
```http
GET /api/testexecution/{executionId}
```

#### Get Recent Executions
```http
GET /api/testcases/executions/recent?limit=10
```

### Seed Data

#### Seed Sample Test Cases
```http
POST /api/seed/testcases
```

## Execution Flow

1. **Setup Phase**
   - Execute all setup operations in order
   - If any setup operation fails, execution stops

2. **Steps Phase**
   - Execute steps in order
   - For each step:
     - Execute actions in order
     - For each action:
       - Wait for `delayBeforeMs`
       - Execute pre-conditions
       - If pre-conditions pass, execute main operations
       - Execute post-conditions
       - Wait for `delayAfterMs`
     - If action fails and `continueOnFailure` is false, stop step
   - If step fails and `continueOnFailure` is false, stop execution

3. **Teardown Phase**
   - Always execute teardown operations (even if test failed)
   - Execute in order

## Execution Results

The execution returns a detailed result structure:

```json
{
  "id": "execution-id",
  "testCaseId": "testcase-id",
  "testCaseName": "My Test Case",
  "success": true,
  "status": "Completed",
  "setupResults": [...],
  "stepResults": [
    {
      "stepName": "Step 1",
      "success": true,
      "actionResults": [
        {
          "actionName": "Action 1",
          "success": true,
          "preConditionResults": [...],
          "operationResults": [...],
          "postConditionResults": [...]
        }
      ]
    }
  ],
  "teardownResults": [...],
  "startedAt": "2026-01-29T10:00:00Z",
  "completedAt": "2026-01-29T10:05:30Z",
  "totalExecutionTimeMs": 330000
}
```

## Best Practices

### 1. **Organize with Pre/Post Conditions**
- Use pre-conditions to verify prerequisites
- Use post-conditions to validate operation results
- Example: Before setting frequency, verify device is on; after setting, verify frequency was applied

### 2. **Use Delays Strategically**
- Add delays when devices need time to process commands
- Use `delayBeforeMs` for device warm-up
- Use `delayAfterMs` for settling time

### 3. **Handle Failures Gracefully**
- Set `continueOnFailure: true` for non-critical operations
- Always run teardown operations to clean up
- Use setup to initialize and teardown to restore state

### 4. **Expected Results**
- Define `expectedResult` for operations that need validation
- The executor will verify the actual result contains the expected string
- Use specific strings for precise validation

### 5. **Timeouts**
- Set appropriate timeouts based on operation complexity
- Network operations: 10-30 seconds
- Device commands: 5-15 seconds
- Long-running SDK methods: 30-60 seconds

### 6. **Tagging**
- Use tags for categorization: "smoke", "regression", "nightly"
- Filter test cases by tags for selective execution
- Combine tags: ["device", "network", "critical"]

### 7. **Test Case Status**
- **Draft**: Under development
- **Active**: Ready for execution
- **Deprecated**: Obsolete but kept for reference

## Example: Complete Test Case

See `Data/SeedTestCases.json` for comprehensive examples including:
- Device configuration test with REST, SCPI, and SDK operations
- Pre and post condition validations
- Multi-step workflows with delays
- Error handling strategies

## Troubleshooting

### Test Case Won't Save
- Check MongoDB connection in appsettings.json
- Verify all required fields are filled
- Check browser console for errors

### Operations Failing
- Verify operation details (URLs, IP addresses, ports)
- Check timeouts are sufficient
- Review execution logs in the result

### Drag & Drop Not Working
- Ensure JavaScript is enabled
- Clear browser cache
- Check for console errors

### Execution Hangs
- Check for infinite loops in delays
- Verify timeout settings
- Review operation configurations

## Advanced Features

### Function Definition Integration
Operations can reference existing function definitions:
```json
{
  "name": "Use Existing Function",
  "functionDefinitionId": "existing-function-id",
  "parameters": {
    "overrideParam": "newValue"
  }
}
```

### Inline vs Referenced Operations
- **Inline**: Define operation details directly in the test case
- **Referenced**: Use `functionDefinitionId` to reference existing function definitions
- **Hybrid**: Reference a function but override parameters

### Execution History
- All executions are stored with full results
- Query execution history by test case ID
- Analyze trends and success rates

## Next Steps

1. Explore sample test cases: `POST /api/seed/testcases`
2. Build your first test case in the UI
3. Execute and review results
4. Create reusable function definitions
5. Build comprehensive test suites

For more information, see:
- [README.md](README.md) - Main documentation
- [FEATURES_SUMMARY.md](FEATURES_SUMMARY.md) - Feature overview
- [SEED_DATA_GUIDE.md](SEED_DATA_GUIDE.md) - Data seeding guide

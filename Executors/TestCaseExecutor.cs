using MiddlewareEngine.Models;
using MiddlewareEngine.Repositories;
using System.Diagnostics;

namespace MiddlewareEngine.Executors;

public class TestCaseExecutor
{
    private readonly IExecutionEngine _executionEngine;
    private readonly IFunctionDefinitionRepository _functionRepository;
    private readonly TestCaseRepository _testCaseRepository;
    private readonly ILogger<TestCaseExecutor> _logger;

    public TestCaseExecutor(
        IExecutionEngine executionEngine,
        IFunctionDefinitionRepository functionRepository,
        TestCaseRepository testCaseRepository,
        ILogger<TestCaseExecutor> logger)
    {
        _executionEngine = executionEngine;
        _functionRepository = functionRepository;
        _testCaseRepository = testCaseRepository;
        _logger = logger;
    }

    public async Task<TestCaseExecution> ExecuteTestCaseAsync(string testCaseId)
    {
        var testCase = await _testCaseRepository.GetByIdAsync(testCaseId);
        if (testCase == null)
        {
            throw new ArgumentException($"Test case not found: {testCaseId}");
        }

        return await ExecuteTestCaseAsync(testCase);
    }

    public async Task<TestCaseExecution> ExecuteTestCaseAsync(TestCase testCase)
    {
        var execution = new TestCaseExecution
        {
            TestCaseId = testCase.Id ?? string.Empty,
            TestCaseName = testCase.Name ?? "Unnamed Test Case",
            StartedAt = DateTime.UtcNow,
            Status = "Running"
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting test case execution: {TestCaseName} (Steps: {StepCount}, Setup: {SetupCount}, Teardown: {TeardownCount})", 
                testCase.Name, testCase.Steps?.Count ?? 0, testCase.SetupOperations?.Count ?? 0, testCase.TeardownOperations?.Count ?? 0);

            // Save initial execution record
            await _testCaseRepository.CreateExecutionAsync(execution);

            // Execute setup operations
            if (testCase.SetupOperations != null && testCase.SetupOperations.Count > 0)
            {
                _logger.LogInformation("Executing {Count} setup operations", testCase.SetupOperations.Count);
                execution.SetupResults = await ExecuteOperationsAsync(testCase.SetupOperations, "Setup");

                // Check if setup failed
                if (execution.SetupResults.Any(r => !r.Success))
                {
                    execution.Status = "Failed";
                    execution.Success = false;
                    execution.ErrorMessage = "Setup operations failed";
                    _logger.LogError("Setup operations failed for test case: {TestCaseName}", testCase.Name);
                    return await FinalizeExecution(execution, stopwatch);
                }
            }
            else
            {
                _logger.LogInformation("No setup operations to execute");
            }

            // Execute steps
            if (testCase.Steps != null && testCase.Steps.Count > 0)
            {
                _logger.LogInformation("Executing {Count} test steps", testCase.Steps.Count);
                foreach (var step in testCase.Steps.OrderBy(s => s.Order))
                {
                    _logger.LogInformation("Executing step: {StepName} (Actions: {ActionCount})", step.Name, step.Actions?.Count ?? 0);
                    var stepResult = await ExecuteStepAsync(step);
                    execution.StepResults.Add(stepResult);

                    if (!stepResult.Success && !step.ContinueOnFailure)
                    {
                        execution.Status = "Failed";
                        execution.Success = false;
                        execution.ErrorMessage = $"Step '{step.Name}' failed";
                        _logger.LogError("Step {StepName} failed, stopping execution", step.Name);
                        break;
                    }
                }
            }
            else
            {
                _logger.LogWarning("No steps to execute in test case: {TestCaseName}", testCase.Name);
            }

            // Execute teardown operations (always run, even on failure)
            if (testCase.TeardownOperations != null && testCase.TeardownOperations.Count > 0)
            {
                _logger.LogInformation("Executing {Count} teardown operations", testCase.TeardownOperations.Count);
                execution.TeardownResults = await ExecuteOperationsAsync(testCase.TeardownOperations, "Teardown");
            }
            else
            {
                _logger.LogInformation("No teardown operations to execute");
            }

            // Determine overall success
            if (execution.Status != "Failed")
            {
                execution.Success = execution.StepResults.All(r => r.Success);
                execution.Status = execution.Success ? "Completed" : "Failed";
            }

            _logger.LogInformation("Test case execution completed: {TestCaseName}, Status: {Status}, Steps Passed: {PassedCount}/{TotalCount}", 
                testCase.Name, execution.Status, execution.StepResults.Count(r => r.Success), execution.StepResults.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing test case: {TestCaseName}", testCase.Name);
            execution.Status = "Failed";
            execution.Success = false;
            execution.ErrorMessage = ex.Message;
        }

        return await FinalizeExecution(execution, stopwatch);
    }

    private async Task<StepResult> ExecuteStepAsync(TestStep step)
    {
        var stepResult = new StepResult
        {
            StepId = step.Id ?? string.Empty,
            StepName = step.Name ?? "Unnamed Step",
            ExecutedAt = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (step.Actions == null || step.Actions.Count == 0)
            {
                _logger.LogWarning("No actions to execute in step: {StepName}", step.Name);
                stepResult.Success = true; // Empty step is considered successful
                return stepResult;
            }

            foreach (var action in step.Actions.OrderBy(a => a.Order))
            {
                // Delay before action
                if (action.DelayBeforeMs > 0)
                {
                    _logger.LogInformation("Delaying {Delay}ms before action: {ActionName}", 
                        action.DelayBeforeMs, action.Name);
                    await Task.Delay(action.DelayBeforeMs);
                }

                _logger.LogInformation("Executing action: {ActionName} (PreConditions: {PreCount}, Operations: {OpCount}, PostConditions: {PostCount})",
                    action.Name, action.PreConditions?.Count ?? 0, action.Operations?.Count ?? 0, action.PostConditions?.Count ?? 0);
                
                var actionResult = await ExecuteActionAsync(action);
                stepResult.ActionResults.Add(actionResult);

                // Delay after action
                if (action.DelayAfterMs > 0)
                {
                    _logger.LogInformation("Delaying {Delay}ms after action: {ActionName}", 
                        action.DelayAfterMs, action.Name);
                    await Task.Delay(action.DelayAfterMs);
                }

                if (!actionResult.Success && !action.ContinueOnFailure)
                {
                    stepResult.Success = false;
                    stepResult.ErrorMessage = $"Action '{action.Name}' failed";
                    _logger.LogError("Action {ActionName} failed, stopping step execution", action.Name);
                    break;
                }
            }

            if (stepResult.ErrorMessage == null)
            {
                stepResult.Success = stepResult.ActionResults.All(r => r.Success);
            }
            
            _logger.LogInformation("Step {StepName} completed. Success: {Success}, Actions: {PassedCount}/{TotalCount}",
                step.Name, stepResult.Success, stepResult.ActionResults.Count(r => r.Success), stepResult.ActionResults.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing step: {StepName}", step.Name);
            stepResult.Success = false;
            stepResult.ErrorMessage = ex.Message;
        }

        stopwatch.Stop();
        stepResult.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
        return stepResult;
    }

    private async Task<TestActionResult> ExecuteActionAsync(TestAction action)
    {
        var actionResult = new TestActionResult
        {
            ActionId = action.Id ?? string.Empty,
            ActionName = action.Name,
            ExecutedAt = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Executing action: {ActionName}", action.Name);

            // Execute pre-conditions
            actionResult.PreConditionResults = await ExecuteOperationsAsync(
                action.PreConditions, $"PreCondition for {action.Name}");

            // Check if pre-conditions failed
            if (actionResult.PreConditionResults.Any(r => !r.Success))
            {
                actionResult.Success = false;
                actionResult.ErrorMessage = "Pre-conditions failed";
                return actionResult;
            }

            // Execute main operations
            actionResult.OperationResults = await ExecuteOperationsAsync(
                action.Operations, $"Operation for {action.Name}");

            // Execute post-conditions (even if operations failed)
            actionResult.PostConditionResults = await ExecuteOperationsAsync(
                action.PostConditions, $"PostCondition for {action.Name}");

            // Determine success
            actionResult.Success = actionResult.OperationResults.All(r => r.Success) &&
                                   actionResult.PostConditionResults.All(r => r.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action: {ActionName}", action.Name);
            actionResult.Success = false;
            actionResult.ErrorMessage = ex.Message;
        }

        stopwatch.Stop();
        actionResult.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
        return actionResult;
    }

    private async Task<List<OperationResult>> ExecuteOperationsAsync(List<Operation> operations, string context)
    {
        var results = new List<OperationResult>();

        foreach (var operation in operations.OrderBy(o => o.Order))
        {
            var result = await ExecuteOperationAsync(operation, context);
            results.Add(result);

            if (!result.Success && !operation.ContinueOnFailure)
            {
                _logger.LogWarning("Operation {OperationName} failed in {Context}, stopping further operations",
                    operation.Name, context);
                break;
            }
        }

        return results;
    }

    private async Task<OperationResult> ExecuteOperationAsync(Operation operation, string context)
    {
        var result = new OperationResult
        {
            OperationId = operation.Id ?? string.Empty,
            OperationName = operation.Name,
            ExecutedAt = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Executing operation: {OperationName} in {Context}", operation.Name, context);

            FunctionDefinition? functionDef = null;

            // Get function definition from ID or create from inline details
            if (!string.IsNullOrEmpty(operation.FunctionDefinitionId))
            {
                _logger.LogInformation("Looking up function with ID: {FunctionId}", operation.FunctionDefinitionId);
                functionDef = await _functionRepository.GetByIdAsync(operation.FunctionDefinitionId);
                if (functionDef == null)
                {
                    _logger.LogError("Function with ID '{FunctionId}' not found", operation.FunctionDefinitionId);
                    throw new InvalidOperationException(
                        $"Function with ID '{operation.FunctionDefinitionId}' not found");
                }
                _logger.LogInformation("Found function: {FunctionName} (FunctionId: {FuncId})", 
                    functionDef.Name, functionDef.FunctionId);
            }
            else if (operation.OperationDetails != null)
            {
                // Create inline function definition
                functionDef = CreateInlineFunctionDefinition(operation);
            }
            else
            {
                throw new InvalidOperationException(
                    "Operation must have either FunctionDefinitionId or OperationDetails");
            }

            // Merge operation parameters with function definition parameters
            var executionParams = new Dictionary<string, object>();
            if (functionDef.Parameters != null)
            {
                foreach (var param in functionDef.Parameters)
                {
                    executionParams[param.Name] = param.DefaultValue ?? string.Empty;
                }
            }
            if (operation.Parameters != null)
            {
                foreach (var param in operation.Parameters)
                {
                    executionParams[param.Key] = param.Value;
                }
            }

            // Execute with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.TimeoutSeconds));
            var executionTask = _executionEngine.ExecuteFunctionAsync(functionDef, executionParams);
            
            var executionResult = await executionTask.WaitAsync(cts.Token);

            result.Result = executionResult.Result;
            result.Success = executionResult.Success;
            result.ErrorMessage = executionResult.ErrorMessage;

            // Validate expected result if specified
            if (result.Success && !string.IsNullOrEmpty(operation.ExpectedResult))
            {
                var actualResult = result.Result?.ToString() ?? string.Empty;
                if (!actualResult.Contains(operation.ExpectedResult, StringComparison.OrdinalIgnoreCase))
                {
                    result.Success = false;
                    result.ErrorMessage = $"Expected result not found. Expected: '{operation.ExpectedResult}', Actual: '{actualResult}'";
                    _logger.LogWarning("Result validation failed for operation: {OperationName}", operation.Name);
                }
            }
        }
        catch (OperationCanceledException)
        {
            result.Success = false;
            result.ErrorMessage = $"Operation timed out after {operation.TimeoutSeconds} seconds";
            _logger.LogError("Operation {OperationName} timed out", operation.Name);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error executing operation: {OperationName}", operation.Name);
        }

        stopwatch.Stop();
        result.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
        return result;
    }

    private FunctionDefinition CreateInlineFunctionDefinition(Operation operation)
    {
        var functionDef = new FunctionDefinition
        {
            Name = operation.Name,
            Description = operation.Description,
            ExecutionType = operation.OperationType,
            ExecutionConfig = new ExecutionConfig(),
            Parameters = new List<FunctionParameter>()
        };

        // Convert operation parameters to function parameters
        if (operation.Parameters != null)
        {
            foreach (var param in operation.Parameters)
            {
                functionDef.Parameters.Add(new FunctionParameter
                {
                    Name = param.Key,
                    DefaultValue = param.Value
                });
            }
        }

        // Map operation details to execution config
        if (operation.OperationDetails != null)
        {
            foreach (var detail in operation.OperationDetails)
            {
                switch (detail.Key.ToLower())
                {
                    case "url":
                    case "endpoint":
                        functionDef.ExecutionConfig.Url = detail.Value?.ToString();
                        break;
                    case "method":
                    case "httpmethod":
                        functionDef.ExecutionConfig.HttpMethod = detail.Value?.ToString();
                        break;
                    case "command":
                    case "scpicommand":
                        functionDef.ExecutionConfig.ScpiCommand = detail.Value?.ToString();
                        break;
                    case "connectionstring":
                    case "connection":
                        functionDef.ExecutionConfig.ConnectionString = detail.Value?.ToString();
                        break;
                    case "ipaddress":
                    case "ip":
                        // For SCPI, we need to build connection string from IP and port
                        var ip = detail.Value?.ToString();
                        functionDef.ExecutionConfig.ConnectionString = $"TCPIP::{ip}::INSTR";
                        break;
                    case "port":
                        // Port is part of connection string for SCPI
                        if (!string.IsNullOrEmpty(functionDef.ExecutionConfig.ConnectionString))
                        {
                            functionDef.ExecutionConfig.ConnectionString = functionDef.ExecutionConfig.ConnectionString.Replace("::INSTR", $"::{detail.Value}::INSTR");
                        }
                        break;
                    case "assemblypath":
                    case "assemblyname":
                        functionDef.ExecutionConfig.AssemblyName = detail.Value?.ToString();
                        break;
                    case "typename":
                    case "classname":
                        functionDef.ExecutionConfig.ClassName = detail.Value?.ToString();
                        break;
                    case "methodname":
                        functionDef.ExecutionConfig.MethodName = detail.Value?.ToString();
                        break;
                    case "body":
                        // Add body as a parameter
                        functionDef.Parameters.Add(new FunctionParameter
                        {
                            Name = "body",
                            DefaultValue = detail.Value
                        });
                        break;
                }
            }
        }

        return functionDef;
    }

    private async Task<TestCaseExecution> FinalizeExecution(TestCaseExecution execution, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        execution.CompletedAt = DateTime.UtcNow;
        execution.TotalExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;

        // Update execution in database
        if (!string.IsNullOrEmpty(execution.Id))
        {
            await _testCaseRepository.UpdateExecutionAsync(execution.Id, execution);
        }

        return execution;
    }
}

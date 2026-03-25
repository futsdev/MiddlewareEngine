using MiddlewareEngine.Models;
using MiddlewareEngine.Repositories;

namespace MiddlewareEngine.Services;

public class TestCaseService
{
    private readonly TestCaseRepository _repository;
    private readonly ILogger<TestCaseService> _logger;

    public TestCaseService(TestCaseRepository repository, ILogger<TestCaseService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<TestCase>> GetAllTestCasesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<TestCase?> GetTestCaseByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<TestCase>> GetTestCasesByTagAsync(string tag)
    {
        return await _repository.GetByTagAsync(tag);
    }

    public async Task<List<TestCase>> GetTestCasesByStatusAsync(string status)
    {
        return await _repository.GetByStatusAsync(status);
    }

    public async Task<TestCase> CreateTestCaseAsync(TestCase testCase)
    {
        // Assign orders if not set
        AssignOrdersToHierarchy(testCase);
        
        _logger.LogInformation("Creating test case: {Name}", testCase.Name);
        return await _repository.CreateAsync(testCase);
    }

    public async Task<bool> UpdateTestCaseAsync(string id, TestCase testCase)
    {
        // Assign orders if not set
        AssignOrdersToHierarchy(testCase);
        
        _logger.LogInformation("Updating test case: {Id}", id);
        return await _repository.UpdateAsync(id, testCase);
    }

    public async Task<bool> DeleteTestCaseAsync(string id)
    {
        _logger.LogInformation("Deleting test case: {Id}", id);
        return await _repository.DeleteAsync(id);
    }

    public async Task<TestCase?> DuplicateTestCaseAsync(string id)
    {
        var original = await _repository.GetByIdAsync(id);
        if (original == null) return null;

        var duplicate = new TestCase
        {
            Name = $"{original.Name} (Copy)",
            Description = original.Description,
            Tags = new List<string>(original.Tags),
            Priority = original.Priority,
            Steps = original.Steps,
            SetupOperations = original.SetupOperations,
            TeardownOperations = original.TeardownOperations,
            Status = "Draft",
            CreatedBy = original.CreatedBy
        };

        return await _repository.CreateAsync(duplicate);
    }

    public async Task<List<TestCaseExecution>> GetExecutionHistoryAsync(string testCaseId)
    {
        return await _repository.GetExecutionsByTestCaseIdAsync(testCaseId);
    }

    public async Task<List<TestCaseExecution>> GetRecentExecutionsAsync(int limit = 10)
    {
        return await _repository.GetRecentExecutionsAsync(limit);
    }

    private void AssignOrdersToHierarchy(TestCase testCase)
    {
        // Assign order to steps
        for (int i = 0; i < testCase.Steps.Count; i++)
        {
            if (testCase.Steps[i].Order == 0)
            {
                testCase.Steps[i].Order = i + 1;
            }

            // Assign order to actions
            for (int j = 0; j < testCase.Steps[i].Actions.Count; j++)
            {
                if (testCase.Steps[i].Actions[j].Order == 0)
                {
                    testCase.Steps[i].Actions[j].Order = j + 1;
                }

                var action = testCase.Steps[i].Actions[j];
                
                // Assign order to pre-conditions
                for (int k = 0; k < action.PreConditions.Count; k++)
                {
                    if (action.PreConditions[k].Order == 0)
                    {
                        action.PreConditions[k].Order = k + 1;
                    }
                }

                // Assign order to operations
                for (int k = 0; k < action.Operations.Count; k++)
                {
                    if (action.Operations[k].Order == 0)
                    {
                        action.Operations[k].Order = k + 1;
                    }
                }

                // Assign order to post-conditions
                for (int k = 0; k < action.PostConditions.Count; k++)
                {
                    if (action.PostConditions[k].Order == 0)
                    {
                        action.PostConditions[k].Order = k + 1;
                    }
                }
            }
        }

        // Assign order to setup/teardown operations
        for (int i = 0; i < testCase.SetupOperations.Count; i++)
        {
            if (testCase.SetupOperations[i].Order == 0)
            {
                testCase.SetupOperations[i].Order = i + 1;
            }
        }

        for (int i = 0; i < testCase.TeardownOperations.Count; i++)
        {
            if (testCase.TeardownOperations[i].Order == 0)
            {
                testCase.TeardownOperations[i].Order = i + 1;
            }
        }
    }

    public async Task<List<TestCaseExecution>> GetExecutionsByTestCaseIdAsync(string testCaseId)
    {
        return await _repository.GetExecutionsByTestCaseIdAsync(testCaseId);
    }

    public async Task<TestCaseExecution?> GetExecutionByIdAsync(string executionId)
    {
        return await _repository.GetExecutionByIdAsync(executionId);
    }
}

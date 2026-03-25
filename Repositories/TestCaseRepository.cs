using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MiddlewareEngine.Configuration;
using MiddlewareEngine.Models;

namespace MiddlewareEngine.Repositories;

public class TestCaseRepository
{
    private readonly IMongoCollection<TestCase> _testCases;
    private readonly IMongoCollection<TestCaseExecution> _executions;

    public TestCaseRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _testCases = database.GetCollection<TestCase>("TestCases");
        _executions = database.GetCollection<TestCaseExecution>("TestCaseExecutions");
    }

    // Test Case CRUD Operations
    public async Task<List<TestCase>> GetAllAsync()
    {
        return await _testCases.Find(_ => true).ToListAsync();
    }

    public async Task<TestCase?> GetByIdAsync(string id)
    {
        return await _testCases.Find(tc => tc.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<TestCase>> GetByTagAsync(string tag)
    {
        return await _testCases.Find(tc => tc.Tags.Contains(tag)).ToListAsync();
    }

    public async Task<List<TestCase>> GetByStatusAsync(string status)
    {
        return await _testCases.Find(tc => tc.Status == status).ToListAsync();
    }

    public async Task<TestCase> CreateAsync(TestCase testCase)
    {
        testCase.CreatedAt = DateTime.UtcNow;
        testCase.UpdatedAt = DateTime.UtcNow;
        await _testCases.InsertOneAsync(testCase);
        return testCase;
    }

    public async Task<bool> UpdateAsync(string id, TestCase testCase)
    {
        // Ensure the ID is correctly set
        testCase.Id = id;
        testCase.UpdatedAt = DateTime.UtcNow;
        
        // Remove any null Ids from nested operations
        CleanOperationIds(testCase.SetupOperations);
        CleanOperationIds(testCase.TeardownOperations);
        if (testCase.Steps != null)
        {
            foreach (var step in testCase.Steps)
            {
                step.Id = null; // Steps don't need IDs
                if (step.Actions != null)
                {
                    foreach (var action in step.Actions)
                    {
                        action.Id = null; // Actions don't need IDs
                        CleanOperationIds(action.PreConditions);
                        CleanOperationIds(action.Operations);
                        CleanOperationIds(action.PostConditions);
                    }
                }
            }
        }
        
        var result = await _testCases.ReplaceOneAsync(tc => tc.Id == id, testCase);
        return result.ModifiedCount > 0;
    }
    
    private void CleanOperationIds(List<Operation>? operations)
    {
        if (operations == null) return;
        foreach (var op in operations)
        {
            op.Id = null; // Operations don't need persisted IDs
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _testCases.DeleteOneAsync(tc => tc.Id == id);
        return result.DeletedCount > 0;
    }

    // Test Case Execution Operations
    public async Task<TestCaseExecution> CreateExecutionAsync(TestCaseExecution execution)
    {
        await _executions.InsertOneAsync(execution);
        return execution;
    }

    public async Task<bool> UpdateExecutionAsync(string id, TestCaseExecution execution)
    {
        var result = await _executions.ReplaceOneAsync(e => e.Id == id, execution);
        return result.ModifiedCount > 0;
    }

    public async Task<TestCaseExecution?> GetExecutionByIdAsync(string id)
    {
        return await _executions.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<TestCaseExecution>> GetExecutionsByTestCaseIdAsync(string testCaseId)
    {
        return await _executions
            .Find(e => e.TestCaseId == testCaseId)
            .SortByDescending(e => e.StartedAt)
            .ToListAsync();
    }

    public async Task<List<TestCaseExecution>> GetRecentExecutionsAsync(int limit = 10)
    {
        return await _executions
            .Find(_ => true)
            .SortByDescending(e => e.StartedAt)
            .Limit(limit)
            .ToListAsync();
    }
}

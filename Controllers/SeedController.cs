using Microsoft.AspNetCore.Mvc;
using MiddlewareEngine.Services;
using MiddlewareEngine.Models;
using System.Text.Json;

namespace MiddlewareEngine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly IDataSeeder _dataSeeder;
    private readonly TestCaseService _testCaseService;
    private readonly ILogger<SeedController> _logger;

    public SeedController(IDataSeeder dataSeeder, TestCaseService testCaseService, ILogger<SeedController> logger)
    {
        _dataSeeder = dataSeeder;
        _testCaseService = testCaseService;
        _logger = logger;
    }

    /// <summary>
    /// Seed the database with sample function definitions
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> SeedDatabase()
    {
        try
        {
            _logger.LogInformation("Manual seed request received");
            await _dataSeeder.SeedAsync();
            
            var count = await _dataSeeder.GetFunctionCountAsync();
            return Ok(new
            {
                message = "Database seeded successfully",
                totalFunctions = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            return StatusCode(500, new { error = "Failed to seed database", details = ex.Message });
        }
    }

    /// <summary>
    /// Get the current count of function definitions
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult> GetCount()
    {
        try
        {
            var count = await _dataSeeder.GetFunctionCountAsync();
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting function count");
            return StatusCode(500, new { error = "Failed to get count" });
        }
    }

    /// <summary>
    /// Seed test cases with sample data
    /// </summary>
    [HttpPost("testcases")]
    public async Task<ActionResult> SeedTestCases()
    {
        try
        {
            _logger.LogInformation("Seeding sample test cases");

            // Sample Test Case 1: Power Supply Voltage Test
            var testCase1 = new TestCase
            {
                Name = "Power Supply Voltage Accuracy Test",
                Description = "Validates voltage output accuracy across different load conditions for manufacturing quality control",
                Priority = "Critical",
                Tags = new List<string> { "Power Supply", "Voltage", "Accuracy", "Functional" },
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                SetupOperations = new List<Operation>
                {
                    new Operation
                    {
                        Order = 1,
                        Name = "Initialize Power Supply",
                        Description = "Connect and initialize power supply controller",
                        OperationType = "RestApi",
                        TimeoutSeconds = 30
                    }
                },
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        Order = 1,
                        Name = "Set Output to 5V",
                        Description = "Configure power supply output to 5.0V",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Set Voltage Command",
                                Description = "Send SCPI command to set 5V",
                                DelayBeforeMs = 1000,
                                Operations = new List<Operation>
                                {
                                    new Operation
                                    {
                                        Order = 1,
                                        Name = "Set Voltage",
                                        OperationType = "ScpiCommand",
                                        TimeoutSeconds = 10
                                    }
                                }
                            },
                            new TestAction
                            {
                                Order = 2,
                                Name = "Measure Output",
                                Description = "Read actual voltage output",
                                DelayBeforeMs = 500,
                                Operations = new List<Operation>
                                {
                                    new Operation
                                    {
                                        Order = 1,
                                        Name = "Read Voltage",
                                        OperationType = "ScpiCommand",
                                        TimeoutSeconds = 10
                                    }
                                }
                            }
                        }
                    },
                    new TestStep
                    {
                        Order = 2,
                        Name = "Set Output to 12V",
                        Description = "Configure power supply output to 12.0V",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Set Voltage Command",
                                Description = "Send SCPI command to set 12V",
                                DelayBeforeMs = 1000,
                                Operations = new List<Operation>
                                {
                                    new Operation
                                    {
                                        Order = 1,
                                        Name = "Set 12V",
                                        OperationType = "ScpiCommand",
                                        TimeoutSeconds = 10
                                    }
                                }
                            }
                        }
                    }
                },
                TeardownOperations = new List<Operation>
                {
                    new Operation
                    {
                        Order = 1,
                        Name = "Disable Output",
                        Description = "Turn off power supply output",
                        OperationType = "ScpiCommand",
                        TimeoutSeconds = 10
                    }
                }
            };

            // Sample Test Case 2: Communication Protocol Test
            var testCase2 = new TestCase
            {
                Name = "RS232 Communication Verification",
                Description = "Tests serial communication integrity and response timing for instrument interface validation",
                Priority = "High",
                Tags = new List<string> { "Communication", "RS232", "Protocol", "Integration" },
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        Order = 1,
                        Name = "Send Identity Query",
                        Description = "Request device identification",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Query IDN",
                                Operations = new List<Operation>
                                {
                                    new Operation
                                    {
                                        Order = 1,
                                        Name = "Send *IDN?",
                                        OperationType = "ScpiCommand",
                                        TimeoutSeconds = 5
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Sample Test Case 3: Calibration Test
            var testCase3 = new TestCase
            {
                Name = "Multi-Point Temperature Calibration",
                Description = "Performs temperature sensor calibration at multiple reference points for production line calibration",
                Priority = "Critical",
                Tags = new List<string> { "Temperature", "Calibration", "Sensor" },
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        Order = 1,
                        Name = "Calibrate at 25°C",
                        Description = "Calibration point at room temperature",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Set Reference Temperature",
                                Operations = new List<Operation>
                                {
                                    new Operation { Order = 1, Name = "Set Temp", OperationType = "RestApi", TimeoutSeconds = 30 }
                                }
                            },
                            new TestAction
                            {
                                Order = 2,
                                Name = "Read Sensor Value",
                                DelayBeforeMs = 2000,
                                Operations = new List<Operation>
                                {
                                    new Operation { Order = 1, Name = "Read Temp", OperationType = "SdkMethod", TimeoutSeconds = 10 }
                                }
                            }
                        }
                    },
                    new TestStep
                    {
                        Order = 2,
                        Name = "Calibrate at 50°C",
                        Description = "Calibration point at elevated temperature",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Set Reference Temperature",
                                Operations = new List<Operation>
                                {
                                    new Operation { Order = 1, Name = "Set 50C", OperationType = "RestApi", TimeoutSeconds = 30 }
                                }
                            }
                        }
                    },
                    new TestStep
                    {
                        Order = 3,
                        Name = "Calibrate at 75°C",
                        Description = "Calibration point at high temperature",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Set Reference Temperature",
                                Operations = new List<Operation>
                                {
                                    new Operation { Order = 1, Name = "Set 75C", OperationType = "RestApi", TimeoutSeconds = 30 }
                                }
                            }
                        }
                    }
                }
            };

            // Sample Test Case 4: Performance Test
            var testCase4 = new TestCase
            {
                Name = "Signal Generator Frequency Sweep",
                Description = "Tests frequency accuracy and settling time across the full operating range",
                Priority = "Medium",
                Tags = new List<string> { "Frequency", "Signal Generator", "Performance" },
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        Order = 1,
                        Name = "Frequency Sweep Test",
                        Description = "Sweep from 1MHz to 100MHz",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Execute Sweep",
                                Operations = new List<Operation>
                                {
                                    new Operation { Order = 1, Name = "Start Sweep", OperationType = "SdkMethod", TimeoutSeconds = 60 }
                                }
                            }
                        }
                    }
                }
            };

            // Sample Test Case 5: Safety Compliance
            var testCase5 = new TestCase
            {
                Name = "Over-Voltage Protection Test",
                Description = "Validates automatic shutdown when output exceeds safe limits for safety certification",
                Priority = "Critical",
                Tags = new List<string> { "Safety", "Over-Voltage", "Protection" },
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        Order = 1,
                        Name = "Trigger Over-Voltage Condition",
                        Description = "Set voltage above threshold",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Set High Voltage",
                                Operations = new List<Operation>
                                {
                                    new Operation { Order = 1, Name = "Set OVP", OperationType = "ScpiCommand", TimeoutSeconds = 5 }
                                }
                            }
                        }
                    },
                    new TestStep
                    {
                        Order = 2,
                        Name = "Verify Protection Activated",
                        Description = "Check that output is disabled",
                        Actions = new List<TestAction>
                        {
                            new TestAction
                            {
                                Order = 1,
                                Name = "Check Status",
                                DelayBeforeMs = 100,
                                Operations = new List<Operation>
                                {
                                    new Operation { Order = 1, Name = "Read Status", OperationType = "ScpiCommand", TimeoutSeconds = 5 }
                                }
                            }
                        }
                    }
                }
            };

            await _testCaseService.CreateTestCaseAsync(testCase1);
            await _testCaseService.CreateTestCaseAsync(testCase2);
            await _testCaseService.CreateTestCaseAsync(testCase3);
            await _testCaseService.CreateTestCaseAsync(testCase4);
            await _testCaseService.CreateTestCaseAsync(testCase5);

            var allTestCases = await _testCaseService.GetAllTestCasesAsync();

            // Get category from tags
            string GetCategory(TestCase tc) => tc.Tags.FirstOrDefault(t => t is "Functional" or "Integration" or "Calibration" or "Performance" or "Safety") ?? "Other";

            return Ok(new
            {
                message = "Test cases seeded successfully",
                totalTestCases = allTestCases.Count,
                seededCases = new[]
                {
                    new { name = testCase1.Name, priority = testCase1.Priority, category = GetCategory(testCase1), steps = testCase1.Steps.Count },
                    new { name = testCase2.Name, priority = testCase2.Priority, category = GetCategory(testCase2), steps = testCase2.Steps.Count },
                    new { name = testCase3.Name, priority = testCase3.Priority, category = GetCategory(testCase3), steps = testCase3.Steps.Count },
                    new { name = testCase4.Name, priority = testCase4.Priority, category = GetCategory(testCase4), steps = testCase4.Steps.Count },
                    new { name = testCase5.Name, priority = testCase5.Priority, category = GetCategory(testCase5), steps = testCase5.Steps.Count }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding test cases");
            return StatusCode(500, new { error = "Failed to seed test cases", details = ex.Message });
        }
    }

    /// <summary>
    /// Clear all test cases from the database
    /// </summary>
    [HttpDelete("testcases")]
    public async Task<ActionResult> ClearTestCases()
    {
        try
        {
            var testCases = await _testCaseService.GetAllTestCasesAsync();
            foreach (var tc in testCases)
            {
                if (!string.IsNullOrEmpty(tc.Id))
                {
                    await _testCaseService.DeleteTestCaseAsync(tc.Id);
                }
            }
            
            return Ok(new { message = "All test cases cleared successfully", count = testCases.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing test cases");
            return StatusCode(500, new { error = "Failed to clear test cases", details = ex.Message });
        }
    }
}

# Sqordia Test Suite

This directory contains comprehensive unit and integration tests for the Sqordia SaaS Business Plan Platform.

##     Test Structure

```
tests/
          Sqordia.Application.UnitTests/          # Unit tests for application services
             Common/                             # Common test utilities and base classes
                ServiceTestBase.cs              # Base class for service tests
                TestDataBuilder.cs              # Utility for building test data
             Services/                           # Service-specific unit tests
                BusinessPlan/                   # BusinessPlanService tests
                Organization/                   # OrganizationService tests
                AI/                            # AiService tests
                Questionnaire/                 # QuestionnaireService tests
                Export/                        # ExportService tests
             Sqordia.Application.UnitTests.csproj
          Sqordia.Infrastructure.IntegrationTests/ # Integration tests
             Services/                           # Service integration tests
                BusinessPlanServiceIntegrationTests.cs
                OrganizationServiceIntegrationTests.cs
                AiServiceIntegrationTests.cs
                QuestionnaireServiceIntegrationTests.cs
                ExportServiceIntegrationTests.cs
             Sqordia.Infrastructure.IntegrationTests.csproj
          run-tests.sh                           # Test runner script
          testsettings.json                      # Test configuration
          README.md                              # This file
```

##     Test Types

### Unit Tests
- **Purpose**: Test individual components in isolation
- **Scope**: Single service methods and business logic
- **Dependencies**: Mocked using Moq framework
- **Database**: In-memory database for data access tests
- **Execution**: Fast, parallel execution

### Integration Tests
- **Purpose**: Test interaction between multiple components
- **Scope**: End-to-end workflows and service integration
- **Dependencies**: Real services with mocked external dependencies
- **Database**: In-memory database with realistic data
- **Execution**: Slower, sequential execution

##     Running Tests

### Prerequisites
- .NET 8.0 SDK
- Git (for cloning the repository)

### Quick Start
```bash
# Run all tests
./tests/run-tests.sh

# Or run tests individually
dotnet test tests/Sqordia.Application.UnitTests/
dotnet test tests/Sqordia.Infrastructure.IntegrationTests/
```

### Manual Test Execution
```bash
# Unit tests only
dotnet test tests/Sqordia.Application.UnitTests/ \
  --configuration Release \
  --verbosity normal

# Integration tests only
dotnet test tests/Sqordia.Infrastructure.IntegrationTests/ \
  --configuration Release \
  --verbosity normal

# Specific test class
dotnet test tests/Sqordia.Application.UnitTests/ \
  --filter "ClassName=Sqordia.Application.UnitTests.Services.BusinessPlan.BusinessPlanServiceTests"

# Specific test method
dotnet test tests/Sqordia.Application.UnitTests/ \
  --filter "MethodName=CreateBusinessPlan_WithValidData_ShouldCreateBusinessPlan"
```

##     Test Coverage

The test suite aims for comprehensive coverage of:

### BusinessPlanService
-    CRUD operations
-    AI integration
-    Financial projections
-    Collaboration features
-    Version control
-    Export functionality

### OrganizationService
-    Organization management
-    Member administration
-    Invitation system
-    Subscription management
-    Billing history
-    Role management

### AiService
-    Strategy suggestions
-    Risk analysis
-    Financial projections
-    Business mentor advice
-    Market analysis
-    Content generation
-    Business plan analysis

### QuestionnaireService
-    Questionnaire generation
-    Answer validation
-    Business plan creation
-    Progress tracking
-    Section management

### ExportService
-    PDF export
-    Word export
-    Excel export
-    PowerPoint export
-    Export history
-    Custom styling

##     Test Configuration

### testsettings.json
The test configuration file contains settings for:
- Test execution parameters
- Code coverage thresholds
- Database configuration
- Mock settings
- Default test data

### Environment Variables
```bash
# Test database connection
export TEST_DATABASE_CONNECTION="Data Source=:memory:"

# Test logging level
export TEST_LOG_LEVEL="Information"

# Test timeout
export TEST_TIMEOUT="30000"
```

##     Code Coverage

The test suite generates code coverage reports in multiple formats:
- **HTML**: Human-readable coverage report
- **XML**: For CI/CD integration
- **JSON**: For programmatic analysis

Coverage reports are generated in the `./TestResults` directory.

### Coverage Targets
- **Overall Coverage**:    80%
- **Service Layer**:    90%
- **Domain Layer**:    85%
- **Application Layer**:    80%

##        Test Utilities

### ServiceTestBase
Base class providing common setup for service tests:
- AutoFixture configuration
- In-memory database setup
- Mock service registration
- Common test data

### TestDataBuilder
Utility class for building test data:
- User creation
- Organization setup
- Business plan generation
- Mock data generation

### Mock Services
- **MockCurrentUserService**: Provides test user context
- **MockEmailService**: Simulates email sending
- **MockAiService**: Provides AI service responses
- **MockExportService**: Simulates file export

##     Debugging Tests

### Running Tests in Debug Mode
```bash
dotnet test tests/Sqordia.Application.UnitTests/ \
  --configuration Debug \
  --verbosity detailed \
  --logger "console;verbosity=detailed"
```

### Attaching Debugger
1. Set breakpoints in test code
2. Run tests in debug mode
3. Attach debugger to test process
4. Step through test execution

### Test Output
```bash
# Enable test output
dotnet test tests/Sqordia.Application.UnitTests/ \
  --logger "console;verbosity=detailed" \
  --collect:"XPlat Code Coverage"
```

##     Writing New Tests

### Unit Test Template
```csharp
public class NewServiceTests : ServiceTestBase
{
    [Fact]
    public async Task MethodName_WithValidInput_ShouldReturnExpectedResult()
    {
        // Arrange
        var request = TestDataBuilder.CreateValidRequest();
        
        // Act
        var result = await _service.MethodAsync(request);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
```

### Integration Test Template
```csharp
public class NewServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task MethodName_WithValidData_ShouldWorkEndToEnd()
    {
        // Arrange
        var testData = await SetupTestDataAsync();
        
        // Act
        var result = await _service.MethodAsync(testData.Request);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        // Verify side effects
        var persistedData = await _context.Entities.FindAsync(testData.Id);
        persistedData.Should().NotBeNull();
    }
}
```

##     Test Categories

Tests are categorized using attributes:
- `[Fact]`: Standard test
- `[Theory]`: Parameterized test
- `[Trait("Category", "Unit")]`: Unit test
- `[Trait("Category", "Integration")]`: Integration test
- `[Trait("Category", "Fast")]`: Fast test
- `[Trait("Category", "Slow")]`: Slow test

### Running Tests by Category
```bash
# Run only unit tests
dotnet test --filter "Category=Unit"

# Run only fast tests
dotnet test --filter "Category=Fast"

# Run only slow tests
dotnet test --filter "Category=Slow"
```

##     Common Issues

### Test Failures
1. **Build Errors**: Ensure all dependencies are restored
2. **Database Issues**: Check in-memory database setup
3. **Mock Issues**: Verify mock configurations
4. **Timing Issues**: Adjust test timeouts

### Performance Issues
1. **Slow Tests**: Use parallel execution for unit tests
2. **Memory Issues**: Clean up test data after each test
3. **Database Issues**: Use in-memory database for unit tests

### Coverage Issues
1. **Low Coverage**: Add more test cases
2. **Missing Coverage**: Check excluded assemblies
3. **False Positives**: Review coverage configuration

##     Best Practices

### Test Naming
- Use descriptive test names
- Follow the pattern: `MethodName_WithCondition_ShouldReturnExpectedResult`
- Include the scenario being tested

### Test Structure
- Follow AAA pattern (Arrange, Act, Assert)
- Keep tests focused and simple
- Use meaningful variable names
- Avoid complex test logic

### Test Data
- Use AutoFixture for generating test data
- Create realistic test scenarios
- Avoid hardcoded values
- Use test data builders for complex objects

### Assertions
- Use FluentAssertions for readable assertions
- Test both success and failure scenarios
- Verify all important properties
- Check side effects when appropriate

##     Contributing

When adding new tests:
1. Follow existing patterns and conventions
2. Ensure adequate test coverage
3. Update this README if needed
4. Run all tests before submitting
5. Add appropriate test categories

##   ž Support

For test-related issues:
1. Check this README first
2. Review existing test examples
3. Check test configuration
4. Verify dependencies are installed
5. Contact the development team

---

**Happy Testing!       **

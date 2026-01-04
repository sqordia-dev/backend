# Sqordia BDD Tests

This directory contains Behavior-Driven Development (BDD) tests for the Sqordia SaaS Business Plan Platform using SpecFlow.

## Overview

BDD tests complement the existing unit and integration tests by focusing on business requirements and user scenarios. These tests serve as living documentation and ensure that the application meets business expectations.

## Test Structure

```
Sqordia.BDDTests/
├── Features/                          # Gherkin feature files
│   ├── BusinessPlanGeneration.feature # Business plan creation and management
│   ├── UserAuthentication.feature     # User registration and authentication
│   ├── OrganizationManagement.feature # Team collaboration and organization features
│   └── AIIntegration.feature         # AI-powered features and enhancements
├── StepDefinitions/                  # Step definition implementations
│   ├── CommonSteps.cs                # Common application steps
│   ├── BusinessPlanSteps.cs          # Business plan specific steps
│   ├── OrganizationSteps.cs          # Organization management steps
│   └── AISteps.cs                    # AI integration steps
├── TestContext.cs                    # Test context and data management
├── Sqordia.BDDTests.csproj           # Project file with SpecFlow dependencies
├── specflow.json                     # SpecFlow configuration
└── README.md                         # This file
```

## Features Covered

### Business Plan Generation
- Complete business plan creation workflow
- AI-powered plan enhancement
- Multi-format export (PDF, Word, Excel)
- Team collaboration on business plans
- Industry-specific plan generation
- Plan validation and completeness checks

### User Authentication
- User registration and email verification
- Secure login and session management
- Password reset functionality
- Account security features
- Session timeout and refresh

### Organization Management
- Organization creation and member management
- Role-based access control
- Team collaboration features
- Subscription management
- Data isolation between organizations

### AI Integration
- Strategic business recommendations
- Risk analysis and assessment
- Market research and competitive analysis
- Financial optimization suggestions
- Content generation and enhancement
- Industry-specific expertise

## Running BDD Tests

### Prerequisites
- .NET 8.0 SDK
- SpecFlow Visual Studio extension (optional, for IDE support)

### Quick Start
```bash
# Run all BDD tests
dotnet test tests/Sqordia.BDDTests/

# Run specific feature
dotnet test tests/Sqordia.BDDTests/ --filter "Feature=BusinessPlanGeneration"

# Run tests with specific tags
dotnet test tests/Sqordia.BDDTests/ --filter "Tag=business-plan"
```

### Running Tests by Category
```bash
# Run only business plan tests
dotnet test --filter "Tag=business-plan"

# Run only authentication tests
dotnet test --filter "Tag=authentication"

# Run only AI integration tests
dotnet test --filter "Tag=ai"

# Run only organization tests
dotnet test --filter "Tag=organization"
```

## Test Tags

Tests are organized using SpecFlow tags for better categorization:

- `@business-plan`: Business plan generation and management
- `@authentication`: User authentication and security
- `@organization`: Organization and team management
- `@ai`: AI-powered features
- `@happy-path`: Standard successful scenarios
- `@integration`: End-to-end integration scenarios
- `@export`: Export functionality
- `@collaboration`: Team collaboration features
- `@validation`: Data validation scenarios
- `@industry-specific`: Industry-specific functionality

## Writing New BDD Tests

### Feature File Structure
```gherkin
Feature: Feature Name
  As a [user type]
  I want to [capability]
  So that [business value]

  Background:
    Given [common setup]

  @tag-name
  Scenario: Scenario description
    Given [initial context]
    When [action is performed]
    Then [expected outcome]
```

### Step Definition Template
```csharp
[Binding]
public class NewFeatureSteps
{
    private readonly TestContext _testContext;

    public NewFeatureSteps(TestContext testContext)
    {
        _testContext = testContext;
    }

    [Given(@"step description")]
    public void GivenStepDescription()
    {
        // Implementation
    }

    [When(@"action description")]
    public async Task WhenActionDescription()
    {
        // Implementation
    }

    [Then(@"expected outcome")]
    public void ThenExpectedOutcome()
    {
        // Assertions
    }
}
```

## Test Data Management

The `TestContext` class provides:
- Test data generation using AutoFixture
- Service mocking and stubbing
- Database setup and cleanup
- Result tracking across scenarios

## Best Practices

### Feature Files
- Use clear, business-focused language
- Keep scenarios focused and atomic
- Use appropriate tags for categorization
- Include background steps for common setup

### Step Definitions
- Keep step definitions focused and reusable
- Use the TestContext for data management
- Implement proper assertions with FluentAssertions
- Handle async operations appropriately

### Test Organization
- Group related scenarios in feature files
- Use consistent naming conventions
- Maintain clear separation of concerns
- Document complex business logic

## Integration with Existing Tests

BDD tests complement the existing test suite by:
- **Unit Tests**: Test individual components (existing)
- **Integration Tests**: Test component interactions (existing)
- **BDD Tests**: Test business requirements and user scenarios (new)

## Configuration

### SpecFlow Configuration (specflow.json)
```json
{
  "language": {
    "feature": "en-US"
  },
  "bindingCulture": {
    "name": "en-US"
  },
  "runtime": {
    "dependencies": [
      {
        "type": "context",
        "implementation": "Sqordia.BDDTests.TestContext"
      }
    ]
  },
  "plugins": [
    {
      "name": "SpecFlow.xUnit"
    }
  ]
}
```

## Debugging BDD Tests

### Running Tests in Debug Mode
```bash
dotnet test tests/Sqordia.BDDTests/ \
  --configuration Debug \
  --verbosity detailed \
  --logger "console;verbosity=detailed"
```

### Attaching Debugger
1. Set breakpoints in step definitions
2. Run tests in debug mode
3. Attach debugger to test process
4. Step through test execution

## Reporting

BDD tests generate reports showing:
- Feature coverage
- Scenario execution results
- Step definition mapping
- Business requirement traceability

## Contributing

When adding new BDD tests:
1. Follow existing patterns and conventions
2. Use appropriate tags for categorization
3. Keep scenarios focused and atomic
4. Update this README if needed
5. Ensure tests are maintainable and readable

## Support

For BDD test-related issues:
1. Check this README first
2. Review existing feature files and step definitions
3. Verify SpecFlow configuration
4. Check test context setup
5. Contact the development team

---

**Happy BDD Testing! 🚀**

# BDD Feature Files

This directory contains Gherkin feature files that define the business requirements and user scenarios for the Sqordia platform.

## Feature Files Overview

### BusinessPlanGeneration.feature
**Purpose**: Tests the complete business plan creation and management workflow
**Key Scenarios**:
- Generate business plan from questionnaire
- AI enhancement with strategic recommendations
- Multi-format export (PDF, Word)
- Team collaboration on business plans
- Industry-specific plan generation
- Plan validation and completeness

**Tags**: `@business-plan`, `@happy-path`, `@ai-integration`, `@export`, `@collaboration`, `@validation`, `@industry-specific`

### UserAuthentication.feature
**Purpose**: Tests user registration, authentication, and security features
**Key Scenarios**:
- User registration with email verification
- Secure login and session management
- Password reset functionality
- Account security features
- Session timeout and refresh

**Tags**: `@authentication`, `@registration`, `@email-verification`, `@login`, `@login-failure`, `@password-reset`, `@security`, `@session-management`

### OrganizationManagement.feature
**Purpose**: Tests organization creation, team management, and collaboration features
**Key Scenarios**:
- Organization creation and member invitation
- Role-based access control
- Team collaboration on business plans
- Subscription management
- Data isolation between organizations
- Collaborative export and sharing

**Tags**: `@organization`, `@creation`, `@member-management`, `@collaboration`, `@permissions`, `@subscription`, `@data-isolation`, `@export-collaboration`

### AIIntegration.feature
**Purpose**: Tests AI-powered features and intelligent business plan enhancement
**Key Scenarios**:
- Strategic business recommendations
- Risk analysis and assessment
- Market research and competitive analysis
- Financial optimization
- Content generation
- Business mentor guidance
- Plan validation and quality scoring
- Industry-specific expertise
- Continuous learning from feedback

**Tags**: `@ai`, `@strategy-suggestions`, `@risk-analysis`, `@market-analysis`, `@financial-optimization`, `@content-generation`, `@mentor-advice`, `@plan-validation`, `@industry-expertise`, `@continuous-improvement`

## Writing New Feature Files

### Structure Template
```gherkin
Feature: Feature Name
  As a [user type]
  I want to [capability]
  So that [business value]

  Background:
    Given [common setup conditions]

  @tag-name
  Scenario: Scenario description
    Given [initial context]
    When [action is performed]
    Then [expected outcome]

  @tag-name
  Scenario Outline: Parameterized scenario
    Given [initial context with <parameter>]
    When [action is performed]
    Then [expected outcome]

    Examples:
      | parameter |
      | value1    |
      | value2    |
```

### Best Practices

1. **Clear Business Language**: Use language that business stakeholders can understand
2. **Focused Scenarios**: Keep scenarios atomic and focused on single behaviors
3. **Appropriate Tags**: Use tags for categorization and selective test execution
4. **Background Steps**: Use Background for common setup across scenarios
5. **Parameterized Tests**: Use Scenario Outline for testing multiple data sets
6. **Descriptive Names**: Use clear, descriptive scenario names

### Tag Guidelines

- **Feature Tags**: `@business-plan`, `@authentication`, `@organization`, `@ai`
- **Scenario Tags**: `@happy-path`, `@error-case`, `@edge-case`
- **Functionality Tags**: `@export`, `@collaboration`, `@validation`
- **Performance Tags**: `@slow`, `@fast`
- **Environment Tags**: `@integration`, `@unit`

## Running Feature-Specific Tests

```bash
# Run specific feature
dotnet test --filter "Feature=BusinessPlanGeneration"

# Run tests with specific tags
dotnet test --filter "Tag=business-plan"
dotnet test --filter "Tag=authentication"

# Run multiple tags
dotnet test --filter "Tag=business-plan&Tag=happy-path"

# Exclude specific tags
dotnet test --filter "Tag!=slow"
```

## Feature File Maintenance

### Adding New Scenarios
1. Identify the appropriate feature file
2. Add the scenario with proper tags
3. Implement corresponding step definitions
4. Update this README if needed

### Modifying Existing Scenarios
1. Update the feature file
2. Update step definitions if needed
3. Ensure backward compatibility
4. Update documentation

### Removing Scenarios
1. Remove from feature file
2. Remove unused step definitions
3. Update this README
4. Verify no other scenarios depend on removed steps

## Integration with Development

### During Development
- Write feature files before implementing functionality
- Use scenarios as acceptance criteria
- Keep scenarios up-to-date with requirements

### During Testing
- Run relevant feature tests for each development area
- Use tags to run specific test suites
- Monitor test results and update scenarios as needed

### During Maintenance
- Review and update scenarios regularly
- Remove obsolete scenarios
- Add new scenarios for new functionality

## Documentation

Each feature file serves as living documentation that:
- Describes business requirements
- Provides examples of system behavior
- Guides development and testing
- Facilitates communication between stakeholders

## Support

For questions about feature files:
1. Review existing feature files for patterns
2. Check step definition implementations
3. Consult the main BDD tests README
4. Contact the development team

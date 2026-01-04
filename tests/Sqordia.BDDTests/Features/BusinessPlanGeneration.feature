Feature: Business Plan Generation
  As a business owner
  I want to generate a comprehensive business plan
  So that I can secure funding and guide my business growth

  Background:
    Given the application is running
    And I am a registered user

  @business-plan @happy-path
  Scenario: Generate business plan from questionnaire
    Given I have completed the business questionnaire with valid data
    When I request to generate a business plan
    Then I should receive a complete business plan
    And the plan should include executive summary
    And the plan should include market analysis
    And the plan should include financial projections
    And the plan should include risk assessment

  @business-plan @ai-integration
  Scenario: AI enhances business plan with strategic recommendations
    Given I have a basic business plan
    When I request AI enhancement
    Then the AI should provide strategic recommendations
    And the recommendations should be relevant to my industry
    And the recommendations should improve the plan quality

  @business-plan @export
  Scenario: Export business plan in multiple formats
    Given I have a completed business plan
    When I export the plan as PDF
    Then I should receive a PDF document
    And the PDF should contain all plan sections
    When I export the plan as Word document
    Then I should receive a Word document
    And the Word document should be editable

  @business-plan @collaboration
  Scenario: Team members collaborate on business plan
    Given I am an organization owner
    And I have invited team members to collaborate
    When team members make edits to the business plan
    Then all changes should be synchronized in real-time
    And version history should be maintained
    And I should be notified of changes

  @business-plan @validation
  Scenario: Business plan validation and completeness check
    Given I have started creating a business plan
    When I attempt to generate the final plan
    And some required sections are incomplete
    Then the system should identify missing sections
    And provide guidance on completing the plan
    And prevent generation until all sections are complete

  @business-plan @industry-specific
  Scenario Outline: Generate industry-specific business plan
    Given I am creating a business plan for a <industry> company
    When I complete the questionnaire with <industry> specific data
    Then the generated plan should include <industry> specific sections
    And the financial projections should be appropriate for <industry>

    Examples:
      | industry        |
      | Technology      |
      | Healthcare      |
      | Retail          |
      | Manufacturing   |
      | Professional Services |

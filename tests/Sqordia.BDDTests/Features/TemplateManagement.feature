Feature: Template Management System
  As a business plan creator
  I want to manage templates for different industries and use cases
  So that I can create professional business plans efficiently

  Background:
    Given the application is running
    And I am logged in as a user

  Scenario: Create a new template
    Given I want to create a template for "Technology Startup"
    When I create a template with the following details:
      | Name | Technology Startup Template |
      | Description | Comprehensive template for tech startups |
      | Category | BusinessPlan |
      | Type | IndustrySpecific |
      | Industry | Technology |
      | Language | English |
      | Country | United States |
    Then the template should be created successfully
    And I should see the template in my templates list

  Scenario: Browse templates by category
    Given there are templates available in the system
    When I browse templates by category "BusinessPlan"
    Then I should see all business plan templates
    And each template should show its name, description, and rating

  Scenario: Search templates by industry
    Given there are templates for different industries
    When I search for templates in the "Healthcare" industry
    Then I should see all healthcare-related templates
    And the results should be relevant to healthcare

  Scenario: Use a public template
    Given there are public templates available
    When I select a public template
    Then I should be able to use the template
    And the template usage should be recorded

  Scenario: Rate a template
    Given I have used a template
    When I rate the template with 5 stars and comment "Excellent template"
    Then the rating should be recorded
    And the template's average rating should be updated

  Scenario: Clone a template
    Given I have access to a template
    When I clone the template with a new name "My Custom Template"
    Then a new template should be created
    And the new template should be based on the original template
    And I should be the owner of the new template

  Scenario: Customize a template
    Given I have selected a template
    When I customize the template with my preferences
    Then the customizations should be saved
    And I should be able to use the customized template

  Scenario: View template analytics
    Given I am the owner of a template
    When I view the template analytics
    Then I should see usage statistics
    And I should see rating information
    And I should see geographic distribution of users

  Scenario: Publish a template
    Given I have created a template
    When I publish the template
    Then the template should be available to other users
    And the template status should be "Published"

  Scenario: Archive a template
    Given I have a template that is no longer needed
    When I archive the template
    Then the template should be hidden from public view
    And the template status should be "Archived"

  Scenario: Get popular templates
    Given there are templates with different usage counts
    When I request popular templates
    Then I should see templates ordered by popularity
    And the most used templates should appear first

  Scenario: Get recent templates
    Given there are templates created at different times
    When I request recent templates
    Then I should see templates ordered by creation date
    And the most recently created templates should appear first

  Scenario: Filter templates by author
    Given there are templates from different authors
    When I filter templates by author "John Doe"
    Then I should see only templates created by "John Doe"
    And the results should be relevant to that author

  Scenario: Template versioning
    Given I have a template with version "1.0"
    When I update the template with new content
    Then the template version should be updated to "1.1"
    And the previous version should be preserved
    And I should be able to see the changelog

  Scenario: Template sections management
    Given I have a template
    When I add a new section to the template
    Then the section should be added successfully
    And I should be able to reorder sections
    And I should be able to configure section properties

  Scenario: Template fields management
    Given I have a template section
    When I add fields to the section
    Then the fields should be added successfully
    And I should be able to configure field properties
    And I should be able to set validation rules

  Scenario: Template sharing
    Given I have a private template
    When I share the template with specific users
    Then those users should be able to access the template
    And the template should remain private to others

  Scenario: Template collaboration
    Given I have a template
    When I invite collaborators to edit the template
    Then the collaborators should be able to edit the template
    And I should be able to see who made what changes
    And I should be able to approve or reject changes

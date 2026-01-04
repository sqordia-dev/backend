Feature: Organization Management
  As an organization owner
  I want to manage my team and collaborate on business plans
  So that we can work together effectively

  Background:
    Given the application is running
    And I am a registered user

  @organization @creation
  Scenario: Create organization and invite team members
    Given I am a registered user
    When I create a new organization
    Then the organization should be created successfully
    And I should be the organization owner
    When I invite team members by email
    Then invitation emails should be sent
    And the invitations should contain secure links

  @organization @member-management
  Scenario: Manage organization members and roles
    Given I am an organization owner
    And I have invited team members
    When team members accept the invitations
    Then they should be added to the organization
    And I should be able to assign roles to members
    When I change a member's role
    Then their permissions should be updated accordingly

  @organization @collaboration
  Scenario: Team collaboration on business plans
    Given I am an organization member
    And my team has created a business plan
    When I make edits to the business plan
    Then other team members should see my changes
    And version history should be maintained
    When multiple members edit simultaneously
    Then the system should handle conflicts gracefully

  @organization @permissions
  Scenario: Role-based access control
    Given I have different user roles in the organization
    When users attempt to access business plans
    Then access should be granted based on their role
    And owners should have full access
    And editors should be able to modify plans
    And viewers should only be able to read plans

  @organization @subscription
  Scenario: Organization subscription management
    Given I am an organization owner
    When I upgrade to a premium subscription
    Then the organization should have premium features
    And billing should be tracked
    When the subscription expires
    Then premium features should be restricted
    And I should receive renewal notifications

  @organization @data-isolation
  Scenario: Organization data isolation
    Given there are multiple organizations in the system
    When users from different organizations access data
    Then each organization should only see their own data
    And business plans should be isolated by organization
    And user permissions should be organization-specific

  @organization @export-collaboration
  Scenario: Collaborative export and sharing
    Given I am working on a business plan with my team
    When I export the plan as PDF
    Then the export should include all team contributions
    And the document should show revision history
    When I share the plan externally
    Then sharing permissions should be respected
    And external users should have appropriate access levels

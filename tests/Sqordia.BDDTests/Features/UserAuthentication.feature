Feature: User Authentication and Registration
  As a potential user
  I want to register and authenticate securely
  So that I can access the business plan platform

  Background:
    Given the application is running
    And the user registration system is available

  @authentication @registration
  Scenario: User registers with valid information
    Given I am a new user
    When I register with valid email and password
    Then my account should be created successfully
    And I should receive a verification email
    And my account should be in pending verification state

  @authentication @email-verification
  Scenario: User verifies email address
    Given I have registered with a valid email
    And I have received a verification email
    When I click the verification link
    Then my email should be verified
    And my account should be activated
    And I should be able to log in

  @authentication @login
  Scenario: User logs in with valid credentials
    Given I have a verified account
    When I log in with correct email and password
    Then I should be authenticated successfully
    And I should receive a JWT token
    And I should be redirected to the dashboard

  @authentication @login-failure
  Scenario: User login fails with invalid credentials
    Given I have a verified account
    When I attempt to log in with incorrect password
    Then the login should fail
    And I should receive an error message
    And my account should not be locked

  @authentication @password-reset
  Scenario: User resets forgotten password
    Given I have a verified account
    When I request a password reset
    Then I should receive a password reset email
    When I click the reset link and set a new password
    Then my password should be updated
    And I should be able to log in with the new password

  @authentication @security
  Scenario: Account security features
    Given I have a verified account
    When I log in from a new device
    Then I should receive a security notification
    And my login session should be tracked
    When I log out
    Then my session should be terminated
    And I should be redirected to the login page

  @authentication @session-management
  Scenario: Session timeout and refresh
    Given I am logged in
    When my session is about to expire
    Then I should receive a warning notification
    When I refresh my session
    Then my session should be extended
    When my session expires
    Then I should be logged out automatically

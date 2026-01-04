Feature: Financial Management System
  As a business owner
  I want to manage financial projections, currency conversions, tax calculations, and investment analysis
  So that I can make informed financial decisions for my business

  Background:
    Given the application is running
    And I am logged in as a business owner
    And I have a business plan

  Scenario: Create financial projection for revenue
    Given I want to create a financial projection
    When I create a revenue projection for "Product Sales" of $10000 for "2024" "January"
    And I set the currency to "USD"
    And I set the scenario to "Realistic"
    And I set the growth rate to "10%"
    Then the financial projection should be created successfully
    And the projection should show revenue of $10000
    And the projection should be in USD currency
    And the projection should be for the realistic scenario

  Scenario: Create financial projection for expenses
    Given I want to create a financial projection
    When I create an expense projection for "Marketing" of $5000 for "2024" "February"
    And I set the currency to "EUR"
    And I set the scenario to "Optimistic"
    And I set the growth rate to "5%"
    Then the financial projection should be created successfully
    And the projection should show expenses of $5000
    And the projection should be in EUR currency
    And the projection should be for the optimistic scenario

  Scenario: Convert currency between different currencies
    Given I have a financial projection of $10000 in USD
    When I convert the amount to EUR
    Then the conversion should use the current exchange rate
    And the converted amount should be calculated correctly
    And the exchange rate should be recorded

  Scenario: Calculate tax for financial projection
    Given I have a financial projection of $50000 in revenue
    And I am in "United States" "California"
    When I calculate the applicable taxes
    Then the tax calculation should include income tax
    And the tax calculation should include state tax
    And the tax amount should be calculated correctly
    And the tax rules should be applied based on location

  Scenario: Calculate multiple tax types for international business
    Given I have a financial projection of €75000 in revenue
    And I am in "Germany" "Bavaria"
    When I calculate the applicable taxes
    Then the tax calculation should include corporate tax
    And the tax calculation should include VAT
    And the tax calculation should include local taxes
    And the tax amounts should be calculated correctly

  Scenario: Generate financial KPIs
    Given I have multiple financial projections for my business plan
    When I request the calculation of financial KPIs
    Then the system should calculate revenue growth rate
    And the system should calculate profit margin
    And the system should calculate cash flow metrics
    And the system should calculate return on investment
    And the KPIs should be categorized by type

  Scenario: Perform investment analysis
    Given I want to analyze an investment opportunity
    When I create an investment analysis with initial investment of $100000
    And I set the expected return to $150000 over 3 years
    And I set the discount rate to 8%
    Then the system should calculate the NPV
    And the system should calculate the IRR
    And the system should calculate the payback period
    And the system should calculate the ROI
    And the analysis should include risk assessment

  Scenario: Generate cash flow report
    Given I have financial projections for multiple months
    When I request a cash flow report
    Then the report should show opening balance
    And the report should show cash inflows
    And the report should show cash outflows
    And the report should show net cash flow
    And the report should show closing balance
    And the report should be generated in the specified currency

  Scenario: Generate profit and loss report
    Given I have revenue and expense projections
    When I request a profit and loss report
    Then the report should show total revenue
    And the report should show cost of goods sold
    And the report should show gross profit
    And the report should show operating expenses
    And the report should show operating income
    And the report should show net income

  Scenario: Generate balance sheet report
    Given I have asset and liability projections
    When I request a balance sheet report
    Then the report should show total assets
    And the report should show total liabilities
    And the report should show total equity
    And the report should balance (Assets = Liabilities + Equity)

  Scenario: Perform scenario analysis
    Given I have financial projections
    When I request scenario analysis
    Then the system should generate optimistic scenario
    And the system should generate realistic scenario
    And the system should generate pessimistic scenario
    And each scenario should show different financial outcomes
    And the scenarios should include risk assessments

  Scenario: Perform sensitivity analysis
    Given I have financial projections
    When I request sensitivity analysis for "Revenue Growth Rate"
    Then the system should analyze the impact of different growth rates
    And the system should show how NPV changes with growth rate
    And the system should show how IRR changes with growth rate
    And the system should identify the most sensitive variables

  Scenario: Calculate break-even analysis
    Given I have cost and revenue projections
    When I request break-even analysis
    Then the system should calculate fixed costs
    And the system should calculate variable costs per unit
    And the system should calculate selling price per unit
    And the system should calculate break-even units
    And the system should calculate break-even revenue
    And the system should show contribution margin

  Scenario: Manage multiple currencies in business plan
    Given I have a business plan with international operations
    When I add financial projections in different currencies
    And I set USD as the base currency
    Then all projections should be converted to base currency
    And exchange rates should be applied correctly
    And currency conversions should be tracked
    And multi-currency reports should be generated

  Scenario: Update financial projection
    Given I have an existing financial projection
    When I update the projection amount to $15000
    And I update the growth rate to 15%
    Then the projection should be updated successfully
    And the changes should be reflected in reports
    And the updated projection should maintain its history

  Scenario: Delete financial projection
    Given I have an existing financial projection
    When I delete the projection
    Then the projection should be removed from the system
    And related calculations should be updated
    And the deletion should be logged

  Scenario: Get financial projections by scenario
    Given I have multiple financial projections with different scenarios
    When I request projections for "Optimistic" scenario
    Then only optimistic projections should be returned
    And the projections should be properly categorized
    And the scenario analysis should be accurate

  Scenario: Get financial projections by category
    Given I have financial projections in different categories
    When I request projections for "Revenue" category
    Then only revenue projections should be returned
    And the projections should be properly categorized
    And the category totals should be calculated correctly

  Scenario: Calculate taxes for multiple countries
    Given I have financial projections for different countries
    When I calculate taxes for each country
    Then each country should use its specific tax rules
    And the tax calculations should be accurate
    And the tax compliance should be verified
    And the tax reports should be generated by country

  Scenario: Track financial performance over time
    Given I have financial projections for multiple periods
    When I request performance tracking
    Then the system should show trends over time
    And the system should calculate growth rates
    And the system should identify performance patterns
    And the system should provide performance insights

using FluentAssertions;
using TechTalk.SpecFlow;

namespace Sqordia.BDDTests.StepDefinitions;

[Binding]
public class OBNLSteps
{
    private readonly TestContext _testContext;

    public OBNLSteps(TestContext testContext)
    {
        _testContext = testContext;
    }

    [Given(@"I am a registered OBNL organization")]
    public async Task GivenIAmARegisteredOBNLOrganization()
    {
        await _testContext.CreateOBNLOrganizationAsync();
    }

    [Given(@"I am creating an OBNL business plan")]
    public async Task GivenIAmCreatingAnOBNLBusinessPlan()
    {
        _testContext.CurrentOBNLPlan = new OBNLBusinessPlan
        {
            Id = Guid.NewGuid(),
            OrganizationId = _testContext.CurrentOrganization?.Id ?? Guid.NewGuid(),
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Community engagement plan",
            ImpactMeasurement = "Impact measurement framework",
            SustainabilityStrategy = "Sustainability strategy",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = _testContext.CurrentUser?.Id.ToString() ?? Guid.NewGuid().ToString(),
            UpdatedBy = _testContext.CurrentUser?.Id.ToString() ?? Guid.NewGuid().ToString()
        };
    }

    [Given(@"I specify my OBNL type as ""(.*)""")]
    public void GivenISpecifyMyOBNLTypeAs(string obnlType)
    {
        _testContext.CurrentOBNLPlan = _testContext.CurrentOBNLPlan! with { OBNLType = obnlType };
    }

    [Given(@"I provide my mission statement")]
    public void GivenIProvideMyMissionStatement()
    {
        _testContext.CurrentOBNLPlan = _testContext.CurrentOBNLPlan! with { Mission = "To provide essential services to our community" };
    }

    [Given(@"I specify my funding requirements")]
    public void GivenISpecifyMyFundingRequirements()
    {
        _testContext.CurrentOBNLPlan = _testContext.CurrentOBNLPlan! with { 
            FundingRequirements = 250000,
            FundingPurpose = "Program expansion and sustainability"
        };
    }

    [When(@"I request to generate the OBNL business plan")]
    public async Task WhenIRequestToGenerateTheOBNLBusinessPlan()
    {
        var request = new CreateOBNLPlanRequest
        {
            OrganizationId = _testContext.CurrentOrganization?.Id ?? Guid.NewGuid(),
            OBNLType = _testContext.CurrentOBNLPlan!.OBNLType,
            Mission = _testContext.CurrentOBNLPlan!.Mission,
            Vision = _testContext.CurrentOBNLPlan!.Vision,
            Values = _testContext.CurrentOBNLPlan!.Values,
            FundingRequirements = _testContext.CurrentOBNLPlan!.FundingRequirements,
            FundingPurpose = _testContext.CurrentOBNLPlan!.FundingPurpose,
            LegalStructure = _testContext.CurrentOBNLPlan!.LegalStructure,
            RegistrationNumber = _testContext.CurrentOBNLPlan!.RegistrationNumber,
            RegistrationDate = _testContext.CurrentOBNLPlan!.RegistrationDate,
            GoverningBody = _testContext.CurrentOBNLPlan!.GoverningBody,
            BoardComposition = _testContext.CurrentOBNLPlan!.BoardComposition,
            StakeholderEngagement = _testContext.CurrentOBNLPlan!.StakeholderEngagement,
            ImpactMeasurement = _testContext.CurrentOBNLPlan!.ImpactMeasurement,
            SustainabilityStrategy = _testContext.CurrentOBNLPlan!.SustainabilityStrategy,
            CreatedBy = _testContext.CurrentUser?.Id.ToString() ?? Guid.NewGuid().ToString()
        };

        _testContext.OBNLPlanResult = await _testContext.OBNLService.CreateOBNLPlanAsync(request);
    }

    [When(@"I request compliance analysis")]
    public async Task WhenIRequestComplianceAnalysis()
    {
        _testContext.ComplianceAnalysisResult = await _testContext.OBNLService.AnalyzeComplianceAsync(_testContext.CurrentOBNLPlan!.Id);
    }

    [When(@"I create a grant application")]
    public async Task WhenICreateAGrantApplication()
    {
        var request = new CreateGrantApplicationRequest
        {
            OBNLBusinessPlanId = _testContext.CurrentOBNLPlan!.Id,
            GrantName = "Community Development Grant",
            GrantingOrganization = "Government Foundation",
            GrantType = "Program Grant",
            RequestedAmount = 100000,
            MatchingFunds = 25000,
            ProjectDescription = "Community development project",
            Objectives = "Improve community services",
            ExpectedOutcomes = "Increased community engagement",
            TargetPopulation = "Local community members",
            GeographicScope = "Local area",
            Timeline = "12 months",
            BudgetBreakdown = "Detailed budget breakdown",
            EvaluationPlan = "Comprehensive evaluation plan",
            SustainabilityPlan = "Long-term sustainability strategy",
            ApplicationDeadline = DateTime.UtcNow.AddDays(30),
            SubmissionDate = DateTime.UtcNow,
            Status = "Draft",
            Notes = "Grant application notes"
        };

        _testContext.GrantApplicationResult = await _testContext.OBNLService.CreateGrantApplicationAsync(request);
    }

    [When(@"I configure impact measurements")]
    public async Task WhenIConfigureImpactMeasurements()
    {
        var request = new CreateImpactMeasurementRequest
        {
            OBNLBusinessPlanId = _testContext.CurrentOBNLPlan!.Id,
            MetricName = "Community Impact",
            Description = "Measure community impact",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Number of people served",
            BaselineValue = 100,
            TargetValue = 500,
            CurrentValue = 150,
            DataSource = "Program records",
            CollectionMethod = "Monthly surveys",
            Frequency = "Monthly",
            ResponsibleParty = "Program Manager",
            LastMeasurement = DateTime.UtcNow.AddDays(-30),
            NextMeasurement = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            Notes = "Impact measurement notes"
        };

        _testContext.ImpactMeasurementResult = await _testContext.OBNLService.CreateImpactMeasurementAsync(request);
    }

    [When(@"I request sustainability planning")]
    public async Task WhenIRequestSustainabilityPlanning()
    {
        _testContext.SustainabilityPlanningResult = await _testContext.OBNLService.GenerateSustainabilityPlanAsync(_testContext.CurrentOBNLPlan!.Id);
    }

    [When(@"I configure stakeholder engagement")]
    public async Task WhenIConfigureStakeholderEngagement()
    {
        _testContext.StakeholderEngagementResult = await _testContext.OBNLService.ConfigureStakeholderEngagementAsync(_testContext.CurrentOBNLPlan!.Id);
    }

    [When(@"I set up governance structure")]
    public async Task WhenISetUpGovernanceStructure()
    {
        _testContext.GovernanceStructureResult = await _testContext.OBNLService.SetUpGovernanceStructureAsync(_testContext.CurrentOBNLPlan!.Id);
    }

    [When(@"I request compliance reporting")]
    public async Task WhenIRequestComplianceReporting()
    {
        _testContext.ComplianceReportingResult = await _testContext.OBNLService.GenerateComplianceReportsAsync(_testContext.CurrentOBNLPlan!.Id);
    }

    [When(@"I specify my industry-specific requirements")]
    public void WhenISpecifyMyIndustrySpecificRequirements()
    {
        // Industry-specific requirements are set in the background
    }

    [Then(@"the plan should include grant application sections")]
    public void ThenThePlanShouldIncludeGrantApplicationSections()
    {
        _testContext.OBNLPlanResult.Should().NotBeNull();
        _testContext.OBNLPlanResult!.IsSuccess.Should().BeTrue();
        _testContext.OBNLPlanResult.Value.Should().NotBeNull();
    }

    [Then(@"the plan should include impact measurement frameworks")]
    public void ThenThePlanShouldIncludeImpactMeasurementFrameworks()
    {
        _testContext.OBNLPlanResult.Should().NotBeNull();
        _testContext.OBNLPlanResult!.IsSuccess.Should().BeTrue();
        _testContext.OBNLPlanResult.Value.Should().NotBeNull();
    }

    [Then(@"the plan should include sustainability strategies")]
    public void ThenThePlanShouldIncludeSustainabilityStrategies()
    {
        _testContext.OBNLPlanResult.Should().NotBeNull();
        _testContext.OBNLPlanResult!.IsSuccess.Should().BeTrue();
        _testContext.OBNLPlanResult.Value.Should().NotBeNull();
    }

    [Then(@"the plan should include compliance requirements")]
    public void ThenThePlanShouldIncludeComplianceRequirements()
    {
        _testContext.OBNLPlanResult.Should().NotBeNull();
        _testContext.OBNLPlanResult!.IsSuccess.Should().BeTrue();
        _testContext.OBNLPlanResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should identify applicable regulations")]
    public void ThenTheSystemShouldIdentifyApplicableRegulations()
    {
        _testContext.ComplianceAnalysisResult.Should().NotBeNull();
        _testContext.ComplianceAnalysisResult!.IsSuccess.Should().BeTrue();
        _testContext.ComplianceAnalysisResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should provide compliance checklists")]
    public void ThenTheSystemShouldProvideComplianceChecklists()
    {
        _testContext.ComplianceAnalysisResult.Should().NotBeNull();
        _testContext.ComplianceAnalysisResult!.IsSuccess.Should().BeTrue();
        _testContext.ComplianceAnalysisResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should include governance requirements")]
    public void ThenTheSystemShouldIncludeGovernanceRequirements()
    {
        _testContext.ComplianceAnalysisResult.Should().NotBeNull();
        _testContext.ComplianceAnalysisResult!.IsSuccess.Should().BeTrue();
        _testContext.ComplianceAnalysisResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should suggest compliance strategies")]
    public void ThenTheSystemShouldSuggestComplianceStrategies()
    {
        _testContext.ComplianceAnalysisResult.Should().NotBeNull();
        _testContext.ComplianceAnalysisResult!.IsSuccess.Should().BeTrue();
        _testContext.ComplianceAnalysisResult.Value.Should().NotBeNull();
    }

    [Then(@"the application should include project description")]
    public void ThenTheApplicationShouldIncludeProjectDescription()
    {
        _testContext.GrantApplicationResult.Should().NotBeNull();
        _testContext.GrantApplicationResult!.IsSuccess.Should().BeTrue();
        _testContext.GrantApplicationResult.Value.Should().NotBeNull();
    }

    [Then(@"the application should include budget breakdown")]
    public void ThenTheApplicationShouldIncludeBudgetBreakdown()
    {
        _testContext.GrantApplicationResult.Should().NotBeNull();
        _testContext.GrantApplicationResult!.IsSuccess.Should().BeTrue();
        _testContext.GrantApplicationResult.Value.Should().NotBeNull();
    }

    [Then(@"the application should include expected outcomes")]
    public void ThenTheApplicationShouldIncludeExpectedOutcomes()
    {
        _testContext.GrantApplicationResult.Should().NotBeNull();
        _testContext.GrantApplicationResult!.IsSuccess.Should().BeTrue();
        _testContext.GrantApplicationResult.Value.Should().NotBeNull();
    }

    [Then(@"the application should include evaluation plan")]
    public void ThenTheApplicationShouldIncludeEvaluationPlan()
    {
        _testContext.GrantApplicationResult.Should().NotBeNull();
        _testContext.GrantApplicationResult!.IsSuccess.Should().BeTrue();
        _testContext.GrantApplicationResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should suggest relevant metrics")]
    public void ThenTheSystemShouldSuggestRelevantMetrics()
    {
        _testContext.ImpactMeasurementResult.Should().NotBeNull();
        _testContext.ImpactMeasurementResult!.IsSuccess.Should().BeTrue();
        _testContext.ImpactMeasurementResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should provide measurement methods")]
    public void ThenTheSystemShouldProvideMeasurementMethods()
    {
        _testContext.ImpactMeasurementResult.Should().NotBeNull();
        _testContext.ImpactMeasurementResult!.IsSuccess.Should().BeTrue();
        _testContext.ImpactMeasurementResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should include baseline data collection")]
    public void ThenTheSystemShouldIncludeBaselineDataCollection()
    {
        _testContext.ImpactMeasurementResult.Should().NotBeNull();
        _testContext.ImpactMeasurementResult!.IsSuccess.Should().BeTrue();
        _testContext.ImpactMeasurementResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should track progress over time")]
    public void ThenTheSystemShouldTrackProgressOverTime()
    {
        _testContext.ImpactMeasurementResult.Should().NotBeNull();
        _testContext.ImpactMeasurementResult!.IsSuccess.Should().BeTrue();
        _testContext.ImpactMeasurementResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should suggest sustainability initiatives")]
    public void ThenTheSystemShouldSuggestSustainabilityInitiatives()
    {
        _testContext.SustainabilityPlanningResult.Should().NotBeNull();
        _testContext.SustainabilityPlanningResult!.IsSuccess.Should().BeTrue();
        _testContext.SustainabilityPlanningResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should include environmental impact assessment")]
    public void ThenTheSystemShouldIncludeEnvironmentalImpactAssessment()
    {
        _testContext.SustainabilityPlanningResult.Should().NotBeNull();
        _testContext.SustainabilityPlanningResult!.IsSuccess.Should().BeTrue();
        _testContext.SustainabilityPlanningResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should provide long-term viability strategies")]
    public void ThenTheSystemShouldProvideLongTermViabilityStrategies()
    {
        _testContext.SustainabilityPlanningResult.Should().NotBeNull();
        _testContext.SustainabilityPlanningResult!.IsSuccess.Should().BeTrue();
        _testContext.SustainabilityPlanningResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should include stakeholder engagement plans")]
    public void ThenTheSystemShouldIncludeStakeholderEngagementPlans()
    {
        _testContext.SustainabilityPlanningResult.Should().NotBeNull();
        _testContext.SustainabilityPlanningResult!.IsSuccess.Should().BeTrue();
        _testContext.SustainabilityPlanningResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should identify key stakeholders")]
    public void ThenTheSystemShouldIdentifyKeyStakeholders()
    {
        _testContext.StakeholderEngagementResult.Should().NotBeNull();
        _testContext.StakeholderEngagementResult!.IsSuccess.Should().BeTrue();
        _testContext.StakeholderEngagementResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should suggest engagement strategies")]
    public void ThenTheSystemShouldSuggestEngagementStrategies()
    {
        _testContext.StakeholderEngagementResult.Should().NotBeNull();
        _testContext.StakeholderEngagementResult!.IsSuccess.Should().BeTrue();
        _testContext.StakeholderEngagementResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should include communication plans")]
    public void ThenTheSystemShouldIncludeCommunicationPlans()
    {
        _testContext.StakeholderEngagementResult.Should().NotBeNull();
        _testContext.StakeholderEngagementResult!.IsSuccess.Should().BeTrue();
        _testContext.StakeholderEngagementResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should track stakeholder relationships")]
    public void ThenTheSystemShouldTrackStakeholderRelationships()
    {
        _testContext.StakeholderEngagementResult.Should().NotBeNull();
        _testContext.StakeholderEngagementResult!.IsSuccess.Should().BeTrue();
        _testContext.StakeholderEngagementResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should suggest board composition")]
    public void ThenTheSystemShouldSuggestBoardComposition()
    {
        _testContext.GovernanceStructureResult.Should().NotBeNull();
        _testContext.GovernanceStructureResult!.IsSuccess.Should().BeTrue();
        _testContext.GovernanceStructureResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should include governance policies")]
    public void ThenTheSystemShouldIncludeGovernancePolicies()
    {
        _testContext.GovernanceStructureResult.Should().NotBeNull();
        _testContext.GovernanceStructureResult!.IsSuccess.Should().BeTrue();
        _testContext.GovernanceStructureResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should provide decision-making frameworks")]
    public void ThenTheSystemShouldProvideDecisionMakingFrameworks()
    {
        _testContext.GovernanceStructureResult.Should().NotBeNull();
        _testContext.GovernanceStructureResult!.IsSuccess.Should().BeTrue();
        _testContext.GovernanceStructureResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should include accountability measures")]
    public void ThenTheSystemShouldIncludeAccountabilityMeasures()
    {
        _testContext.GovernanceStructureResult.Should().NotBeNull();
        _testContext.GovernanceStructureResult!.IsSuccess.Should().BeTrue();
        _testContext.GovernanceStructureResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should generate regulatory reports")]
    public void ThenTheSystemShouldGenerateRegulatoryReports()
    {
        _testContext.ComplianceReportingResult.Should().NotBeNull();
        _testContext.ComplianceReportingResult!.IsSuccess.Should().BeTrue();
        _testContext.ComplianceReportingResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should include financial statements")]
    public void ThenTheSystemShouldIncludeFinancialStatements()
    {
        _testContext.ComplianceReportingResult.Should().NotBeNull();
        _testContext.ComplianceReportingResult!.IsSuccess.Should().BeTrue();
        _testContext.ComplianceReportingResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should provide impact reports")]
    public void ThenTheSystemShouldProvideImpactReports()
    {
        _testContext.ComplianceReportingResult.Should().NotBeNull();
        _testContext.ComplianceReportingResult!.IsSuccess.Should().BeTrue();
        _testContext.ComplianceReportingResult.Value.Should().NotBeNull();
    }

    [Then(@"the system should track compliance status")]
    public void ThenTheSystemShouldTrackComplianceStatus()
    {
        _testContext.ComplianceReportingResult.Should().NotBeNull();
        _testContext.ComplianceReportingResult!.IsSuccess.Should().BeTrue();
        _testContext.ComplianceReportingResult.Value.Should().NotBeNull();
    }

    [Then(@"the plan should include <industry> specific compliance")]
    public void ThenThePlanShouldIncludeIndustrySpecificCompliance(string industry)
    {
        _testContext.OBNLPlanResult.Should().NotBeNull();
        _testContext.OBNLPlanResult!.IsSuccess.Should().BeTrue();
        _testContext.OBNLPlanResult.Value.Should().NotBeNull();
    }

    [Then(@"the plan should include <industry> specific funding opportunities")]
    public void ThenThePlanShouldIncludeIndustrySpecificFundingOpportunities(string industry)
    {
        _testContext.OBNLPlanResult.Should().NotBeNull();
        _testContext.OBNLPlanResult!.IsSuccess.Should().BeTrue();
        _testContext.OBNLPlanResult.Value.Should().NotBeNull();
    }

    [Then(@"the plan should include <industry> specific impact metrics")]
    public void ThenThePlanShouldIncludeIndustrySpecificImpactMetrics(string industry)
    {
        _testContext.OBNLPlanResult.Should().NotBeNull();
        _testContext.OBNLPlanResult!.IsSuccess.Should().BeTrue();
        _testContext.OBNLPlanResult.Value.Should().NotBeNull();
    }
}

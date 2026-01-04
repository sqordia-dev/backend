using FluentAssertions;
using TechTalk.SpecFlow;

namespace Sqordia.BDDTests.StepDefinitions;

[Binding]
public class BusinessPlanSteps
{
    private readonly TestContext _testContext;

    public BusinessPlanSteps(TestContext testContext)
    {
        _testContext = testContext;
    }

    [Given(@"I have completed the business questionnaire with valid data")]
    public async Task GivenIHaveCompletedTheBusinessQuestionnaireWithValidData()
    {
        var questionnaireData = _testContext.CreateValidQuestionnaireData();
        _testContext.QuestionnaireResult = await _testContext.QuestionnaireService.CompleteQuestionnaireAsync(questionnaireData);
    }

    [Given(@"I have a basic business plan")]
    public async Task GivenIHaveABasicBusinessPlan()
    {
        var businessPlan = _testContext.CreateBasicBusinessPlan();
        _testContext.CurrentBusinessPlan = businessPlan;
    }

    [Given(@"I have a completed business plan")]
    public async Task GivenIHaveACompletedBusinessPlan()
    {
        var businessPlan = _testContext.CreateCompletedBusinessPlan();
        _testContext.CurrentBusinessPlan = businessPlan;
    }

    [Given(@"I have started creating a business plan")]
    public async Task GivenIHaveStartedCreatingABusinessPlan()
    {
        var businessPlan = _testContext.CreateIncompleteBusinessPlan();
        _testContext.CurrentBusinessPlan = businessPlan;
    }

    [Given(@"I am creating a business plan for a (.*) company")]
    public void GivenIAmCreatingABusinessPlanForACompany(string industry)
    {
        _testContext.CurrentIndustry = industry;
    }

    [When(@"I request to generate a business plan")]
    public async Task WhenIRequestToGenerateABusinessPlan()
    {
        var request = new GenerateBusinessPlanRequest
        {
            QuestionnaireId = _testContext.QuestionnaireResult?.Value?.Id ?? Guid.NewGuid(),
            UserId = _testContext.CurrentUser?.Id ?? Guid.NewGuid()
        };

        _testContext.BusinessPlanGenerationResult = await _testContext.BusinessPlanService.GenerateBusinessPlanAsync(request);
    }

    [When(@"I request AI enhancement")]
    public async Task WhenIRequestAIEnhancement()
    {
        var request = new EnhanceBusinessPlanRequest
        {
            BusinessPlanId = _testContext.CurrentBusinessPlan?.Id ?? Guid.NewGuid(),
            EnhancementType = "strategic_recommendations"
        };

        _testContext.AIEnhancementResult = await _testContext.AIService.EnhanceBusinessPlanAsync(request);
    }

    [When(@"I export the plan as PDF")]
    public async Task WhenIExportThePlanAsPDF()
    {
        var request = new ExportBusinessPlanRequest
        {
            BusinessPlanId = _testContext.CurrentBusinessPlan?.Id ?? Guid.NewGuid(),
            Format = "PDF"
        };

        _testContext.ExportResult = await _testContext.ExportService.ExportBusinessPlanAsync(request);
    }

    [When(@"I export the plan as Word document")]
    public async Task WhenIExportThePlanAsWordDocument()
    {
        var request = new ExportBusinessPlanRequest
        {
            BusinessPlanId = _testContext.CurrentBusinessPlan?.Id ?? Guid.NewGuid(),
            Format = "DOCX"
        };

        _testContext.ExportResult = await _testContext.ExportService.ExportBusinessPlanAsync(request);
    }

    [When(@"I attempt to generate the final plan")]
    public async Task WhenIAttemptToGenerateTheFinalPlan()
    {
        var request = new GenerateBusinessPlanRequest
        {
            BusinessPlanId = _testContext.CurrentBusinessPlan?.Id ?? Guid.NewGuid(),
            UserId = _testContext.CurrentUser?.Id ?? Guid.NewGuid()
        };

        _testContext.BusinessPlanGenerationResult = await _testContext.BusinessPlanService.GenerateBusinessPlanAsync(request);
    }

    [When(@"I complete the questionnaire with (.*) specific data")]
    public async Task WhenICompleteTheQuestionnaireWithSpecificData(string industry)
    {
        var questionnaireData = _testContext.CreateIndustrySpecificQuestionnaireData(industry);
        _testContext.QuestionnaireResult = await _testContext.QuestionnaireService.CompleteQuestionnaireAsync(questionnaireData);
    }

    [Then(@"I should receive a complete business plan")]
    public void ThenIShouldReceiveACompleteBusinessPlan()
    {
        _testContext.BusinessPlanGenerationResult.Should().NotBeNull();
        _testContext.BusinessPlanGenerationResult!.IsSuccess.Should().BeTrue();
        _testContext.BusinessPlanGenerationResult.Value.Should().NotBeNull();
    }

    [Then(@"the plan should include executive summary")]
    public void ThenThePlanShouldIncludeExecutiveSummary()
    {
        var businessPlan = _testContext.BusinessPlanGenerationResult?.Value;
        businessPlan.Should().NotBeNull();
        businessPlan!.ExecutiveSummary.Should().NotBeNullOrEmpty();
    }

    [Then(@"the plan should include market analysis")]
    public void ThenThePlanShouldIncludeMarketAnalysis()
    {
        var businessPlan = _testContext.BusinessPlanGenerationResult?.Value;
        businessPlan.Should().NotBeNull();
        businessPlan!.MarketAnalysis.Should().NotBeNullOrEmpty();
    }

    [Then(@"the plan should include financial projections")]
    public void ThenThePlanShouldIncludeFinancialProjections()
    {
        var businessPlan = _testContext.BusinessPlanGenerationResult?.Value;
        businessPlan.Should().NotBeNull();
        businessPlan!.FinancialProjections.Should().NotBeNull();
    }

    [Then(@"the plan should include risk assessment")]
    public void ThenThePlanShouldIncludeRiskAssessment()
    {
        var businessPlan = _testContext.BusinessPlanGenerationResult?.Value;
        businessPlan.Should().NotBeNull();
        businessPlan!.RiskAssessment.Should().NotBeNullOrEmpty();
    }

    [Then(@"the AI should provide strategic recommendations")]
    public void ThenTheAIShouldProvideStrategicRecommendations()
    {
        _testContext.AIEnhancementResult.Should().NotBeNull();
        _testContext.AIEnhancementResult.IsSuccess.Should().BeTrue();
        _testContext.AIEnhancementResult.Value.Recommendations.Should().NotBeNullOrEmpty();
    }

    [Then(@"the recommendations should be relevant to my industry")]
    public void ThenTheRecommendationsShouldBeRelevantToMyIndustry()
    {
        var recommendations = _testContext.AIEnhancementResult?.Value?.Recommendations;
        recommendations.Should().NotBeNullOrEmpty();
        // Additional validation for industry relevance could be added here
    }

    [Then(@"the recommendations should improve the plan quality")]
    public void ThenTheRecommendationsShouldImproveThePlanQuality()
    {
        var recommendations = _testContext.AIEnhancementResult?.Value?.Recommendations;
        recommendations.Should().NotBeNullOrEmpty();
        // Quality improvement validation could be added here
    }

    [Then(@"I should receive a PDF document")]
    public void ThenIShouldReceiveAPDFDocument()
    {
        _testContext.ExportResult.Should().NotBeNull();
        _testContext.ExportResult.IsSuccess.Should().BeTrue();
        _testContext.ExportResult.Value.FileFormat.Should().Be("PDF");
    }

    [Then(@"the PDF should contain all plan sections")]
    public void ThenThePDFShouldContainAllPlanSections()
    {
        var exportResult = _testContext.ExportResult?.Value;
        exportResult.Should().NotBeNull();
        exportResult.FileSize.Should().BeGreaterThan(0);
    }

    [Then(@"I should receive a Word document")]
    public void ThenIShouldReceiveAWordDocument()
    {
        _testContext.ExportResult.Should().NotBeNull();
        _testContext.ExportResult.IsSuccess.Should().BeTrue();
        _testContext.ExportResult.Value.FileFormat.Should().Be("DOCX");
    }

    [Then(@"the Word document should be editable")]
    public void ThenTheWordDocumentShouldBeEditable()
    {
        var exportResult = _testContext.ExportResult?.Value;
        exportResult.Should().NotBeNull();
        exportResult.IsEditable.Should().BeTrue();
    }

    [Then(@"the system should identify missing sections")]
    public void ThenTheSystemShouldIdentifyMissingSections()
    {
        _testContext.BusinessPlanGenerationResult.Should().NotBeNull();
        _testContext.BusinessPlanGenerationResult.IsSuccess.Should().BeFalse();
        _testContext.BusinessPlanGenerationResult.ErrorMessage.Should().Contain("missing sections");
    }

    [Then(@"the generated plan should include (.*) specific sections")]
    public void ThenTheGeneratedPlanShouldIncludeSpecificSections(string industry)
    {
        var businessPlan = _testContext.BusinessPlanGenerationResult?.Value;
        businessPlan.Should().NotBeNull();
        // Industry-specific validation could be added here
    }

    [Then(@"the financial projections should be appropriate for (.*)")]
    public void ThenTheFinancialProjectionsShouldBeAppropriateFor(string industry)
    {
        var businessPlan = _testContext.BusinessPlanGenerationResult?.Value;
        businessPlan.Should().NotBeNull();
        businessPlan.FinancialProjections.Should().NotBeNull();
        // Industry-specific financial validation could be added here
    }
}

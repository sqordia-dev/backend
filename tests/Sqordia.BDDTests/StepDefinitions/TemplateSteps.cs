using TechTalk.SpecFlow;

namespace Sqordia.BDDTests.StepDefinitions;

[Binding]
public class TemplateSteps
{
    private readonly TestContext _testContext;

    public TemplateSteps(TestContext testContext)
    {
        _testContext = testContext;
    }

    [Given(@"I want to create a template for ""([^""]*)""")]
    public void GivenIWantToCreateATemplateFor(string templateName)
    {
        _testContext.CurrentTemplateName = templateName;
    }

    [When(@"I create a template with the following details:")]
    public void WhenICreateATemplateWithTheFollowingDetails(Table table)
    {
        var templateData = table.Rows[0];
        var request = new CreateTemplateRequest
        {
            Name = templateData["Name"],
            Description = templateData["Description"],
            Category = templateData["Category"],
            Type = templateData["Type"],
            Industry = templateData["Industry"],
            Language = templateData["Language"],
            Country = templateData["Country"]
        };

        _testContext.TemplateResult = _testContext.TemplateService.CreateTemplateAsync(request).Result;
    }

    [Then(@"the template should be created successfully")]
    public void ThenTheTemplateShouldBeCreatedSuccessfully()
    {
        _testContext.TemplateResult!.IsSuccess.Should().BeTrue();
        _testContext.TemplateResult.Value.Should().NotBeNull();
    }

    [Then(@"I should see the template in my templates list")]
    public void ThenIShouldSeeTheTemplateInMyTemplatesList()
    {
        var templates = _testContext.TemplateService.GetUserTemplatesAsync().Result;
        templates.IsSuccess.Should().BeTrue();
        templates.Value.Should().Contain(t => t.Name == _testContext.CurrentTemplateName);
    }

    [Given(@"there are templates available in the system")]
    public void GivenThereAreTemplatesAvailableInTheSystem()
    {
        // Create some sample templates
        var techTemplate = new CreateTemplateRequest
        {
            Name = "Technology Startup Template",
            Description = "Template for tech startups",
            Category = "BusinessPlan",
            Type = "IndustrySpecific",
            Industry = "Technology"
        };

        var healthcareTemplate = new CreateTemplateRequest
        {
            Name = "Healthcare Business Template",
            Description = "Template for healthcare businesses",
            Category = "BusinessPlan",
            Type = "IndustrySpecific",
            Industry = "Healthcare"
        };

        _testContext.TemplateService.CreateTemplateAsync(techTemplate);
        _testContext.TemplateService.CreateTemplateAsync(healthcareTemplate);
    }

    [When(@"I browse templates by category ""([^""]*)""")]
    public void WhenIBrowseTemplatesByCategory(string category)
    {
        _testContext.TemplateCategoryResult = _testContext.TemplateService.GetTemplatesByCategoryAsync(category).Result;
    }

    [Then(@"I should see all business plan templates")]
    public void ThenIShouldSeeAllBusinessPlanTemplates()
    {
        _testContext.TemplateCategoryResult!.IsSuccess.Should().BeTrue();
        _testContext.TemplateCategoryResult.Value.Should().NotBeEmpty();
    }

    [Then(@"each template should show its name, description, and rating")]
    public void ThenEachTemplateShouldShowItsNameDescriptionAndRating()
    {
        foreach (var template in _testContext.TemplateCategoryResult!.Value!)
        {
            template.Name.Should().NotBeNullOrEmpty();
            template.Description.Should().NotBeNullOrEmpty();
            template.Rating.Should().BeGreaterOrEqualTo(0);
        }
    }

    [When(@"I search for templates in the ""([^""]*)"" industry")]
    public void WhenISearchForTemplatesInTheIndustry(string industry)
    {
        _testContext.TemplateSearchResult = _testContext.TemplateService.SearchTemplatesByIndustryAsync(industry).Result;
    }

    [Then(@"I should see all healthcare-related templates")]
    public void ThenIShouldSeeAllHealthcareRelatedTemplates()
    {
        _testContext.TemplateSearchResult!.IsSuccess.Should().BeTrue();
        _testContext.TemplateSearchResult.Value.Should().NotBeEmpty();
    }

    [Then(@"the results should be relevant to healthcare")]
    public void ThenTheResultsShouldBeRelevantToHealthcare()
    {
        foreach (var template in _testContext.TemplateSearchResult!.Value!)
        {
            template.Industry.Should().Be("Healthcare");
        }
    }

    [Given(@"there are public templates available")]
    public void GivenThereArePublicTemplatesAvailable()
    {
        var publicTemplate = new CreateTemplateRequest
        {
            Name = "Public Business Template",
            Description = "A public template for general use",
            Category = "BusinessPlan",
            Type = "Standard",
            IsPublic = true
        };

        _testContext.TemplateService.CreateTemplateAsync(publicTemplate);
    }

    [When(@"I select a public template")]
    public void WhenISelectAPublicTemplate()
    {
        var publicTemplates = _testContext.TemplateService.GetPublicTemplatesAsync().Result;
        _testContext.SelectedTemplate = publicTemplates.Value!.First();
    }

    [Then(@"I should be able to use the template")]
    public void ThenIShouldBeAbleToUseTheTemplate()
    {
        _testContext.SelectedTemplate.Should().NotBeNull();
        _testContext.SelectedTemplate!.IsPublic.Should().BeTrue();
    }

    [Then(@"the template usage should be recorded")]
    public void ThenTheTemplateUsageShouldBeRecorded()
    {
        var usageResult = _testContext.TemplateService.RecordTemplateUsageAsync(_testContext.SelectedTemplate!.Id, "Use").Result;
        usageResult.IsSuccess.Should().BeTrue();
    }

    [Given(@"I have used a template")]
    public void GivenIHaveUsedATemplate()
    {
        _testContext.SelectedTemplate = _testContext.TemplateService.GetPublicTemplatesAsync().Result.Value!.First();
    }

    [When(@"I rate the template with (\d+) stars and comment ""([^""]*)""")]
    public void WhenIRateTheTemplateWithStarsAndComment(int rating, string comment)
    {
        _testContext.RatingResult = _testContext.TemplateService.RateTemplateAsync(_testContext.SelectedTemplate!.Id, rating, comment).Result;
    }

    [Then(@"the rating should be recorded")]
    public void ThenTheRatingShouldBeRecorded()
    {
        _testContext.RatingResult!.IsSuccess.Should().BeTrue();
    }

    [Then(@"the template's average rating should be updated")]
    public void ThenTheTemplatesAverageRatingShouldBeUpdated()
    {
        var updatedTemplate = _testContext.TemplateService.GetTemplateByIdAsync(_testContext.SelectedTemplate!.Id).Result;
        updatedTemplate.IsSuccess.Should().BeTrue();
        updatedTemplate.Value!.Rating.Should().BeGreaterThan(0);
    }

    [Given(@"I have access to a template")]
    public void GivenIHaveAccessToATemplate()
    {
        _testContext.SelectedTemplate = _testContext.TemplateService.GetPublicTemplatesAsync().Result.Value!.First();
    }

    [When(@"I clone the template with a new name ""([^""]*)""")]
    public void WhenICloneTheTemplateWithANewName(string newName)
    {
        _testContext.CloneResult = _testContext.TemplateService.CloneTemplateAsync(_testContext.SelectedTemplate!.Id, newName).Result;
    }

    [Then(@"a new template should be created")]
    public void ThenANewTemplateShouldBeCreated()
    {
        _testContext.CloneResult!.IsSuccess.Should().BeTrue();
        _testContext.CloneResult.Value.Should().NotBeNull();
    }

    [Then(@"the new template should be based on the original template")]
    public void ThenTheNewTemplateShouldBeBasedOnTheOriginalTemplate()
    {
        _testContext.CloneResult!.Value!.Name.Should().NotBe(_testContext.SelectedTemplate!.Name);
        _testContext.CloneResult.Value.Description.Should().Be(_testContext.SelectedTemplate.Description);
    }

    [Then(@"I should be the owner of the new template")]
    public void ThenIShouldBeTheOwnerOfTheNewTemplate()
    {
        _testContext.CloneResult!.Value!.Author.Should().Be(_testContext.CurrentUser!.Email);
    }

    [When(@"I customize the template with my preferences")]
    public void WhenICustomizeTheTemplateWithMyPreferences()
    {
        var customization = new TemplateCustomizationRequest
        {
            TemplateId = _testContext.SelectedTemplate!.Id,
            Customizations = "{\"sections\": [{\"name\": \"Custom Section\", \"content\": \"Custom content\"}]}"
        };

        _testContext.CustomizationResult = _testContext.TemplateService.CustomizeTemplateAsync(customization).Result;
    }

    [Then(@"the customizations should be saved")]
    public void ThenTheCustomizationsShouldBeSaved()
    {
        _testContext.CustomizationResult!.IsSuccess.Should().BeTrue();
    }

    [Then(@"I should be able to use the customized template")]
    public void ThenIShouldBeAbleToUseTheCustomizedTemplate()
    {
        _testContext.CustomizationResult!.Value.Should().NotBeNull();
    }

    [Given(@"I am the owner of a template")]
    public void GivenIAmTheOwnerOfATemplate()
    {
        _testContext.OwnedTemplate = _testContext.TemplateService.GetUserTemplatesAsync().Result.Value!.First();
    }

    [When(@"I view the template analytics")]
    public void WhenIViewTheTemplateAnalytics()
    {
        _testContext.AnalyticsResult = _testContext.TemplateService.GetTemplateAnalyticsAsync(_testContext.OwnedTemplate!.Id).Result;
    }

    [Then(@"I should see usage statistics")]
    public void ThenIShouldSeeUsageStatistics()
    {
        _testContext.AnalyticsResult!.IsSuccess.Should().BeTrue();
        _testContext.AnalyticsResult.Value.Should().NotBeNull();
    }

    [Then(@"I should see rating information")]
    public void ThenIShouldSeeRatingInformation()
    {
        _testContext.AnalyticsResult!.Value!.AverageRating.Should().BeGreaterOrEqualTo(0);
    }

    [Then(@"I should see geographic distribution of users")]
    public void ThenIShouldSeeGeographicDistributionOfUsers()
    {
        _testContext.AnalyticsResult!.Value!.TopCountries.Should().NotBeEmpty();
    }

    [Given(@"I have created a template")]
    public void GivenIHaveCreatedATemplate()
    {
        _testContext.OwnedTemplate = _testContext.TemplateService.GetUserTemplatesAsync().Result.Value!.First();
    }

    [When(@"I publish the template")]
    public void WhenIPublishTheTemplate()
    {
        _testContext.PublishResult = _testContext.TemplateService.PublishTemplateAsync(_testContext.OwnedTemplate!.Id).Result;
    }

    [Then(@"the template should be available to other users")]
    public void ThenTheTemplateShouldBeAvailableToOtherUsers()
    {
        _testContext.PublishResult!.IsSuccess.Should().BeTrue();
        _testContext.PublishResult.Value!.IsPublic.Should().BeTrue();
    }

    [Then(@"the template status should be ""([^""]*)""")]
    public void ThenTheTemplateStatusShouldBe(string status)
    {
        _testContext.PublishResult!.Value!.Status.Should().Be(status);
    }

    [Given(@"I have a template that is no longer needed")]
    public void GivenIHaveATemplateThatIsNoLongerNeeded()
    {
        _testContext.OwnedTemplate = _testContext.TemplateService.GetUserTemplatesAsync().Result.Value!.First();
    }

    [When(@"I archive the template")]
    public void WhenIArchiveTheTemplate()
    {
        _testContext.ArchiveResult = _testContext.TemplateService.ArchiveTemplateAsync(_testContext.OwnedTemplate!.Id).Result;
    }

    [Then(@"the template should be hidden from public view")]
    public void ThenTheTemplateShouldBeHiddenFromPublicView()
    {
        _testContext.ArchiveResult!.IsSuccess.Should().BeTrue();
        _testContext.ArchiveResult.Value!.IsPublic.Should().BeFalse();
    }

    [Given(@"there are templates with different usage counts")]
    public void GivenThereAreTemplatesWithDifferentUsageCounts()
    {
        // Create templates with different usage counts
        var popularTemplate = new CreateTemplateRequest
        {
            Name = "Popular Template",
            Description = "A very popular template",
            Category = "BusinessPlan",
            Type = "Standard",
            IsPublic = true
        };

        _testContext.TemplateService.CreateTemplateAsync(popularTemplate);
    }

    [When(@"I request popular templates")]
    public void WhenIRequestPopularTemplates()
    {
        _testContext.PopularTemplatesResult = _testContext.TemplateService.GetPopularTemplatesAsync(10).Result;
    }

    [Then(@"I should see templates ordered by popularity")]
    public void ThenIShouldSeeTemplatesOrderedByPopularity()
    {
        _testContext.PopularTemplatesResult!.IsSuccess.Should().BeTrue();
        _testContext.PopularTemplatesResult.Value.Should().NotBeEmpty();
    }

    [Then(@"the most used templates should appear first")]
    public void ThenTheMostUsedTemplatesShouldAppearFirst()
    {
        var templates = _testContext.PopularTemplatesResult!.Value!;
        for (int i = 0; i < templates.Count - 1; i++)
        {
            templates[i].UsageCount.Should().BeGreaterOrEqualTo(templates[i + 1].UsageCount);
        }
    }

    [Given(@"there are templates created at different times")]
    public void GivenThereAreTemplatesCreatedAtDifferentTimes()
    {
        // Create templates at different times
        var recentTemplate = new CreateTemplateRequest
        {
            Name = "Recent Template",
            Description = "A recently created template",
            Category = "BusinessPlan",
            Type = "Standard",
            IsPublic = true
        };

        _testContext.TemplateService.CreateTemplateAsync(recentTemplate);
    }

    [When(@"I request recent templates")]
    public void WhenIRequestRecentTemplates()
    {
        _testContext.RecentTemplatesResult = _testContext.TemplateService.GetRecentTemplatesAsync(10).Result;
    }

    [Then(@"I should see templates ordered by creation date")]
    public void ThenIShouldSeeTemplatesOrderedByCreationDate()
    {
        _testContext.RecentTemplatesResult!.IsSuccess.Should().BeTrue();
        _testContext.RecentTemplatesResult.Value.Should().NotBeEmpty();
    }

    [Then(@"the most recently created templates should appear first")]
    public void ThenTheMostRecentlyCreatedTemplatesShouldAppearFirst()
    {
        var templates = _testContext.RecentTemplatesResult!.Value!;
        for (int i = 0; i < templates.Count - 1; i++)
        {
            templates[i].CreatedAt.Should().BeAfter(templates[i + 1].CreatedAt);
        }
    }

    [Given(@"there are templates from different authors")]
    public void GivenThereAreTemplatesFromDifferentAuthors()
    {
        // Create templates from different authors
        var authorTemplate = new CreateTemplateRequest
        {
            Name = "Author Template",
            Description = "A template from a specific author",
            Category = "BusinessPlan",
            Type = "Standard",
            Author = "John Doe",
            IsPublic = true
        };

        _testContext.TemplateService.CreateTemplateAsync(authorTemplate);
    }

    [When(@"I filter templates by author ""([^""]*)""")]
    public void WhenIFilterTemplatesByAuthor(string author)
    {
        _testContext.AuthorTemplatesResult = _testContext.TemplateService.GetTemplatesByAuthorAsync(author).Result;
    }

    [Then(@"I should see only templates created by ""([^""]*)""")]
    public void ThenIShouldSeeOnlyTemplatesCreatedBy(string author)
    {
        _testContext.AuthorTemplatesResult!.IsSuccess.Should().BeTrue();
        _testContext.AuthorTemplatesResult.Value.Should().NotBeEmpty();
    }

    [Then(@"the results should be relevant to that author")]
    public void ThenTheResultsShouldBeRelevantToThatAuthor()
    {
        foreach (var template in _testContext.AuthorTemplatesResult!.Value!)
        {
            template.Author.Should().Be("John Doe");
        }
    }
}

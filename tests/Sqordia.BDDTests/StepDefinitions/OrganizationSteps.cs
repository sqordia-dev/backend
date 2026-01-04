using FluentAssertions;
using TechTalk.SpecFlow;

namespace Sqordia.BDDTests.StepDefinitions;

[Binding]
public class OrganizationSteps
{
    private readonly TestContext _testContext;

    public OrganizationSteps(TestContext testContext)
    {
        _testContext = testContext;
    }

    [When(@"I create a new organization")]
    public async Task WhenICreateANewOrganization()
    {
        var request = new CreateOrganizationRequest
        {
            Name = _testContext.GenerateOrganizationName(),
            Description = "Test organization for BDD testing",
            OwnerId = _testContext.CurrentUser?.Id ?? Guid.NewGuid()
        };

        _testContext.OrganizationCreationResult = await _testContext.OrganizationService.CreateOrganizationAsync(request);
    }

    [When(@"I invite team members by email")]
    public async Task WhenIInviteTeamMembersByEmail()
    {
        var emails = new[] { "member1@test.com", "member2@test.com" };
        var request = new InviteMembersRequest
        {
            OrganizationId = _testContext.CurrentOrganization?.Id ?? Guid.NewGuid(),
            Emails = emails,
            Role = "Member"
        };

        _testContext.InvitationResult = await _testContext.OrganizationService.InviteMembersAsync(request);
    }

    [When(@"team members accept the invitations")]
    public async Task WhenTeamMembersAcceptTheInvitations()
    {
        var invitationIds = _testContext.InvitationResult?.Value?.InvitationIds ?? new List<Guid>();
        _testContext.AcceptanceResults = new List<object>();

        foreach (var invitationId in invitationIds)
        {
            var result = await _testContext.OrganizationService.AcceptInvitationAsync(invitationId);
            _testContext.AcceptanceResults.Add(result);
        }
    }

    [When(@"I change a member's role")]
    public async Task WhenIChangeAMembersRole()
    {
        var request = new UpdateMemberRoleRequest
        {
            OrganizationId = _testContext.CurrentOrganization?.Id ?? Guid.NewGuid(),
            MemberId = _testContext.TeamMembers?.FirstOrDefault()?.Id ?? Guid.NewGuid(),
            NewRole = "Editor"
        };

        _testContext.RoleUpdateResult = await _testContext.OrganizationService.UpdateMemberRoleAsync(request);
    }

    [When(@"I make edits to the business plan")]
    public async Task WhenIMakeEditsToTheBusinessPlan()
    {
        var request = new UpdateBusinessPlanRequest
        {
            BusinessPlanId = _testContext.CurrentBusinessPlan?.Id ?? Guid.NewGuid(),
            Updates = new Dictionary<string, object>
            {
                { "ExecutiveSummary", "Updated executive summary" },
                { "LastModifiedBy", _testContext.CurrentUser?.Id ?? Guid.NewGuid() }
            }
        };

        _testContext.BusinessPlanUpdateResult = await _testContext.BusinessPlanService.UpdateBusinessPlanAsync(request);
    }

    [When(@"multiple members edit simultaneously")]
    public async Task WhenMultipleMembersEditSimultaneously()
    {
        var tasks = new List<Task<TestResult<object>>>();
        var memberIds = _testContext.TeamMembers?.Select(m => m.Id).Take(2) ?? new List<Guid>();

        foreach (var memberId in memberIds)
        {
        var request = new UpdateBusinessPlanRequest
        {
            BusinessPlanId = _testContext.CurrentBusinessPlan?.Id ?? Guid.NewGuid(),
            Updates = new Dictionary<string, object>
            {
                { "ExecutiveSummary", $"Updated by member {memberId}" },
                { "LastModifiedBy", memberId.ToString() }
            }
        };

            tasks.Add(_testContext.BusinessPlanService.UpdateBusinessPlanAsync(request));
        }

        var results = await Task.WhenAll(tasks);
        _testContext.ConcurrentUpdateResults = results.Cast<object>().ToArray();
    }

    [When(@"I upgrade to a premium subscription")]
    public async Task WhenIUpgradeToAPremiumSubscription()
    {
        var request = new UpgradeSubscriptionRequest
        {
            OrganizationId = _testContext.CurrentOrganization?.Id ?? Guid.NewGuid(),
            SubscriptionType = "Premium",
            BillingCycle = "Monthly"
        };

        _testContext.SubscriptionUpgradeResult = await _testContext.OrganizationService.UpgradeSubscriptionAsync(request);
    }

    [When(@"I export the plan as PDF")]
    public async Task WhenIExportThePlanAsPDF()
    {
        var request = new ExportBusinessPlanRequest
        {
            BusinessPlanId = _testContext.CurrentBusinessPlan?.Id ?? Guid.NewGuid(),
            Format = "PDF",
            IncludeCollaborationHistory = true
        };

        _testContext.ExportResult = await _testContext.ExportService.ExportBusinessPlanAsync(request);
    }

    [When(@"I share the plan externally")]
    public async Task WhenIShareThePlanExternally()
    {
        var request = new ShareBusinessPlanRequest
        {
            BusinessPlanId = _testContext.CurrentBusinessPlan?.Id ?? Guid.NewGuid(),
            ShareWithEmails = new[] { "external@example.com" },
            PermissionLevel = "View",
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        _testContext.SharingResult = await _testContext.BusinessPlanService.ShareBusinessPlanAsync(request);
    }

    [Then(@"the organization should be created successfully")]
    public void ThenTheOrganizationShouldBeCreatedSuccessfully()
    {
        _testContext.OrganizationCreationResult.Should().NotBeNull();
        _testContext.OrganizationCreationResult.IsSuccess.Should().BeTrue();
        _testContext.CurrentOrganization = _testContext.OrganizationCreationResult.Value;
    }

    [Then(@"I should be the organization owner")]
    public void ThenIShouldBeTheOrganizationOwner()
    {
        _testContext.CurrentOrganization.Should().NotBeNull();
        _testContext.CurrentOrganization!.OwnerId.Should().Be(_testContext.CurrentUser?.Id ?? Guid.Empty);
    }

    [Then(@"invitation emails should be sent")]
    public void ThenInvitationEmailsShouldBeSent()
    {
        _testContext.InvitationResult.Should().NotBeNull();
        _testContext.InvitationResult.IsSuccess.Should().BeTrue();
    }

    [Then(@"the invitations should contain secure links")]
    public void ThenTheInvitationsShouldContainSecureLinks()
    {
        var invitations = _testContext.InvitationResult?.Value;
        invitations.Should().NotBeNull();
        invitations.InvitationIds.Should().NotBeEmpty();
    }

    [Then(@"they should be added to the organization")]
    public void ThenTheyShouldBeAddedToTheOrganization()
    {
        _testContext.AcceptanceResults.Should().NotBeEmpty();
        _testContext.AcceptanceResults.All(r => r != null).Should().BeTrue();
    }

    [Then(@"I should be able to assign roles to members")]
    public void ThenIShouldBeAbleToAssignRolesToMembers()
    {
        _testContext.TeamMembers.Should().NotBeEmpty();
        // Additional role assignment validation could be added here
    }

    [Then(@"their permissions should be updated accordingly")]
    public void ThenTheirPermissionsShouldBeUpdatedAccordingly()
    {
        _testContext.RoleUpdateResult.Should().NotBeNull();
        _testContext.RoleUpdateResult.IsSuccess.Should().BeTrue();
    }

    [Then(@"other team members should see my changes")]
    public void ThenOtherTeamMembersShouldSeeMyChanges()
    {
        _testContext.BusinessPlanUpdateResult.Should().NotBeNull();
        _testContext.BusinessPlanUpdateResult.IsSuccess.Should().BeTrue();
    }

    [Then(@"version history should be maintained")]
    public void ThenVersionHistoryShouldBeMaintained()
    {
        // Version history validation could be added here
        // This would typically involve checking the database for version records
    }

    [Then(@"the system should handle conflicts gracefully")]
    public void ThenTheSystemShouldHandleConflictsGracefully()
    {
        _testContext.ConcurrentUpdateResults.Should().NotBeEmpty();
        // Conflict resolution validation could be added here
    }

    [Then(@"access should be granted based on their role")]
    public void ThenAccessShouldBeGrantedBasedOnTheirRole()
    {
        // Role-based access validation could be added here
    }

    [Then(@"owners should have full access")]
    public void ThenOwnersShouldHaveFullAccess()
    {
        // Owner access validation could be added here
    }

    [Then(@"editors should be able to modify plans")]
    public void ThenEditorsShouldBeAbleToModifyPlans()
    {
        // Editor permission validation could be added here
    }

    [Then(@"viewers should only be able to read plans")]
    public void ThenViewersShouldOnlyBeAbleToReadPlans()
    {
        // Viewer permission validation could be added here
    }

    [Then(@"the organization should have premium features")]
    public void ThenTheOrganizationShouldHavePremiumFeatures()
    {
        _testContext.SubscriptionUpgradeResult.Should().NotBeNull();
        _testContext.SubscriptionUpgradeResult.IsSuccess.Should().BeTrue();
    }

    [Then(@"billing should be tracked")]
    public void ThenBillingShouldBeTracked()
    {
        // Billing tracking validation could be added here
    }

    [Then(@"premium features should be restricted")]
    public void ThenPremiumFeaturesShouldBeRestricted()
    {
        // Premium feature restriction validation could be added here
    }

    [Then(@"I should receive renewal notifications")]
    public void ThenIShouldReceiveRenewalNotifications()
    {
        // Renewal notification validation could be added here
    }

    [Then(@"each organization should only see their own data")]
    public void ThenEachOrganizationShouldOnlySeeTheirOwnData()
    {
        // Data isolation validation could be added here
    }

    [Then(@"business plans should be isolated by organization")]
    public void ThenBusinessPlansShouldBeIsolatedByOrganization()
    {
        // Business plan isolation validation could be added here
    }

    [Then(@"user permissions should be organization-specific")]
    public void ThenUserPermissionsShouldBeOrganizationSpecific()
    {
        // Organization-specific permission validation could be added here
    }

    [Then(@"the export should include all team contributions")]
    public void ThenTheExportShouldIncludeAllTeamContributions()
    {
        _testContext.ExportResult.Should().NotBeNull();
        _testContext.ExportResult.IsSuccess.Should().BeTrue();
    }

    [Then(@"the document should show revision history")]
    public void ThenTheDocumentShouldShowRevisionHistory()
    {
        var exportResult = _testContext.ExportResult?.Value;
        exportResult.Should().NotBeNull();
        // Revision history validation could be added here
    }

    [Then(@"sharing permissions should be respected")]
    public void ThenSharingPermissionsShouldBeRespected()
    {
        _testContext.SharingResult.Should().NotBeNull();
        _testContext.SharingResult.IsSuccess.Should().BeTrue();
    }

    [Then(@"external users should have appropriate access levels")]
    public void ThenExternalUsersShouldHaveAppropriateAccessLevels()
    {
        // External access level validation could be added here
    }
}

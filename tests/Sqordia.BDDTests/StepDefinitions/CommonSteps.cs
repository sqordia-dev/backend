using FluentAssertions;
using TechTalk.SpecFlow;

namespace Sqordia.BDDTests.StepDefinitions;

[Binding]
public class CommonSteps
{
    private readonly TestContext _testContext;

    public CommonSteps(TestContext testContext)
    {
        _testContext = testContext;
    }

    [Given(@"the application is running")]
    public void GivenTheApplicationIsRunning()
    {
        _testContext.ApplicationStarted = true;
    }

    [Given(@"I am a registered user")]
    public async Task GivenIAmARegisteredUser()
    {
        // Create a test user in the database
        await _testContext.CreateTestUserAsync();
    }

    [Given(@"I am a new user")]
    public void GivenIAmANewUser()
    {
        _testContext.IsNewUser = true;
    }

    [Given(@"I have a verified account")]
    public async Task GivenIHaveAVerifiedAccount()
    {
        await _testContext.CreateVerifiedUserAsync();
    }

    [Given(@"I am an organization owner")]
    public async Task GivenIAmAnOrganizationOwner()
    {
        await _testContext.CreateOrganizationOwnerAsync();
    }

    [Given(@"I am an organization member")]
    public async Task GivenIAmAnOrganizationMember()
    {
        await _testContext.CreateOrganizationMemberAsync();
    }

    [Given(@"the user registration system is available")]
    public void GivenTheUserRegistrationSystemIsAvailable()
    {
        _testContext.RegistrationSystemAvailable = true;
    }

    [Given(@"the AI service is available")]
    public void GivenTheAIServiceIsAvailable()
    {
        _testContext.AIServiceAvailable = true;
    }

    [When(@"I register with valid email and password")]
    public async Task WhenIRegisterWithValidEmailAndPassword()
    {
        var registrationRequest = _testContext.CreateValidRegistrationRequest();
        _testContext.RegistrationResult = await _testContext.AuthenticationService.RegisterAsync(registrationRequest);
    }

    [When(@"I log in with correct email and password")]
    public async Task WhenILogInWithCorrectEmailAndPassword()
    {
        var loginRequest = _testContext.CreateValidLoginRequest();
        _testContext.LoginResult = await _testContext.AuthenticationService.LoginAsync(loginRequest);
    }

    [When(@"I attempt to log in with incorrect password")]
    public async Task WhenIAttemptToLogInWithIncorrectPassword()
    {
        var loginRequest = _testContext.CreateInvalidLoginRequest();
        _testContext.LoginResult = await _testContext.AuthenticationService.LoginAsync(loginRequest);
    }

    [Then(@"my account should be created successfully")]
    public void ThenMyAccountShouldBeCreatedSuccessfully()
    {
        _testContext.RegistrationResult.Should().NotBeNull();
        _testContext.RegistrationResult!.IsSuccess.Should().BeTrue();
    }

    [Then(@"I should be authenticated successfully")]
    public void ThenIShouldBeAuthenticatedSuccessfully()
    {
        _testContext.LoginResult.Should().NotBeNull();
        _testContext.LoginResult!.IsSuccess.Should().BeTrue();
    }

    [Then(@"the login should fail")]
    public void ThenTheLoginShouldFail()
    {
        _testContext.LoginResult.Should().NotBeNull();
        _testContext.LoginResult!.IsSuccess.Should().BeFalse();
    }

    [Then(@"I should receive an error message")]
    public void ThenIShouldReceiveAnErrorMessage()
    {
        _testContext.LoginResult.Should().NotBeNull();
        _testContext.LoginResult!.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}

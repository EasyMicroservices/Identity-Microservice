using EasyMicroservices.IdentityMicroservice.Tests.Fixtures;
using EasyMicroservices.ServiceContracts;
using Identity.GeneratedServices;
using Microsoft.Extensions.DependencyInjection;

namespace EasyMicroservices.IdentityMicroservice.Tests;
public class LoginTests : IClassFixture<AuthenticationTestFixture>
{
    AuthenticationClient _authenticationClient;
    public LoginTests(AuthenticationTestFixture authenticationTestFixture)
    {
        _authenticationClient = authenticationTestFixture.ServiceProvider.GetService<AuthenticationClient>();
    }

    [Fact]
    public async Task LoginOwnerAsync()
    {
        await GetLoginOwnerAsync();
    }

    public async Task<string> GetLoginOwnerAsync()
    {
        var loginResult = await _authenticationClient.LoginByPersonalAccessTokenAsync(new LoginByPersonalAccessTokenRequestContract()
        {
            PersonalAccessToken = "ownerpat"
        }).AsCheckedResult(x => x.Result);
        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.Token);
        return loginResult.Token;
    }

    public async Task<string> GetLoginAsync(UserSummaryContract userSummaryContract)
    {
        var loginResult = await _authenticationClient.LoginAsync(userSummaryContract)
            .AsCheckedResult(x => x.Result);
        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.Token);
        return loginResult.Token;
    }
}

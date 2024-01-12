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
        await GetLoginOwnerAsync(_authenticationClient);
    }

    public static async Task<string> GetLoginOwnerAsync(AuthenticationClient authenticationClient)
    {
        var loginResult = await authenticationClient.LoginByPersonalAccessTokenAsync(new LoginByPersonalAccessTokenRequestContract()
        {
            PersonalAccessToken = "ownerpat"
        }).AsCheckedResult(x => x.Result);
        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.Token);
        return loginResult.Token;
    }

    public static async Task<string> GetLoginAsync(UserSummaryContract userSummaryContract, AuthenticationClient authenticationClient)
    {
        var loginResult = await authenticationClient.LoginAsync(userSummaryContract)
            .AsCheckedResult(x => x.Result);
        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.Token);
        return loginResult.Token;
    }
}

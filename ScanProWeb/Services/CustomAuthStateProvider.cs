using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ScanProWeb;

public class CustomAuthStateProvider(IJSRuntime js) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = null;
        try { token = await js.InvokeAsync<string?>("localStorage.getItem", "authToken"); }
        catch { token = null; }
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwt = null;
        try { jwt = handler.ReadJwtToken(token); }
        catch { return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())); }

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await js.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", "authToken");
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}
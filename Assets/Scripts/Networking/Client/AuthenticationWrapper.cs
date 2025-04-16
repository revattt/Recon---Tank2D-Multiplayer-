using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxRetries = 5)
    {
        if (AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }
        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already authenticated!");
            await Authenticating();
            return AuthState;
        }
        await SignInAnonymouslyAsync(maxRetries);
        return AuthState;
    }

    private static async Task<AuthState> Authenticating()
    {
        while (AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }
        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int maxRetries)
    {
        AuthState = AuthState.Authenticating;
        int retries = 0;

        while (AuthState == AuthState.Authenticating && retries < maxRetries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            catch (AuthenticationException authException)
            {
                Debug.LogError(authException);
                AuthState = AuthState.Error;
            }
            catch (RequestFailedException requestException)
            {
                Debug.LogError(requestException);
                AuthState = AuthState.Error;
            }
            retries++;
            await Task.Delay(1000);
        }

        if (AuthState != AuthState.Authenticated)
        {
            Debug.Log("Player not signed in successfully!");
            AuthState = AuthState.Timeout;
        }
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    Timeout
}
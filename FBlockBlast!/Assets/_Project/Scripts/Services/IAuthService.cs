using System;
using System.Threading.Tasks;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Result of an authentication attempt.
    /// </summary>
    public class AuthResult
    {
        public bool Success;
        public string ProviderId;     // "google", "apple", ""
        public string DisplayName;
        public string Email;
        public string AvatarUrl;
        public string ErrorMessage;

        public static AuthResult Failed(string error) => new AuthResult
        {
            Success = false,
            ErrorMessage = error
        };
    }

    /// <summary>
    /// Abstraction for authentication operations.
    /// Implementations can target Firebase Auth, Google Sign-In SDK, UGS Auth, etc.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Returns true if the player is currently signed in with a provider.
        /// </summary>
        bool IsSignedIn { get; }

        /// <summary>
        /// The current provider ID ("google", "apple", "" for anonymous).
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Display name from the linked account.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Email from the linked account.
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Avatar/profile picture URL from the linked account.
        /// </summary>
        string AvatarUrl { get; }

        /// <summary>
        /// Signs in with Google. Returns the result of the sign-in attempt.
        /// </summary>
        Task<AuthResult> SignInWithGoogleAsync();

        /// <summary>
        /// Signs in with Discord via OAuth2. Returns the result of the sign-in attempt.
        /// </summary>
        Task<AuthResult> SignInWithDiscordAsync();

        /// <summary>
        /// Links an email/password account. Returns the result of the link attempt.
        /// </summary>
        Task<AuthResult> LinkEmailAsync(string email, string password);

        /// <summary>
        /// Signs out from the current provider. Returns true on success.
        /// </summary>
        Task<bool> SignOutAsync();

        /// <summary>
        /// Signs out from a specific provider. Returns true on success.
        /// </summary>
        Task<bool> SignOutProviderAsync(string providerId);

        /// <summary>
        /// Fired when auth state changes (sign-in, sign-out).
        /// </summary>
        event Action<bool> OnAuthStateChanged;
    }
}

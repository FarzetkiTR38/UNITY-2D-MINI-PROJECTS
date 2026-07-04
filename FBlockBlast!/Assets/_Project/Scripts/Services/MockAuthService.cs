using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Mock implementation of IAuthService for development and testing.
    /// Simulates Google, Discord, and Email sign-in flows with configurable delays.
    /// Replace with FirebaseAuthService or UGS Auth wrapper for production.
    /// </summary>
    public class MockAuthService : IAuthService
    {
        private bool _isSignedIn;
        private string _providerId = "";
        private string _displayName = "";
        private string _email = "";
        private string _avatarUrl = "";

        public bool IsSignedIn => _isSignedIn;
        public string ProviderId => _providerId;
        public string DisplayName => _displayName;
        public string Email => _email;
        public string AvatarUrl => _avatarUrl;

        public event Action<bool> OnAuthStateChanged;

        /// <summary>
        /// Simulates a Google Sign-In with a 1.5 second delay.
        /// Always succeeds in mock mode.
        /// </summary>
        public async Task<AuthResult> SignInWithGoogleAsync()
        {
            Debug.Log("[MockAuthService] Simulating Google Sign-In...");

            // Simulate network delay
            await Task.Delay(1500);

            _isSignedIn = true;
            _providerId = "google";
            _displayName = "MockPlayer";
            _email = "mockplayer@gmail.com";
            _avatarUrl = "";

            Debug.Log($"[MockAuthService] Google sign-in successful: {_displayName} ({_email})");

            OnAuthStateChanged?.Invoke(true);

            return new AuthResult
            {
                Success = true,
                ProviderId = _providerId,
                DisplayName = _displayName,
                Email = _email,
                AvatarUrl = _avatarUrl
            };
        }

        /// <summary>
        /// Simulates a Discord OAuth2 Sign-In with a 2 second delay.
        /// Always succeeds in mock mode.
        /// </summary>
        public async Task<AuthResult> SignInWithDiscordAsync()
        {
            Debug.Log("[MockAuthService] Simulating Discord Sign-In...");

            await Task.Delay(2000);

            _isSignedIn = true;
            _providerId = "discord";
            _displayName = "MockUser#1234";
            _email = "mockuser@discord.com";
            _avatarUrl = "";

            Debug.Log($"[MockAuthService] Discord sign-in successful: {_displayName}");

            OnAuthStateChanged?.Invoke(true);

            return new AuthResult
            {
                Success = true,
                ProviderId = _providerId,
                DisplayName = _displayName,
                Email = _email,
                AvatarUrl = _avatarUrl
            };
        }

        /// <summary>
        /// Simulates an Email/Password link with a 1 second delay.
        /// Always succeeds in mock mode.
        /// </summary>
        public async Task<AuthResult> LinkEmailAsync(string email, string password)
        {
            Debug.Log($"[MockAuthService] Simulating Email link for: {email}...");

            await Task.Delay(1000);

            _isSignedIn = true;
            _providerId = "email";
            _displayName = email.Split('@')[0];
            _email = email;
            _avatarUrl = "";

            Debug.Log($"[MockAuthService] Email link successful: {email}");

            OnAuthStateChanged?.Invoke(true);

            return new AuthResult
            {
                Success = true,
                ProviderId = _providerId,
                DisplayName = _displayName,
                Email = email,
                AvatarUrl = _avatarUrl
            };
        }

        /// <summary>
        /// Simulates sign-out from all providers.
        /// </summary>
        public async Task<bool> SignOutAsync()
        {
            Debug.Log("[MockAuthService] Signing out...");

            await Task.Delay(300);

            _isSignedIn = false;
            _providerId = "";
            _displayName = "";
            _email = "";
            _avatarUrl = "";

            OnAuthStateChanged?.Invoke(false);

            Debug.Log("[MockAuthService] Signed out successfully.");
            return true;
        }

        /// <summary>
        /// Simulates sign-out from a specific provider.
        /// </summary>
        public async Task<bool> SignOutProviderAsync(string providerId)
        {
            Debug.Log($"[MockAuthService] Signing out from provider: {providerId}...");

            await Task.Delay(300);

            // In mock mode, only clear state if this was the active provider
            if (_providerId == providerId)
            {
                _isSignedIn = false;
                _providerId = "";
                _displayName = "";
                _email = "";
                _avatarUrl = "";
            }

            OnAuthStateChanged?.Invoke(false);

            Debug.Log($"[MockAuthService] Provider '{providerId}' disconnected.");
            return true;
        }
    }
}

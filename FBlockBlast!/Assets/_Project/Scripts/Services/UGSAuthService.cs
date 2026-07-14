using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Real implementation of IAuthService using Unity Gaming Services (UGS).
    /// Handles Anonymous and Email sign-in directly.
    /// Google/Discord require external native plugins to fetch tokens before passing to UGS.
    /// </summary>
    public class UGSAuthService : IAuthService
    {
        public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
        
        public string ProviderId 
        {
            get
            {
                if (!IsSignedIn) return "";
                // If the player has linked accounts, UGS returns them in PlayerInfo
                // For MVP, we simplify:
                return "anonymous"; 
            }
        }

        public string DisplayName => AuthenticationService.Instance.PlayerName;
        
        // Email and Avatar are not natively exposed without fetching PlayerInfo in UGS
        public string Email => "";
        public string AvatarUrl => "";

        public event Action<bool> OnAuthStateChanged;

        public UGSAuthService()
        {
            // Subscribe to UGS Auth events
            AuthenticationService.Instance.SignedIn += () => 
            {
                Debug.Log($"[UGSAuthService] Signed in! Player ID: {AuthenticationService.Instance.PlayerId}");
                OnAuthStateChanged?.Invoke(true);
            };
            
            AuthenticationService.Instance.SignedOut += () => 
            {
                Debug.Log("[UGSAuthService] Signed out!");
                OnAuthStateChanged?.Invoke(false);
            };
        }

        /// <summary>
        /// Authenticates the user anonymously using UGS.
        /// Call this during boot.
        /// </summary>
        public async Task<AuthResult> SignInAnonymouslyAsync()
        {
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log("[UGSAuthService] Attempting Anonymous Sign-in...");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                return new AuthResult
                {
                    Success = true,
                    ProviderId = "anonymous",
                    DisplayName = AuthenticationService.Instance.PlayerId
                };
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"[UGSAuthService] Auth error: {ex.Message}");
                return AuthResult.Failed(ex.Message);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"[UGSAuthService] Network error: {ex.Message}");
                return AuthResult.Failed(ex.Message);
            }
        }

        public async Task<AuthResult> SignInWithGoogleAsync()
        {
            Debug.Log("[UGSAuthService] Google Sign-In via Google Play Games requested.");
            
            var tcs = new TaskCompletionSource<AuthResult>();

            try
            {
                // Play Games v10.15 için yapılandırma (Server Auth Code istiyoruz)
                var config = new GooglePlayGames.BasicApi.PlayGamesClientConfiguration.Builder()
                    .RequestServerAuthCode(false)
                    .Build();
                PlayGamesPlatform.InitializeInstance(config);
                PlayGamesPlatform.Activate();

                PlayGamesPlatform.Instance.Authenticate(async (bool success) =>
                {
                    if (success)
                    {
                        Debug.Log("[UGSAuthService] Google Play Games giriş başarılı.");
                        
                        string authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                        
                        if (string.IsNullOrEmpty(authCode))
                        {
                            Debug.LogError("[UGSAuthService] Google Play Games'ten Auth Code alınamadı!");
                            tcs.SetResult(AuthResult.Failed("Auth code is null or empty."));
                            return;
                        }

                        try
                        {
                            // Alınan kodu Unity Gaming Services'e bağla
                            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(authCode);
                            Debug.Log("[UGSAuthService] UGS ile Google Play Games başarıyla bağlandı!");
                            
                            tcs.SetResult(new AuthResult
                            {
                                Success = true,
                                ProviderId = "google",
                                DisplayName = PlayGamesPlatform.Instance.GetUserDisplayName()
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[UGSAuthService] UGS Bağlantı Hatası: {ex.Message}");
                            tcs.SetResult(AuthResult.Failed(ex.Message));
                        }
                    }
                    else
                    {
                        Debug.LogError($"[UGSAuthService] Google Play Games giriş başarısız.");
                        tcs.SetResult(AuthResult.Failed("Play Games sign-in failed."));
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSAuthService] Google Login API hatası: {ex.Message}");
                tcs.SetResult(AuthResult.Failed(ex.Message));
            }

            return await tcs.Task;
        }

        public async Task<AuthResult> SignInWithDiscordAsync()
        {
            Debug.LogWarning("[UGSAuthService] Discord backend not implemented yet. Returning mock success.");
            await Task.Delay(1000);
            return new AuthResult { Success = true, ProviderId = "discord", DisplayName = "DiscordPlayer" };
        }

        public async Task<AuthResult> LinkEmailAsync(string email, string password)
        {
            try
            {
                Debug.Log($"[UGSAuthService] Attempting to link email: {email}");
                await AuthenticationService.Instance.AddUsernamePasswordAsync(email, password);
                
                return new AuthResult
                {
                    Success = true,
                    ProviderId = "email",
                    Email = email
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSAuthService] Email link failed: {ex.Message}");
                return AuthResult.Failed(ex.Message);
            }
        }

        public async Task<bool> SignOutAsync()
        {
            if (IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
                return true;
            }
            return false;
        }

        public async Task<bool> SignOutProviderAsync(string providerId)
        {
            try
            {
                if (providerId == "email")
                {
                    // UGS doesn't natively "unlink" username/password easily from client, 
                    // usually you just sign out completely.
                    AuthenticationService.Instance.SignOut();
                }
                else if (providerId == "google")
                {
                    await AuthenticationService.Instance.UnlinkGoogleAsync();
                }
                else
                {
                    Debug.LogWarning($"[UGSAuthService] Unlink for {providerId} is not implemented.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSAuthService] Unlink failed: {ex.Message}");
                return false;
            }
        }
    }
}

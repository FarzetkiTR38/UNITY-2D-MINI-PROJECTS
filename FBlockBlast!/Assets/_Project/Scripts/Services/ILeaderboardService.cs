using System.Threading.Tasks;
using NeonGalaxy.Data;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Abstraction for leaderboard operations.
    /// Implementations can target UGS, mock data, or custom backends.
    /// </summary>
    public interface ILeaderboardService
    {
        /// <summary>
        /// Returns true if the player is authenticated with the online service.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Returns true if the service is currently in an online state.
        /// </summary>
        bool IsOnline { get; }

        /// <summary>
        /// Authenticates the player with the online service.
        /// Returns true on success.
        /// </summary>
        Task<bool> AuthenticateAsync();

        /// <summary>
        /// Submits a score to the leaderboard.
        /// If offline, the score should be queued for later submission.
        /// Returns true if successfully submitted (or queued).
        /// </summary>
        Task<bool> SubmitScoreAsync(int score);

        /// <summary>
        /// Fetches the leaderboard data.
        /// Returns cached data if offline.
        /// </summary>
        Task<CachedLeaderboard> FetchLeaderboardAsync();

        /// <summary>
        /// Attempts to submit any pending scores that were queued while offline.
        /// </summary>
        Task FlushPendingSubmissionsAsync();
    }
}

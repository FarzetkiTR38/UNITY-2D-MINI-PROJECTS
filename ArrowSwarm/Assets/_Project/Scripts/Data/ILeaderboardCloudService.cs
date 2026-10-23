namespace ArrowSwarm.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>Async leaderboard operations, independent of the provider's authentication implementation.</summary>
    public interface ILeaderboardCloudService
    {
        /// <summary>Whether requests can currently reach the provider.</summary>
        bool IsOnline();
        /// <summary>Initializes the provider and its player identity.</summary>
        Task<bool> EnsureInitializedAsync();
        /// <summary>Updates the authenticated display name.</summary>
        Task<bool> UpdatePlayerNameAsync(string playerName);
        /// <summary>Stores the current score and profile metadata.</summary>
        Task<bool> SubmitScoreAsync(int highestLevel, int totalStars, string playerName, string countryCode);
        /// <summary>Fetches ordered global rows with authoritative player identity flags.</summary>
        Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 10);
        /// <summary>Fetches the authenticated player's rank, or a negative value when unavailable.</summary>
        Task<int> FetchPlayerRankAsync();
    }
}

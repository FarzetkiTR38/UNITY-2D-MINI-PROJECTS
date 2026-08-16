using System.Threading.Tasks;
using NeonGalaxy.Data;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Abstraction for cloud save operations.
    /// Implementations can target UGS Cloud Save, Firebase, or custom backends.
    /// </summary>
    public interface ICloudSaveService
    {
        /// <summary>
        /// Returns true if the cloud save service is available and ready.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Saves the given data to the cloud under the specified key.
        /// Returns true on success.
        /// </summary>
        Task<bool> SaveAsync(string key, SaveData data);

        /// <summary>
        /// Loads save data from the cloud for the specified key.
        /// Returns null if no cloud save exists or service is unavailable.
        /// </summary>
        Task<SaveData> LoadAsync(string key);

        /// <summary>
        /// Deletes the cloud save for the specified key.
        /// Returns true on success.
        /// </summary>
        Task<bool> DeleteAsync(string key);

        /// <summary>
        /// Generates and returns the next unique guest number.
        /// Uses server-side counter to ensure global uniqueness.
        /// Returns -1 if the service is unavailable (caller should use local fallback).
        /// </summary>
        Task<int> GetNextGuestNumberAsync();

        /// <summary>
        /// Saves public player data (e.g. avatar string/id) that can be read by other players.
        /// </summary>
        Task<bool> SavePublicDataAsync(string key, string value);

        /// <summary>
        /// Loads public player data for a specific player ID (e.g. fetching their avatar for Leaderboard).
        /// </summary>
        Task<string> LoadPublicDataForPlayerAsync(string playerId, string key);
    }
}

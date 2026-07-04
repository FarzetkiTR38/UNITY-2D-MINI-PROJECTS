using System.Threading.Tasks;
using UnityEngine;
using NeonGalaxy.Data;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Mock implementation of ICloudSaveService for development and testing.
    /// Uses PlayerPrefs as a simple key-value store instead of a real cloud backend.
    /// Replace with UGS Cloud Save or Firebase wrapper for production.
    /// </summary>
    public class MockCloudSaveService : ICloudSaveService
    {
        private const string PREFS_PREFIX = "mock_cloud_";
        private const string GUEST_COUNTER_KEY = "mock_cloud_guest_counter";

        public bool IsAvailable => true;

        /// <summary>
        /// Saves data to PlayerPrefs as JSON (simulates cloud save).
        /// </summary>
        public async Task<bool> SaveAsync(string key, SaveData data)
        {
            Debug.Log($"[MockCloudSave] Saving data for key: {key}");

            await Task.Delay(500); // Simulate network latency

            try
            {
                string json = JsonUtility.ToJson(data, false);
                PlayerPrefs.SetString(PREFS_PREFIX + key, json);
                PlayerPrefs.Save();
                Debug.Log("[MockCloudSave] Save successful.");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MockCloudSave] Save failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads data from PlayerPrefs (simulates cloud load).
        /// </summary>
        public async Task<SaveData> LoadAsync(string key)
        {
            Debug.Log($"[MockCloudSave] Loading data for key: {key}");

            await Task.Delay(500); // Simulate network latency

            string prefKey = PREFS_PREFIX + key;

            if (!PlayerPrefs.HasKey(prefKey))
            {
                Debug.Log("[MockCloudSave] No cloud save found.");
                return null;
            }

            try
            {
                string json = PlayerPrefs.GetString(prefKey);
                var data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("[MockCloudSave] Load successful.");
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MockCloudSave] Load failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes data from PlayerPrefs (simulates cloud delete).
        /// </summary>
        public async Task<bool> DeleteAsync(string key)
        {
            Debug.Log($"[MockCloudSave] Deleting data for key: {key}");

            await Task.Delay(300);

            PlayerPrefs.DeleteKey(PREFS_PREFIX + key);
            PlayerPrefs.Save();

            Debug.Log("[MockCloudSave] Delete successful.");
            return true;
        }

        /// <summary>
        /// Returns the next unique guest number using a local PlayerPrefs counter.
        /// In production, this would be a server-side atomic counter.
        /// </summary>
        public async Task<int> GetNextGuestNumberAsync()
        {
            Debug.Log("[MockCloudSave] Generating next guest number...");

            await Task.Delay(200); // Simulate minimal latency

            int current = PlayerPrefs.GetInt(GUEST_COUNTER_KEY, 0);
            current++;
            PlayerPrefs.SetInt(GUEST_COUNTER_KEY, current);
            PlayerPrefs.Save();

            Debug.Log($"[MockCloudSave] Guest number assigned: {current}");
            return current;
        }
    }
}

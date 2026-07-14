using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeonGalaxy.Data;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using Random = UnityEngine.Random;
using SaveData = NeonGalaxy.Data.SaveData;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Real implementation of ICloudSaveService using Unity Gaming Services.
    /// Synchronizes local SaveData to the cloud.
    /// </summary>
    public class UGSCloudSaveService : ICloudSaveService
    {
        public bool IsAvailable => UnityServices.State == ServicesInitializationState.Initialized;

        public async Task<bool> SaveAsync(string key, SaveData data)
        {
            if (!IsAvailable) return false;

            try
            {
                // We serialize to JSON first to ensure Unity's JsonUtility handles 
                // any Unity-specific types correctly before sending to UGS.
                string json = JsonUtility.ToJson(data);
                
                var dataToSave = new Dictionary<string, object> { { key, json } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
                
                Debug.Log($"[UGSCloudSaveService] Saved data to cloud under key: {key}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSCloudSaveService] SaveAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<SaveData> LoadAsync(string key)
        {
            if (!IsAvailable) return null;

            try
            {
                var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
                
                if (results.TryGetValue(key, out var item))
                {
                    // Cloud Save stores it as an object, we retrieve it as a string and deserialize
                    string json = item.Value.GetAs<string>();
                    
                    if (!string.IsNullOrEmpty(json))
                    {
                        var data = JsonUtility.FromJson<SaveData>(json);
                        Debug.Log($"[UGSCloudSaveService] Loaded data from cloud under key: {key}");
                        return data;
                    }
                }
                
                Debug.Log($"[UGSCloudSaveService] No cloud save found for key: {key}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSCloudSaveService] LoadAsync failed: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string key)
        {
            if (!IsAvailable) return false;

            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
                Debug.Log($"[UGSCloudSaveService] Deleted cloud save for key: {key}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UGSCloudSaveService] DeleteAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetNextGuestNumberAsync()
        {
            // Simple MVP implementation: Random 4-digit number
            // Note: UGS doesn't natively provide a "global counter" without Cloud Code.
            // This is perfectly fine because we rely on UGS PlayerId for actual uniqueness.
            
            await Task.Delay(10); // Interface requires async
            return Random.Range(1000, 9999);
        }
    }
}

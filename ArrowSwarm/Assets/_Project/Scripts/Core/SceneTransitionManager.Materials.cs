namespace ArrowSwarm.Core
{
    using UnityEngine;

    /// <summary>Owns runtime transition material instances without changing authored material assets.</summary>
    public partial class SceneTransitionManager
    {
        private Material _overrideSource;
        private Material _overrideInstance;

        private Material GetOverrideMaterial()
        {
            if (_overrideInstance != null && _overrideSource == _customMaterialOverride) return _overrideInstance;
            if (_overrideInstance != null) Destroy(_overrideInstance);
            _overrideSource = _customMaterialOverride;
            _overrideInstance = new Material(_overrideSource)
            {
                name = "Mat_Runtime_" + _overrideSource.name,
                hideFlags = HideFlags.HideAndDontSave
            };
            return _overrideInstance;
        }

        protected override void OnDestroy()
        {
            foreach (var material in _materialCache.Values)
                if (material != null) Destroy(material);
            _materialCache.Clear();
            if (_overrideInstance != null) Destroy(_overrideInstance);
            _overrideSource = null; _overrideInstance = null;
            base.OnDestroy();
        }
    }
}

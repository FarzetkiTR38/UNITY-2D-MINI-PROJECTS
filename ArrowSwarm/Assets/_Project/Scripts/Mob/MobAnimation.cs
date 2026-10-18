namespace ArrowSwarm.Mob
{
    using UnityEngine;

    /// <summary>Controls the authored sprite loop across freeze and pooled mob lifetimes.</summary>
    [DisallowMultipleComponent]
    public sealed class MobAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _stateName = "Base Layer.Flutter";

        private int _stateHash;

        /// <summary>Whether an authored animation controller is assigned.</summary>
        public bool IsConfigured => _animator != null && _animator.runtimeAnimatorController != null;

        private void Awake()
        {
            _stateHash = Animator.StringToHash(_stateName);
        }

        /// <summary>Starts the sprite loop at a deterministic phase for this mob.</summary>
        public void Initialize(int seed)
        {
            if (!IsConfigured) return;
            _animator.enabled = true;
            _animator.speed = 1f;
            // Integer hashing spreads sequential spawn IDs across the loop without shared random state.
            uint phase = unchecked((uint)seed * 2654435761u);
            float normalizedPhase = (phase & 0x00FFFFFFu) / 16777216f;
            _animator.Play(_stateHash, 0, normalizedPhase);
            _animator.Update(0f);
        }

        /// <summary>Stops wing motion while frozen, preserving its current animation phase.</summary>
        public void SetFrozen(bool frozen)
        {
            if (IsConfigured) _animator.speed = frozen ? 0f : 1f;
        }

        /// <summary>Clears the previous lifetime's speed and frame before returning to the pool.</summary>
        public void ResetAnimation()
        {
            if (!IsConfigured) return;
            _animator.speed = 1f;
            if (_animator.isActiveAndEnabled)
            {
                _animator.Play(_stateHash, 0, 0f);
                _animator.Update(0f);
            }
            _animator.enabled = false;
        }

        private void OnDisable()
        {
            ResetAnimation();
        }
    }
}

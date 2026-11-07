namespace ArrowSwarm.Puzzle
{
    using ArrowSwarm.Ads;

    /// <summary>Owns one exhausted-Rocket reward request for the current scene and attempt only.</summary>
    public sealed partial class PuzzleSession
    {
        private int _rocketRewardRequest;

        /// <summary>Whether the current attempt is displaying its exhausted-Rocket modal.</summary>
        public bool RocketPromptOpen { get; private set; }

        /// <summary>Whether an owned Rocket rewarded request is still awaiting its result.</summary>
        public bool RocketAdPending { get; private set; }

        private void OpenRocketPrompt()
        {
            if (!Idle || Preview || RocketCharges > 0) return;
            RocketArmed = false; RocketPromptOpen = true; Notify();
        }

        private void ResetRocketPrompt()
        {
            _rocketRewardRequest++;
            RocketPromptOpen = RocketAdPending = false;
        }

        /// <summary>Dismisses the modal without spending or granting a charge; an active ad retains ownership.</summary>
        public void CloseRocketPrompt()
        {
            if (!RocketPromptOpen || RocketAdPending) return;
            ResetRocketPrompt(); Notify();
        }

        /// <summary>Requests one charge only after explicit Watch Ad input; failure remains retryable.</summary>
        public void WatchRocketAd()
        {
            if (!RocketPromptOpen || RocketAdPending || Preview || Active != this || Won || Lost) return;
            int version = _version, request = ++_rocketRewardRequest;
            RocketAdPending = true; Notify();
            AdManager.Instance.ShowRewardedAd(success => CompleteRocketReward(version, request, success));
        }

        private void CompleteRocketReward(int version, int request, bool success)
        {
            if (this == null || Active != this || version != _version || request != _rocketRewardRequest ||
                !RocketAdPending || !RocketPromptOpen || Won || Lost || Preview) return;
            RocketAdPending = false; _rocketRewardRequest++;
            if (success)
            {
                RocketCharges++; RocketPromptOpen = false;
                SetMessage("puzzle_rocket_added", "Rocket added. Tap the rocket to use it.");
            }
            Notify();
        }
    }
}

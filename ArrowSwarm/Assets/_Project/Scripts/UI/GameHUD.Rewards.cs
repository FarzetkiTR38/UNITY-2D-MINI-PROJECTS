namespace ArrowSwarm.UI
{
    using ArrowSwarm.Ads;
    using ArrowSwarm.Data;
    using ArrowSwarm.Skills;
    using ArrowSwarm.Tips;

    /// <summary>Credits an earned skill once while limiting automatic use to its original live board.</summary>
    public partial class GameHUD
    {
        private void RequestSkillReward(bool freeze)
        {
            var context = new DefenseRewardContext(this);
            var data = DataManager.Instance;
            AdManager.Instance?.ShowRewardedAd(success =>
            {
                if (!success) return;
                if (data != null)
                {
                    if (freeze) data.ModifyFreezeCount(1);
                    else data.ModifyTipCount(1);
                }
                if (!context.IsCurrent) return;
                if (freeze) FreezeManager.Instance?.UseFreeze();
                else TipManager.Instance?.UseTip();
                UpdateSkillBadges();
            });
        }
    }
}

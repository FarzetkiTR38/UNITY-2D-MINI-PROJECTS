namespace ArrowSwarm.Mob
{
    using System.Collections.Generic;

    /// <summary>Tracks the exact spawn instances temporarily reversing to close a death gap.</summary>
    public partial class MobSpawner
    {
        private readonly List<GapCloseParticipant> _gapCloseParticipants = new List<GapCloseParticipant>();

        private readonly struct GapCloseParticipant
        {
            internal readonly Mob Mob;
            internal readonly int SpawnId;

            internal GapCloseParticipant(Mob mob) { Mob = mob; SpawnId = mob.MobId; }
        }

        private void CancelGapClose()
        {
            if (_gapCloseCoroutine != null)
            {
                StopCoroutine(_gapCloseCoroutine);
                _gapCloseCoroutine = null;
            }
            RestoreGapCloseParticipants();
        }

        private void RestoreGapCloseParticipants()
        {
            for (int i = 0; i < _gapCloseParticipants.Count; i++)
            {
                var participant = _gapCloseParticipants[i];
                Mob mob = participant.Mob;
                if (mob != null && mob.gameObject.activeSelf && mob.IsAlive && mob.MobId == participant.SpawnId)
                    mob.Movement.SetSpeed(_baseMobSpeed);
            }
            _gapCloseParticipants.Clear();
        }
    }
}

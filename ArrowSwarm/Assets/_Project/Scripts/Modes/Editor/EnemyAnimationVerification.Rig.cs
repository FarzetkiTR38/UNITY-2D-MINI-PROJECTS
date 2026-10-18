namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Mob;
    using UnityEngine;

    /// <summary>Compares local rig poses and the three authored renderers in editor-only lifecycle checks.</summary>
    public static partial class EnemyAnimationVerification
    {
        private struct LocalPose
        {
            private readonly Vector3 _position, _scale;
            private readonly Quaternion _rotation;

            internal LocalPose(Transform target)
            { _position = target.localPosition; _rotation = target.localRotation; _scale = target.localScale; }

            internal bool Matches(Transform target)
            {
                return (target.localPosition - _position).sqrMagnitude < .00000001f &&
                    (target.localScale - _scale).sqrMagnitude < .00000001f &&
                    Quaternion.Angle(target.localRotation, _rotation) < .001f;
            }
        }

        private static void AddPose(List<LocalPose> poses, Transform target)
        {
            for (int index = 0; index < poses.Count; index++) if (poses[index].Matches(target)) return;
            poses.Add(new LocalPose(target));
        }

        private static bool RigTint(Mob mob, Func<Color, bool> test)
        {
            var animator = mob.GetComponentInChildren<Animator>(true);
            var renderers = animator.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length != 3) return false;
            foreach (var renderer in renderers) if (!test(renderer.color)) return false;
            return true;
        }

        private static bool WingsHeld(Animator animator, LocalPose left, LocalPose right)
        { return left.Matches(animator.transform.Find("WingLeft")) && right.Matches(animator.transform.Find("WingRight")); }
    }
}

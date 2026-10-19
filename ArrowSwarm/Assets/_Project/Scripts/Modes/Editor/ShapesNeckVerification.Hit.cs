namespace ArrowSwarm.Modes.Editor
{
    using ArrowSwarm.Puzzle;
    using UnityEngine;

    /// <summary>Confirms the complete centered triangle is selectable when enlarged for a hint.</summary>
    public static partial class ShapesNeckVerification
    {
        private static readonly Vector2[] HeadHitProbes =
        {
            new Vector2(0f, .98f), new Vector2(-.94f, .02f), new Vector2(.94f, .02f),
            new Vector2(-.2f, .5f), new Vector2(.2f, .5f)
        };

        private static void CheckHeadHitSurface(PuzzleArrowView view, SpriteRenderer head)
        {
            Bounds bounds = head.sprite.bounds;
            for (int i = 0; i < HeadHitProbes.Length; i++)
            {
                Vector2 probe = HeadHitProbes[i];
                Vector3 local = new Vector3(bounds.center.x + bounds.extents.x * probe.x,
                    Mathf.Lerp(bounds.min.y, bounds.max.y, probe.y), 0f);
                Vector2 world = head.transform.TransformPoint(local);
                Check(view.DistanceTo(world) <= .0001f,
                    "head hit probe " + i + " selects its visible tip/base/interior");
            }
            Vector2 far = (Vector2)head.transform.position + new Vector2(10000f, 10000f);
            Check(view.DistanceTo(far) > 1000f, "head selection remains local and excludes distant empty space");
        }
    }
}

namespace ArrowSwarm.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Lightweight, texture-free sparkle, snowflake and soft ray artwork authored in the Inspector.</summary>
    [AddComponentMenu("UI/Arrow Swarm/Skill Accent")]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UISkillAccentGraphic : MaskableGraphic
    {
        private enum Shape { Sparkle, Snowflake, Rays }
        [SerializeField] private Shape _shape;
        [Min(3), SerializeField] private int _rayCount = 12;
        [Range(1f, 14f), SerializeField] private float _rayWidth = 7f;
        [Range(.02f, .6f), SerializeField] private float _innerRadius = .24f;

        /// <summary>Builds the authored accent using a small UI mesh and the standard masked UI material.</summary>
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            Vector2 center = bounds.center;
            float radius = Mathf.Min(bounds.width, bounds.height) * .5f;
            if (_shape == Shape.Rays) Rays(mesh, center, radius);
            else if (_shape == Shape.Snowflake) Snowflake(mesh, center, radius);
            else Sparkle(mesh, center, radius);
        }

        private void Sparkle(VertexHelper mesh, Vector2 center, float radius)
        {
            Color edge = color; edge.a = 0;
            Color core = Color.Lerp(color, Color.white, .84f); core.a = color.a;
            Add(mesh, center, core);
            for (int ring = 0; ring < 3; ring++)
            for (int point = 0; point < 8; point++)
            {
                float angle = point * Mathf.PI * .25f;
                float length = point % 2 == 0 ? 1f : .22f;
                Vector2 offset = new Vector2(Mathf.Cos(angle) * .78f, Mathf.Sin(angle)) * radius * length;
                float scale = ring == 0 ? .5f : ring == 1 ? .88f : 1f;
                Add(mesh, center + offset * scale, ring == 0 ? core : ring == 1 ? color : edge);
            }
            for (int point = 0; point < 8; point++)
            {
                int next = (point + 1) % 8;
                mesh.AddTriangle(0, 1 + point, 1 + next);
                for (int ring = 0; ring < 2; ring++)
                {
                    int start = 1 + ring * 8;
                    Quad(mesh, start + point, start + next, start + 8 + next, start + 8 + point);
                }
            }
        }

        private void Snowflake(VertexHelper mesh, Vector2 center, float radius)
        {
            float width = radius * .072f;
            for (int arm = 0; arm < 6; arm++)
            {
                float angle = arm * Mathf.PI / 3f + Mathf.PI * .5f;
                Vector2 direction = Direction(angle);
                Segment(mesh, center, center + direction * radius * .91f, width, color);
                for (int branch = -1; branch <= 1; branch += 2)
                {
                    Vector2 tip = center + direction * radius * .64f;
                    Vector2 root = tip + Direction(angle + branch * Mathf.PI * .29f) * radius * .27f;
                    Segment(mesh, tip, root, width * .85f, color);
                }
            }
        }

        private void Rays(VertexHelper mesh, Vector2 center, float radius)
        {
            for (int ray = 0; ray < _rayCount; ray++)
            {
                float angle = ray * Mathf.PI * 2f / _rayCount + .12f;
                float length = radius * (.87f + .1f * Mathf.Sin(ray * 2.43f));
                int first = mesh.currentVertCount;
                for (int section = 0; section < 5; section++)
                {
                    float t = section / 4f;
                    float distance = Mathf.Lerp(_innerRadius * radius, length, t);
                    float halfAngle = _rayWidth * Mathf.Deg2Rad * .5f;
                    for (int across = -1; across <= 1; across++)
                    {
                        Color tint = color;
                        tint.a *= across == 0 ? Mathf.Sin(t * Mathf.PI) * .5f : 0;
                        Add(mesh, center + Direction(angle + halfAngle * across) * distance, tint);
                    }
                }
                for (int section = 0; section < 4; section++)
                for (int across = 0; across < 2; across++)
                {
                    int start = first + section * 3 + across;
                    Quad(mesh, start, start + 1, start + 4, start + 3);
                }
            }
        }

        private static Vector2 Direction(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        private static void Segment(VertexHelper mesh, Vector2 from, Vector2 to, float width, Color tint)
        {
            Vector2 direction = (to - from).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * width * .5f;
            int first = mesh.currentVertCount;
            Add(mesh, from - normal, tint); Add(mesh, from + normal, tint);
            Add(mesh, to + normal, tint); Add(mesh, to - normal, tint);
            Quad(mesh, first, first + 1, first + 2, first + 3);
        }

        private static void Add(VertexHelper mesh, Vector2 point, Color tint)
        {
            var vertex = UIVertex.simpleVert; vertex.position = point; vertex.color = tint;
            mesh.AddVert(vertex);
        }

        private static void Quad(VertexHelper mesh, int a, int b, int c, int d)
        { mesh.AddTriangle(a, b, c); mesh.AddTriangle(c, d, a); }
    }
}

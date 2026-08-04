using UnityEngine;

namespace SmokeSystem
{
    /// <summary>Generates a soft, irregular "puff" sprite at runtime so the smoke system needs no external texture asset.</summary>
    static class SmokeTextureUtility
    {
        static Texture2D cached;

        /// <summary>
        /// A lumpy, cauliflower-ish puff instead of a single perfect circle, so overlapping
        /// copies read as smoke instead of a field of identical soft dots — a plain radial
        /// gradient blocks vision fine up close but looks obviously synthetic once several
        /// overlap at a distance. Built from a few smaller off-center lobes layered onto the
        /// main body, plus fine Perlin-noise density variation for internal detail, then
        /// hard-masked by the full-canvas circle so the lobes can never push alpha above 0 right
        /// at the texture border — the same "hard square edge" failure mode the original
        /// opaque-quad material bug came from, just reintroduced a different way if skipped here.
        /// </summary>
        public static Texture2D GetSoftCircleTexture(int size = 128)
        {
            if (cached != null)
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = "SmokeSoftPuff (Generated)",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            Vector2 center = new Vector2(size - 1, size - 1) * 0.5f;
            float maxDist = size * 0.5f;
            var pixels = new Color32[size * size];

            // Fixed seed: this is generated once and cached/shared for the whole session, so it
            // only needs to look good, not vary between runs.
            var rng = new System.Random(12345);
            const int lobeCount = 5;
            var lobeCenters = new Vector2[lobeCount];
            var lobeRadii = new float[lobeCount];
            for (int i = 0; i < lobeCount; i++)
            {
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2);
                float offset = (float)rng.NextDouble() * maxDist * 0.4f;
                lobeCenters[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * offset;
                lobeRadii[i] = maxDist * Mathf.Lerp(0.4f, 0.7f, (float)rng.NextDouble());
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x, y);

                    float shape = SoftCircle(p, center, maxDist * 0.7f);
                    for (int i = 0; i < lobeCount; i++)
                        shape = Mathf.Max(shape, SoftCircle(p, lobeCenters[i], lobeRadii[i]));

                    // Hard boundary guarantee, independent of where the lobes above landed.
                    shape *= SoftCircle(p, center, maxDist);

                    // Fine internal density variation so it doesn't read as a flat gradient.
                    float noise = Mathf.PerlinNoise(x * 0.12f + 7.3f, y * 0.12f + 4.1f);
                    float alpha = shape * Mathf.Lerp(0.75f, 1f, noise);
                    alpha = Mathf.Pow(Mathf.Clamp01(alpha), 1.3f);

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            cached = tex;
            return cached;
        }

        static float SoftCircle(Vector2 p, Vector2 center, float radius)
        {
            float dist = Vector2.Distance(p, center) / radius;
            return Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(dist));
        }
    }
}

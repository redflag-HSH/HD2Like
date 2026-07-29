using UnityEngine;

namespace SmokeSystem
{
    /// <summary>Generates a soft radial-gradient sprite at runtime so the smoke system needs no external texture asset.</summary>
    static class SmokeTextureUtility
    {
        static Texture2D cached;

        public static Texture2D GetSoftCircleTexture(int size = 64)
        {
            if (cached != null)
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = "SmokeSoftCircle (Generated)",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            Vector2 center = new Vector2(size - 1, size - 1) * 0.5f;
            float maxDist = size * 0.5f;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    float alpha = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(dist));
                    alpha = Mathf.Pow(alpha, 1.6f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            cached = tex;
            return cached;
        }
    }
}

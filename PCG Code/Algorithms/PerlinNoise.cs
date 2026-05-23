using System.Collections.Generic;
using UnityEngine;

public static class PerlinNoise
{
    private static int[] permutation;
    private static int[] p;

    public static void SetSeed(int seed)
    {
        var rand = new System.Random(seed);
        permutation = new int[256];
        for (int i = 0; i < 256; i++) permutation[i] = i;
        for (int i = 0; i < 256; i++)
        {
            int j = rand.Next(256);
            int temp = permutation[i];
            permutation[i] = permutation[j];
            permutation[j] = temp;
        }
        p = new int[512];
        for (int i = 0; i < 512; i++) p[i] = permutation[i % 256];
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private static float Lerp(float a, float b, float t) => a + t * (b - a);
    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    private static float Noise(float x, float y)
    {
        int xi = Mathf.FloorToInt(x) & 255;
        int yi = Mathf.FloorToInt(y) & 255;
        float xf = x - Mathf.Floor(x);
        float yf = y - Mathf.Floor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = p[p[xi] + yi];
        int ab = p[p[xi] + yi + 1];
        int ba = p[p[xi + 1] + yi];
        int bb = p[p[xi + 1] + yi + 1];

        float a = Grad(aa, xf, yf);
        float b = Grad(ba, xf - 1, yf);
        float c = Grad(ab, xf, yf - 1);
        float d = Grad(bb, xf - 1, yf - 1);

        float lerp1 = Lerp(a, b, u);
        float lerp2 = Lerp(c, d, u);
        return Lerp(lerp1, lerp2, v);
    }

    public class Octave
    {
        public float scale;
        public float amplitude;
        public Octave(float scale, float amplitude)
        {
            this.scale = scale;
            this.amplitude = amplitude;
        }
    }

    public static float[,] GenerateNoiseMap(int width, int height, float xMin, float xMax, float yMin, float yMax,
                                            List<Octave> octaves, bool useOctaves, int seed)
    {
        if (octaves == null || octaves.Count == 0)
            throw new System.ArgumentException("Octaves list cannot be empty.");
        SetSeed(seed);

        float[,] noiseMap = new float[width, height];
        float totalAmplitude = 0f;
        if (useOctaves)
        {
            foreach (var oct in octaves)
                totalAmplitude += oct.amplitude;
        }
        else
        {
            totalAmplitude = octaves[0].amplitude;
        }

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            float ny = Mathf.Lerp(yMin, yMax, t);
            for (int x = 0; x < width; x++)
            {
                float s = (float)x / (width - 1);
                float nx = Mathf.Lerp(xMin, xMax, s);
                float sum = 0f;
                if (useOctaves)
                {
                    foreach (var oct in octaves)
                    {
                        float val = Noise(nx * oct.scale, ny * oct.scale);
                        sum += val * oct.amplitude;
                    }
                }
                else
                {
                    var oct = octaves[0];
                    sum = Noise(nx * oct.scale, ny * oct.scale) * oct.amplitude;
                }
                // Normalize to 0..1
                float normalized = (sum / totalAmplitude + 1f) / 2f;
                noiseMap[x, y] = Mathf.Clamp01(normalized);
            }
        }
        return noiseMap;
    }
}
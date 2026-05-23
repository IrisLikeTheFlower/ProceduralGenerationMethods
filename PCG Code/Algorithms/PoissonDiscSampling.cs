using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float fieldWidth, float fieldHeight,
                                               float radiusMin, float radiusMax,
                                               int k, int seed)
    {
        if (radiusMin <= 0) throw new System.ArgumentException("radiusMin must be > 0");
        if (radiusMax < radiusMin) throw new System.ArgumentException("radiusMax must be >= radiusMin");

        var rand = new System.Random(seed);
        float cellSize = radiusMin / Mathf.Sqrt(2f);
        int gridWidth = Mathf.CeilToInt(fieldWidth / cellSize);
        int gridHeight = Mathf.CeilToInt(fieldHeight / cellSize);
        Vector2?[,] grid = new Vector2?[gridWidth, gridHeight];

        List<Vector2> points = new List<Vector2>();
        List<Vector2> active = new List<Vector2>();

        // First point
        Vector2 first = new Vector2((float)rand.NextDouble() * fieldWidth,
                                    (float)rand.NextDouble() * fieldHeight);
        points.Add(first);
        active.Add(first);
        AddToGrid(grid, first, cellSize);

        while (active.Count > 0)
        {
            int idx = rand.Next(active.Count);
            Vector2 point = active[idx];
            bool found = false;

            for (int tries = 0; tries < k; tries++)
            {
                float angle = (float)rand.NextDouble() * 2f * Mathf.PI;
                float dist = (float)rand.NextDouble() * (radiusMax - radiusMin) + radiusMin;
                Vector2 candidate = point + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

                if (candidate.x >= 0 && candidate.x < fieldWidth && candidate.y >= 0 && candidate.y < fieldHeight)
                {
                    if (IsValid(candidate, grid, cellSize, radiusMin))
                    {
                        points.Add(candidate);
                        active.Add(candidate);
                        AddToGrid(grid, candidate, cellSize);
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                active.RemoveAt(idx);
            }
        }
        return points;
    }

    private static void AddToGrid(Vector2?[,] grid, Vector2 point, float cellSize)
    {
        int gx = Mathf.FloorToInt(point.x / cellSize);
        int gy = Mathf.FloorToInt(point.y / cellSize);
        grid[gx, gy] = point;
    }

    private static bool IsValid(Vector2 candidate, Vector2?[,] grid, float cellSize, float radiusMin)
    {
        int gx = Mathf.FloorToInt(candidate.x / cellSize);
        int gy = Mathf.FloorToInt(candidate.y / cellSize);
        int minX = Mathf.Max(0, gx - 2);
        int maxX = Mathf.Min(grid.GetLength(0) - 1, gx + 2);
        int minY = Mathf.Max(0, gy - 2);
        int maxY = Mathf.Min(grid.GetLength(1) - 1, gy + 2);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (grid[x, y].HasValue)
                {
                    float dx = candidate.x - grid[x, y].Value.x;
                    float dy = candidate.y - grid[x, y].Value.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < radiusMin) return false;
                }
            }
        }
        return true;
    }
}
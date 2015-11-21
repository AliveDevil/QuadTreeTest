using System;
using UnityEngine;

public class SimplexNoise
{
    private static int[][] grad3 =
    {
            new[] { 1, 1, 0}, new[] { -1,  1, 0}, new[] { 1, -1,  0}, new[] { -1, -1,  0},
            new[] { 1, 0, 1}, new[] { -1,  0, 1}, new[] { 1,  0, -1}, new[] { -1,  0, -1},
            new[] { 0, 1, 1}, new[] {  0, -1, 1}, new[] { 0,  1, -1}, new[] {  0, -1, -1}
        };

    private int[] p, perm;
    private System.Random random;

    public float this[Vector2 v]
    {
        get { return Noise(v); }
    }

    public SimplexNoise() : this(0)
    {
    }

    public SimplexNoise(int seed)
    {
        Seed(seed);
        p = new int[256];
        perm = new int[512];
    }

    public void Initialize()
    {
        for (int i = 0; i < p.Length; i++)
        {
            p[i] = i;
        }
        for (int c = 0; c < 3; c++)
        {
            for (int i = 0; i < p.Length; i++)
            {
                int j = random.Next(0, p.Length);
                int temp = p[i];
                p[i] = p[j];
                p[j] = temp;
            }
        }

        p.CopyTo(perm, 0);
        p.CopyTo(perm, 256);
    }

    public float Noise(Vector2 v, float amplitude)
    {
        return Noise(v) * amplitude;
    }

    public float Noise(Vector2 v)
    {
        const float F2 = 0.36602540378443864676372317075294f;
        const float G2 = 0.21132486540518711774542560974902f;
        const float DG2 = 2 * G2;

        float n0, n1, n2;
        float s = (v.x + v.y) * F2;
        int i = fastfloor(v.x + s);
        int j = fastfloor(v.y + s);

        float t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = v.x - X0;
        float y0 = v.y - Y0;

        int i1, j1;
        if (x0 > y0) { i1 = 1; j1 = 0; }
        else { i1 = 0; j1 = 1; }

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1.0f + DG2;
        float y2 = y0 - 1.0f + DG2;
        int ii = i & 255;
        int jj = j & 255;
        int gi0 = perm[ii + perm[jj]] % 12;
        int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
        int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;
        float t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0) n0 = 0;
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * dot(grad3[gi0], x0, y0);
        }
        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0) n1 = 0;
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * dot(grad3[gi1], x1, y1);
        }
        float t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0) n2 = 0;
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * dot(grad3[gi2], x2, y2);
        }
        return 70.0f * (n0 + n1 + n2);
    }

    public float Noise(Vector3 v)
    {
        const float F3 = 0.33333333333333333333333333333333f;
        const float G3 = 0.16666666666666666666666666666666f;
        const float TG3 = 0.33333333333333333333333333333333f;
        float n0, n1, n2, n3;

        float s = (v.x + v.y + v.z) * F3;
        int i = fastfloor(v.x + s);
        int j = fastfloor(v.y + s);
        int k = fastfloor(v.z + s);
        float t = (i + j + k) * G3;
        float X0 = i - t;
        float Y0 = j - t;
        float Z0 = k - t;
        float x0 = v.x - X0;
        float y0 = v.y - Y0;
        float z0 = v.z - Z0;

        int i1, j1, k1;
        int i2, j2, k2;
        if (x0 >= y0)
        {
            if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
            else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
            else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
        }
        else
        {
            if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
            else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
            else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
        }

        float x1 = x0 - i1 + G3;
        float y1 = y0 - j1 + G3;
        float z1 = z0 - k1 + G3;
        float x2 = x0 - i2 + 2.0f * G3;
        float y2 = y0 - j2 + 2.0f * G3;
        float z2 = z0 - k2 + 2.0f * G3;
        float x3 = x0 - 1.0f + TG3;
        float y3 = y0 - 1.0f + TG3;
        float z3 = z0 - 1.0f + TG3;

        int ii = i & 255;
        int jj = j & 255;
        int kk = k & 255;
        int gi0 = perm[ii + perm[jj + perm[kk]]] % 12;
        int gi1 = perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]] % 12;
        int gi2 = perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]] % 12;
        int gi3 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]] % 12;

        float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
        if (t0 < 0) n0 = 0;
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * dot(grad3[gi0], x0, y0, z0);
        }
        float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
        if (t1 < 0) n1 = 0;
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * dot(grad3[gi1], x1, y1, z1);
        }
        float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
        if (t2 < 0) n2 = 0;
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * dot(grad3[gi2], x2, y2, z2);
        }
        float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
        if (t3 < 0) n3 = 0;
        else
        {
            t3 *= t3;
            n3 = t3 * t3 * dot(grad3[gi3], x3, y3, z3);
        }

        return 32 * (n0 + n1 + n2 + n3);
    }

    public void Seed(int seed)
    {
        random = new System.Random(seed);
    }

    private static float dot(int[] g, float x, float y)
    {
        return g[0] * x + g[1] * y;
    }

    private static float dot(int[] g, float x, float y, float z)
    {
        return g[0] * x + g[1] * y + g[2] * z;
    }

    private static int fastfloor(float f)
    {
        return f > 0 ? (int)f : (int)f - 1;
    }
}

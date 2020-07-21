using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for(int ii = 0;ii<size;ii++)
        {
            for(int j = 0;j<size;j++)
            {
                float x = ii / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[ii, j] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float val)
    {
        float a = 3f;
        float b = 2.2f;

        return Mathf.Pow(val, a) / (Mathf.Pow(val, a) + Mathf.Pow(b - (b * val), a));
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.ShaderGraph;
using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noisemap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffset = new Vector2[octaves]
;
        for(int ii = 0; ii<octaves;ii++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffset[ii] = new Vector2(offsetX, offsetY);
        }
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfwidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for(int y = 0; y < mapHeight; y++)
        {
            for (int x = 0;x<mapWidth;  x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int ii = 0; ii < octaves; ii++)
                {
                    float sampleX = (x-halfwidth) / scale * frequency + octaveOffset[ii].x;
                    float sampleY = (y-halfHeight) / scale * frequency + octaveOffset[ii].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight<minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noisemap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noisemap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noisemap[x, y]);
            }
        }

                return noisemap;
    }

}

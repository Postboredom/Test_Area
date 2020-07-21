using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.ShaderGraph;
using UnityEngine;

public static class Noise 
{

    public enum NormalizeMode {Local, Global, Global_Test}
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffset = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;
        ;
        for(int ii = 0; ii<octaves;ii++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffset[ii] = new Vector2(offsetX, offsetY);
            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfwidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for(int y = 0; y < mapHeight; y++)
        {
            for (int x = 0;x<mapWidth;  x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int ii = 0; ii < octaves; ii++)
                {
                    float sampleX = (x-halfwidth + octaveOffset[ii].x) / scale * frequency;
                    float sampleY = (y-halfHeight + octaveOffset[ii].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if(noiseHeight<minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else if (normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2 *maxPossibleHeight/1.8f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight,0,int.MaxValue);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + maxPossibleHeight) / (1.5f * maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Lerp(0, normalizedHeight,1f);
                }
            }
        }

                return noiseMap;
    }

}

using System.Collections;
using System.Collections.Generic;
//using UnityEditor.ShaderGraph.Drawing.Slots;
using UnityEngine;
using UnityEngine.AI;


public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _meshHeightCurve,int levelOfDetail)
    {
        AnimationCurve meshHeightCurve = new AnimationCurve(_meshHeightCurve.keys);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / 2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int vertsPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(vertsPerLine, vertsPerLine);
        int vertIndex = 0;

        for (int y = 0; y < height; y+= meshSimplificationIncrement) 
        {
            for (int x = 0; x < width; x+= meshSimplificationIncrement) 
            {
                meshData.verts[vertIndex] = new Vector3(topLeftX + x, meshHeightCurve.Evaluate( heightMap[x, y] ) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width-1 && y<height-1)
                {
                    meshData.AddTriangle(vertIndex, vertIndex + vertsPerLine + 1, vertIndex + vertsPerLine);
                    meshData.AddTriangle(vertIndex + vertsPerLine + 1, vertIndex, vertIndex + 1);

                }

                vertIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] verts;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth,int meshHeight)
    {
        verts = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1)*6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[verts.Length];
        int triangleCount = triangles.Length / 3;
        for(int ii = 0; ii < triangleCount; ii++)
        {
            int normalTriangleIndex = ii * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex+1];
            int vertexIndexC = triangles[normalTriangleIndex+2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for(int ii = 0;ii<vertexNormals.Length;ii++)
        {
            vertexNormals[ii].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA,int indexB, int indexC)
    {
        Vector3 pointA = verts[indexA];
        Vector3 pointB = verts[indexB];
        Vector3 pointC = verts[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.normals = CalculateNormals();
        return mesh;
    }
}
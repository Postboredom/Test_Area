using System.Collections;
using System.Collections.Generic;
using System.Data;
//using UnityEditor.ShaderGraph.Drawing.Slots;
using UnityEngine;
using UnityEngine.AI;


public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _meshHeightCurve,int levelOfDetail)
    {
        AnimationCurve meshHeightCurve = new AnimationCurve(_meshHeightCurve.keys);
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2*meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1) / 2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        int vertsPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData( vertsPerLine);

        int[,] vertexIndiciesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            { bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;
               if(isBorderVertex)
                {
                    vertexIndiciesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndiciesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y+= meshSimplificationIncrement) 
        {
            for (int x = 0; x < borderedSize; x+= meshSimplificationIncrement) 
            {
                int vertIndex = vertexIndiciesMap[x, y];
                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                float height = meshHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height , topLeftZ - percent.y * meshSizeUnsimplified);

                meshData.AddVertex(vertexPosition, percent, vertIndex);

                if (x < borderedSize-1 && y<borderedSize-1)
                {
                    int a = vertexIndiciesMap[x, y];
                    int b = vertexIndiciesMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndiciesMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndiciesMap[x+ meshSimplificationIncrement, y + meshSimplificationIncrement];
                    meshData.AddTriangle(a,d,c);
                    meshData.AddTriangle(d,a,b);

                }

                vertIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    Vector3[] verts;
    int[] triangles;
    Vector2[] uvs;

    Vector3[] borderVerts;
    int[] borederTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    public MeshData(int vertsPerLine)
    {
        verts = new Vector3[vertsPerLine * vertsPerLine];
        uvs = new Vector2[vertsPerLine * vertsPerLine];
        triangles = new int[(vertsPerLine - 1) * (vertsPerLine - 1)*6];

        borderVerts = new Vector3[vertsPerLine * 4 + 4];
        borederTriangles = new int[24 * vertsPerLine];
    }

    public void AddVertex(Vector3 vertexPos, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            borderVerts[-vertexIndex - 1] = vertexPos;
        }
        else
        {
            verts[vertexIndex] = vertexPos;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            borederTriangles[borderTriangleIndex] = a;
            borederTriangles[borderTriangleIndex+1] = b;
            borederTriangles[borderTriangleIndex+2] = c;
            borderTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
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

        int bordertriangleCount = borederTriangles.Length / 3;
        for (int ii = 0; ii < bordertriangleCount; ii++)
        {
            int normalTriangleIndex = ii * 3;
            int vertexIndexA = borederTriangles[normalTriangleIndex];
            int vertexIndexB = borederTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borederTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int ii = 0;ii<vertexNormals.Length;ii++)
        {
            vertexNormals[ii].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA,int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVerts[-indexA - 1] : verts[indexA];
        Vector3 pointB = (indexB < 0) ? borderVerts[-indexB - 1] : verts[indexB];
        Vector3 pointC = (indexC < 0) ? borderVerts[-indexC - 1] : verts[indexC]; ;

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{
    public int nXPerChunk;
    public int nYPerChunk;
    public int nZPerChunk;
    public float chunkGridSize;

    public Material chunkMaterial;
    public float thresholdValue;
    

    Dictionary<Vector3, Chunk> chunkHashMap = new Dictionary<Vector3, Chunk>();

    public void AddChunk(Vector3 positionChunkCenter)
    {
        chunkHashMap.Add(positionChunkCenter, new Chunk(positionChunkCenter, nXPerChunk, nYPerChunk, nZPerChunk, chunkGridSize, thresholdValue, chunkMaterial));
    }

}

public class Chunk
{
    public Vector3 positionChunkCenter;
    public Mesh mesh;

    public ScalarFieldPoint[] scalarField;

    public float thresholdValue;

    GameObject chunkGameObject;
    GameObject marchingTerrain;

    int nX;
    int nY;
    int nZ;
    float gridSize;

    public Chunk(Vector3 positionChunkCenter, int nX, int nY, int nZ, float gridSize, float thresholdValue, Material material)
    {
        this.positionChunkCenter = positionChunkCenter;
        this.nX = nX;
        this.nY = nY;
        this.nZ = nZ;
        this.gridSize = gridSize;

        scalarField = new ScalarFieldPoint[nX * nY * nZ];

        for (int i = 0; i < nX * nY * nZ; i++)
        {
            scalarField[i] = new ScalarFieldPoint();
        }

        chunkGameObject = new GameObject("Marching Cubes Chunk");
        chunkGameObject.AddComponent<MeshFilter>();
        chunkGameObject.AddComponent<MeshRenderer>();
        chunkGameObject.GetComponent<Renderer>().material = material;

        marchingTerrain = GameObject.Find("MarchingTerrain");

        InitializeScalarField();
        RebuildChunkMesh();

        chunkGameObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    public void RebuildChunkMesh()
    {
        mesh = marchingTerrain.GetComponent<MarchingCubes>().GetMeshFromField(scalarField, thresholdValue);
    }

    public void InitializeScalarField()
    {
        scalarField = marchingTerrain.GetComponent<NoiseTerrain>().InitializeScalarField(nX, nY, nZ, gridSize, positionChunkCenter);
    }


}

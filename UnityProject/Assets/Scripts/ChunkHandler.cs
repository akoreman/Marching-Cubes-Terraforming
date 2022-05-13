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
    
    float chunkXDimension;
    float chunkYDimension;

    float chunkZDimension;

    Dictionary<Vector3, Chunk> chunkHashMap = new Dictionary<Vector3, Chunk>();
    public Dictionary<Vector3, Chunk> activeChunkHashMap = new Dictionary<Vector3, Chunk>();

    void Start()
    {
        chunkXDimension = (nXPerChunk - 1) * chunkGridSize;
        chunkYDimension = (nYPerChunk - 1) * chunkGridSize;
        chunkZDimension = (nZPerChunk - 1) * chunkGridSize;
    }

    public void AddChunk(Vector3 positionChunkCenter)
    {
        chunkHashMap.Add(positionChunkCenter, new Chunk(positionChunkCenter, nXPerChunk, nYPerChunk, nZPerChunk, chunkGridSize, thresholdValue, chunkMaterial));
    }

    public void AddChunk(Vector3 position, Chunk chunk)
    {
        chunkHashMap.Add(position, chunk);
    }

    public void AddChunkFromPoint(Vector3 position)
    {
        Vector3 positionChunkCenter = GetNearestChunkCenter(position);

        chunkHashMap.Add(positionChunkCenter, new Chunk(positionChunkCenter, nXPerChunk, nYPerChunk, nZPerChunk, chunkGridSize, thresholdValue, chunkMaterial));
    }


    public Chunk GetChunkFromPosition(Vector3 position)
    {
        Vector3 nearestChunkCenter = GetNearestChunkCenter(position);

        if (chunkHashMap.ContainsKey(nearestChunkCenter)) {return chunkHashMap[nearestChunkCenter]; }

        return null;
    }

    public Chunk GetChunkFromIndices(int[] index)
    {
        Vector3 position = new Vector3(index[0] * chunkXDimension, index[1] * chunkYDimension, index[2] * chunkZDimension);

        return GetChunkFromPosition(position);
    }

    public Vector3 GetNearestChunkCenter(Vector3 position)
    {
        Vector3 returnVector = new Vector3(0f,0f,0f);

        returnVector.x = Mathf.Round(position.x / chunkXDimension) * chunkXDimension;
        returnVector.y = Mathf.Round(position.y / chunkYDimension) * chunkYDimension;
        returnVector.z = Mathf.Round(position.z / chunkZDimension) * chunkZDimension;


        return returnVector;
    }

    public int[] GetChunkIndex(Chunk chunk)
    {
        int [] returnArray = new int[3];

        returnArray[0] = Mathf.RoundToInt(chunk.positionChunkCenter.x / chunkXDimension);
        returnArray[1] = Mathf.RoundToInt(chunk.positionChunkCenter.y / chunkYDimension);
        returnArray[2] = Mathf.RoundToInt(chunk.positionChunkCenter.z / chunkZDimension);

        return returnArray;
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

    public bool chunkVisible;

    public Chunk(Vector3 positionChunkCenter, int nX, int nY, int nZ, float gridSize, float thresholdValue, Material material)
    {
        this.positionChunkCenter = positionChunkCenter;
        this.nX = nX ;
        this.nY = nY ;
        this.nZ = nZ ;
        this.gridSize = gridSize;

        this.chunkVisible = true;


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

    public void ChangeScalarField(float valueChange, Vector3 localPosition, int radius)
    {
        Vector3Int fieldPointPosition = new Vector3Int(Mathf.RoundToInt(localPosition.x /  gridSize), Mathf.RoundToInt(localPosition.y / gridSize), Mathf.RoundToInt(localPosition.z / gridSize));

        
    }

    public void InitializeScalarField()
    {
        scalarField = marchingTerrain.GetComponent<NoiseTerrain>().InitializeScalarField(nX, nY, nZ, gridSize, positionChunkCenter);
    }

    public void HideChunk()
    {
        chunkGameObject.SetActive(false);
        chunkVisible = false;
    }

    public void ShowChunk()
    {
        chunkGameObject.SetActive(true);
        chunkVisible = true;
    }

}

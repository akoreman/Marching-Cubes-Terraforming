using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


    // Add a new chunk to chunkHashMap by specifying the (0,0,0) corner position, note there is no check to see whether there is a collision.
    public void AddChunk(Vector3 positionChunkCorner)
    {
        chunkHashMap.Add(positionChunkCorner, new Chunk(positionChunkCorner, nXPerChunk, nYPerChunk, nZPerChunk, chunkGridSize, thresholdValue, chunkMaterial));
    }

    /*
    public void AddChunk(Vector3 position, Chunk chunk)
    {
        chunkHashMap.Add(position, chunk);
    }
    */

    // Given a point in 3D space, add a new chunk to chunkHashMap which encapsulates that point.
    public void AddChunkFromPoint(Vector3 position)
    {
        Vector3 positionChunkCorner = GetNearestChunkCorner(position);

        chunkHashMap.Add(positionChunkCorner, new Chunk(positionChunkCorner, nXPerChunk, nYPerChunk, nZPerChunk, chunkGridSize, thresholdValue, chunkMaterial));
    }

    // Given a point in 3D space return the chunk which encapsulates that point (if present), other return null.
    public Chunk GetChunkFromPosition(Vector3 position)
    {
        Vector3 nearestChunkCenter = GetNearestChunkCorner(position);

        if (chunkHashMap.ContainsKey(nearestChunkCenter)) {return chunkHashMap[nearestChunkCenter]; }

        return null;
    }

    // Given the indices of a chunk, return the chunk.
    public Chunk GetChunkFromIndices(int[] index)
    {
        Vector3 position = new Vector3(index[0] * chunkXDimension, index[1] * chunkYDimension, index[2] * chunkZDimension);

        return GetChunkFromPosition(position);
    }

    // Given a point return the position of the corner of the chunk that encapsulates it.
    public Vector3 GetNearestChunkCorner(Vector3 position)
    {
        Vector3 returnVector = new Vector3(0f,0f,0f);

        returnVector.x = Mathf.Floor(position.x / chunkXDimension) * chunkXDimension;
        returnVector.y = Mathf.Floor(position.y / chunkYDimension) * chunkYDimension;
        returnVector.z = Mathf.Floor(position.z / chunkZDimension) * chunkZDimension;


        return returnVector;
    }

    // Given a chunk return it's chunk index.
    public int[] GetChunkIndex(Chunk chunk)
    {
        int [] returnArray = new int[3];

        returnArray[0] = Mathf.FloorToInt(chunk.positionChunkCorner.x / chunkXDimension);
        returnArray[1] = Mathf.FloorToInt(chunk.positionChunkCorner.y / chunkYDimension);
        returnArray[2] = Mathf.FloorToInt(chunk.positionChunkCorner.z / chunkZDimension);

        return returnArray;
    }

}

// Class which handles the chunks.
public class Chunk
{
    public Vector3 positionChunkCorner;
    public Mesh mesh;

    public ScalarFieldPoint[] scalarField;

    public Dictionary<Vector3, ScalarFieldPoint> scalarFieldDict;

    public float thresholdValue;

    GameObject chunkGameObject;
    GameObject marchingTerrain;

    int nX;
    int nY;
    int nZ;
    float gridSize;

    public bool chunkVisible;

    public Chunk(Vector3 positionChunkCorner, int nX, int nY, int nZ, float gridSize, float thresholdValue, Material material)
    {
        this.positionChunkCorner = positionChunkCorner;
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

        BuildFieldPointDictionary();

        RebuildChunkMesh();
    }

    // Re-marching cube the mesh given the scalarfield of the chunk.
    public void RebuildChunkMesh()
    {
        mesh = marchingTerrain.GetComponent<MarchingCubes>().GetMeshFromField(scalarField, thresholdValue);
        chunkGameObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    // WIP
    public void ChangeScalarField(float valueChange, Vector3 localPosition, int radius)
    {
        Vector3 fieldPointPosition = new Vector3(Mathf.RoundToInt(localPosition.x /  gridSize) * gridSize, Mathf.RoundToInt(localPosition.y / gridSize) * gridSize, Mathf.RoundToInt(localPosition.z / gridSize) * gridSize);
        

        ScalarFieldPoint changePoint = scalarFieldDict[fieldPointPosition];
        scalarFieldDict.Remove(fieldPointPosition);
        changePoint.potential += valueChange;

        scalarFieldDict.Add(fieldPointPosition,changePoint);
        scalarField = scalarFieldDict.Values.ToArray();

        RebuildChunkMesh();
    }

    public void InitializeScalarField()
    {
        scalarField = marchingTerrain.GetComponent<NoiseTerrain>().InitializeScalarField(nX, nY, nZ, gridSize, positionChunkCorner);
    }

    public void BuildFieldPointDictionary()
    {
        scalarFieldDict = new Dictionary<Vector3, ScalarFieldPoint>();

        foreach (ScalarFieldPoint x in scalarField)
        {
            scalarFieldDict.Add(x.position, x);
        }
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

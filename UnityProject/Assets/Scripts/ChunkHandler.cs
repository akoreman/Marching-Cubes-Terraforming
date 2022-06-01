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
    public Chunk AddChunkFromPoint(Vector3 position)
    {
        Vector3 positionChunkCorner = GetNearestChunkCorner(position);

        Chunk chunk = new Chunk(positionChunkCorner, nXPerChunk, nYPerChunk, nZPerChunk, chunkGridSize, thresholdValue, chunkMaterial);
        chunkHashMap.Add(positionChunkCorner, chunk);

        return chunk;
    }

    // Given a point in 3D space return the chunk which encapsulates that point (if present), other return null.
    public Chunk GetChunkFromPosition(Vector3 position)
    {
        Vector3 nearestChunkCenter = GetNearestChunkCorner(position);

        if (chunkHashMap.ContainsKey(nearestChunkCenter)) { return chunkHashMap[nearestChunkCenter]; }

        return null;
    }

    // Given the indices of a chunk, return the chunk.
    public Chunk GetChunkFromIndices(Vector3Int index)
    {
        Vector3 position = new Vector3(index.x * chunkXDimension, index.y * chunkYDimension, index.z * chunkZDimension);

        return GetChunkFromPosition(position);
    }

    // Given a point return the position of the corner of the chunk that encapsulates it.
    public Vector3 GetNearestChunkCorner(Vector3 position)
    {
        Vector3 returnVector = Vector3.zero;

        returnVector.x = Mathf.Floor(position.x / chunkXDimension) * chunkXDimension;
        returnVector.y = Mathf.Floor(position.y / chunkYDimension) * chunkYDimension;
        returnVector.z = Mathf.Floor(position.z / chunkZDimension) * chunkZDimension;


        return returnVector;
    }

    // Given a chunk return it's chunk index.
    public Vector3Int GetChunkIndex(Chunk chunk)
    {
        Vector3Int returnVector = Vector3Int.zero;

        returnVector.x = Mathf.FloorToInt(chunk.positionChunkCorner.x / chunkXDimension);
        returnVector.y = Mathf.FloorToInt(chunk.positionChunkCorner.y / chunkYDimension);
        returnVector.z = Mathf.FloorToInt(chunk.positionChunkCorner.z / chunkZDimension);

        return returnVector;
    }

    public Vector3Int GetChunkIndex(Vector3 positionChunkCorner)
    {
        Vector3Int returnVector = Vector3Int.zero;

        returnVector.x = Mathf.FloorToInt(positionChunkCorner.x / chunkXDimension);
        returnVector.y = Mathf.FloorToInt(positionChunkCorner.y / chunkYDimension);
        returnVector.z = Mathf.FloorToInt(positionChunkCorner.z / chunkZDimension);

        return returnVector;
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

    GameObject chunkHandler;

    int nX;
    int nY;
    int nZ;
    float gridSize;

    Vector3Int chunkIndex;

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
 

        this.chunkIndex = marchingTerrain.GetComponent<ChunkHandler>().GetChunkIndex(this);

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
    public void ChangeScalarField(float valueChange, Vector3 localPosition, int radius, bool overlap)
    {
        Vector3Int fieldPointIndex = new Vector3Int(Mathf.RoundToInt(localPosition.x /  gridSize), Mathf.RoundToInt(localPosition.y /  gridSize), Mathf.RoundToInt(localPosition.z /  gridSize));
        Vector3 fieldPointPosition = new Vector3(Mathf.RoundToInt(localPosition.x /  gridSize) * gridSize, Mathf.RoundToInt(localPosition.y / gridSize) * gridSize, Mathf.RoundToInt(localPosition.z / gridSize) * gridSize);
        
        ScalarFieldPoint changePoint = scalarFieldDict[fieldPointPosition];
        scalarFieldDict.Remove(fieldPointPosition);
        changePoint.potential += valueChange;

        scalarFieldDict.Add(fieldPointPosition,changePoint);
        scalarField = scalarFieldDict.Values.ToArray();

        RebuildChunkMesh();

        if (!overlap) { return; }

        MonoBehaviour.print(localPosition);

        //return;

        if (fieldPointIndex.x == 0 && this.GetNeighbour("back") != null)
        {
            //this.GetNeighbour("back").ChangeScalarField(valueChange, new Vector3(nX * gridSize, localPosition.y, localPosition.z), radius, false);
            this.GetNeighbour("back").ChangeScalarField(valueChange, new Vector3(localPosition.x, localPosition.y, localPosition.z), radius, false);
        }

        if (fieldPointIndex.y == 0 && this.GetNeighbour("bottom") != null)
        {
            //this.GetNeighbour("bottom").ChangeScalarField(valueChange, new Vector3(localPosition.x, nY * gridSize, localPosition.z), radius, false);
            this.GetNeighbour("bottom").ChangeScalarField(valueChange, new Vector3(localPosition.x, localPosition.y, localPosition.z), radius, false);

        }

        if (fieldPointIndex.z == 0 && this.GetNeighbour("right") != null)
        {
            //this.GetNeighbour("right").ChangeScalarField(valueChange, new Vector3( localPosition.x, localPosition.y, nZ * gridSize), radius, false);
            this.GetNeighbour("right").ChangeScalarField(valueChange, new Vector3( localPosition.x, localPosition.y, localPosition.z), radius, false);
        }

        if (fieldPointIndex.x == nX && this.GetNeighbour("forward") != null)
        {
            //this.GetNeighbour("forward").ChangeScalarField(valueChange, new Vector3(0f, localPosition.y, localPosition.z), radius, false);
            this.GetNeighbour("forward").ChangeScalarField(valueChange, new Vector3(localPosition.x, localPosition.y, localPosition.z), radius, false);
        }

        if (fieldPointIndex.y == nY && this.GetNeighbour("top") != null)
        {
            //this.GetNeighbour("top").ChangeScalarField(valueChange, new Vector3(localPosition.x, 0f, localPosition.z), radius, false);
            this.GetNeighbour("top").ChangeScalarField(valueChange, new Vector3(localPosition.x, localPosition.y, localPosition.z), radius, false);
        }

        if (fieldPointIndex.z == nZ && this.GetNeighbour("left") != null)
        {
            //this.GetNeighbour("left").ChangeScalarField(valueChange, new Vector3( localPosition.x, localPosition.y, 0f), radius, false);
            this.GetNeighbour("left").ChangeScalarField(valueChange, new Vector3( localPosition.x, localPosition.y, localPosition.z), radius, false);
        }
        

        

        
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

    public Chunk GetNeighbour(string direction)
    {
        Vector3Int returnIndex = this.chunkIndex;
        
        if (direction == "up") 
        { 
            returnIndex.y += 1;
            
            return  marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "down") 
        { 
            returnIndex.y -= 1;
            
            return  marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "forward") 
        { 
            returnIndex.x += 1;
            
            return  marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "back") 
        { 
            returnIndex.x -= 1;
            
            return  marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "left") 
        { 
            returnIndex.z -= 1;
            
            return  marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "right") 
        { 
            returnIndex.z += 1;
            
            return  marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        return null;
    }

}

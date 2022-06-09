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

    //Chunk[] neighbouringChunks;

    public Chunk(Vector3 positionChunkCorner, int nX, int nY, int nZ, float gridSize, float thresholdValue, Material material)
    {
        this.positionChunkCorner = positionChunkCorner;
        this.nX = nX ;
        this.nY = nY ;
        this.nZ = nZ ;
        this.gridSize = gridSize;

        this.chunkVisible = true;

        marchingTerrain = GameObject.Find("MarchingTerrain");
        this.chunkIndex = marchingTerrain.GetComponent<ChunkHandler>().GetChunkIndex(this);

        chunkGameObject = new GameObject("Chunk " + this.chunkIndex.x.ToString() + " " + this.chunkIndex.y.ToString() + " " + this.chunkIndex.z.ToString());
        chunkGameObject.AddComponent<MeshFilter>();
        chunkGameObject.AddComponent<MeshRenderer>();
        chunkGameObject.GetComponent<Renderer>().material = material;

        //neighbouringChunks = new Chunk[6];

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

    /*
    void tryFillNeighbourList()
    {

    }
    */

    // WIP

    List<ScalarFieldPoint> getPointsWithinRadius(Vector3 worldPosition, float radius)
    {
        List<ScalarFieldPoint> returnList = new List<ScalarFieldPoint>();

        Vector3 closestFieldPointPosition = new Vector3(Mathf.RoundToInt(worldPosition.x /  gridSize) * gridSize, Mathf.RoundToInt(worldPosition.y / gridSize) * gridSize, Mathf.RoundToInt(worldPosition.z / gridSize) * gridSize);

        if (Vector3.Distance(closestFieldPointPosition, worldPosition) > radius) { return returnList; }

        returnList.Add(scalarFieldDict[closestFieldPointPosition]);

        int numFieldPointsXPos = Mathf.FloorToInt((radius - (closestFieldPointPosition.x - worldPosition.x)) / gridSize);
        int numFieldPointsXNeg = Mathf.FloorToInt((radius + (closestFieldPointPosition.x - worldPosition.x)) / gridSize);
        int numFieldPointsYPos = Mathf.FloorToInt((radius - (closestFieldPointPosition.y - worldPosition.y)) / gridSize);
        int numFieldPointsYNeg = Mathf.FloorToInt((radius + (closestFieldPointPosition.y - worldPosition.y)) / gridSize);
        int numFieldPointsZPos = Mathf.FloorToInt((radius - (closestFieldPointPosition.z - worldPosition.z)) / gridSize);
        int numFieldPointsZNeg = Mathf.FloorToInt((radius + (closestFieldPointPosition.z - worldPosition.z)) / gridSize);

        for (int i = 1; i <= numFieldPointsXPos; i++) { returnList.Add(scalarFieldDict[closestFieldPointPosition + i * new Vector3(gridSize,0,0)]); }
        for (int i = 1; i <= numFieldPointsXNeg; i++) { returnList.Add(scalarFieldDict[closestFieldPointPosition - i * new Vector3(gridSize,0,0)]); }
        for (int i = 1; i <= numFieldPointsYPos; i++) { returnList.Add(scalarFieldDict[closestFieldPointPosition + i * new Vector3(0,gridSize,0)]); }
        for (int i = 1; i <= numFieldPointsYNeg; i++) { returnList.Add(scalarFieldDict[closestFieldPointPosition - i * new Vector3(0,gridSize,0)]); }
        for (int i = 1; i <= numFieldPointsZPos; i++) { returnList.Add(scalarFieldDict[closestFieldPointPosition + i * new Vector3(0,0,gridSize)]); }
        for (int i = 1; i <= numFieldPointsZNeg; i++) { returnList.Add(scalarFieldDict[closestFieldPointPosition - i * new Vector3(0,0,gridSize)]); }

        return returnList;
    }

    public void ChangeScalarFieldPoint(ScalarFieldPoint point, float value)
    {
        scalarFieldDict.Remove(point.position);
        point.potential += value;

        scalarFieldDict.Add(point.position, point);
        scalarField = scalarFieldDict.Values.ToArray();
    }

    public void ChangeScalarFieldRadius()
    {

    }



    public void ChangeScalarField(float valueChange, Vector3 worldPosition, float radius, bool overlap)
    {

        Vector3 localPosition = worldPosition - this.positionChunkCorner;

        //List<ScalarFieldPoint> changedPoints = getPointsWithinRadius(worldPosition, radius);

        Vector3Int fieldPointIndex = new Vector3Int(Mathf.RoundToInt(localPosition.x /  gridSize) % nX, Mathf.RoundToInt(localPosition.y /  gridSize) % nY, Mathf.RoundToInt(localPosition.z /  gridSize) % nZ);
        Vector3 fieldPointPosition = new Vector3(Mathf.RoundToInt(worldPosition.x /  gridSize) * gridSize, Mathf.RoundToInt(worldPosition.y / gridSize) * gridSize, Mathf.RoundToInt(worldPosition.z / gridSize) * gridSize);
        
        if (!overlap) 
        { 
            MonoBehaviour.print("Receiver chunkindex: " + this.chunkIndex);
            MonoBehaviour.print("Receiver field node position: " + worldPosition);
            MonoBehaviour.print("Receiver field node index: " + fieldPointIndex);
        }

        ScalarFieldPoint changePoint = scalarFieldDict[fieldPointPosition];
        scalarFieldDict.Remove(fieldPointPosition);
        changePoint.potential += valueChange;

        scalarFieldDict.Add(fieldPointPosition, changePoint);
        scalarField = scalarFieldDict.Values.ToArray();

        /*
        for (int i = 0; i <= radius; i++)
        {
            Vector3Int returnIndex = this.chunkIndex;

            for (int j = 0; j < 6; j++)
            {
                
                this.ChangeScalarField();
            }
        }
        */

        RebuildChunkMesh();

        

        if (!overlap) 
        { 
            return;
        }

        //return;
        MonoBehaviour.print("Calling chunkindex: " + this.chunkIndex);
        MonoBehaviour.print("Calling field node position: " + localPosition);
        MonoBehaviour.print("Calling field node index: " + fieldPointIndex);

        //return;
        
        if (fieldPointIndex.x == 0 && this.GetNeighbour("back") != null)
        {
            //this.GetNeighbour("back").ChangeScalarField(valueChange, new Vector3(nX * gridSize, localPosition.y, localPosition.z), radius, false);
            MonoBehaviour.print("back");
            this.GetNeighbour("back").ChangeScalarField(valueChange, worldPosition, radius, false);
            
        }

        if (fieldPointIndex.y == 0 && this.GetNeighbour("down") != null)
        {
            //this.GetNeighbour("bottom").ChangeScalarField(valueChange, new Vector3(localPosition.x, nY * gridSize, localPosition.z), radius, false);
            MonoBehaviour.print("down");
            this.GetNeighbour("down").ChangeScalarField(valueChange, worldPosition, radius, false);
            
        }

        if (fieldPointIndex.z == 0 && this.GetNeighbour("left") != null)
        {
            //this.GetNeighbour("right").ChangeScalarField(valueChange, new Vector3( localPosition.x, localPosition.y, nZ * gridSize), radius, false);
            MonoBehaviour.print("left");
            this.GetNeighbour("left").ChangeScalarField(valueChange, worldPosition, radius, false);
           
        }

        if (fieldPointIndex.x == nX && this.GetNeighbour("forward") != null)
        {
            //this.GetNeighbour("forward").ChangeScalarField(valueChange, new Vector3(0f, localPosition.y, localPosition.z), radius, false);
            MonoBehaviour.print("forward"); 
            this.GetNeighbour("forward").ChangeScalarField(valueChange, worldPosition, radius, false);
                      
        }

        if (fieldPointIndex.y == nY && this.GetNeighbour("up") != null)
        {
            //this.GetNeighbour("top").ChangeScalarField(valueChange, new Vector3(localPosition.x, 0f, localPosition.z), radius, false);
            MonoBehaviour.print("up");
            this.GetNeighbour("up").ChangeScalarField(valueChange, worldPosition, radius, false);
            
        }

        if (fieldPointIndex.z == nZ && this.GetNeighbour("right") != null)
        {
            //this.GetNeighbour("left").ChangeScalarField(valueChange, new Vector3( localPosition.x, localPosition.y, 0f), radius, false);
            MonoBehaviour.print("right");
            this.GetNeighbour("right").ChangeScalarField(valueChange, worldPosition, radius, false);
            
        }
        








        /*
        if (fieldPointIndex.x == 0 && this.GetNeighbour("back") != null)
        {
            this.GetNeighbour("back").ChangeScalarField(valueChange, new Vector3(nX * gridSize, localPosition.y, localPosition.z), radius, false);
        }

        if (fieldPointIndex.y == 0 && this.GetNeighbour("bottom") != null)
        {
            this.GetNeighbour("bottom").ChangeScalarField(valueChange, new Vector3(localPosition.x, nY * gridSize, localPosition.z), radius, false);
        }

        if (fieldPointIndex.z == 0 && this.GetNeighbour("right") != null)
        {
            this.GetNeighbour("right").ChangeScalarField(valueChange, new Vector3( localPosition.x, localPosition.y, nZ * gridSize), radius, false);
        }

        if (fieldPointIndex.x == nX && this.GetNeighbour("forward") != null)
        {
            this.GetNeighbour("forward").ChangeScalarField(valueChange, new Vector3(0f, localPosition.y, localPosition.z), radius, false);
        }

        if (fieldPointIndex.y == nY && this.GetNeighbour("top") != null)
        {
            this.GetNeighbour("top").ChangeScalarField(valueChange, new Vector3(localPosition.x, 0f, localPosition.z), radius, false);
        }

        if (fieldPointIndex.z == nZ && this.GetNeighbour("left") != null)
        {
            this.GetNeighbour("left").ChangeScalarField(valueChange, new Vector3( localPosition.x, localPosition.y, 0f), radius, false);
        }
        */

        

        
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
            
            return marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "down") 
        { 
            returnIndex.y -= 1;
            
            return marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "forward") 
        { 
            returnIndex.x += 1;
            
            return marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "back") 
        { 
            returnIndex.x -= 1;
            
            return marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "left") 
        { 
            returnIndex.z -= 1;
            
            return marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        if (direction == "right") 
        { 
            returnIndex.z += 1;
            
            return marchingTerrain.GetComponent<ChunkHandler>().GetChunkFromIndices(returnIndex);
        }

        MonoBehaviour.print("Invalid direction string.");

        return null;
    }

    public Dictionary<int, Vector3Int> numberToDirectionTranslationTable = new Dictionary<int, Vector3Int>{ 
                                                                                                    {0, new Vector3Int(1,0,0)}, 
                                                                                                    {1, new Vector3Int(-1,0,0)},
                                                                                                    {2, new Vector3Int(-1,0,0)}, 
                                                                                                    {3, new Vector3Int(-1,0,0)},
                                                                                                    {4, new Vector3Int(-1,0,0)}, 
                                                                                                    {5, new Vector3Int(-1,0,0)}
                                                                                                    };

}


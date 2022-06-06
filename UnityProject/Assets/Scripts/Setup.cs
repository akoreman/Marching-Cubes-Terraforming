using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

// This script sets up the field and updates the vertices.

public class Setup : MonoBehaviour
{
    GameObject marchingCubes;

    public int initialChunkDimension;
    public float drawDistance;

    public float clickDistance;

    public Camera camera;

    public int padding;

    Transform cameraTransform;
    
    float cameraPOV;
    float cameraAspect;

    // Get POV and aspect ratio of the active camera.
    void Awake()
    {
        cameraTransform = camera.transform;
        cameraPOV = camera.fieldOfView;
        cameraAspect = camera.aspect;
    }

    int nX;
    int nY;
    int nZ;

    float gridSize;

    void Start()
    {
        marchingCubes = this.gameObject;

        nX = marchingCubes.GetComponent<ChunkHandler>().nXPerChunk;
        nY = marchingCubes.GetComponent<ChunkHandler>().nYPerChunk;
        nZ = marchingCubes.GetComponent<ChunkHandler>().nZPerChunk;
        
        gridSize = marchingCubes.GetComponent<ChunkHandler>().chunkGridSize;
        
        // Spawn the chunks which are loaded on startup.
        for (int i = -1 * Mathf.RoundToInt(initialChunkDimension / 2); i < Mathf.RoundToInt(initialChunkDimension / 2); i++)
            for (int j = -1 * Mathf.RoundToInt(initialChunkDimension / 2); j < Mathf.RoundToInt(initialChunkDimension / 2); j++)
                for (int k = -1 * Mathf.RoundToInt(initialChunkDimension / 2); k < Mathf.RoundToInt(initialChunkDimension / 2); k++)
                    marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(i*(nX-1)*gridSize,j*(nY-1)*gridSize,k*(nZ-1)*gridSize));

        UpdateChunksInView();
    }

    void Update()
    {
        ClearActiveChunkDictionnary();
        UpdateChunksInView();

        // Handles the input which is used to change the underlying scalar field.
        if (Input.GetKeyDown("s"))
        {
            Vector3 clickPoint = cameraTransform.position + cameraTransform.forward.normalized * clickDistance;

            Chunk chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(clickPoint);

            //clickPoint.x = clickPoint.x;// % (nX * gridSize);
            //clickPoint.y = clickPoint.y;// % (nY * gridSize);
            //clickPoint.z = clickPoint.z;// % (nZ * gridSize);

            chunk.ChangeScalarField(-2.5f, clickPoint, 10, true);
        }

        if (Input.GetKeyDown("a"))
        {
            Vector3 clickPoint = cameraTransform.position + cameraTransform.forward.normalized * clickDistance;

            Chunk chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(clickPoint);

            //clickPoint.x = clickPoint.x;// % (nX * gridSize);
            //clickPoint.y = clickPoint.y;// % (nY * gridSize);
            //clickPoint.z = clickPoint.z;// % (nZ * gridSize);

            chunk.ChangeScalarField(2.5f, clickPoint, 10, true);
        }
    }

    // Method to first hide all chunks in the active chunk dictionary and clear the active chunk dictionary.
    void ClearActiveChunkDictionnary()
    {
        foreach (KeyValuePair<Vector3, Chunk> positionChunkPair in marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap)
            positionChunkPair.Value.HideChunk();

        marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap = new Dictionary<Vector3, Chunk>();   
    }

    // Method to calculate which chunks are within view, show those and add them to the active chunk dictionary.
    void UpdateChunksInView()
    {
        cameraPOV = camera.fieldOfView;
        cameraAspect = camera.aspect;

        // Convert the POV to degrees.
        cameraPOV *= Mathf.PI/180;

        // Calculate the height and witdh of the draw volume.
        float height = Mathf.Tan(cameraPOV / 2 ) * drawDistance;
        float width = height * cameraAspect;

        // Calcute the corner points of the draw volume and find or spawn the corresonding chunks.
        Vector3 midPoint = cameraTransform.position + cameraTransform.forward.normalized * drawDistance;
        Vector3 midUpPoint = midPoint + cameraTransform.up.normalized * height;
        Vector3 midDownPoint = midPoint - cameraTransform.up.normalized * height;

        Vector3[] pointArray = new Vector3[4];

        // The four corner points of the view volume.
        // leftDownPoint
        pointArray[0] = midPoint - cameraTransform.up.normalized * height - cameraTransform.right * width;
        // rightDownPoint
        pointArray[1] = midPoint - cameraTransform.up.normalized * height + cameraTransform.right * width;
        // leftUpPoint
        pointArray[2] = midPoint + cameraTransform.up.normalized * height - cameraTransform.right * width;
        // rightUpPoint
        pointArray[3] = midPoint + cameraTransform.up.normalized * height + cameraTransform.right * width;


        Chunk chunk;
        
        Vector3Int chunkIndexLeftDown = Vector3Int.zero;
        Vector3Int chunkIndexRightUp = Vector3Int.zero;
        
        // Create the four corner chunks.
        for (int i = 0; i < 4; i++)
        {
            chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(pointArray[i]);

            if (chunk == null) { chunk = marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(pointArray[i]); }

            chunk.ShowChunk();
            marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap.Add(pointArray[i], chunk);

            if (i == 0) { chunkIndexLeftDown = marchingCubes.GetComponent<ChunkHandler>().GetChunkIndex(chunk); }
            if (i == 3) { chunkIndexRightUp = marchingCubes.GetComponent<ChunkHandler>().GetChunkIndex(chunk); }
        }

        // Find and draw the chunks to connect the corner points found above.
        for (int i = Mathf.Min(chunkIndexLeftDown.x,chunkIndexRightUp.x) - padding; i <= Mathf.Max(chunkIndexLeftDown.x, chunkIndexRightUp.x) + padding; i++)
            for (int j =Mathf.Min(chunkIndexLeftDown.y,chunkIndexRightUp.y) - padding; j <= Mathf.Max(chunkIndexLeftDown.y, chunkIndexRightUp.y) + padding; j++)
                for (int k = Mathf.Min(chunkIndexLeftDown.z,chunkIndexRightUp.z) - padding; k <= Mathf.Max(chunkIndexLeftDown.z, chunkIndexRightUp.z) + padding; k++)
                {
                    Vector3 point = new Vector3(i*(nX-1)*gridSize, j*(nY-1)*gridSize, k*(nY-1)*gridSize);

                    chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(point);

                    if (chunk == null) { chunk =marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(point); }

                    chunk.ShowChunk();
                    marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap.Add(point, chunk);
                }

    }

    
}



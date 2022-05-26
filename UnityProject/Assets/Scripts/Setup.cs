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

        for (int i = 0; i < initialChunkDimension; i++)
            for (int j = 0; j < initialChunkDimension; j++)
                for (int k = 0; k < initialChunkDimension; k++)
                    marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(i*(nX-1)*gridSize,j*(nY-1)*gridSize,k*(nZ-1)*gridSize));

        UpdateChunksInView();
    }

    void Update()
    {
        ClearActiveChunkDictionnary();
        UpdateChunksInView();

        // Handles the input which is used to change the underlying scalar field.
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Vector3 clickPoint = cameraTransform.position + cameraTransform.forward.normalized * clickDistance;


            Chunk chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(clickPoint);

            clickPoint.x = clickPoint.x % (nX * gridSize);
            clickPoint.y = clickPoint.y % (nY * gridSize);
            clickPoint.z = clickPoint.z % (nZ * gridSize);

            chunk.ChangeScalarField(2f, clickPoint, 10, true);
        }
    }

    // Method to first hide all chunks in the active chunk dictionary and clear the active chunk dictionary.
    void ClearActiveChunkDictionnary()
    {
        foreach (KeyValuePair<Vector3, Chunk> x in marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap)
            x.Value.HideChunk();

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

        Chunk chunk;
        
        Vector3Int chunkIndexLeftDown;
        Vector3 leftDownPoint = midPoint - cameraTransform.up.normalized * height - cameraTransform.right * width;
        
        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(leftDownPoint);

        if (chunk == null) 
        { 
            marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(leftDownPoint); 
            chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(leftDownPoint); 
        }

        chunk.ShowChunk();
        marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap.Add(leftDownPoint, chunk);



        chunkIndexLeftDown = marchingCubes.GetComponent<ChunkHandler>().GetChunkIndex(chunk);

        Vector3 rightDownPoint = midPoint - cameraTransform.up.normalized * height + cameraTransform.right * width;

        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(rightDownPoint);

        if (chunk == null) 
        {
             marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(rightDownPoint); 
             chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(rightDownPoint);
        }

        chunk.ShowChunk();
        marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap.Add(rightDownPoint, chunk);



        Vector3 leftUpPoint = midPoint + cameraTransform.up.normalized * height - cameraTransform.right * width;

        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(leftUpPoint);

        if (chunk == null)
        {  
            marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(leftUpPoint); 
            chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(leftUpPoint);
        }

        chunk.ShowChunk();
        marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap.Add(leftUpPoint, chunk);


        Vector3Int chunkIndexRightUp;
        Vector3 rightUpPoint = midPoint + cameraTransform.up.normalized * height + cameraTransform.right * width;

        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(rightUpPoint);

        if (chunk == null) 
        {
            marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(rightUpPoint); 
            chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(rightUpPoint);
        }

        chunk.ShowChunk();
        marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap.Add(rightUpPoint, chunk);


        chunkIndexRightUp = marchingCubes.GetComponent<ChunkHandler>().GetChunkIndex(chunk);


        // Find and draw the chunks to connect the corner points found above.
        for (int i = Mathf.Min(chunkIndexLeftDown.x,chunkIndexRightUp.x) - padding; i <= Mathf.Max(chunkIndexLeftDown.x, chunkIndexRightUp.x) + padding; i++)
            for (int j =Mathf.Min(chunkIndexLeftDown.y,chunkIndexRightUp.y) - padding; j <= Mathf.Max(chunkIndexLeftDown.y, chunkIndexRightUp.y) + padding; j++)
                for (int k = Mathf.Min(chunkIndexLeftDown.z,chunkIndexRightUp.z) - padding; k <= Mathf.Max(chunkIndexLeftDown.z, chunkIndexRightUp.z) + padding; k++)
                {
                    Vector3 point = new Vector3(i*(nX-1)*gridSize, j*(nY-1)*gridSize, k*(nY-1)*gridSize);

                    chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(point);

                    if (chunk == null)
                    { 
                        marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(point); 
                        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(point);
                    }

                    chunk.ShowChunk();
                    marchingCubes.GetComponent<ChunkHandler>().activeChunkHashMap.Add(point, chunk);
                }



    }

    
}



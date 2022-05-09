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

    public Camera camera;

    Transform cameraTransform;
    
    float cameraPOV;
    float cameraAspect;

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
        
        for (int i = 0; i < initialChunkDimension; i++)
            for (int j = 0; j < initialChunkDimension; j++)
                for (int k = 0; k < initialChunkDimension; k++)
                    marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(i*(nX-1)*gridSize,j*(nY-1)*gridSize,k*(nZ-1)*gridSize));
    }

    void Update()
    {
        cameraPOV = camera.fieldOfView;
        cameraAspect = camera.aspect;

        cameraPOV *= Mathf.PI/180;

        float height = Mathf.Tan(cameraPOV / 2 ) * drawDistance;
        float width = height * cameraAspect;

        Vector3 midPoint = cameraTransform.position + cameraTransform.forward.normalized * drawDistance;
        Vector3 midUpPoint = midPoint + cameraTransform.up.normalized * height;
        Vector3 midDownPoint = midPoint - cameraTransform.up.normalized * height;

        Chunk chunk;
        


        int[] chunkIndexLeftDown;
        Vector3 leftDownPoint = midPoint - cameraTransform.up.normalized * height - cameraTransform.right * width;
        
        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(leftDownPoint);

        if (chunk == null) 
        { 
            marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(leftDownPoint); 
            chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(leftDownPoint);
        }

        chunkIndexLeftDown = marchingCubes.GetComponent<ChunkHandler>().GetChunkIndex(chunk);



        Vector3 rightDownPoint = midPoint - cameraTransform.up.normalized * height + cameraTransform.right * width;

        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(rightDownPoint);

        if (chunk == null) 
        {
             marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(rightDownPoint); 
             chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(rightDownPoint);
        }


        Vector3 leftUpPoint = midPoint + cameraTransform.up.normalized * height - cameraTransform.right * width;

        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(leftUpPoint);

        if (chunk == null)
        {  
            marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(leftUpPoint); 
            chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(leftUpPoint);
        }


        int[] chunkIndexRightUp;
        Vector3 rightUpPoint = midPoint + cameraTransform.up.normalized * height + cameraTransform.right * width;

        chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(rightUpPoint);

        if (chunk == null) 
        {
            marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(rightUpPoint); 
            chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(rightUpPoint);
        }

        chunkIndexRightUp = marchingCubes.GetComponent<ChunkHandler>().GetChunkIndex(chunk);

        //print(chunkIndexRightUp[0]);

        for (int i = Mathf.Min(chunkIndexLeftDown[0],chunkIndexRightUp[0]) ; i < Mathf.Max(chunkIndexLeftDown[0], chunkIndexRightUp[0]); i++)
            for (int j =Mathf.Min(chunkIndexLeftDown[1],chunkIndexRightUp[1]) ; j < Mathf.Max(chunkIndexLeftDown[1], chunkIndexRightUp[1]); j++)
                for (int k = Mathf.Min(chunkIndexLeftDown[2],chunkIndexRightUp[2]) ; k < Mathf.Max(chunkIndexLeftDown[2], chunkIndexRightUp[2]); k++)
                {
                    Vector3 point = new Vector3(i*(nX-1)*gridSize, j*(nY-1)*gridSize, k*(nY-1)*gridSize);

                    //print(point);
                    
                    chunk = marchingCubes.GetComponent<ChunkHandler>().GetChunkFromPosition(point);

                    if (chunk == null) { marchingCubes.GetComponent<ChunkHandler>().AddChunkFromPoint(point); }
                }

    }

    
}



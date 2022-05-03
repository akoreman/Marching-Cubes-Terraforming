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

    Camera camera;


    void Start()
    {
        camera = camera.current;
        marchingCubes = this.gameObject;

        
        int nX = marchingCubes.GetComponent<ChunkHandler>().nXPerChunk;
        int nY = marchingCubes.GetComponent<ChunkHandler>().nYPerChunk;
        int nZ = marchingCubes.GetComponent<ChunkHandler>().nZPerChunk;
        
        float gridSize = marchingCubes.GetComponent<ChunkHandler>().chunkGridSize;
        

        for (int i = 0; i < initialChunkDimension; i++)
            for (int j = 0; j < initialChunkDimension; j++)
                for (int k = 0; k < initialChunkDimension; k++)
                    marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(i*(nX-1)*gridSize,j*(nY-1)*gridSize,k*(nZ-1)*gridSize));

        
    }

    void Update()
    {
        Vector3 drawPoint = camera.transform.forward + camera.transform.forward.normalized * drawDistance;

        

    }

    
}



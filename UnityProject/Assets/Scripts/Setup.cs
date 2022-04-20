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


    void Start()
    {
        marchingCubes = this.gameObject;
        //marchingCubes.GetComponent<NoiseTerrain>().BuildScalarField(nX,nY,nZ, gridSize);
        marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(0f,0f,0f));

        marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(9.5f,0f,0f));

        marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(-9.5f,0f,0f));

        marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(0f,0f,9.5f));

        marchingCubes.GetComponent<ChunkHandler>().AddChunk(new Vector3(0f,0f,-9.5f));


        
    }



    
}



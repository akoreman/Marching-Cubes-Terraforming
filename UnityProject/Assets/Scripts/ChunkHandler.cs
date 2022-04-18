using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{
    Dictionary<Vector3, Chunk> chunkHashMap = new Dictionary<Vector3, Chunk>();

    void AddChunk(Vector3 positionChunkCenter)
    {
        chunkHashMap.Add(positionChunkCenter, new Chunk(positionChunkCenter));
    }

}

public class Chunk
{
    public Vector3 positionChunkCenter;
    public Mesh mesh;


    public Chunk(Vector3 positionChunkCenter)
    {
        this.positionChunkCenter = positionChunkCenter;
    }

    /*

    void UpdateChunkMesh()
    {

    }

    



    */


}

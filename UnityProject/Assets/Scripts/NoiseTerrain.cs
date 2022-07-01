using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;


// Handles the construction and updating of the scalar field using the Job system.
// TODO make sure scalar fields of neighbouring chunks overlap.
public class NoiseTerrain : MonoBehaviour
{
    public float fieldExponent = 1.0f;

    public ScalarFieldPoint[] InitializeScalarField(int nX, int nY, int nZ, float gridSize, Vector3 centerOffset)
    {
        nX++;
        nY++;
        nZ++;

        ScalarFieldPoint[] scalarField = new ScalarFieldPoint[nX * nY * nZ];
        //ScalarFieldPoint[] scalarField = new ScalarFieldPoint[(nX+1) * (nY+1) * (nZ+1)];

        NativeHashMap<int, ScalarFieldPoint> scalarFieldMap = new NativeHashMap<int, ScalarFieldPoint>(nX * nY * nZ, Allocator.TempJob);
        //NativeHashMap<int, ScalarFieldPoint> scalarFieldMap = new NativeHashMap<int, ScalarFieldPoint>((nX+1) * (nY+1) * (nZ+1), Allocator.TempJob);

        UpdatePotentialJob potentialModificationJob;
        
        // Create the job instance which handles the updating of the scalar field.
        potentialModificationJob = new UpdatePotentialJob()
        {
            nX = nX,
            nY = nY,
            nZ = nZ,
            gridSize = gridSize,
            centerOffset = centerOffset,
            ScalarFieldWriter = scalarFieldMap.AsParallelWriter(),
            fieldExponent = fieldExponent
        };
        
        /*
        potentialModificationJob = new UpdatePotentialJob()
        {
            nX = nX + 1,
            nY = nY + 1,
            nZ = nZ + 1,
            gridSize = gridSize,
            centerOffset = centerOffset,
            ScalarFieldWriter = scalarFieldMap.AsParallelWriter(),
            fieldExponent = fieldExponent
        };
        */
        JobHandle potentialModificationJobHandle = potentialModificationJob.Schedule(nX * nY * nZ, default);
        //JobHandle potentialModificationJobHandle = potentialModificationJob.Schedule((nX+1) * (nY+1) * (nZ+1), default);

        potentialModificationJobHandle.Complete();

        
        for (int i = 0; i < (nX * nY * nZ); i++)
            scalarField[i] = scalarFieldMap[i];
        
        /*
        for (int i = 0; i < ((nX+1) * (nY+1) * (nZ+1)); i++)
            scalarField[i] = scalarFieldMap[i];
        */
        scalarFieldMap.Dispose();

        return scalarField;
    }

    // The job which handles the scalar field construction.
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct UpdatePotentialJob : IJobParallelFor
    {
        [ReadOnly]
        public int nX;

        [ReadOnly]
        public int nY;

        [ReadOnly]
        public int nZ;

        [ReadOnly]
        public float gridSize;

        [ReadOnly]
        public Vector3 centerOffset;

        [WriteOnly]
        public NativeHashMap<int, ScalarFieldPoint>.ParallelWriter ScalarFieldWriter;

        [ReadOnly]
        public float fieldExponent;

        public void Execute(int i)
        {
            BuildScalarField(i);
        }

        void BuildScalarField(int i)
        {
            //Position position = GetCoordsFromLinear(i);
            Vector3Int positionIndex = GetCoordsFromLinear(i); 

            ScalarFieldPoint scalarFieldPoint;

            scalarFieldPoint.position = new Vector3(positionIndex.x * gridSize, positionIndex.y * gridSize, positionIndex.z * gridSize) + centerOffset;
            //scalarFieldPoint.potential = scalarFieldPoint.position.y;
            
            float noise = Mathf.PerlinNoise(scalarFieldPoint.position.x/50, scalarFieldPoint.position.z/50) * 100;

            if (scalarFieldPoint.position.y > noise)
            {
                scalarFieldPoint.potential = 1.0f;
                //scalarFieldPoint.potential = noise;
            } 
            else
            {
                scalarFieldPoint.potential = 0.0f; 
            }
            
            ScalarFieldWriter.TryAdd(i, scalarFieldPoint);
        }

        Vector3Int GetCoordsFromLinear(int index)
        {
            Vector3Int output = Vector3Int.zero;

            output.x = index / (nY * nZ);
            output.y = (index % (nY * nZ)) / nZ;
            output.z = index % nZ;

            return output;
        }
        
        /*
        Vector3Int GetCoordsFromLinear(int index)
        {
            Vector3Int output = Vector3Int.zero;

            output.x = index / ((nY+1) * (nZ+1));
            output.y = (index % ((nY+1) * (nZ+1))) / (nZ+1);
            output.z = index % (nZ+1);

            return output;
        }
        */
    }
}

public struct ScalarFieldPoint
{
    public Vector3 position;
    public float potential;
}


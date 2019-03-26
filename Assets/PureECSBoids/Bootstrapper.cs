using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

public class Bootstrapper : MonoBehaviour
{
    public float spawnArea = 50;

    public int nbAgents = 10;

    public Mesh boidMesh;
    public Material boidMaterial;

    void Start()
    {
        EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

        Unity.Mathematics.Random random = new Unity.Mathematics.Random(666);

        for(int i = 0; i < nbAgents; ++i)
        {
            Entity boidEntity = entityManager.CreateEntity(
            ComponentType.Create<PureBoidData>(),
            ComponentType.Create<Position>(),
            ComponentType.Create<Rotation>(),
            ComponentType.Create<RenderMesh>());

            entityManager.SetComponentData(boidEntity, new PureBoidData
            {
                speed = 15f
            });

            entityManager.SetComponentData(boidEntity, new Position
            {
                Value = new float3(random.NextFloat(-spawnArea, spawnArea), random.NextFloat(-spawnArea, spawnArea), random.NextFloat(-spawnArea, spawnArea))
            });

            entityManager.SetComponentData(boidEntity, new Rotation
            {
                Value = quaternion.Euler(
                    random.NextFloat(-180.0f, 180.0f),
                    random.NextFloat(-180.0f, 180.0f),
                    random.NextFloat(-180.0f, 180.0f)
                )
            });

            entityManager.SetSharedComponentData(boidEntity, new RenderMesh
            {
                mesh = boidMesh,
                material = boidMaterial
            });
        }        
    }
}


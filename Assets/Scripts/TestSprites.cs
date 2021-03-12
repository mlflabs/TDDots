using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Example of spawning Sprites using quad mesh and material.
/// </summary>
public class TestSprites : MonoBehaviour
{
    // Amount of entities to spawn.
    [SerializeField]
    private int entitiesToSpawn = 10;

    // Reference to the sprite mesh - quad.
    [SerializeField]
    private Mesh spriteMesh;

    // Reference to the material with sprite texture.
    [SerializeField]
    private Material spriteMaterial;

    /// <summary>
    /// Unity method called on the first frame.
    /// </summary>
    void Start()
    {
        GenerateEntities();
    }

    /// <summary>
    /// Generating example.
    /// </summary>
    public void GenerateEntities()
    {
        // Storing reference to entity manager.
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Creating temp array for entities.
        NativeArray<Entity> spriteEntities = new NativeArray<Entity>(entitiesToSpawn, Allocator.Temp);

        // Creating archetype for sprite.
        var spriteArchetype = entityManager.CreateArchetype(
                //////typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(Translation)
            );

        // Creting entities.
        entityManager.CreateEntity(spriteArchetype, spriteEntities);

        // Creating Randomness.
        var rnd = new Unity.Mathematics.Random((uint)System.DateTime.UtcNow.Ticks);

        // Looping over entities
        for (int i = 0; i < entitiesToSpawn; i++)
        {
            var spriteEntity = spriteEntities[i];

            // Assigning values to the renderer.
            ////////////entityManager.SetSharedComponentData(spriteEntity, new RenderMesh { mesh = spriteMesh, material = spriteMaterial });

            // Assigning random position.
            entityManager.SetComponentData(spriteEntity, new Translation { Value = rnd.NextFloat3(new float3(-5, -3, 0), new float3(5, 3, 0)) });
        }

        // Clearing native array for entities.
        spriteEntities.Dispose();
    }
}
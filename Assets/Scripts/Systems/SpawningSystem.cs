using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[AlwaysSynchronizeSystem]
public class SpawningSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //Creating new entity, so need buffered commands
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        Entities.ForEach((ref TimeIntervalData timer, in SpawnerData spawner, in Translation position) =>
        {
            //Can't shoot when cooldown is still active
            if (timer.time < timer.interval) { return; }

            //Spawn new entity with random position
            var newEntity = ecb.Instantiate(spawner.prefab);
            var newPositionValue = new float3(GetRandomInRange(spawner.spawnAreaHorizontal), position.Value.y, position.Value.z);
            Translation newPos = new Translation()
            {
                Value = newPositionValue
            };
            ecb.AddComponent<Translation>(newEntity, newPos);

            //Reset data
            timer.time = 0;
        }).Run();

        //Execute buffered commands and dispose of buffer
        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }

    static float GetRandomInRange(float2 range)
    {
        return UnityEngine.Random.Range(range.x, range.y);
    }
}
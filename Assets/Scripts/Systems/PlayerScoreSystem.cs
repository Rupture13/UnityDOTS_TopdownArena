using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class PlayerScoreSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        Entities.WithoutBurst().WithAll<PlayerScoreTag>().ForEach((Entity player) =>
        {
            GameManager.main.IncreasePlayerScore(10);
            ecb.RemoveComponent<PlayerScoreTag>(player);
        }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }
}
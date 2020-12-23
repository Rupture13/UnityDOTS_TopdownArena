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
        //Removing component from entity (structural change), so buffer is needed
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        //"Run" and "WithoutBurst" due to calling GameManager singleton
        Entities.WithoutBurst().WithAll<PlayerScoreTag>().ForEach((Entity player) =>
        {
            //Calling GameManager to actually do the logic and UI
            GameManager.main.IncreasePlayerScore(10);
            ecb.RemoveComponent<PlayerScoreTag>(player);
        }).Run();

        //Run buffered commands and dispose of buffer afterwards
        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }
}
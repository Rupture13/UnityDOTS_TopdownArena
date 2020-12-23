using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[AlwaysSynchronizeSystem]
public class PowerUpSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        Entities.ForEach((Entity entity, ref TimeIntervalData timer, ref PowerUpData powerUp) =>
        {
            var remainingPowerUpTime = powerUp.powerUpDuration - deltaTime;
            if (remainingPowerUpTime <= 0)
            {
                timer.interval = powerUp.originalShootingInterval;
                ecb.RemoveComponent<PowerUpData>(entity);
            }
            else
            {
                timer.interval = powerUp.newShootingInterval;
                powerUp.powerUpDuration = remainingPowerUpTime;
            }
        }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }
}
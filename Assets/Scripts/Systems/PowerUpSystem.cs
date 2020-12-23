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
        //Cache deltaTime now, instead of calling it in the ForEach (which it doesn't like)
        float deltaTime = Time.DeltaTime;
        //Possibly removing component from entity (structural change), so buffer is needed
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        Entities.ForEach((Entity entity, ref TimeIntervalData timer, ref PowerUpData powerUp) =>
        {
            //Decrease remaining powerUp time by subtracting deltaTime
            var remainingPowerUpTime = powerUp.powerUpDuration - deltaTime;
            //If powerUp effect is over, restore original interval setting and remove powerup component
            if (remainingPowerUpTime <= 0)
            {
                timer.interval = powerUp.originalShootingInterval;
                ecb.RemoveComponent<PowerUpData>(entity);
            }
            else //Else make sure interval setting is the powerUp setting
                //(technically only needed first frame of powerup effect, but this still seemed cleanest)
            {
                timer.interval = powerUp.newShootingInterval;
                powerUp.powerUpDuration = remainingPowerUpTime;
            }
        }).Run();

        //Run any buffered commands and dispose of buffer
        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }
}
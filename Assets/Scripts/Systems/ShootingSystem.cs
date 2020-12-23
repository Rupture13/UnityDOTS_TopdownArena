using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[AlwaysSynchronizeSystem]
[UpdateBefore(typeof(PlayerInputSystem))]
public class ShootingSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //Creating new entity, so need buffered commands
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        Entities.ForEach((ref TimeIntervalData timer, in ShootingData shooter, in Translation playerPos) =>
        {
            //Can't shoot when cooldown is still active
            if (timer.time < timer.interval) { return; }

            //Can't shoot when nothing told the manual shooting to actually shoot
            if (shooter.manualShooting && !shooter.canShoot) { return; }

            //Spawn new bullet with location of shooting entity
            var newEntity = ecb.Instantiate(shooter.bulletPrefab);
            Translation newPos = new Translation()
            {
                Value = playerPos.Value
            };
            ecb.AddComponent<Translation>(newEntity, newPos);

            //Reset data
            timer.time = 0;

        }).Run();

        //Run buffered commands and dispose of buffer
        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }
}
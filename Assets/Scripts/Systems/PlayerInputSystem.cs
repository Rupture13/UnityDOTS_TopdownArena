using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
public class PlayerInputSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        Entities.ForEach((ref MovementData movement, ref ShootingData shooter, in InputData input) => 
        {
            //Set movement data based on player movement input
            movement.direction = int2.zero;
            movement.direction.x += Input.GetKey(input.rightKey) ? 1 : 0;
            movement.direction.x -= Input.GetKey(input.leftKey) ? 1 : 0;

            shooter.canShoot = Input.GetKey(input.fireKey);
        }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }
}

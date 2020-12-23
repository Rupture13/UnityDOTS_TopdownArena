using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[AlwaysSynchronizeSystem]
public class MovementSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Translation position, in MovementData movement) => 
        {
            //Change x and z position values with direction*speed, clamped between bounds limits
            position.Value.x = math.clamp(position.Value.x + (movement.direction.x * movement.speed * deltaTime), 
                                        movement.boundsHorizontal.x, movement.boundsHorizontal.y);
            position.Value.z = math.clamp(position.Value.z + (movement.direction.y * movement.speed * deltaTime),
                                        movement.boundsVertical.x, movement.boundsVertical.y);
        }).Run();

        return default;
    }
}

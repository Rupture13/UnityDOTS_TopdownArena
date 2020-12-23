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

        Entities.ForEach((ref Translation location, in MovementData movement) => 
        {
            location.Value.x = math.clamp(location.Value.x + (movement.direction.x * movement.speed * deltaTime), 
                                        movement.boundsHorizontal.x, movement.boundsHorizontal.y);
            location.Value.z = math.clamp(location.Value.z + (movement.direction.y * movement.speed * deltaTime),
                                        movement.boundsVertical.x, movement.boundsVertical.y);
        }).Run();

        return default;
    }
}

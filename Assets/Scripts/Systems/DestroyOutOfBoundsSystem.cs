﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[AlwaysSynchronizeSystem]
public class DestroyOutOfBoundsSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        Entities
            .WithAll<DestroyOnOutOfBoundsTag>() //We don't read from this component, so only checking presence
            .ForEach((Entity entity, in Translation position, in MovementData movement) => 
            {
                //If entity is out of bounds, delete it (as buffered command)
                if (!IsBetweenBounds(position.Value.x, movement.boundsHorizontal) 
                    || !IsBetweenBounds(position.Value.z, movement.boundsVertical))
                {
                    ecb.DestroyEntity(entity);
                }
            }).Run();

        //Run buffered commands and dispose of buffer
        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }

    /// <summary>
    /// Determines whether or not the provided value is within the provided bounds
    /// </summary>
    /// <param name="evaluated">The position value to be evaluated</param>
    /// <param name="bounds">The bounds to be evaluated on</param>
    /// <returns>True if the value is within bounds (exclusive), and false if the value is out of bounds</returns>
    public static bool IsBetweenBounds(float evaluated, float2 bounds)
    {
        return (evaluated > bounds.x && evaluated < bounds.y);
    }
}

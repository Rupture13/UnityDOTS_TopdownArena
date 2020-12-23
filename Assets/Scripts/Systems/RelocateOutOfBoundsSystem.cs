﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[AlwaysSynchronizeSystem]
public class RelocateOutOfBoundsSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entities.ForEach((ref Translation position, in RelocateOnOutOfBoundsData relocation, in MovementData movement) =>
        {
            if (!DestroyOutOfBoundsSystem.IsBetweenBounds(position.Value.x, movement.boundsHorizontal)
                    || !DestroyOutOfBoundsSystem.IsBetweenBounds(position.Value.z, movement.boundsVertical))
            {
                position.Value = relocation.relocationPosition;
            }
        }).Run();

        return default;
    }
}
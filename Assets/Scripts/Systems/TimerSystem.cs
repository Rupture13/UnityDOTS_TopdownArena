using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class TimerSystem : JobComponentSystem
{
    //Simple timer system that keeps count of a time
    //To be checked with this same component's interval to see if something should happened
    //Checking that happens in other systems
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = Time.DeltaTime;

        Entities.ForEach((ref TimeIntervalData timer) => 
        {
            timer.time += deltaTime;
        }).Run();

        return default;
    }
}
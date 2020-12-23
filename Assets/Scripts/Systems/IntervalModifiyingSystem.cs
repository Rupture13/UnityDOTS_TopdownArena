using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class IntervalModifiyingSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = Time.DeltaTime;

        Entities.ForEach((ref TimeIntervalData timer, in IntervalModifierOverTimeData modifierData) => 
        {
            timer.interval = math.clamp(timer.interval + modifierData.modifierPerSecond * deltaTime, 
                                        modifierData.clampRange.x, modifierData.clampRange.y);
        }).Run();

        return default;
    }
}
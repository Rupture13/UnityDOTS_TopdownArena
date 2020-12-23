using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[AlwaysSynchronizeSystem]
public class PlayerHealthSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);
        bool killPlayer = false;

        Entities.ForEach((Entity player, ref HealthData playerHealth, in DamageData incomingDamage) =>
        {
            var newHealth = playerHealth.health - incomingDamage.damage;
            if (newHealth <= 0)
            {
                killPlayer = true;
            }
            else
            {
                playerHealth.health = newHealth;
                ecb.RemoveComponent<DamageData>(player);
            }
        }).Run();

        if (killPlayer)
        {
            Entities.WithAll<PlayerTag>().ForEach((Entity entity) =>
            {
                ecb.DestroyEntity(entity);
            }).Run();
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }
}
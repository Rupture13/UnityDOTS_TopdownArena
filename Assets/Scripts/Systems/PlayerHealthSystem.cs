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

        //Without burst because we call our gamemanager singleton.
        //This loop won't run often, so this is fine
        Entities.WithoutBurst().ForEach((Entity player, ref HealthData playerHealth, in DamageData incomingDamage) =>
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
            GameManager.main.UpdatePlayerHealth(math.max(0, newHealth));
        }).Run();

        //If player has no health, delete all entities for possible replay session
        //Deleting all entities instead of disposing of world,
        //because disposing of the world removes all systems too,
        //which, unlike entities, don't get instantiated on scene load
        if (killPlayer)
        {
            var entityManager = EntityManager;

            //Without burst because we call EntityManager.UniversalQuery
            Entities.WithoutBurst().WithAll<PlayerTag>().ForEach((Entity entity) =>
            {
                ecb.DestroyEntity(entityManager.UniversalQuery);
            }).Run();
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();

        return default;
    }
}
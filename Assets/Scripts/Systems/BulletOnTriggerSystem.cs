using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
[AlwaysSynchronizeSystem]
public class BulletOnTriggerSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    struct BulletOnTriggerSystemJob : ITriggerEventsJob
    {
        //Collections of entities with components
        //ReadOnly so that threads don't have to wait for eachother
        [ReadOnly] public ComponentDataFromEntity<PlayerBulletTag> allPlayerBullets;
        [ReadOnly] public ComponentDataFromEntity<EnemyBulletTag> allEnemyBullets;
        [ReadOnly] public ComponentDataFromEntity<InputData> allPlayers;
        [ReadOnly] public ComponentDataFromEntity<EnemyTag> allEnemies;
        [ReadOnly] public ComponentDataFromEntity<PowerUpTag> allPowerUps;
        [ReadOnly] public ComponentDataFromEntity<TimeIntervalData> allTimers;

        public EntityCommandBuffer ecb;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            //Don't do anything if it's just bullets colliding
            if ((allPlayerBullets.Exists(entityA) && allPlayerBullets.Exists(entityB)) 
                || (allEnemyBullets.Exists(entityA) && allEnemyBullets.Exists(entityB))) { return; }

            //If playerBullet hits enemy, destroy both
            if ((allPlayerBullets.Exists(entityA) && allEnemies.Exists(entityB)) 
                || (allPlayerBullets.Exists(entityB) && allEnemies.Exists(entityA)))
            {
                ecb.DestroyEntity(entityA);
                ecb.DestroyEntity(entityB);
            }

            //If enemy or enemybullet hits player, destroy enemy/enemybullet and damage player
            if (allPlayers.Exists(entityA) && (allEnemies.Exists(entityB) || allEnemyBullets.Exists(entityB)))
            {
                DamageData playerDamage = new DamageData() { damage = 1 };
                ecb.AddComponent<DamageData>(entityA, playerDamage);

                ecb.DestroyEntity(entityB);
            }
            else if (allPlayers.Exists(entityB) && (allEnemies.Exists(entityA) || allEnemyBullets.Exists(entityA)))
            {
                DamageData playerDamage = new DamageData() { damage = 1 };
                ecb.AddComponent<DamageData>(entityB, playerDamage);

                ecb.DestroyEntity(entityA);
            }

            //If powerUp hits player, destroy powerup and give player powerup effect
            if (allPlayers.Exists(entityA) && allPowerUps.Exists(entityB))
            {
                TimeIntervalData oldData = allTimers[entityA];
                PowerUpTag newData = allPowerUps[entityB];

                PowerUpData powerupEffect = new PowerUpData()
                {
                    originalShootingInterval = oldData.interval,
                    newShootingInterval = newData.effectValue,
                    powerUpDuration = newData.effectDuration
                };
                ecb.AddComponent<PowerUpData>(entityA, powerupEffect);
                ecb.DestroyEntity(entityB);
            }
            else if (allPlayers.Exists(entityB) && allPowerUps.Exists(entityA))
            {
                TimeIntervalData oldData = allTimers[entityB];
                PowerUpTag newData = allPowerUps[entityA];

                PowerUpData powerupEffect = new PowerUpData()
                {
                    originalShootingInterval = oldData.interval,
                    newShootingInterval = newData.effectValue,
                    powerUpDuration = newData.effectDuration
                };
                ecb.AddComponent<PowerUpData>(entityB, powerupEffect);
                ecb.DestroyEntity(entityA);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {


        var job = new BulletOnTriggerSystemJob
        {
            allPlayerBullets = GetComponentDataFromEntity<PlayerBulletTag>(true),
            allEnemyBullets = GetComponentDataFromEntity<EnemyBulletTag>(true),
            allPlayers = GetComponentDataFromEntity<InputData>(true),
            allEnemies = GetComponentDataFromEntity<EnemyTag>(true),
            allPowerUps = GetComponentDataFromEntity<PowerUpTag>(true),
            allTimers = GetComponentDataFromEntity<TimeIntervalData>(true),
            ecb = commandBufferSystem.CreateCommandBuffer()
        };

        JobHandle jobHandle = job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, 
            inputDeps);

        //Run commands when safe to do so, instead of waiting for the entire job necessarily
        commandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
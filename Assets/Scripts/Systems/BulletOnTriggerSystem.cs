using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics;
using Unity.Physics.Systems;

//Update after physics systems
[UpdateAfter(typeof(EndFramePhysicsSystem))]
[AlwaysSynchronizeSystem]
public class BulletOnTriggerSystem : JobComponentSystem
{
    //Information on physics, necessary for scheduling the job
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    //Commandbuffer system to create an entitycommandbuffer with in the job
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        //Initialising prerequisites for job
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    struct BulletOnTriggerSystemJob : ITriggerEventsJob
    {
        //Collections of entities with specific components
        //ReadOnly so that threads don't have to wait for eachother
        [ReadOnly] public ComponentDataFromEntity<PlayerBulletTag> allPlayerBullets;
        [ReadOnly] public ComponentDataFromEntity<EnemyBulletTag> allEnemyBullets;
        [ReadOnly] public ComponentDataFromEntity<InputData> allPlayers;
        [ReadOnly] public ComponentDataFromEntity<EnemyTag> allEnemies;
        [ReadOnly] public ComponentDataFromEntity<PowerUpTag> allPowerUps;
        [ReadOnly] public ComponentDataFromEntity<TimeIntervalData> allTimers;

        public EntityCommandBuffer ecb;

        //Body of the job that gets run
        //Here all the trigger collisions get handled
        public void Execute(TriggerEvent triggerEvent)
        {
            //Each triggerevent has both colliding entities attached
            //We don't know what entity represents what (player,bullet,enemy,etc)
            //To determine that, we check these entities with the readonly collections
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            //Don't do anything if it's just bullets colliding
            if ((allPlayerBullets.Exists(entityA) && allPlayerBullets.Exists(entityB)) 
                || (allEnemyBullets.Exists(entityA) && allEnemyBullets.Exists(entityB))) { return; }

            //If playerBullet hits enemy, destroy both and spawn scoreEntity
            if ((allPlayerBullets.Exists(entityA) && allEnemies.Exists(entityB)) 
                || (allPlayerBullets.Exists(entityB) && allEnemies.Exists(entityA)))
            {
                ecb.DestroyEntity(entityA);
                ecb.DestroyEntity(entityB);

                //We spawn a scoreEntity that gets handled later (picked up by a system through its tag component)
                //This is so that we don't have to make a call to main thread here and we can schedule this job over multiple threads...
                //...to increase performance
                var scoreEntity = ecb.CreateEntity();
                ecb.AddComponent<PlayerScoreTag>(scoreEntity);
            }

            //If enemy or enemybullet hits player, destroy enemy/enemybullet and damage player
            //Double if because we don't know whether entityA is the player and entityB the enemy/bullet, or the other way around
            if (allPlayers.Exists(entityA) && (allEnemies.Exists(entityB) || allEnemyBullets.Exists(entityB)))
            {
                //Once again adding a component that gets picked up by a system later to keep performance for this job
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
            //Double if again
            if (allPlayers.Exists(entityA) && allPowerUps.Exists(entityB))
            {
                //Caching regular player component data in powerup component, to set it back when the powerup effect is over
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
        //Prepare job with data
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

        //Schedule job
        JobHandle jobHandle = job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, 
            inputDeps);

        //Run commands when safe to do so, instead of waiting for the entire job necessarily
        commandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
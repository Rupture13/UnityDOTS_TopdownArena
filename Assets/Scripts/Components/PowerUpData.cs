using Unity.Entities;

[GenerateAuthoringComponent]
public struct PowerUpData : IComponentData
{
    public float originalShootingInterval;
    public float newShootingInterval;
    public float powerUpDuration;
}

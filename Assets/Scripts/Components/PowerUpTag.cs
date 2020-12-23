using Unity.Entities;

[GenerateAuthoringComponent]
public struct PowerUpTag : IComponentData
{
    public float effectValue;
    public float effectDuration;
}

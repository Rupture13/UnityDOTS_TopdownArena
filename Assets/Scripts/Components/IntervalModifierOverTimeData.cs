using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct IntervalModifierOverTimeData : IComponentData
{
    public float modifierPerSecond;
    public float2 clampRange;
}

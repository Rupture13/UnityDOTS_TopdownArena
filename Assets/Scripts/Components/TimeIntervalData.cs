using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TimeIntervalData : IComponentData
{
    public float time;
    public float interval;
}

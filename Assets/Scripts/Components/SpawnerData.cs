using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SpawnerData : IComponentData
{
    public Entity prefab;
    public float2 spawnAreaHorizontal;
}

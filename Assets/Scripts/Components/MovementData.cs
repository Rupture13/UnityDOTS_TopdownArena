using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct MovementData : IComponentData
{
    public int2 direction;
    public float2 boundsHorizontal;
    public float2 boundsVertical;
    public float speed;
}


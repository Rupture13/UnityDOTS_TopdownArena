using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct InputData : IComponentData
{
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode fireKey;
}

﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RelocateOnOutOfBoundsData : IComponentData
{
    public float3 relocationPosition;
}
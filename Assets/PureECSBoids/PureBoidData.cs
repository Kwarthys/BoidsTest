using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct PureBoidData : IComponentData
{
    public float speed;
}

[InternalBufferCapacity(4)]
public /*unsafe */struct NeighborsEntityBuffer : IBufferElementData
{
    public Entity Value;
}

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class PureBoidSystem : JobComponentSystem
{
    private struct BoidMovementJob : IJobProcessComponentData<PureBoidData, Position, Rotation>
    {
        public void Execute(ref PureBoidData data, ref Position pos, ref Rotation rot)
        {
            float3 forward = math.mul(rot.Value, new float3(0.0f, 1.0f, 0.0f));
            pos.Value += data.speed * forward;

            rot.Value = math.mul(rot.Value, quaternion.RotateX(0.1f));
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        BoidMovementJob job = new BoidMovementJob{};
        return job.Schedule(this, inputDeps);
    }
    
}


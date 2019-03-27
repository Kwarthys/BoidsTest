using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;



public class PureBoidSystem : JobComponentSystem
{
    ComponentGroup boidGroup;
    NativeMultiHashMap<int, int> laMap;
    NativeArray<Data> dataArray;

    protected override void OnCreateManager()
    {
        boidGroup = GetComponentGroup(typeof(Position), typeof(Rotation), typeof(PureBoidData));
    }

    struct Data
    {
        public float3 position;
        public float4 rotation;
    }


    protected override void OnStartRunning()
    {
        int boidCount = boidGroup.CalculateLength();

        laMap = new NativeMultiHashMap<int, int>(boidCount, Allocator.Persistent);
        dataArray = new NativeArray<Data>(boidCount, Allocator.Persistent);
    }
    protected override void OnStopRunning()
    {
        laMap.Dispose();
        dataArray.Dispose();
    }


    private struct FillListJob : IJobProcessComponentDataWithEntity<PureBoidData, Position, Rotation>
    {
        [WriteOnly] public NativeArray<Data> toFill;

        public void Execute(Entity entity, int index, ref PureBoidData boidData, ref Position pos, ref Rotation rot)
        {
            toFill[index] = new Data
            {
                position = pos.Value,
                rotation = rot.Value.value
            };
        }
    }

    private struct ApplyFLockRulesJob : IJobProcessComponentDataWithEntity<PureBoidData, Position, Rotation>
    {
        public float deltaTime;
        [ReadOnly] public NativeArray<Data> dataArray;

        public void Execute(Entity entity, int index, ref PureBoidData data, ref Position pos, ref Rotation rot)
        {
            float3 forward = math.mul(rot.Value, new float3(0.0f, 1.0f, 0.0f));
            float3 upward = math.mul(rot.Value, new float3(0.0f, 0.0f, 1.0f));

            for (int i = 0; i < dataArray.Length; ++i)
            {
                if(i != index)
                {
                    float distance = math.distance(dataArray[i].position, pos.Value);
                    if (distance < 10f)
                    {
                        float angle = math.degrees(math.acos(math.dot(math.normalize(dataArray[i].position - pos.Value), forward)));
                        if(angle < 100f)
                        {
                            float3 direction;
                            float3 newHeading;

                            float3 otherForward = math.mul(dataArray[i].rotation, new float3(0.0f, 1.0f, 0.0f));

                            //Outward force
                            direction = math.normalize(- dataArray[i].position + pos.Value);
                            newHeading = forward + 0.5f * (10 - distance) * deltaTime * direction;                            
                            rot.Value = quaternion.LookRotationSafe(upward, newHeading);

                            //Inward force
                            direction = -direction;
                            newHeading = forward + 0.5f * distance * deltaTime * direction;
                            rot.Value = quaternion.LookRotationSafe(upward, newHeading);

                            //flocking force
                            direction = otherForward - forward;
                            newHeading = forward + 0.5f * distance * deltaTime * direction;
                            rot.Value = quaternion.LookRotationSafe(upward, newHeading);
                        }
                    }
                }
            }

            //ForceToCenter
            if (math.distance(float3.zero, pos.Value) > 100)
            {
                float3 direction = math.normalize(float3.zero - pos.Value);
                float3 newHeading = forward + 0.5f * (math.distance(float3.zero, pos.Value) - 100) * deltaTime * direction;
                rot.Value = quaternion.LookRotationSafe(upward, newHeading);
            }

        }
    }

    private struct BoidMovementJob : IJobProcessComponentData<PureBoidData, Position, Rotation>
    {
        public float deltaTime;

        public void Execute(ref PureBoidData data, ref Position pos, ref Rotation rot)
        {
            float3 forward = math.mul(rot.Value, new float3(0.0f, 1.0f, 0.0f));            

            //Move
            pos.Value += data.speed * deltaTime * forward;

        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dt = Time.deltaTime;

        FillListJob listJob = new FillListJob
        {
            toFill = dataArray
        };

        JobHandle listJH = listJob.ScheduleGroup(boidGroup, inputDeps);

        ApplyFLockRulesJob flockJob = new ApplyFLockRulesJob
        {
            deltaTime = dt,
            dataArray = this.dataArray
        };

        JobHandle flockJobHandle = flockJob.ScheduleGroup(boidGroup, listJH);

        BoidMovementJob job = new BoidMovementJob
        {
            deltaTime = dt
        };

        return job.Schedule(this, flockJobHandle);
    }
    
}


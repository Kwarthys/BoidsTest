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


    private struct ManageDetectionJob : IJobParallelFor
    {
        [WriteOnly] public NativeMultiHashMap<int, int> toFill;
        [ReadOnly] public NativeArray<Data> list;

        public void Execute(int index)
        {
            toFill.Remove(index);

            for(int i = 0; i < list.Length; ++i)
            {
                if(i != index)
                {
                    if(math.distance(list[i].position, list[index].position) < 10f)
                    {
                        toFill.Add(index, i);
                    }
                }
            }
        }
    }


    private struct BoidMovementJob : IJobProcessComponentData<PureBoidData, Position, Rotation>
    {
        public float deltaTime;

        public void Execute(ref PureBoidData data, ref Position pos, ref Rotation rot)
        {
            float3 forward = math.mul(rot.Value, new float3(0.0f, 1.0f, 0.0f));
            pos.Value += data.speed * deltaTime * forward;

            //WILL SOON DISAPEAR
            rot.Value = math.mul(rot.Value, quaternion.RotateX(5f * deltaTime));
        }
    }


    private struct ApplyFLockRulesJob : IJobProcessComponentDataWithEntity<PureBoidData, Position, Rotation>
    {
        public float deltaTime;
        [ReadOnly] public NativeMultiHashMap<int, int> perceptionData;
        [ReadOnly] public NativeArray<Data> dataArray;

        public void Execute(Entity entity, int index, ref PureBoidData data, ref Position pos, ref Rotation rot)
        {
            NativeMultiHashMapIterator<int> i;
            int item;
            if(perceptionData.TryGetFirstValue(index, out item, out i))
            {
                Debug.Log(index + " sees " + item);

                while (perceptionData.TryGetNextValue(out item, ref i))
                {
                    Debug.Log(index + " sees " + item);
                }
            }

            
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

        ManageDetectionJob detecJob = new ManageDetectionJob
        {
            toFill = laMap
        };

        JobHandle dataJobHandle = detecJob.Schedule(boidGroup.CalculateLength(), 64, listJH);

        ApplyFLockRulesJob flockJob = new ApplyFLockRulesJob
        {
            deltaTime = dt,
            perceptionData = laMap,
            dataArray = this.dataArray
        };

        JobHandle flockJobHandle = flockJob.ScheduleGroup(boidGroup, dataJobHandle);

        BoidMovementJob job = new BoidMovementJob
        {
            deltaTime = dt
        };

        return job.Schedule(this, flockJobHandle);
    }
    
}


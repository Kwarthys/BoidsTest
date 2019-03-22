using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class BoidData : MonoBehaviour
{
    public float speed = 10f;
    public Transform sensor;
    public float detectionRange = 10f;
    public float detectionAngle = 110f;

    public float explosionForce = 1f;
    public float implosionForce = 1f;
    public float followForce = 1f;
    public float toCenterForce = 1f;
}

class BoidSystem : ComponentSystem
{
    private int frameIndex = 0;
    private int amountToRefresh = 10;
    private Dictionary<Comp, List<Comp>> perceptions = new Dictionary<Comp, List<Comp>>();

    struct Comp
    {
        public BoidData data;
        public Transform t;
    }

    private bool isDetected(Comp detector, Comp detectee)
    {
        if (detector.data.sensor == null) return false;
        float angle = Vector3.Angle(detector.data.sensor.forward, detectee.t.position - detector.data.sensor.position);
        return angle < detector.data.detectionAngle;
    }


    private void addToDictionnary(Dictionary<Comp, List<Comp>> perceptions, Comp key, Comp newValue)
    {
        if (perceptions.ContainsKey(key))
        {
            if(!perceptions[key].Contains(newValue))
            {
                perceptions[key].Add(newValue);
            }
        }
        else
        {
            List<Comp> detectedList = new List<Comp>();
            detectedList.Add(newValue);
            perceptions.Add(key, detectedList);
        }
    }

    protected override void OnUpdate()
    {
        ComponentGroupArray<Comp> list = GetEntities<Comp>();

        Dictionary<Comp, List<Comp>> inRange = new Dictionary<Comp, List<Comp>>();

        int offset = 1;

        for (int iBoidA = 0; iBoidA < list.Length; iBoidA++)
        {
            for (int iBoidB = offset; iBoidB < list.Length; iBoidB++)
            {
                if (iBoidB != iBoidA)
                {
                    Comp boidA = list[iBoidA];
                    Comp boidB = list[iBoidB];

                    if (Vector3.Distance(boidA.t.position, boidB.t.position) < boidA.data.detectionRange)
                    {
                        addToDictionnary(inRange, boidA, boidB);
                        addToDictionnary(inRange, boidB, boidA);
                    }
                }
            }

            offset++;
        }

        int boidPerFrame = list.Length < amountToRefresh ? 1 : list.Length / amountToRefresh;
        //Debug.Log("BoidPerFrame " + boidPerFrame);

        int startingOffset = (frameIndex * boidPerFrame) % list.Length;
        //Debug.Log(startingOffset + " = (" + frameIndex + " * " + boidPerFrame + " )% " + list.Length);

        for (int iBoidA = startingOffset; iBoidA < list.Length && iBoidA < startingOffset + boidPerFrame; iBoidA++)
        {
            //Debug.Log(frameIndex + " Refreshing " + list[iBoidA].t.name);

            if (perceptions.ContainsKey(list[iBoidA]))
            {
                perceptions[list[iBoidA]].Clear();
            }

            if (inRange.ContainsKey(list[iBoidA]))
            {
                foreach(Comp boidB in inRange[list[iBoidA]])
                {
                    Comp boidA = list[iBoidA];

                    if (Vector3.Distance(boidA.t.position, boidB.t.position) < boidA.data.detectionRange)
                    {
                        if (isDetected(boidA, boidB))
                        {
                            addToDictionnary(perceptions, boidA, boidB);
                        }
                        if (isDetected(boidB, boidA))
                        {
                            addToDictionnary(perceptions, boidB, boidA);
                        }
                        
                    }
                }
            }
        }

        frameIndex++;


        foreach(Comp e in list)
        {
            if(perceptions.ContainsKey(e))
            {
                foreach (Comp otherBoid in perceptions[e])
                {
                    applyForces(e, otherBoid);
                }
            }

            applyForces(e);

            e.t.position += e.t.forward * e.data.speed;
        }
    }

    private void applyForces(Comp boid)
    {
        //BackToCenter
        Vector3 forceToCenter = boid.data.toCenterForce * Vector3.Distance(Vector3.zero, boid.t.position) * -boid.t.position.normalized / 500;
        if (Vector3.Distance(Vector3.zero, boid.t.position) > 100)
        {
            Vector3 direction = Vector3.ClampMagnitude(forceToCenter, 1);
            boid.t.forward = Vector3.RotateTowards(boid.t.forward, direction, 0.05f, 10f);
        }
    }

    private void applyForces(Comp boid, Comp otherBoid)
    {

        float distance = Vector3.Distance(otherBoid.t.position, boid.t.position);

        //Explosion
        Vector3 expForce = boid.data.explosionForce * (boid.data.detectionRange - distance) * (boid.t.position - otherBoid.t.position).normalized / boid.data.detectionRange;
        if (expForce.magnitude > 0 && expForce != Vector3.zero)
        {
            Vector3 direction = Vector3.ClampMagnitude(expForce, 1);
            boid.t.forward = Vector3.RotateTowards(boid.t.forward, direction, 0.05f, 10f);
        }

        //Implosion
        Vector3 implForce = boid.data.followForce * distance * (otherBoid.t.position - boid.t.position).normalized / boid.data.detectionRange;
        if (implForce.magnitude > 0 && implForce != Vector3.zero)
        {
            Vector3 direction = Vector3.ClampMagnitude(implForce, 1);
            //Debug.DrawRay(boid.t.position, direction * 50, Color.blue, 0.5f);
            boid.t.forward = Vector3.RotateTowards(boid.t.forward, direction, 0.05f, 10f);
        }

        //Follow
        Vector3 followForce = boid.data.followForce * Vector3.Distance(otherBoid.t.forward, boid.t.forward) * (otherBoid.t.forward - boid.t.forward).normalized / boid.data.detectionRange;
        if (followForce.magnitude > 0 && followForce != Vector3.zero)
        {
            Vector3 direction = Vector3.ClampMagnitude(followForce, 1);
            //Debug.DrawRay(boid.t.position, direction * 50, Color.green, 0.5f);
            boid.t.forward = Vector3.RotateTowards(boid.t.forward, direction, 0.05f, 10f);
        }
    }
}

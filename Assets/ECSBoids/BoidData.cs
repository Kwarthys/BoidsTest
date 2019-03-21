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


    private void addToDetecs(Dictionary<Comp, List<Comp>> perceptions, Comp detector, Comp detected)
    {
        if (perceptions.ContainsKey(detector))
        {
            perceptions[detector].Add(detected);
        }
        else
        {
            List<Comp> detectedList = new List<Comp>();
            detectedList.Add(detected);
            perceptions.Add(detector, detectedList);
        }
    }

    protected override void OnUpdate()
    {
        Dictionary<Comp, List<Comp>> perceptions = new Dictionary<Comp, List<Comp>>();

        perceptions.Clear();

        ComponentGroupArray<Comp> list = GetEntities<Comp>();
        List<Comp> visited = new List<Comp>();

        foreach (Comp boidA in list)
        {
            foreach (Comp boidB in list)
            {
                if (boidB.t != boidA.t) // comparing the transforms as am not allowed to compare the struct
                {
                    if (!visited.Contains(boidB))
                    {
                        if(Vector3.Distance(boidA.t.position, boidB.t.position) < boidA.data.detectionRange)
                        {
                            if (isDetected(boidA, boidB))
                            {
                                addToDetecs(perceptions, boidA, boidB);
                            }
                            if (isDetected(boidB, boidA))
                            {
                                addToDetecs(perceptions, boidB, boidA);
                            }
                        }
                    }
                }
            }

            visited.Add(boidA);
        }


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
        if (expForce.magnitude > 0)
        {
            Vector3 direction = Vector3.ClampMagnitude(expForce, 1);
            boid.t.forward = Vector3.RotateTowards(boid.t.forward, direction, 0.05f, 10f);
        }

        //Implosion

        //Follow
    }
}

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
    struct comp
    {
        public BoidData data;
        public Transform t;
    }

    protected override void OnUpdate()
    {
        foreach(comp e in GetEntities<comp>())
        {

            Vector3 direction = e.t.forward;
            //e.t.position += e.data.speed * e.t.forward;
            //e.t.Rotate(0f, 0f, e.data.speed);

            RaycastHit[] hits = Physics.SphereCastAll(e.data.sensor.position, e.data.detectionRange, e.data.sensor.forward, 0.1f);

            foreach(RaycastHit hit in hits)
            {
                if(hit.transform != e.t)
                {
                    if (e.data.sensor != null)
                    {
                        float angle = Vector3.Angle(e.data.sensor.forward, hit.transform.position - e.data.sensor.position);
                        if(angle < e.data.detectionAngle)
                        {
                            //Debug.Log(e.t.name + " " + hit.transform.name + " at " + Time.time);

                            float distance = Vector3.Distance(hit.transform.position, e.t.position);

                            //Explosion
                            Vector3 expForce = e.data.explosionForce * (e.data.detectionRange - distance) * (hit.transform.forward - e.t.forward).normalized / e.data.detectionRange;
                            if(expForce.magnitude > 0)
                            {
                                Debug.DrawRay(e.t.position, expForce, Color.red, 0.5f);
                                direction += Vector3.ClampMagnitude(expForce, 1);
                            }

                            //Implosion
                            //direction += e.data.implosionForce * distance * (hit.transform.forward - e.t.forward);

                            //Follow
                        }
                    }
                }

                //BackToCenter
                Vector3 forceToCenter = e.data.toCenterForce * Vector3.Distance(Vector3.zero, e.t.position) * -e.t.position.normalized / 500;
                if(Vector3.Distance(Vector3.zero, e.t.position) > 20)
                {
                    direction += Vector3.ClampMagnitude(forceToCenter,1);
                }
            }

            if(Vector3.Angle(direction, e.t.forward) > 10)
            {
                direction = Vector3.ProjectOnPlane(direction, e.t.forward);
                Debug.Log(direction.magnitude);
                direction += e.t.forward * (1 - direction.magnitude);
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            e.t.rotation = Quaternion.Slerp(e.t.rotation, targetRotation, 0.05f);

            //e.t.LookAt(e.t.position + direction);
            e.t.position += direction;
        }
    }

}

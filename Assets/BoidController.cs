using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour {

    private GameController gameController;

    private int index;

    private Transform nose;

    private Rigidbody rb;

    private void Start()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * 20;

        nose = transform.Find("nose").GetComponent<Transform>();

        rb = GetComponent<Rigidbody>();

        index = Random.Range(-1000, 1000);
    }

    public void registerController(GameController gc)
    {
        gameController = gc;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        List<BoidController> others = gameController.getInRangeBoids(this);

        rb.angularVelocity = Vector3.zero;

        rb.velocity = 20 * transform.forward;

        if (others != null)
        {
            Vector3 othersVelocity = new Vector3();

            int sum = 0;

            foreach (BoidController boid in others)
            {
                if(boid.rb != null)
                {
                    othersVelocity += boid.rb.velocity;
                    sum++;

                    flockForceTowards(boid);
                    flockForceOutwards(boid);
                }
            }

            othersVelocity /= sum;

            followFlock(othersVelocity);
        }

        getBackToCenter();

        randomInput();

    }

    private void getBackToCenter()
    {
        Vector3 centerPos = gameController.focusPoint.position;
        Vector3 forceTowardsCenter = Vector3.ProjectOnPlane(centerPos-transform.position, transform.forward).normalized * Vector3.Distance(transform.position, centerPos) * Vector3.Distance(transform.position, centerPos) / 600;
        rb.AddForceAtPosition(forceTowardsCenter, nose.position);
    }

    private void followFlock(Vector3 othersVelocity)
    {
        Vector3 force = (othersVelocity - rb.velocity).normalized * gameController.cohesionForce;

        force = Vector3.ProjectOnPlane(force, transform.forward);

        rb.AddForceAtPosition(force, nose.position);
    }

    private void flockForceTowards(BoidController boid)
    {
        Vector3 force = (boid.transform.position - transform.position).normalized * gameController.linkingForce;

        force = Vector3.ProjectOnPlane(force, transform.forward);

        rb.AddForceAtPosition(force, nose.position);
    }

    private void flockForceOutwards(BoidController boid)
    {
        float weight = (gameController.detectionRange - Vector3.Distance(boid.transform.position, transform.position)) * gameController.explosionForce;

        Vector3 force = - (boid.transform.position - transform.position).normalized * weight;

        force = Vector3.ProjectOnPlane(force, transform.forward);

        rb.AddForceAtPosition(force, nose.position);
    }

    private void randomInput()
    {
        Vector3 force = new Vector3(Mathf.PerlinNoise(index*1f / 500f, 0) * 2 - 1, Mathf.PerlinNoise((index+5)*1f / 500f, 0) * 2 - 1, Mathf.PerlinNoise((index+20)*1f / 500f, 0) * 2 - 1);

        force = Vector3.ProjectOnPlane(force, transform.forward) * gameController.randomForce;

        rb.AddForceAtPosition(force, nose.position);

        index++;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public GameObject boidPrefab;

    public int flockSize = 2;

    public float detectionRange = 80;
    public float detectionAngle = 60;

    public int spawnArea = 20;

    public Camera cam;

    private BoidController[] boidsList;

    private Dictionary<BoidController, List<BoidController>> boidsDetection = new Dictionary<BoidController, List<BoidController>>();

    private int index = 0;


    [Header("Boids Rules")]
    public float cohesionForce = 5;
    public float linkingForce = 5;
    public float explosionForce = 15;
    public float randomForce = 15;
    public float distanceToGetBack = 20;

    public Transform focusPoint;

    public void Start()
    {
        boidsList = new BoidController[flockSize];

        GameObject boid = Instantiate(boidPrefab, new Vector3(Random.Range(-spawnArea, spawnArea), Random.Range(-spawnArea, spawnArea), Random.Range(-spawnArea, spawnArea)), Random.rotation);
        BoidController bc = boid.GetComponent<BoidController>();
        bc.registerController(GetComponent<GameController>());
        boidsList[0] = bc;
        boid.name = "Original";

        for (int i = 1; i < flockSize; i++)
        {
            boid = Instantiate(boid, new Vector3(Random.Range(-spawnArea, spawnArea), Random.Range(-spawnArea, spawnArea), Random.Range(-spawnArea, spawnArea)), Random.rotation);

            bc = boid.GetComponent<BoidController>();

            bc.registerController(GetComponent<GameController>());

            boidsList[i] = bc;

            boid.name = "Boid" + i;
        }
    }

    private void FixedUpdate()
    {
        boidsDetection.Clear();

        proceduralDetection();

        moveCamera();
    }

    private void proceduralDetection()
    {
        int offset = 1;
        for (int iBoidA = 0; iBoidA < boidsList.Length; iBoidA++)
        {
            for (int iBoidB = offset; iBoidB < boidsList.Length; iBoidB++)
            {
                if (inRange(boidsList[iBoidA], boidsList[iBoidB]))
                {
                    if (isDetected(boidsList[iBoidA], boidsList[iBoidB]))
                    {
                        addToDetecs(boidsList[iBoidA], boidsList[iBoidB]);
                        //Debug.Log(index + " - " + boidsList[iBoidA].name + " detects " + boidsList[iBoidB].name);
                    }

                    if (isDetected(boidsList[iBoidB], boidsList[iBoidA]))
                    {
                        addToDetecs(boidsList[iBoidB], boidsList[iBoidA]);
                        //Debug.Log(index + " - " + boidsList[iBoidB].name + " detects " + boidsList[iBoidA].name);
                    }
                }
            }

            offset++;
        }
        index++;
    }

    private void addToDetecs(BoidController detector, BoidController detected)
    {
        if (boidsDetection.ContainsKey(detector))
        {
            boidsDetection[detector].Add(detected);
        }
        else
        {
            List<BoidController> detectedList = new List<BoidController>();
            detectedList.Add(detected);
            boidsDetection.Add(detector, detectedList);
        }
    }

    private bool inRange(BoidController b1, BoidController b2)
    {
        float distance = Vector3.Distance(b1.transform.position, b2.transform.position);
        return distance < detectionRange;
    }

    private bool isDetected(BoidController detector, BoidController detectee)
    {
        if (detector.sensor == null) return false;
        float angle = Vector3.Angle(detector.sensor.forward, detectee.transform.position - detector.sensor.position);
        return angle < detectionAngle;
    }


    public List<BoidController> getInRangeBoids(BoidController meSelf)
    {
        List<BoidController> list = new List<BoidController>();

        if (boidsDetection.TryGetValue(meSelf, out list))
        {
            return boidsDetection[meSelf];
        }
        return null;
    }

    private void moveCamera()
    {
        cam.transform.LookAt(boidsList[0].transform);
    }

}

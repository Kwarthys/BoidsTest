using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class spawner : MonoBehaviour
{

    public GameObject boidPrefab;

    public int spawnArea = 50;

    public int flockSize = 50;


    void Start()
    {
        //boids = new GameObject[flockSize];

        GameObject boid = Instantiate(boidPrefab, new Vector3(Random.Range(-spawnArea, spawnArea), Random.Range(-spawnArea, spawnArea), Random.Range(-spawnArea, spawnArea)), Random.rotation);
        boid.name = "Original";

        //boids[0] = boid;

        for (int i = 1; i < flockSize; i++)
        {
            boid = Instantiate(boid, new Vector3(Random.Range(-spawnArea, spawnArea), Random.Range(-spawnArea, spawnArea), Random.Range(-spawnArea, spawnArea)), Random.rotation);

            boid.name = "Boid" + i;

            //boids[i] = boid;
        }

    }
}

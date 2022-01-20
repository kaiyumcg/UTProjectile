using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KProjectile;

public class TestControl : MonoBehaviour
{
    [SerializeField] ProjectileControl control;
    [SerializeField] Transform ballPrefab;
    [SerializeField] float speed = 20f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            control.StartPlayerInput((points) =>
            {
                var ball = Instantiate(ballPrefab) as Transform;
                control.TravelAlongProjectilePath(ball, speed, points, isLooping : false, () =>
                {
                    Debug.Log("Reached");
                });
            });
        }
    }
}

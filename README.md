

Projectile control and movement scripts for unity powered games. Currently only transform based movement is supported. Uses DOTS for snapping features.

**<span style="text-decoration:underline;">Installation:</span>**

Install **<span style="text-decoration:underline;">Burst and Job</span>** from package manager. Copy the “ProjectileMovement” folder inside your unity project. 

**<span style="text-decoration:underline;">Usage:</span>**


```
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
                control.TravelAlongProjectilePath(ball, speed, points,       isLooping : false, () =>
                {
                    Debug.Log("Reached");
                });
            });
        }
    }
```


This will start the control script as well as show projectile in line renderer or in a custom way. At the release of control(when mouse button/touch is up), a prefab will move along the trajectory. Configure the “ProjectileControl” component in the inspector to your liking.

Check the ext.cs file for some handy extension methods as well.

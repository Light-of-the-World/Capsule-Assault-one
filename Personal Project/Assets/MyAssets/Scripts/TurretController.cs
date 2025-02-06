using System.ComponentModel;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// Controls a turret that can rotate its base and weapon to track and shoot at enemies.
/// The turret has two parts: a rotating base and a weapon that can aim up/down.
/// Supports multiple projectile spawn points for simultaneous firing and explosion effects on impact.
/// </summary> 
///     THIS WAS NOT ORIGINALLY MY CODE! THOUGH IT HAS BEEN HEAVILY MODIFIED.
public class TurretController : MonoBehaviour
{
    #region Turret Movement
    [Space(0)]
    [Header("================ TURRET MOVEMENT ================")]
    [Header("Controls how the turret base and weapon mount rotate to track targets")]
    [Header("")]

    [Header(">> Base Rotation")]
    [Tooltip("How fast the base can rotate left/right")]
    [Range(0, 360)]
    [SerializeField] private float baseRotationSpeed = 180f;

    [Tooltip("Angle precision for stopping rotation")]
    [Range(0.01f, 1f)]
    [SerializeField] private float alignmentThreshold = 0.1f;

    [Space(30)]
    [Header(">> Weapon Mount")]
    [Tooltip("How fast the weapon can pivot up/down (degrees per second)")]
    [Range(0, 360)]
    [SerializeField] private float weaponRotationSpeed = 180f;
    [SerializeField] private float turretCameraRotationSpeed = 5000f;


    [Tooltip("Physical part of the turret that aims up/down")]
    [SerializeField] private Transform weaponMount;

    [Tooltip("Target point for aiming calculations (usually at barrel tip)")]
    [SerializeField] private Transform aimReference;
    #endregion

    #region Enemy Detection
    [Space(30)]
    [Header("================ TARGET DETECTION ================")]
    [Header("Settings for how the turret detects and engages enemies")]
    [Header("")]

    [Tooltip("Maximum distance to engage detected enemies")]
    [Range(1f, 200f)]
    [SerializeField] private float attackRange = 20f;
    #endregion

    #region Combat Settings
    [Space(30)]
    [Header("================== COMBAT ===================")]
    [Header("Weapon firing behavior and projectile settings")]
    [Header("")]

    [Header(">> Firing Controls")]
    [Tooltip("List of points where projectiles spawn from")]
    [SerializeField] public List<Transform> projectileSpawnPoints = new List<Transform>();

    [Tooltip("Projectile object to create when firing")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("Particle effect prefab for projectile explosion")]
    [SerializeField] private GameObject explosionPrefab;

    [Tooltip("Shots per second per spawn point")]
    [Range(0.1f, 1000f)]
    [SerializeField] private float fireRate = 10f;

    [Space(10)]
    [Header(">> Projectile Behavior")]
    [Tooltip("Projectile travel speed")]
    [Range(1f, 100f)]
    [SerializeField] private float projectileSpeed = 30f;

    [Tooltip("How quickly projectiles can turn")]
    [Range(0f, 360f)]
    [SerializeField] private float projectileRotationSpeed = 300f;

    [Tooltip("Projectile tracking aggression (higher = tighter tracking)")]
    [Range(0f, 10f)]
    [SerializeField] private float projectileHomingStrength = 7f;

    [Tooltip("Time before projectile self-destructs")]
    [Range(0.1f, 10f)]
    [SerializeField] private float projectileLifetime = 3f;
    #endregion

    #region Animation
    [Space(30)]
    [Header("================== ANIMATION ==================")]
    [Header("Animator component for turret firing animations")]
    [Header("")]

    [SerializeField] private Animator animator;

    [Tooltip("Speed multiplier for attack animation")]
    [Range(0.1f, 10f)]
    public float animationSpeed = 1f;
    #endregion

    // Private tracking variables
    public GameObject enemy;                     // Current target
    public GameObject[] allEnemies;              // A full list of all enemies alive currently
    public GameObject[] possibleTargets;         // A list of enemies that are within, or almost in, attack range
    public GameObject[] betterTargets;           // A list of enemies that are within 1/2 of the attack range   
    public Transform turretCamHolder;            // Seperate camera holder, free of turret rotation.
    public Camera turretCam;                     // This is the actual camera. This snaps to the enemy.
    public AudioClip turretFireSound;            // Plays the firing sound of the turret. It is the firing sound of lowest-caliber Adv Cannon shells in From The Depths
    public GameObject turretAll;                 // Gets the turret holder itself so that it can delete everything related to the turret
    public TextMeshPro turretTimer;              // Timer floating above the turret
    private AudioSource turretAudio;             // Source of the audio
    private CameraControl cameraControl;
    private EnemyAI enemyAi;
    private bool isBaseRotating = false;         // Is the base currently rotating?
    private bool isWeaponRotating = false;       // Is the weapon currently rotating?
    private float targetWeaponAngle = 0f;        // Target angle for weapon rotation
    private float currentWeaponAngle = 0f;       // Current weapon angle
    private bool isInRange = false;              // Is the enemy in attack range?
    private float nextFireTime = 0f;             // When we can fire next
    private bool isReadyToFire = false;          // Whether all conditions for firing are met
    private bool isFiring = false;               // Whether the turret is actively firing
    private int numberOfTargets;
    private float timeSinceLastFired;
    private float timeLeft = 30f;
    private Vector3 flatDirection;
    private Vector3 directionToEnemy;
    private Vector3 cameraUpDirection;
    private Quaternion spawnRotation;
    private void Start()
    {
        //enemy = GameObject.FindGameObjectWithTag("Enemy");
        /*
        InvokeRepeating("FindASuitableTarget", 0.1f, 0.2f);
        */
        InvokeRepeating("EnsureEnemyIsVisible", 0.15f, 0.2f);
        turretAudio = aimReference.GetComponent<AudioSource>();
        cameraControl = GameObject.FindGameObjectWithTag("PlayerCam").GetComponent<CameraControl>();
        spawnRotation = cameraControl.orientation.rotation;

        // Validate spawn points
        if (projectileSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No projectile spawn points assigned to turret. Please assign at least one spawn point in the inspector.");
        }

        // Set up layer collision ignores
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Projectile"), LayerMask.NameToLayer("Projectile"), true);
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime * Time.timeScale;
        turretTimer.text = "" + Math.Round(timeLeft, 0);
        timeSinceLastFired += Time.deltaTime * Time.timeScale;
        if (timeLeft <= 0)
        {
            DestroyTurret();
        }
        // Safety check - make sure we have all required components
        if (weaponMount == null || aimReference == null)
        {
            Debug.LogWarning("Missing required components: " +
                (weaponMount == null ? "WeaponMount " : "") +
                (aimReference == null ? "AimReference" : ""));
            UpdateFiringState(false);
            return;
        }
        if (!enemy)
        {
            ReturnToNeutral();
            return;
        }
        // Check to see if any enemies are currently in the list of possible targets
        // Check if enemy is in range
        float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
        bool newInRange = distanceToEnemy <= attackRange;

        // Update range status
        if (newInRange != isInRange)
        {
            isInRange = newInRange;
        }

        // Calculate direction to enemy
        directionToEnemy = (enemy.transform.position - transform.position).normalized;
        flatDirection = new Vector3(directionToEnemy.x, 0, directionToEnemy.z);
        cameraUpDirection = new Vector3(0, directionToEnemy.y, 0);
        //Snap turret camera to enemy
        if (enemy)
        {
            Quaternion targetCameraRotation = Quaternion.LookRotation(flatDirection, cameraUpDirection);
            turretCam.transform.rotation = Quaternion.RotateTowards(
            turretCam.transform.rotation,
            targetCameraRotation,
            turretCameraRotationSpeed * Time.deltaTime);
        }
        // Only proceed if we have a valid direction
        if (flatDirection != Vector3.zero)
        {
            // Calculate the rotation needed to face the enemy
            Quaternion targetBaseRotation = Quaternion.LookRotation(flatDirection, Vector3.up);

            // If we're not facing the right direction, rotate the turret
            if (!Mathf.Approximately(Quaternion.Angle(weaponMount.transform.rotation, targetBaseRotation), 0f))
            {
                isBaseRotating = false;
                CalculateWeaponRotation();
                if (isWeaponRotating)
                {
                    RotateWeapon(targetBaseRotation);
                }
            }

            // Update ready-to-fire status
            isReadyToFire = isInRange;

            // Try to fire if everything is aligned and we're in range
            if (isReadyToFire && Time.time >= nextFireTime)
            {
                FireProjectile();
                UpdateFiringState(true);
            }
            else if (!isReadyToFire && isFiring)
            {
                UpdateFiringState(false);
            }
        }
        else
        {
            UpdateFiringState(false);
        }

        // Draw debug line to show if enemy is in range
        if (enemy != null && aimReference != null)
        {
            Debug.DrawLine(aimReference.position, enemy.transform.position, Color.blue);
        }
        ForceTargetLastEnemy();
    }

    public void UpdateTargetList(GameObject enemyToAdd)
    {
        // Step 1: Find all enemies
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Step 2: Create a list to store the valid targets
        List<GameObject> validTargets = new List<GameObject>();
        List<GameObject> goodTargets = new List<GameObject>();

        // Step 3: Iterate through allEnemies and check distance
        foreach (GameObject enemy in allEnemies)
        {
            if (enemy == null) return;
            enemyAi = enemy.GetComponent<EnemyAI>();
            if (Vector3.Distance(transform.position, enemy.transform.position) <= (attackRange / 2) && !enemyAi.isDead)
            {
                goodTargets.Add(enemy); // Add as priority target if within half the range
            }
            if (Vector3.Distance(transform.position, enemy.transform.position) <= attackRange && !enemyAi.isDead)
            {
                validTargets.Add(enemy); // Add to valid targets if within range
            }
        }
        validTargets.Add(enemyToAdd);
        // Step 4: Convert the list to an array
        possibleTargets = validTargets.ToArray();
        betterTargets = goodTargets.ToArray();
        FindASuitableTarget();
    }

    
    public void UpdateTargetListAfterKill(GameObject enemyToRemove)
    {
        if (!possibleTargets.Contains(enemyToRemove))
        {
            Debug.Log("Enemy killed isn't in our list, no need to remove");
            return;
        }
        // Step 1: Find all enemies
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Step 2: Create a list to store the valid targets
        List<GameObject> validTargets = new List<GameObject>();
        List<GameObject> goodTargets = new List<GameObject>();

        // Step 3: Iterate through allEnemies and check distance
        foreach (GameObject enemy in allEnemies)
        {
            if (enemy == null) return;
            enemyAi = enemy.GetComponent<EnemyAI>();
            if (Vector3.Distance(transform.position, enemy.transform.position) <= (attackRange / 2) && !enemyAi.isDead)
            {
                goodTargets.Add(enemy); // Add as priority target if within half the range
            }
            if (Vector3.Distance(transform.position, enemy.transform.position) <= attackRange && !enemyAi.isDead)
            {
                validTargets.Add(enemy); // Add to valid targets if within range
            }
        }
        if (validTargets.Contains(enemyToRemove))
        {
            validTargets.Remove(enemyToRemove);
        }
        if (goodTargets.Contains(enemyToRemove))
        {
            goodTargets.Remove(enemyToRemove);
        }
        // Step 4: Convert the list to an array
        possibleTargets = validTargets.ToArray();
        betterTargets = goodTargets.ToArray();
        FindingANewTarget();
    }

    private void ForceTargetLastEnemy()
    {
        numberOfTargets = possibleTargets.Count();
        if (numberOfTargets == 1)
        {
            enemy = possibleTargets[0];
        }
    }
    private void FindASuitableTarget()
    {
        if (!enemy || !isInRange)
        {
            // Step 2: Create a list to store the valid targets
            List<GameObject> validTargets = new List<GameObject>();
            List<GameObject> goodTargets = new List<GameObject>();
            // Step 3: Iterate through allEnemies and check distance
            foreach (GameObject enemy in allEnemies)
            {
                if (enemy == null) return;
                enemyAi = enemy.GetComponent<EnemyAI>();
                if (Vector3.Distance(transform.position, enemy.transform.position) <= (attackRange / 2) && !enemyAi.isDead)
                {
                    goodTargets.Add(enemy); // Add as priority target if within half the range
                }
                if (Vector3.Distance(transform.position, enemy.transform.position) <= attackRange && !enemyAi.isDead)
                {
                    validTargets.Add(enemy); // Add to valid targets if within range
                }
            }
            // Step 4: Convert the list to an array
            possibleTargets = validTargets.ToArray();
            betterTargets = goodTargets.ToArray();
            int betterTargetsIndex = UnityEngine.Random.Range(0, betterTargets.Length);
            if (betterTargetsIndex > 0)
            {
                enemy = betterTargets[betterTargetsIndex];
                Debug.Log("Better enemy found");
                Invoke("EnsureEnemyIsVisible", 0.01f);
                return;
            }
            int targetsIndex = UnityEngine.Random.Range(0, possibleTargets.Length);
            if (targetsIndex > 0)
            {
                enemy = possibleTargets[targetsIndex];
            }
            if (!enemy)
            {
                ReturnToNeutral();
                return;
            }
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy <= attackRange)
            {
                //enemyIndex = Random.Range(0, allEnemies.Length);
                Debug.Log("Chosen enemy was not in range (THIS SHOULDN'T BE POSSIBLE)");
                Invoke("FindASuitableTarget", 0.01f);
                return;
            }
            // Only proceed if we have a valid direction
            if (flatDirection != Vector3.zero)
            {
                Quaternion targetCameraRotation = Quaternion.LookRotation(flatDirection, cameraUpDirection);
                turretCam.transform.rotation = Quaternion.RotateTowards(
                turretCam.transform.rotation,
                targetCameraRotation,
                turretCameraRotationSpeed * Time.deltaTime);
                Ray ray = turretCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Middle of screen
                RaycastHit hit;
                //check if ray hits something
                if (Physics.Raycast(ray, out hit)) //hit an enemy or wall
                {
                    if (!hit.collider.CompareTag("Walls"))
                    {
                        Debug.Log("Enemy spotted");
                        return;
                    }
                    else //(hit.collider.CompareTag("Walls"))
                    {
                        Debug.Log("New Enemy behind a wall");
                        Invoke("FindASuitableTarget", 0.01f);
                        return;
                    }
                }
            }
        }
    }

    private void FindingANewTarget()
    {
        int betterTargetsIndex = UnityEngine.Random.Range(0, betterTargets.Length);
        if (betterTargetsIndex > 0)
        {
            enemy = betterTargets[betterTargetsIndex];
            betterTargetsIndex.ToString();
            Debug.Log("Better enemy found: {betterTargetsIndex}");
            Invoke("EnsureEnemyIsVisible", 0.01f);
            return;
        }
        else
        {
            Debug.Log("Still looking");
            int targetsIndex = UnityEngine.Random.Range(0, possibleTargets.Length);
            if (targetsIndex > 0)
            {
                enemy = possibleTargets[targetsIndex];
                Quaternion targetCameraRotation = Quaternion.LookRotation(flatDirection, cameraUpDirection);
                turretCam.transform.rotation = Quaternion.RotateTowards(
                turretCam.transform.rotation,
                targetCameraRotation,
                turretCameraRotationSpeed * Time.deltaTime);
            }
            if (!enemy)
            {
                ReturnToNeutral();
                return;
            }
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy <= attackRange)
            {
                //enemyIndex = Random.Range(0, allEnemies.Length);
                Debug.Log("New target in range");
                Invoke("FindASuitableTarget", 0.01f);
                return;
            }
            // Only proceed if we have a valid direction
            if (flatDirection != Vector3.zero)
            {
                Ray ray = turretCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Middle of screen
                RaycastHit hit;
                //check if ray hits something
                if (Physics.Raycast(ray, out hit)) //hit an enemy or wall
                {
                    if (hit.collider.CompareTag("Walls"))
                    {
                        Debug.Log("New Enemy behind a wall");
                        Invoke("FindingANewTarget", 0.01f);
                        return;
                    }
                    else
                    {
                        Debug.Log("Enemy spotted");
                    }
                }
            }
        }
    }

    private void EnsureEnemyIsVisible()
    {
        //if (enemy == null) FindingANewTarget();
        // Only proceed if we have a valid direction
        if (flatDirection != Vector3.zero)
        {
            Ray ray = turretCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Middle of screen
            RaycastHit hit;
            //check if ray hits something
            if (Physics.Raycast(ray, out hit)) //hit an enemy or wall
            {
                if (hit.collider.CompareTag("Walls"))
                {
                    Debug.Log("Current Enemy behind a wall. Retargetting");
                    Invoke("FindingANewTarget", 0.04f);
                    return;
                }
                /*
                else if (hit.collider.CompareTag("Walls"))
                {
                    Debug.Log("Enemy is still visible");
                    return;
                }
                */
            }
        }
    }

    private void ReturnToNeutral()
    {
        if (timeSinceLastFired > 1f)
        {
            turretCam.transform.rotation = Quaternion.RotateTowards(
                turretCam.transform.rotation,
                spawnRotation,
                turretCameraRotationSpeed * Time.deltaTime);
        }
    }


    private void UpdateFiringState(bool firing)
    {
        if (isFiring != firing)
        {
            isFiring = firing;
            if (animator != null)
            {
                animator.SetBool("Attack", firing);
                animator.speed = animationSpeed;
            }
        }
    }

    private void RotateBase(Quaternion targetRotation)
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            baseRotationSpeed * Time.deltaTime
        );
    }

    private void CalculateWeaponRotation()
    {
        Vector3 toEnemy = enemy.transform.position - aimReference.position;
        Vector3 localToEnemy = transform.InverseTransformDirection(toEnemy);
        Vector3 localAimForward = transform.InverseTransformDirection(aimReference.forward);

        float targetAngle = -Mathf.Atan2(localToEnemy.y, localToEnemy.z) * Mathf.Rad2Deg;
        currentWeaponAngle = -Mathf.Atan2(localAimForward.y, localAimForward.z) * Mathf.Rad2Deg;

        float angleDifference = Mathf.DeltaAngle(currentWeaponAngle, targetAngle);

        if (Mathf.Abs(angleDifference) > alignmentThreshold)
        {
            isWeaponRotating = true;
            targetWeaponAngle = targetAngle;
        }
        else
        {
            isWeaponRotating = false;
        }
    }

    private void RotateWeapon(Quaternion targetRotation)
    {
        /*
        // Calculate the shortest rotation path
        float angleDifference = Mathf.DeltaAngle(currentWeaponAngle, targetWeaponAngle);

        // Calculate maximum rotation this frame based on rotation speed
        float maxRotationThisFrame = weaponRotationSpeed * Time.deltaTime;
        // Determine actual rotation amount, clamped by speed
        float rotationAmount = Mathf.Clamp(angleDifference, -maxRotationThisFrame, maxRotationThisFrame);
        */
        // Apply rotation
        weaponMount.transform.rotation = Quaternion.RotateTowards(
            weaponMount.transform.rotation,
            targetRotation,
            baseRotationSpeed * Time.deltaTime);
        // Check if we've reached the target angle
        /*
        if (Mathf.Abs(angleDifference) <= alignmentThreshold)
        {
            isWeaponRotating = false;
        }
        */
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoints.Count == 0)
        {
            Debug.LogError("Cannot fire: " +
                (projectilePrefab == null ? "No projectile prefab assigned. " : "") +
                (projectileSpawnPoints.Count == 0 ? "No spawn points assigned." : ""));
            return;
        }
        Ray ray = turretCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Middle of screen
        RaycastHit hit;
        //check if ray hits something
        if (Physics.Raycast(ray, out hit)) //hit an enemy or wall
        {
            if (hit.collider.CompareTag("Walls"))
            {
                Debug.Log("Refusing to fire at a wall");
                Invoke("FindingANewTarget", 0.01f);
                nextFireTime = Time.time + (1f / fireRate);
                return;
            }
        }

        // Fire from each spawn point
        foreach (Transform spawnPoint in projectileSpawnPoints)
        {
            if (spawnPoint == null)
            {
                Debug.LogWarning("Null spawn point found in list!");
                continue;
            }

            GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            turretAudio.PlayOneShot(turretFireSound, 1.0f);


            // Set the layer for the projectile and all its children
            SetLayerRecursively(projectile, LayerMask.NameToLayer("Projectile"));

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = projectile.AddComponent<Rigidbody>();
            }

            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            HomingProjectile behavior = projectile.AddComponent<HomingProjectile>();
            behavior.rotationSpeed = projectileRotationSpeed;
            behavior.homingStrength = projectileHomingStrength;
            behavior.maxLifetime = projectileLifetime;
            //behavior.explosionPrefab = explosionPrefab;  // Pass the explosion prefab
            //RecheckForEnemies();
            behavior.Initialize(enemy, projectileSpeed);
        }

        nextFireTime = Time.time + (1f / fireRate);
        timeSinceLastFired = 0;
    }

    private void DestroyTurret()
    {
        Destroy(turretAll);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw attack range
        Gizmos.color = Color.yellow;
        DrawWireDisc(transform.position, Vector3.up, attackRange);
    }
#endif

    private void DrawWireDisc(Vector3 center, Vector3 normal, float radius)
    {
        int segments = 32;
        Vector3 previousPoint = center + (Vector3.forward * radius);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            Vector3 newPoint = center + new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }
}
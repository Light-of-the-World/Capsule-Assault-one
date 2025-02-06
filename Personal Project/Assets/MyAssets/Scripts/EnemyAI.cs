using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject thisEnemy;

    public GameObject[] turretArray;

    private GameManager gameManager;
    private PlayerController playerController;
    private TurretController turretController;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    //enemy stats
    [Header ("Health")]
    public float startingHealth = 10f;
    public float currentHealth = 10f;
    public float currentDamage = 0f;
    public bool isDead;

    //attacking
    [Header ("Attack")]
    public float timeBetweenAttacks = 1f;
    bool alreadyAttacked;

    //states
    public float attackRange = 0.8f;
    public bool playerInAttackRange;



    private void Awake()
    {
        player = GameObject.Find("PlayerObj").transform;
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        agent = GetComponent<NavMeshAgent>();
        thisEnemy = gameObject;
        startingHealth = 10f;
        isDead = false;
    }

    private void Update()
    {
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        if (!playerInAttackRange) ChasePlayer();
        else AttackPlayer();
        CheckHealth();
    }
    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }
    private void AttackPlayer()
    {
        transform.LookAt(player);

        if(!alreadyAttacked)
        {
            //Attack code here
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
            playerController.TakeDamage(20);
        }
    }

    public void TakeDamage(float damageSource)
    {
        currentHealth -= damageSource;
        currentDamage += damageSource;
    }

    private void CheckHealth()
    {
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            // Gather all the turrets aiming at this enemy and re-adjust them
            turretArray = GameObject.FindGameObjectsWithTag("TurretWithScript");
            foreach (GameObject turret in turretArray)
            {
                turretController = turret.GetComponent<TurretController>();
                /*
                if (turretController.enemy == this.thisEnemy)
                {
                    //turretController.TargettedEnemyKilled();
                    turretController.UpdateTargetListAfterKill(thisEnemy);
                }
                */
                turretController.UpdateTargetListAfterKill(thisEnemy);
            }

            Invoke("DespawnEnemy", 0.05f);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void DespawnEnemy()
    {
        gameManager.DecreaseEnemyCount();
        gameManager.UpdateMoney(5);
        gameManager.UpdateScore();
        Destroy(gameObject);
    }
}

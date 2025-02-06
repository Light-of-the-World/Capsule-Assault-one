using UnityEngine;
using UnityEngine.AI;

public class TitleEnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    //enemy stats
    [Header("Health")]
    public float startingHealth = 10f;
    public float currentHealth = 10f;
    public float currentDamage = 0f;

    //attacking
    [Header("Attack")]
    public float timeBetweenAttacks = 1f;
    bool alreadyAttacked;

    //states
    public float attackRange = 1f;
    public bool playerInAttackRange;



    private void Awake()
    {
        player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();
        startingHealth = 10f;
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

        if (!alreadyAttacked)
        {
            //Attack code here
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    public void TakeDamage(float damageSource)
    {
        currentHealth -= damageSource;
        currentDamage += damageSource;
    }

    private void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
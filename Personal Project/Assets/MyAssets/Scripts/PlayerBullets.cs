using UnityEngine;
using UnityEngine.Rendering;

public class PlayerBullets : MonoBehaviour
{
    //Assignables
    public Rigidbody playerBulletRb;
    private GameManager gameManager;
    public LayerMask whatIsEnemies;

    //Stats for later
    //public float levelDamageModifier

    //Damage
    public float playerBulletDamage = 2f;

    //Lifetime
    public int maxCollisions = 1;
    public float maxLifetime = 1f;
    public bool activateOnTouch = true;

    int collisions;
    PhysicsMaterial physics_mat;
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Setup();
    }

    void Update()
    {
        //When to activate boolet
        if (collisions == maxCollisions) DestroyPlayerBullet();

        //count down lifetime
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) DestroyPlayerBullet();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            collision.collider.GetComponent<EnemyAI>().TakeDamage(playerBulletDamage);
            gameManager.PlayHitmarker();
            DestroyPlayerBullet();
        }
        //Count up collisions
        collisions++;
    }
    private void Setup()
    {
        //create a new PhysicsMaterial
        physics_mat = new PhysicsMaterial();
        physics_mat.frictionCombine = PhysicsMaterialCombine.Maximum;
        physics_mat.bounceCombine = PhysicsMaterialCombine.Maximum;
        //assign material to a collider
        GetComponent<BoxCollider>().material = physics_mat;
    }
    private void DestroyPlayerBullet()
    {
        Destroy(gameObject);
    }
}

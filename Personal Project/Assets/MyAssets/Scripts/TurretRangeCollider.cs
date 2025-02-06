using UnityEngine;

public class TurretRangeCollider : MonoBehaviour
{
    public GameObject turretWithScript;
    private TurretController turretScript;
    bool triggerActivated = false;
    void Start()
    {
        turretScript = turretWithScript.GetComponent<TurretController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerActivated)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("ColliderTriggered, setting as enemy");
                var enemy = other.gameObject;
                turretScript.UpdateTargetList(enemy);
                triggerActivated = true;
                Invoke("ResetTrigger", 0.2f);
            }
        }
    }

    private void ResetTrigger()
    {
        if (triggerActivated)
        {
            triggerActivated = false;
        }
    }
}

using UnityEngine;

public class TitleTurretRangeCollider : MonoBehaviour
{
    public GameObject turretWithScript;
    private TitleScreenTurreT titleScreenTurretScript;
    bool triggerActivated = false;
    void Start()
    {
        titleScreenTurretScript = turretWithScript.GetComponent<TitleScreenTurreT>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerActivated)
        {
            if (other.CompareTag("TitleEnemy"))
            {
                Debug.Log("ColliderTriggered, setting as enemy");
                var enemy = other.gameObject;
                titleScreenTurretScript.TitleColliderTarget(enemy);
                triggerActivated = true;
                Invoke("ResetTrigger", 1);
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

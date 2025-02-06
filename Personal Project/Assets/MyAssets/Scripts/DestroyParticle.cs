using UnityEngine;

public class DestroyParticle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("DestroyThisParticle", 0.5f);
    }

    private void DestroyThisParticle()
    {
        Destroy(gameObject);
    }
}

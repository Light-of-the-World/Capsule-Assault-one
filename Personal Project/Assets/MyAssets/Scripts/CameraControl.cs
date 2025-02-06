using Unity.VisualScripting;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float sensX = 400f;
    public float sensY = 400f;
    float xRotation;
    float yRotation;

    public Transform orientation;
    public Vector2 trackedRotation = Vector2.zero;
    private GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        orientation.rotation = Quaternion.Euler(0, 0, 0);
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        CamOrbit();
        if (gameManager.isGameOver == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (gameManager.paused == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (gameManager.paused == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void CamOrbit()
    {
        if (gameManager.isGameOver == false)
        {
            //get mouse input
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            //translate this into rotation
            yRotation += mouseX;
            xRotation -= mouseY;

            //Clamp the vertical looking
            xRotation = Mathf.Clamp(xRotation, -85, 85);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }
}

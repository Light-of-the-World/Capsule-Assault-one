using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButtonScript : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public void LoadGameScene()
    {
        loadingText.gameObject.SetActive(true);
        SceneManager.LoadScene("MyGame");
    }

}

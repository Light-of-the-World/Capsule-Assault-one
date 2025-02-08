using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtonScript : MonoBehaviour
{
    public void LoadMenuScene()
    {
        Time.timeScale = 1f;
        StatTrackerScript.Instance.SaveStats();
        SceneManager.LoadScene("TitleScreen");
    }
}

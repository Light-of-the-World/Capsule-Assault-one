using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class TitleGameManager : MonoBehaviour
{
    public GameObject titleEnemyPrefab;
    public GameObject spawnPos;
    public GameObject mainCanvas;
    public GameObject howCanvas;

    public Button howButton;
    public Button quitButton;
    public Button backButton;
    public GameObject quitCanvas;

    private void Start()
    {
        InvokeRepeating("SpawnTitleEnemy", 3f, 3f);
    }

    private void SpawnTitleEnemy()
    {
        Instantiate(titleEnemyPrefab, spawnPos.transform.position, titleEnemyPrefab.transform.rotation);
    }

    public void HowToPlayClicked()
    {
        mainCanvas.SetActive(false);
        howCanvas.SetActive(true);
    }
    public void BackButtonClicked()
    {
        howCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    public void CloseConfirmation()
    {
        quitCanvas.SetActive(true);
        mainCanvas.SetActive(false);
    }

    public void NotClosingGame()
    {
        quitCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}

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
    public GameObject statsCanvas;
    public GameObject quitCanvas;

    public Button howButton;
    public Button quitButton;
    public Button backButton;

    public TextMeshProUGUI HighScoreText;
    public TextMeshProUGUI TotalScoreText;
    public TextMeshProUGUI HighestRoundText;
    public TextMeshProUGUI TotalRoundsSurvived;
    public TextMeshProUGUI MoneyEarnedText;
    public TextMeshProUGUI EnemiesKilledText;
    public TextMeshProUGUI EnemiedKilledByPlayerText;
    public TextMeshProUGUI EnemiesKilledByTurretText;
    public TextMeshProUGUI TurretsPlacedText;
    public TextMeshProUGUI TotalBulletsFiredText;

    private void Start()
    {
        InvokeRepeating("SpawnTitleEnemy", 3f, 3f);
        StatTrackerScript.Instance.LoadStats();
    }

    private void SpawnTitleEnemy()
    {
        Instantiate(titleEnemyPrefab, spawnPos.transform.position, titleEnemyPrefab.transform.rotation);
    }

    public void StatsClicked()
    {
        mainCanvas.SetActive(false);
        statsCanvas.SetActive(true);
    }

    public void HowToPlayClicked()
    {
        mainCanvas.SetActive(false);
        howCanvas.SetActive(true);
    }
    public void BackButtonClicked()
    {
        howCanvas.SetActive(false);
        statsCanvas.SetActive(false);
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

    public void SetStats()
    {
        HighScoreText.text = "Highest Score: " + StatTrackerScript.Instance.HighScore;
        TotalScoreText.text = "Total score across all games: " + StatTrackerScript.Instance.TotalScore;
        HighestRoundText.text = "Highest wave: " + StatTrackerScript.Instance.HighRound;
        TotalRoundsSurvived.text = "Total rounds survived: " + StatTrackerScript.Instance.TotalRounds;
        MoneyEarnedText.text = "Total money earned: " + StatTrackerScript.Instance.TotalMoney;
        EnemiesKilledText.text = "Total enemies killed: " + StatTrackerScript.Instance.TotalEnemies;
        EnemiedKilledByPlayerText.text = "Enemies killed by player: " + StatTrackerScript.Instance.EnemiesByGun;
        EnemiesKilledByTurretText.text = "Enemies killed by turrets: " + StatTrackerScript.Instance.EnemiesByTurret;
        TurretsPlacedText.text = "Turrets placed: " + StatTrackerScript.Instance.TurretsPlaced;
        TotalBulletsFiredText.text = "Total bullets fired: " + StatTrackerScript.Instance.BulletsFired;
    }

    public void CloseGame()
    {
        StatTrackerScript.Instance.SaveStats();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}

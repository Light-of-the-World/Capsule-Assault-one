using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject hitmarker;
    private GameObject[] enemiesToDespawn;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI healthRegenText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI applyText;
    [SerializeField] private TextMeshProUGUI volumeNumText;
    [SerializeField] private TextMeshProUGUI sensNumText;
    [SerializeField] private GameObject centerDot;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensSlider;
    private float upperSpawnRangeX = -22f;
    private float lowerSpawnRangeX = 22f;
    private float upperSpawnRangeZ = 0f;
    private float lowerSpawnRangeZ = -30f;
    private float startDelay = 2;
    private float spawnInterval = 1.5f;
    public int enemiesLeftInWave;
    public float enemyHealth = 10f;
    public int waveNumber = 0;
    public int enemiesToSpawn;
    public int waveNumberScaling = 2;
    public int waveNumberScalingTracker;
    public bool paused;
    public int money;
    public int score;
    public float playerHealth;
    public float healthRegenDelay = 5;
    public bool isGameOver;
    public bool atFullHealth;
    // Start is called before the first frame update
    void Start()
    {
        paused = false;
        Time.timeScale = 1f;
        isGameOver = false;
        score = 0;
        scoreText.text = "Score: " + score;
        money = 0;
        moneyText.text = "Money: " + money;
        playerHealth = 100;
        healthText.text = "Health: " + playerHealth;
        atFullHealth = true;
        volumeSlider.value = OptionsSaverScript.Instance.Volume;
        volumeNumText.text = "" + OptionsSaverScript.Instance.Volume;
        sensSlider.value = OptionsSaverScript.Instance.Sensitivity;
        sensNumText.text = "" + OptionsSaverScript.Instance.Sensitivity;
    }

    // Update is called once per frame
    void Update()
    {
        WaveHandler();
        HealthHandler();
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            ChangePaused();
        }
    }

    void ChangePaused()
    {
        if (!paused)
        {
            paused = true;
            pauseScreen.SetActive(true);
            restartButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
            applyButton.gameObject.SetActive(true);
            applyText.text = "Apply";
            centerDot.SetActive(false);
            Time.timeScale = 0f;
        }
        else
        {
            paused = false;
            pauseScreen.SetActive(false);
            restartButton.gameObject.SetActive(false);
            mainMenuButton.gameObject.SetActive(false);
            applyButton.gameObject.SetActive(false);
            centerDot.SetActive(true);

            Time.timeScale = 1f;
        }
    }

    void WaveHandler()
    {
        waveText.text = "Wave: " + waveNumber;
        if (enemiesLeftInWave == 0)
        {
            waveNumber++;
            waveNumberScalingTracker++;
            if (waveNumberScalingTracker == 5)
            {
                waveNumberScaling *= 2;
                waveNumberScalingTracker = 0;
            }
            enemiesToSpawn = (waveNumber * 3) + 5 + (waveNumberScaling);
            if (enemiesToSpawn > 200)
            {
                enemiesToSpawn = 200;
            }
            SpawnEnemyWave(enemiesToSpawn);
            enemiesLeftInWave = enemiesToSpawn;
        }
    }

    void HealthHandler()
    {
        healthRegenText.text = "Health Regen Delay: " + healthRegenDelay;
        if (playerHealth < 100) { atFullHealth = false; }
        if (!atFullHealth)
        {
            healthRegenDelay -= (Time.deltaTime * Time.timeScale);
            if (healthRegenDelay < 0)
            {
                playerHealth += 20 * (Time.deltaTime * Time.timeScale);
            }
            if (playerHealth >= 100)
            {
                playerHealth = 100;
                atFullHealth = true;
            }
            if (playerHealth <= 0)
            {
                GameOver();
            }
        }
        healthText.text = "Health: " + Math.Round(playerHealth, 1);
    }

    void SpawnEnemyWave(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        }
    }

    private Vector3 GenerateSpawnPosition()
    {
        Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(lowerSpawnRangeX, upperSpawnRangeX), 0.1f, UnityEngine.Random.Range(lowerSpawnRangeZ, upperSpawnRangeZ));
        if (spawnPos.x < 0f)
        {
            spawnPos.x = (UnityEngine.Random.Range(-18f, -22f));
        }
        else if (spawnPos.x == 0f)
        {
            spawnPos.x = (UnityEngine.Random.Range(-18f, -22f));
        }
        else if (spawnPos.x > 0f)
        {
            spawnPos.x = (UnityEngine.Random.Range(18f, 22f));
        }
        return spawnPos;
    }

    public void DecreaseEnemyCount()
    {
        enemiesLeftInWave--;
    }

    public void UpdateScore()
    {
        score += 14 + waveNumber;
        StatTrackerScript.Instance.TotalScore += 14 + waveNumber;
        scoreText.text = "Score: " + score;
    }

    public void UpdateMoney (int amount)
    {
        money += amount;
        moneyText.text = "Money: " + money;
    }

    public void PlayHitmarker()
    {
        hitmarker.gameObject.SetActive(true);
        Invoke("DeactivateHitmarker", 0.1f);
    }
    public void DeactivateHitmarker()
    {
        hitmarker.gameObject.SetActive(false);
    }

    public void ResetHealthDelay()
    {
        healthRegenDelay = 5;
    }

    public void GameOver()
    {
        if (!isGameOver)
        {
            isGameOver=true;
            StatTrackerScript.Instance.TotalRounds += (waveNumber - 1);
            if (waveNumber > StatTrackerScript.Instance.HighRound)
            {
                StatTrackerScript.Instance.HighRound = waveNumber;
            }
            if (score > StatTrackerScript.Instance.HighScore)
            {
                StatTrackerScript.Instance.HighScore = score;
            }
            gameOverText.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
            mainMenuButton.gameObject.SetActive(true);
            enemiesToDespawn = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemiesToDespawn)
            {
                Destroy(enemy);
            }
            StatTrackerScript.Instance.SaveStats();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ApplyButtonClicked()
    {
        OptionsSaverScript.Instance.Volume = volumeSlider.value;
        OptionsSaverScript.Instance.Sensitivity = (int)sensSlider.value;
        OptionsSaverScript.Instance.SaveOptions();
        applyText.text = "Applied!";
    }
}
using UnityEngine;
using System.IO;

public class StatTrackerScript : MonoBehaviour
{
    public static StatTrackerScript Instance;

    public int HighScore;
    public int TotalScore;
    public int HighRound;
    public int TotalRounds;
    public int TotalMoney;
    public int TotalEnemies;
    public int EnemiesByGun;
    public int EnemiesByTurret;
    public int TurretsPlaced;
    public int BulletsFired;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadStats();
    }
    [System.Serializable]
    class SaveData
    {
        public int HighScore;
        public int TotalScore;
        public int HighRound;
        public int TotalRounds;
        public int TotalMoney;
        public int TotalEnemies;
        public int EnemiesByGun;
        public int EnemiesByTurret;
        public int TurretsPlaced;
        public int BulletsFired;
    }

    public void SaveStats()
    {
        SaveData data = new SaveData();
        data.HighScore = HighScore;
        data.TotalScore = TotalScore;
        data.HighRound = HighRound;
        data.TotalRounds = TotalRounds;
        data.TotalMoney = TotalMoney;
        data.TotalEnemies = TotalEnemies;
        data.EnemiesByGun = EnemiesByGun;
        data.EnemiesByTurret = EnemiesByTurret;
        data.TurretsPlaced = TurretsPlaced;
        data.BulletsFired = BulletsFired;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    public void LoadStats()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            HighScore = data.HighScore;
            TotalScore = data.TotalScore;
            HighRound = data.HighRound;
            TotalRounds = data.TotalRounds;
            TotalMoney = data.TotalMoney;
            TotalEnemies = data.TotalEnemies;
            EnemiesByGun = data.EnemiesByGun;
            EnemiesByTurret = data.EnemiesByTurret;
            TurretsPlaced = data.TurretsPlaced;
            BulletsFired = data.BulletsFired;
        }
        GameObject.Find("TitleGameManager").GetComponent<TitleGameManager>().SetStats();
    }
}

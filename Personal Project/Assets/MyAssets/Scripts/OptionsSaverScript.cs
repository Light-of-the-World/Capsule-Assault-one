using UnityEngine;
using System.IO;

public class OptionsSaverScript : MonoBehaviour
{
    public static OptionsSaverScript Instance;

    public float Volume;
    public int Sensitivity;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadOptionsAtGameLaunch();
    }
    [System.Serializable]
    class SaveOptionsData
    {
        public float Volume;
        public int Sensitivity;
    }

    public void SaveOptions()
    {
        SaveOptionsData data = new SaveOptionsData();
        data.Volume = Volume;
        data.Sensitivity = Sensitivity;

        string json = JsonUtility.ToJson(data);

        File.WriteAllText(Application.persistentDataPath + "/options.json", json);
    }

    public void LoadOptionsAtGameLaunch()
    {
        string path = Application.persistentDataPath + "/options.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveOptionsData data = JsonUtility.FromJson<SaveOptionsData>(json);

            Volume = data.Volume;
            Sensitivity = data.Sensitivity;
        }
        else
        {
            Volume = 0.8f;
            Sensitivity = 5;
        }
    }
}

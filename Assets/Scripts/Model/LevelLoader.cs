using UnityEngine;
using System.IO;

namespace Game.Match3.Model
{
    public class LevelLoader
    {
        private static string levelsPath = Application.dataPath + "/Resources/Levels/";
        private static string progressKey = "CurrentLevel"; 
        private int currentLevelIndex = 0; 
        public int CurrentLevelIndex => currentLevelIndex;
        public LevelLoader()
        {
            LoadProgress(); 
        }

        public int[,] LoadLevel(int levelIndex)
        {
            string fileName = $"level_{levelIndex}.txt";
            string filePath = levelsPath + fileName;

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                int rows = int.Parse(lines[0]);
                int columns = int.Parse(lines[1]);

                int[,] boardDefinition = new int[rows, columns];
                for (int y = 0; y < rows; y++)
                {
                    string[] row = lines[y + 2].Split(' ');
                    for (int x = 0; x < columns; x++)
                    {
                        boardDefinition[y, x] = int.Parse(row[x]);
                    }
                }

                currentLevelIndex = levelIndex; 
                SaveProgress(); 
                return boardDefinition;
            }
            else
            {
                Debug.LogError($"Level file not found: {fileName}");
                return null;
            }
        }

        public int[,] LoadNextLevel()
        {
            return LoadLevel(currentLevelIndex + 1);
        }

        public void SaveProgress()
        {
            PlayerPrefs.SetInt(progressKey, currentLevelIndex);
            PlayerPrefs.Save();
            Debug.Log($"Progress saved. Current Level: {currentLevelIndex}");
        }

        //todo use custom saving system
        public void LoadProgress()
        {
            if (PlayerPrefs.HasKey(progressKey))
            {
                currentLevelIndex = PlayerPrefs.GetInt(progressKey);
                Debug.Log($"Progress loaded. Current Level: {currentLevelIndex}");
            }
            else
            {
                currentLevelIndex = 1; 
                Debug.Log("No saved progress found. Starting from Level 1.");
            }
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(progressKey);
            currentLevelIndex = 1;
            Debug.Log("Progress reset. Starting from Level 1.");
        }

        public int[,] GetCurrentLevelBoard()
        {
            return LoadLevel(currentLevelIndex);
        }
    }
}

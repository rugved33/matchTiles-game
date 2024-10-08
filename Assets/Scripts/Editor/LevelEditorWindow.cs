using UnityEditor;
using UnityEngine;
using System.IO;

namespace Game.Match3
{
    public class LevelEditorWindow : EditorWindow
    {
        private int rows = 6;
        private int columns = 6;
        private int[,] boardDefinition;
        private string fileName = "level_1"; // Default file name, without extension

        [MenuItem("Tools/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        private void OnEnable()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            boardDefinition = new int[rows, columns];
        }

        private void OnGUI()
        {
            GUILayout.Label("Level Designer", EditorStyles.boldLabel);

            rows = EditorGUILayout.IntField("Rows", rows);
            columns = EditorGUILayout.IntField("Columns", columns);

            if (boardDefinition.GetLength(0) != rows || boardDefinition.GetLength(1) != columns)
            {
                InitializeBoard();
            }

            for (int y = 0; y < rows; y++)
            {
                GUILayout.BeginHorizontal();
                for (int x = 0; x < columns; x++)
                {
                    boardDefinition[y, x] = EditorGUILayout.IntField(boardDefinition[y, x], GUILayout.Width(30));
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("Save/Load Level", EditorStyles.boldLabel);

            fileName = EditorGUILayout.TextField("File Name", fileName);

            if (GUILayout.Button("Save Level"))
            {
                SaveLevel();
            }

            if (GUILayout.Button("Load Level"))
            {
                LoadLevel();
            }
        }

        private void SaveLevel()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("Please specify a valid file name.");
                return;
            }

            if (!fileName.EndsWith(".txt"))
            {
                fileName += ".txt";
            }

            string filePath = Application.dataPath + "/Resources/Levels/" + fileName;

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(rows);
                writer.WriteLine(columns);
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < columns; x++)
                    {
                        writer.Write(boardDefinition[y, x] + " ");
                    }
                    writer.WriteLine();
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Level saved as {fileName}");
        }

        private void LoadLevel()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("Please specify a valid file name.");
                return;
            }

            if (!fileName.EndsWith(".txt"))
            {
                fileName += ".txt";
            }

            string filePath = Application.dataPath + "/Resources/Levels/" + fileName;

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                rows = int.Parse(lines[0]);
                columns = int.Parse(lines[1]);

                boardDefinition = new int[rows, columns];
                for (int y = 0; y < rows; y++)
                {
                    string[] row = lines[y + 2].Split(' ');
                    for (int x = 0; x < columns; x++)
                    {
                        boardDefinition[y, x] = int.Parse(row[x]);
                    }
                }

                Debug.Log($"Level {fileName} loaded successfully.");
            }
            else
            {
                Debug.LogError($"Level file not found: {fileName}");
            }
        }
    }
}

using Game.Match3.Model;
using Game.Match3.ViewComponents;
using UnityEngine;

namespace Game.Match3
{

	public class Boot : MonoBehaviour
	{

		[SerializeField] private BoardRenderer boardRenderer;

		private LevelLoader levelLoader;
		private void Start()
		{
			levelLoader = new LevelLoader();

			int[,] boardDefinition = levelLoader.GetCurrentLevelBoard();
				
			if (boardDefinition != null)
            {
                var pieceSpawner = new PieceSpawner();
                var board = Board.Create(boardDefinition, pieceSpawner);
                boardRenderer.Initialize(board, board.PieceSpawner);
            }
		}

		public void NextLevel()
        {
            int[,] nextBoard = levelLoader.LoadNextLevel();

            if (nextBoard != null)
            {
                var pieceSpawner = new PieceSpawner();
                var board = Board.Create(nextBoard, pieceSpawner);
                boardRenderer.Initialize(board, board.PieceSpawner);
            }
        }

		public void ResetProgress()
        {
            levelLoader.ResetProgress();
        }

	}

}

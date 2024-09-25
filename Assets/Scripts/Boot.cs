using Game.Match3.Model;
using Game.Match3.ViewComponents;
using UnityEngine;

namespace Game.Match3
{

	public class Boot : MonoBehaviour
	{

		[SerializeField] private BoardRenderer boardRenderer;

		void Start()
		{

			int[,] boardDefinition = {
				{3, 3, 1, 2, 3, 3},
				{2, 2, 1, 2, 3, 3},
				{1, 1, 0, 0, 2, 2},
				{2, 2, 0, 0, 1, 1},
				{1, 1, 2, 2, 1, 1},
				{1, 1, 2, 2, 1, 1},
			};

			var pieceSpawner = new PieceSpawner();
			var board = Board.Create(boardDefinition, pieceSpawner);

			boardRenderer.Initialize(board, board.PieceSpawner);
		}

	}

}

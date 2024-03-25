using Game.Match3.Model;
using UnityEngine;

namespace Game.Match3.ViewComponents
{

	public class BoardRenderer : MonoBehaviour
	{

		[SerializeField] private PieceTypeDatabase pieceTypeDatabase;
		[SerializeField] private VisualPiece visualPiecePrefab;

		private Board board;

		public void Initialize(Board board)
		{
			this.board = board;

			CenterCamera();
			CreateVisualPiecesFromBoardState();
		}

		private void CenterCamera()
		{
			Camera.main.transform.position = new Vector3((board.Width - 1) * 0.5f, -(board.Height - 1) * 0.5f);
		}

		private void CreateVisualPiecesFromBoardState()
		{
			DestroyVisualPieces();

			foreach (var pieceInfo in board.IteratePieces())
			{

				var visualPiece = CreateVisualPiece(pieceInfo.piece);
				visualPiece.transform.localPosition = LogicPosToVisualPos(pieceInfo.pos.x, pieceInfo.pos.y);

			}
		}

		public Vector3 LogicPosToVisualPos(float x, float y)
		{
			return new Vector3(x, -y, -y);
		}

		private BoardPos ScreenPosToLogicPos(float x, float y)
		{

			var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(x, y, -Camera.main.transform.position.z));
			var boardSpace = transform.InverseTransformPoint(worldPos);

			return new BoardPos()
			{
				x = Mathf.RoundToInt(boardSpace.x),
				y = -Mathf.RoundToInt(boardSpace.y)
			};

		}

		private VisualPiece CreateVisualPiece(Piece piece)
		{

			var pieceObject = Instantiate(visualPiecePrefab, transform, true);
			var sprite = pieceTypeDatabase.GetSpriteForPieceType(piece.type);
			pieceObject.SetSprite(sprite);
			return pieceObject;

		}

		private void DestroyVisualPieces()
		{
			foreach (var visualPiece in GetComponentsInChildren<VisualPiece>())
			{
				Object.Destroy(visualPiece.gameObject);
			}
		}

		private void Update()
		{

			if (Input.GetMouseButtonDown(0))
			{

				var pos = ScreenPosToLogicPos(Input.mousePosition.x, Input.mousePosition.y);

				if (board.IsWithinBounds(pos.x, pos.y))
				{
					board.Resolve(pos.x, pos.y);
					CreateVisualPiecesFromBoardState();
				}

			}
		}

	}

}

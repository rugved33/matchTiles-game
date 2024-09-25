using Game.Match3.Model;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Match3.ViewComponents
{
    public class BoardRenderer : MonoBehaviour
    {
        [SerializeField] private PieceTypeDatabase pieceTypeDatabase;
        [SerializeField] private VisualPiece visualPiecePrefab;
        [SerializeField] private float moveSpeed = 5f; 
        [SerializeField] private float spawnDelay = 0.2f; 
        [SerializeField] private float spawnOffset = 2f; 

        private Board board;
        private IPieceSpawner pieceSpawner;
        private Dictionary<Piece, VisualPiece> visualPieces = new Dictionary<Piece, VisualPiece>();
        private ResolveResult resolveResult;
		private bool inputEnabled = true;

        public void Initialize(Board board, IPieceSpawner pieceSpawner)
        {
            this.board = board;
            this.pieceSpawner = pieceSpawner;

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
            visualPieces.Clear();

            foreach (var pieceInfo in board.IteratePieces())
            {
                if (pieceInfo.piece != null)
                {
                    var visualPiece = CreateVisualPiece(pieceInfo.piece);
                    visualPiece.transform.localPosition = LogicPosToVisualPos(pieceInfo.pos.x, pieceInfo.pos.y);
                    visualPieces[pieceInfo.piece] = visualPiece;
                }
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

            Debug.Log($"Created visual piece for piece type {piece.type}");

            return pieceObject;
        }

        private void DestroyVisualPieces()
        {
            foreach (var visualPiece in GetComponentsInChildren<VisualPiece>())
            {
                Destroy(visualPiece.gameObject);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && inputEnabled)
            {
                var pos = ScreenPosToLogicPos(Input.mousePosition.x, Input.mousePosition.y);

                if (board.IsWithinBounds(pos.x, pos.y))
                {
					inputEnabled = false;
                    resolveResult = board.Resolve(pos.x, pos.y);
                    StartCoroutine(AnimateBoardChanges());
                }
            }
        }

        private IEnumerator AnimateBoardChanges()
        {
            DestroyClearedPieces();

            yield return AnimateFallingPieces();

            // Use the resolveResult to spawn and animate new pieces based on CreationTime
            yield return SpawnAndAnimateNewPieces(resolveResult);
        }

        private void DestroyClearedPieces()
        {
            var piecesToDestroy = new List<Piece>();

            foreach (var pieceInfo in visualPieces)
            {
                if (pieceInfo.Key == null || !board.TryGetPiecePos(pieceInfo.Key, out _, out _))
                {
                    piecesToDestroy.Add(pieceInfo.Key);
                }
            }

            foreach (var piece in piecesToDestroy)
            {
                if (visualPieces.ContainsKey(piece))
                {
                    Destroy(visualPieces[piece].gameObject);
                    visualPieces.Remove(piece);
                }
            }
        }

        private IEnumerator AnimateFallingPieces()
        {
            bool anyPieceMoved = true;

            while (anyPieceMoved)
            {
                anyPieceMoved = false;

                foreach (var pieceInfo in board.IteratePieces())
                {
                    var piece = pieceInfo.piece;
                    if (piece == null) continue;

                    if (!visualPieces.ContainsKey(piece))
                    {
                        continue;
                    }

                    var visualPieceObj = visualPieces[piece];
                    Vector3 targetPosition = LogicPosToVisualPos(pieceInfo.pos.x, pieceInfo.pos.y);

                    if (visualPieceObj.transform.localPosition != targetPosition)
                    {
                        anyPieceMoved = true;
                        visualPieceObj.transform.localPosition = Vector3.MoveTowards(
                            visualPieceObj.transform.localPosition,
                            targetPosition,
                            moveSpeed * Time.deltaTime
                        );
                    }
                }

                yield return null; 
            }
        }

        private IEnumerator SpawnAndAnimateNewPieces(ResolveResult resolveResult)
        {

            var newPieces = new List<KeyValuePair<Piece, ChangeInfo>>();

            foreach (var change in resolveResult.changes)
            {
                if (change.Value.WasCreated)
                {
                    newPieces.Add(new KeyValuePair<Piece, ChangeInfo>(change.Key, change.Value));
                }
            }

            newPieces.Sort((a, b) => a.Value.CreationTime.CompareTo(b.Value.CreationTime));

            foreach (var newPieceEntry in newPieces)
            {
                var newPiece = newPieceEntry.Key;
                var changeInfo = newPieceEntry.Value;

                var visualPiece = CreateVisualPiece(newPiece);


                visualPiece.transform.localPosition = new Vector3(changeInfo.ToPos.x, -(changeInfo.ToPos.y + spawnOffset), -(changeInfo.ToPos.y + spawnOffset));
                visualPieces[newPiece] = visualPiece;

                yield return StartCoroutine(AnimatePieceFall(visualPiece, newPiece, changeInfo.ToPos.x, changeInfo.ToPos.y));

                yield return new WaitForSeconds(spawnDelay);
            }

			inputEnabled = true;
        }

        private IEnumerator AnimatePieceFall(VisualPiece visualPiece, Piece piece, int targetX, int targetY)
        {
            Vector3 targetPosition = LogicPosToVisualPos(targetX, targetY);

            while (visualPiece.transform.localPosition != targetPosition)
            {
                visualPiece.transform.localPosition = Vector3.MoveTowards(
                    visualPiece.transform.localPosition,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );
                yield return null; 
            }

            Debug.Log($"Piece {piece.type} reached target position ({targetX}, {targetY})");
        }
    }
}

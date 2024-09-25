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
        private Dictionary<BoardPos, Piece> previousState = new Dictionary<BoardPos, Piece>();

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
            if (Input.GetMouseButtonDown(0))
            {
                var pos = ScreenPosToLogicPos(Input.mousePosition.x, Input.mousePosition.y);

                if (board.IsWithinBounds(pos.x, pos.y))
                {
                    SaveBoardState(); 
                    board.Resolve(pos.x, pos.y);
                    StartCoroutine(AnimateBoardChanges());
                }
            }
        }

        private void SaveBoardState()
        {
            previousState.Clear();
            foreach (var pieceInfo in board.IteratePieces())
            {
                if (pieceInfo.piece != null)
                {
                    previousState[new BoardPos(pieceInfo.pos.x, pieceInfo.pos.y)] = pieceInfo.piece;
                }
            }
            Debug.Log($"Saved {previousState.Count} pieces in the previous state.");
        }

        private List<BoardPos> GetEmptyPositions()
        {
            List<BoardPos> emptyPositions = new List<BoardPos>();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var piece = board.GetAt(x, y);

                    // Check if the position is empty now but was previously filled
                    if (piece == null && previousState.ContainsKey(new BoardPos(x, y)))
                    {
                        emptyPositions.Add(new BoardPos(x, y));
                    }
                }
            }

            Debug.Log($"Found {emptyPositions.Count} empty positions to fill.");
            return emptyPositions;
        }

        private IEnumerator AnimateBoardChanges()
        {
            DestroyClearedPieces();

            yield return AnimateFallingPieces();

            var emptyPositions = GetEmptyPositions();

            yield return SpawnAndAnimateNewPieces(emptyPositions);
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

        private IEnumerator SpawnAndAnimateNewPieces(List<BoardPos> emptyPositions)
        {
            foreach (var emptyPos in emptyPositions)
            {
                Debug.Log($"Spawning new piece at position ({emptyPos.x}, {emptyPos.y})");

                var newPiece = board.CreatePiece(pieceSpawner.CreateBasicPiece(), emptyPos.x, emptyPos.y); 
                Debug.Log($"New piece created with type {newPiece.type}");

                var visualPiece = CreateVisualPiece(newPiece); 

                visualPiece.transform.localPosition = new Vector3(emptyPos.x, -(emptyPos.y + spawnOffset), -(emptyPos.y + spawnOffset));

                visualPieces[newPiece] = visualPiece; 

                StartCoroutine(AnimatePieceFall(visualPiece, newPiece, emptyPos.x, emptyPos.y));

                yield return new WaitForSeconds(spawnDelay);
            }
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

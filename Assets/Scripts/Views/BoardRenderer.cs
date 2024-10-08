﻿using Game.Match3.Model;
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
        private List<Vector3> destroyedTilePositions = new List<Vector3>();
        private ResolveResult resolveResult;
		private bool inputEnabled = true;
        private const float FallDelay = 0.2f;
        public event System.Action<int,int> OnPieceClicked;

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
            visualPieces.Clear();
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0) && inputEnabled)
            {
                var pos = ScreenPosToLogicPos(Input.mousePosition.x, Input.mousePosition.y);
                OnPieceClicked?.Invoke(pos.x, pos.y);
            }
        }

        public void ToggleInput(bool value)
        {
            inputEnabled = value;
        }

        public void AnimateBoardChanges(ResolveResult resolveResult)
        {
            this.resolveResult = resolveResult;
            StartCoroutine(Co_AnimateBoardChanges());
        }

        private IEnumerator Co_AnimateBoardChanges()
        {
            DestroyClearedPieces();

            yield return AnimateFallingPieces();

            yield return SpawnAndAnimateNewPieces(resolveResult);
        }

        public void ShakePiece(Piece piece)
        {
           visualPieces.TryGetValue(piece, out VisualPiece visualPiece);
           visualPiece.DOShakePosition();
        }

        private void DestroyClearedPieces()
        {
            destroyedTilePositions.Clear();
            var piecesToDestroy = new List<Piece>();

            foreach (var pieceInfo in visualPieces)
            {
                if (pieceInfo.Key == null || !board.TryGetPiecePos(pieceInfo.Key, out _, out _))
                {
                    piecesToDestroy.Add(pieceInfo.Key);
                    destroyedTilePositions.Add(visualPieces[pieceInfo.Key].transform.localPosition);
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

            foreach (var destroyedPos in destroyedTilePositions)
            {
                foreach (var pieceInfo in visualPieces)
                {
                    var visualPiece = pieceInfo.Value;

                    if (visualPiece.transform.localPosition.y > destroyedPos.y && visualPiece.transform.localPosition.x == destroyedPos.x)
                    {
                        visualPiece.DoJump();
                        anyPieceMoved = true;
                    }
                }
            }

            yield return new WaitForSeconds(FallDelay);

            List<VisualPiece> movedPieces = new List<VisualPiece>();

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
                        movedPieces.Add(visualPieceObj);
                    }
                }

                yield return null;
            }

            foreach (var visualPiece in movedPieces)
            {
                visualPiece.DOShakePosition();
            }
        }

        private IEnumerator SpawnAndAnimateNewPieces(ResolveResult resolveResult)
        {

            var newPieces = new List<KeyValuePair<Piece, ChangeInfo>>();
            List<VisualPiece> spawnedPieces = new List<VisualPiece>();

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
                spawnedPieces.Add(visualPiece);

                visualPiece.transform.localPosition = new Vector3(changeInfo.ToPos.x, -(changeInfo.ToPos.y + spawnOffset), -(changeInfo.ToPos.y + spawnOffset));
                visualPieces[newPiece] = visualPiece;

                yield return StartCoroutine(AnimatePieceFall(visualPiece, newPiece, changeInfo.ToPos.x, changeInfo.ToPos.y));

                yield return new WaitForSeconds(spawnDelay);
            }

            foreach (var visualPiece in spawnedPieces)
            {
                visualPiece.DOShakePosition();
            }
            yield return new WaitForSeconds(FallDelay);
            ToggleInput(true);
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

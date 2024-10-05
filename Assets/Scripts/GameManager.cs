using System;
using Game.Match3;
using Game.Match3.ViewComponents;
using Game.Match3.Model;
using Game.Match3.Presenter;

namespace Game.Match3
{
    public class GameManager
    {
        public GameState GameState { get; private set; }
        public BoardRenderer BoardRenderer{ get; private set; }
        public LevelLoader LevelLoader { get; private set; }
        public BoardPresenter BoardPresenter { get; private set; }
        public PieceSpawner PieceSpawner { get; private set; }

        public GameManager(BoardRenderer boardRenderer,
                            LevelLoader levelLoader,
                            PieceSpawner pieceSpawner)
        {
            BoardRenderer = boardRenderer;
            PieceSpawner = pieceSpawner;
            LevelLoader  = levelLoader;
        }

        public void StartGame()
        {
            var levelDefinition = LevelLoader.GetCurrentLevelBoard();

            if(levelDefinition != null)
            {
                var board = Board.Create(levelDefinition, PieceSpawner);

                GameState = new GameState(board);

                BoardPresenter = new BoardPresenter(GameState, BoardRenderer, PieceSpawner);

                BoardRenderer.Initialize(GameState.Board, GameState.Board.PieceSpawner);
            }
        }
    }
}
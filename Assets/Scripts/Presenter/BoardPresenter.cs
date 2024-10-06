using Game.Match3.Model;
using Game.Match3.ViewComponents;


namespace Game.Match3.Presenter
{
    public class BoardPresenter
    {
        private GameState gameState;
        private BoardRenderer boardRenderer; 
        private IPieceSpawner pieceSpawner;

        public BoardPresenter(GameState gameState, BoardRenderer boardRenderer, IPieceSpawner pieceSpawner)
        {
            this.gameState = gameState; 
            this.boardRenderer = boardRenderer;
            this.pieceSpawner = pieceSpawner;

            this.boardRenderer.OnPieceClicked += HandlePieceClick;
        }

        private void HandlePieceClick(int x, int y)
        {
            if (gameState.Board.IsWithinBounds(x, y))
            {
                if (gameState.Board.HasConnections(x, y))
                {
                    boardRenderer.ToggleInput(false);
                    ResolveBoard(x, y); 
                }
                else
                {
                    boardRenderer.ShakePiece(gameState.Board.GetAt(x, y));
                }
            }
        }

        private void ResolveBoard(int x, int y)
        {
            //todo use command here
            gameState.ResolveBoard(x, y); 
            boardRenderer.AnimateBoardChanges(gameState.ResolveResult);
        }


        public void SaveGameState()
        {
           //todo
        }

        public void LoadGameState()
        {
            //todo
        }
    }
}

namespace Game.Match3.Model
{
    [System.Serializable]
    public class GameState
    {
        public Board Board { get; private set; }
        public int Score { get; private set; } 
        public ResolveResult ResolveResult{ get; private set; }

        public GameState(Board board)
        {
            Board = board;
            Score = 0;
        }

        public void ResolveBoard(int x, int y)
        {
            ResolveResult = Board.Resolve(x,y);
        }
    }
}
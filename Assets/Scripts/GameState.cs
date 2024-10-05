using Game.Match3.Model;

namespace Game.Match3
{
    [System.Serializable]
    public class GameState
    {
        public Board Board { get; private set; }
        public int Score { get; private set; } 

        public GameState(Board board)
        {
            Board = board;
            Score = 0;
        }
    }
}
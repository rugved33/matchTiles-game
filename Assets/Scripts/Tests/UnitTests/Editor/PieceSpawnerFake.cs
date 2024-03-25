using Game.Match3.Model;

namespace Game.Match3.Tests.UnitTests
{
    public class PieceSpawnerFake : IPieceSpawner
    {

        private readonly int value;

        public PieceSpawnerFake(int value)
        {
            this.value = value;
        }

        public int CreateBasicPiece()
        {
            return value;
        }
    }
}
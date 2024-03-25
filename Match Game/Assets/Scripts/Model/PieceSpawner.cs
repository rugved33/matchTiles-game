namespace Game.Match3.Model
{

	public class PieceSpawner : IPieceSpawner
	{

		public int CreateBasicPiece()
		{
			return UnityEngine.Random.Range(0, 4);
		}

	}

}
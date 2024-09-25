namespace Game.Match3.Model
{

	public struct BoardPos
	{
		public int x;
		public int y;

		public BoardPos(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public bool IsValid()
		{
			// A position is valid if both x and y are greater than or equal to 0
			return x >= 0 && y >= 0;
		}
	}

}
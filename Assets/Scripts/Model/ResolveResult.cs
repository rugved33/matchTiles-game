using System.Collections.Generic;

namespace Game.Match3.Model
{

	public class ResolveResult
	{
		public readonly Dictionary<Piece, ChangeInfo> changes = new Dictionary<Piece, ChangeInfo>();
	}

}
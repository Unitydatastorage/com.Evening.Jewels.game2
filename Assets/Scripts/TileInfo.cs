namespace MatchThreeEngine
{
	public readonly struct TileInfo
	{
		public readonly int X;
		public readonly int Y;

		public readonly int TypeId;

		public TileInfo(int x, int y, int typeId)
		{
			X = x;
			Y = y;

			TypeId = typeId;
		}
	}
}

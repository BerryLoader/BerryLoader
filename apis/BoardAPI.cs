using UnityEngine;

namespace BerryLoaderNS
{
	public static class BoardAPI
	{
		public static CreatePackLine BoardToPackLine(string boardId)
		{
			return BoardToPackLine(WorldManager.instance.Boards.Find(x => x.Id == boardId));
		}

		public static CreatePackLine BoardToPackLine(GameBoard board)
		{
			return board.GetComponentInChildren<CreatePackLine>();
		}
	}
}

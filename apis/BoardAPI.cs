using UnityEngine;

namespace BerryLoaderNS
{
	public static class BoardAPI
	{
		public static GameBoard CreateBoard(string boardId)
		{
			GameBoard board = MonoBehaviour.Instantiate(WorldManager.instance.Boards[1]);
			board.Id = boardId;
			board.transform.position = new Vector3((WorldManager.instance.Boards.Count - 1) * 100, -0.528f, 0);
			WorldManager.instance.Boards.Add(board);
			foreach (Transform child in BoardToPackLine(board).transform)
				MonoBehaviour.Destroy(child.gameObject);
			var pb = new MaterialPropertyBlock();
			pb.SetVector("_Color", new Vector4(1, 0, 0, 1));
			pb.SetVector("_Color2", new Vector4(0, 1, 0, 1));
			pb.SetVector("_StoneColor", new Vector4(0, 0, 1, 1));
			board.gameObject.GetComponent<MeshRenderer>().SetPropertyBlock(pb);
			foreach (var sr in board.transform.GetComponentsInChildren<SpriteRenderer>())
				MonoBehaviour.Destroy(sr.gameObject);
			return board;
		}

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

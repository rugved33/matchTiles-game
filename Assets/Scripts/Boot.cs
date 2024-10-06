using Game.Match3.Model;
using Game.Match3.ViewComponents;
using Scripts.Core;
using UnityEngine;

namespace Game.Match3
{
	public class Boot : MonoBehaviour, IDependencyProvider
	{

		[Inject] private BoardRenderer boardRenderer;
		[Inject] private LevelLoader levelLoader;
		[Inject] private PieceSpawner pieceSpawner;

		private GameManager gameManager;

		private void Start()
		{
			gameManager = new GameManager(boardRenderer, levelLoader, pieceSpawner);
			gameManager.StartGame();
		}
	}
}

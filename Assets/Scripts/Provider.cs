using Game.Match3.Model;
using Game.Match3.ViewComponents;
using Scripts.Core;
using UnityEngine;

namespace Game.Match3
{
    public class Provider : MonoBehaviour , IDependencyProvider
    {
        [SerializeField] private BoardRenderer boardRenderer;

        private LevelLoader levelLoader;
        private PieceSpawner pieceSpawner;

        [Provide]
        public LevelLoader ProvideLevelLoader()
        {
            if(levelLoader == null)
            {
                levelLoader = new LevelLoader();
            }
            return levelLoader;
        }

        [Provide]
        public BoardRenderer ProvideBoardRenderer()
        {
            return boardRenderer;
        }

        [Provide]
        public PieceSpawner ProvidePieceSpawner()
        {
            if(pieceSpawner == null)
            {
                pieceSpawner = new PieceSpawner();
            }
            return pieceSpawner;
        }
    }
}
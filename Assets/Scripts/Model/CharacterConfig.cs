using UnityEngine;
using UnityEngine.UI;

namespace Game.Match3.Model
{
    [System.Serializable]
    public class CharacterConfig
    {
        public string Id;
        public string Name;
        public Sprite Icon;
        public GameObject Prefab;
        public CharacterType CharacterType;
        public CharacterStats CharacterStats;
    }

    [System.Serializable]
    public class CharacterStats
    {
        public int MaxHealth;
        public float Damage;
    }

    public enum CharacterType
    {
        Strength,
        Poison,
        Intelligence,
        Healer
    }
}
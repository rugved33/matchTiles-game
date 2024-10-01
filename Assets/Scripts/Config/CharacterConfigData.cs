using Game.Match3.Model;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfigData", menuName = "CharacterConfigData", order = 0)]
public class CharacterConfigData : ScriptableObject 
{
    public CharacterConfig[] CharacterConfigs;
}
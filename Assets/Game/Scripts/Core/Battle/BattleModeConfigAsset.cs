using UnityEngine;

namespace Game.Scripts.Core.Battle
{
    [CreateAssetMenu(menuName = "CubeClash/Battle Mode Config", fileName = "BattleModeConfig")]
    public sealed class BattleModeConfigAsset : ScriptableObject
    {
        [SerializeField] private int _modeId;
        [SerializeField] private int _maxHp = 3;
        [SerializeField] private int _diceCount = 2;
        [SerializeField] private int _diceMin = 1;
        [SerializeField] private int _diceMax = 6;

        public BattleModeConfig CreateConfig() => new(_modeId, _maxHp, _diceCount, _diceMin, _diceMax);
    }
}

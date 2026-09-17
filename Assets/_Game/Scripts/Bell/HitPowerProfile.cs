using UnityEngine;

namespace Crane.Bell
{
    /// <summary>
    /// ヒットの威力の計算設定。
    /// </summary>
    [CreateAssetMenu(fileName = "HitPowerProfile", menuName = "Crane/Hit Power Profile")]
    public class HitPowerProfile : ScriptableObject
    {
        /// <summary>使う計算式。</summary>
        [Tooltip("使う計算式。")]
        public HitPowerFormula Formula = HitPowerFormula.Impulse;

        /// <summary>計算結果に掛ける倍率。</summary>
        [Tooltip("計算結果に掛ける倍率。")]
        public float Multiplier = 1f;
    }
}

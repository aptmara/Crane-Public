using Crane.Data;
using UnityEngine;

namespace Crane.Score
{
    /// <summary>
    /// ヒットの威力(実数)を、煩悩・ご利益の増減量(整数)へ変換する。
    /// </summary>
    public static class HitAmountCalculator
    {
        /// <summary>
        /// 威力に倍率を掛け、指定した丸め方で整数にする。
        /// </summary>
        /// <param name="hitPower">ヒットの威力。</param>
        /// <param name="multiplier">威力に掛ける倍率。</param>
        /// <param name="roundingMode">小数点以下の丸め方。</param>
        /// <returns>0 以上の増減量。</returns>
        public static int Calculate(float hitPower, float multiplier, HitBonnoRoundingMode roundingMode)
        {
            float value = Mathf.Max(0f, hitPower * multiplier);

            switch (roundingMode)
            {
                case HitBonnoRoundingMode.Floor:
                    return Mathf.FloorToInt(value);

                case HitBonnoRoundingMode.Ceil:
                    return Mathf.CeilToInt(value);

                case HitBonnoRoundingMode.Round:
                default:
                    return Mathf.RoundToInt(value);
            }
        }
    }
}

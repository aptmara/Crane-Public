using Crane.Data;
using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// ヒットの威力を、演出の大きさを決める「強さ」(0〜1)へ換算する。
    /// </summary>
    /// <remarks>
    /// エフェクト画像・数字・画面揺れ・跳ねる「ご利益」は、すべてこの強さから大きさを決める。
    /// 強さは威力に比例し、<see cref="GameSettings.HitEffectPowerForMaxScale"/> で 1 になる。
    /// <code>
    /// 強さ     = clamp01(威力 / 最大になる威力)
    /// 拡大率   = 最小拡大率 + (最大拡大率 - 最小拡大率) × 強さ
    /// </code>
    /// </remarks>
    public static class HitStrength
    {
        /// <summary>
        /// 威力から強さを求める。
        /// </summary>
        /// <param name="hitPower">ヒットの威力。</param>
        /// <param name="settings">演出の設定。</param>
        /// <returns>0〜1 の強さ。</returns>
        public static float FromPower(float hitPower, GameSettings settings)
        {
            float powerForMax = Mathf.Max(settings.HitEffectPowerForMaxScale, Mathf.Epsilon);
            return Mathf.Clamp01(hitPower / powerForMax);
        }

        /// <summary>
        /// 煩悩・ご利益の増減量から強さを求める。
        /// </summary>
        /// <param name="amount">ヒットで増減した量。</param>
        /// <param name="settings">演出の設定と、威力から増減量への換算倍率。</param>
        /// <returns>0〜1 の強さ。</returns>
        /// <remarks>
        /// 増減量は「威力 × 換算倍率」なので、換算倍率で割って威力に戻してから求める。
        /// </remarks>
        public static float FromAmount(int amount, GameSettings settings)
        {
            float multiplier = Mathf.Max(settings.HitBonnoScoreMultiplier, Mathf.Epsilon);
            return FromPower(amount / multiplier, settings);
        }

        /// <summary>
        /// 強さからエフェクトと数字の拡大率を求める。
        /// </summary>
        /// <param name="strength">0〜1 の強さ。</param>
        /// <param name="settings">演出の設定。</param>
        /// <returns>拡大率。</returns>
        public static float ToEffectScale(float strength, GameSettings settings)
        {
            return Mathf.Lerp(settings.HitEffectMinScale, settings.HitEffectMaxScale, strength);
        }
    }
}

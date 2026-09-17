using UnityEngine;

namespace Crane.Common
{
    /// <summary>
    /// 秒数を物理の固定ステップ数へ換算する。
    /// </summary>
    /// <remarks>
    /// 原作は「同じ配置なら同じ動きが再現される」ことを重視している。
    /// 物理は固定ステップで進むため、制限時間・鐘の押し返し・異常加速の判定など、
    /// 物理の結果に影響する時間はフレーム時間(Time.deltaTime)ではなく固定ステップ数で数える。
    /// フレームの長さが毎回違っても、同じステップで同じ処理が起きるようにするため。
    /// </remarks>
    public static class FixedStepTime
    {
        /// <summary>
        /// 秒数を、現在の固定ステップ幅での最も近いステップ数にする。
        /// </summary>
        /// <param name="seconds">秒数。</param>
        /// <returns>ステップ数(0 以上)。</returns>
        public static int SecondsToSteps(float seconds)
        {
            return SecondsToSteps(seconds, Time.fixedDeltaTime);
        }

        /// <summary>
        /// 秒数を、指定した固定ステップ幅での最も近いステップ数にする。
        /// </summary>
        /// <param name="seconds">秒数。</param>
        /// <param name="stepSeconds">1 ステップの秒数。</param>
        /// <returns>ステップ数(0 以上)。</returns>
        /// <remarks>
        /// 30 ÷ 0.02 が浮動小数点の誤差で 1500.0001 になっても 1501 にならないよう、切り上げではなく四捨五入する。
        /// </remarks>
        public static int SecondsToSteps(float seconds, float stepSeconds)
        {
            if (seconds <= 0f)
            {
                return 0;
            }

            return Mathf.RoundToInt(seconds / stepSeconds);
        }

        /// <summary>
        /// ステップ数を、現在の固定ステップ幅での秒数にする。
        /// </summary>
        /// <param name="steps">ステップ数。</param>
        /// <returns>秒数。</returns>
        public static float StepsToSeconds(int steps)
        {
            return steps * Time.fixedDeltaTime;
        }
    }
}

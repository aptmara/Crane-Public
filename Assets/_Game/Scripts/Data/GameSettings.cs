using UnityEngine;

namespace Crane.Data
{
    /// <summary>
    /// ゲームルールとヒット演出の調整値。
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Crane/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        /// <summary>固定するフレームレート。</summary>
        [Header("Application")]
        [Tooltip("固定するフレームレート。")]
        public int TargetFrameRate = 60;

        /// <summary>Playing フェーズの制限時間(秒)。</summary>
        [Header("Rule")]
        [Tooltip("Playingフェーズの制限時間(秒)。原作値は30秒。")]
        public float PlayDurationSeconds = 30f;

        /// <summary>ゲーム開始時の煩悩数。</summary>
        [Tooltip("ゲーム開始時の煩悩数。原作値は108。")]
        public int StartingBonno = 108;

        /// <summary>煩悩を祓い切った時に保証する、スーパーご利益タイムの最低残り秒数。</summary>
        [Tooltip("煩悩を祓い切った時点の残り時間がこれより短ければ、この秒数まで延長する。")]
        public float BenefitTimeMinimumSeconds = 10f;

        /// <summary>最も弱いヒット(威力 0)でのエフェクトと数字の拡大率。</summary>
        [Header("Hit Effect")]
        [Tooltip("最も弱いヒット(威力0)でのエフェクトと数字の拡大率。")]
        public float HitEffectMinScale = 0.25f;

        /// <summary>最も強いヒットでのエフェクトと数字の拡大率。</summary>
        [Tooltip("最も強いヒットでのエフェクトと数字の拡大率。")]
        public float HitEffectMaxScale = 0.5f;

        /// <summary>この威力以上のヒットで演出が最大になる。その間は威力に比例して大きくなる。</summary>
        [Tooltip("この威力以上のヒットで演出が最大になる。その間は威力に比例して大きくなる。実測したヒットの威力はおおむね10〜360。")]
        public float HitEffectPowerForMaxScale = 300f;

        /// <summary>ヒットの威力から煩悩・ご利益の量へ換算する倍率。</summary>
        [Header("Hit Bonno")]
        [Tooltip("ヒットの威力から煩悩・ご利益の量へ換算する倍率。")]
        public float HitBonnoScoreMultiplier = 1f;

        /// <summary>換算時の丸め方。</summary>
        [Tooltip("換算時の丸め方。")]
        public HitBonnoRoundingMode HitBonnoRoundingMode = HitBonnoRoundingMode.Round;

        /// <summary>ヒット数値の「煩悩」「ご利益」ラベル部分のフォントサイズ。</summary>
        [Header("Hit Bonno Text")]
        [Tooltip("ラベル部分のフォントサイズ。")]
        public int HitBonnoFontSize = 32;

        /// <summary>ラベルに対する数字部分のフォントサイズ比。</summary>
        [Tooltip("ラベルに対する数字部分のフォントサイズ比。")]
        public float HitBonnoNumberSizeRatio = 1.5f;

        /// <summary>TextMesh の文字サイズ。</summary>
        [Tooltip("TextMesh の文字サイズ。")]
        public float HitBonnoCharacterSize = 0.1f;

        /// <summary>表示中に上昇する距離(ワールド単位)。</summary>
        [Tooltip("表示中に上昇する距離(ワールド単位)。")]
        public float HitBonnoRiseDistance = 1f;

        /// <summary>表示時間(秒)。</summary>
        [Tooltip("表示時間(秒)。")]
        public float HitBonnoDisplayDurationSeconds = 1f;
    }
}

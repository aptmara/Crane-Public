using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// 煩悩・ご利益の表示に共通して使う文言と色。
    /// </summary>
    /// <remarks>
    /// ヒット数値・スコア表示・結果画面で同じ値を使うため、1 か所にまとめている。
    /// </remarks>
    public static class ScoreStyle
    {
        /// <summary>煩悩のラベル。</summary>
        public const string BonnoLabel = "煩悩";

        /// <summary>ご利益のラベル。</summary>
        public const string BenefitLabel = "ご利益";

        /// <summary>ご利益の文字色(明るい緑)。</summary>
        public static readonly Color BenefitColor = new Color32(0x2A, 0xD7, 0x00, 0xFF);

        /// <summary>煩悩を祓えなかった時の背景色(灰色)。</summary>
        public static readonly Color FailedBackgroundColor = new Color32(0x80, 0x80, 0x80, 0xFF);
    }
}

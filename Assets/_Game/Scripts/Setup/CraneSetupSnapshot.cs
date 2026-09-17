using Crane.Hammer;
using UnityEngine;

namespace Crane.Setup
{
    /// <summary>
    /// PLAY を押した瞬間に確定した、クレーンと振り子の初期姿勢。
    /// </summary>
    /// <remarks>
    /// Setup 画面と物理シミュレーションの境界で受け渡す値オブジェクト。
    /// 生成後は変更できない。座標は Setup Canvas のローカル座標(px)。
    /// 同じスナップショットからは同じ物理結果が再現される(原作のリプレイ性)。
    /// </remarks>
    public sealed class CraneSetupSnapshot
    {
        /// <summary>振り子全体の支点(第一リンクの根元、ブームの先端)。</summary>
        public Vector2 UpperLinkPivot { get; }

        /// <summary>第一リンクと第二リンクの関節。</summary>
        public Vector2 LowerLinkPivot { get; }

        /// <summary>第二リンクの先端(ハンマーの位置)。</summary>
        public Vector2 HammerPosition { get; }

        /// <summary>選択したハンマー。質量や威力倍率など、物理と演出の両方が参照する。</summary>
        public HammerDefinition Hammer { get; }

        /// <summary>
        /// 初期姿勢を作る。
        /// </summary>
        /// <param name="upperLinkPivot">振り子全体の支点。</param>
        /// <param name="lowerLinkPivot">第一リンクと第二リンクの関節。</param>
        /// <param name="hammerPosition">ハンマーの位置。</param>
        /// <param name="hammer">選択したハンマー。</param>
        public CraneSetupSnapshot(Vector2 upperLinkPivot, Vector2 lowerLinkPivot, Vector2 hammerPosition, HammerDefinition hammer)
        {
            UpperLinkPivot = upperLinkPivot;
            LowerLinkPivot = lowerLinkPivot;
            HammerPosition = hammerPosition;
            Hammer = hammer;
        }

        /// <summary>
        /// ログ出力用の文字列を返す。
        /// </summary>
        /// <returns>各座標とハンマーの ID を並べた文字列。</returns>
        public override string ToString()
        {
            return "upperLinkPivot=" + UpperLinkPivot
                + " lowerLinkPivot=" + LowerLinkPivot
                + " hammerPosition=" + HammerPosition
                + " hammer=" + Hammer.Id;
        }
    }
}

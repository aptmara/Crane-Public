using Crane.Hammer;
using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// 二重振り子を組み立てるための、ワールド座標系の初期条件。
    /// </summary>
    /// <remarks>
    /// <see cref="SetupToPhysicsConverter"/> が Setup の姿勢(Canvas 座標)から作る。生成後は変更できない。
    /// </remarks>
    public sealed class CranePhysicsSetup
    {
        /// <summary>振り子全体の支点のワールド座標。</summary>
        public Vector3 RootPosition { get; }

        /// <summary>第一リンクの長さ(ワールド単位)。</summary>
        public float UpperLinkLength { get; }

        /// <summary>第二リンクの長さ(ワールド単位)。</summary>
        public float LowerLinkLength { get; }

        /// <summary>第一リンクの初期角度(度、ワールド基準)。</summary>
        public float UpperLinkAngleDeg { get; }

        /// <summary>第二リンクの第一リンクに対する初期相対角(度)。</summary>
        public float LowerLinkRelativeAngleDeg { get; }

        /// <summary>先端に付けるハンマー。</summary>
        public HammerDefinition Hammer { get; }

        /// <summary>Setup Canvas の 1px がワールドで占める大きさ。見た目のサイズ変換に使う。</summary>
        public Vector2 CanvasUnitWorldSize { get; }

        /// <summary>
        /// 初期条件を作る。
        /// </summary>
        /// <param name="rootPosition">振り子全体の支点のワールド座標。</param>
        /// <param name="upperLinkLength">第一リンクの長さ。</param>
        /// <param name="lowerLinkLength">第二リンクの長さ。</param>
        /// <param name="upperLinkAngleDeg">第一リンクの初期角度(度)。</param>
        /// <param name="lowerLinkRelativeAngleDeg">第二リンクの初期相対角(度)。</param>
        /// <param name="hammer">先端に付けるハンマー。</param>
        /// <param name="canvasUnitWorldSize">Setup Canvas の 1px のワールドでの大きさ。</param>
        public CranePhysicsSetup(
            Vector3 rootPosition,
            float upperLinkLength,
            float lowerLinkLength,
            float upperLinkAngleDeg,
            float lowerLinkRelativeAngleDeg,
            HammerDefinition hammer,
            Vector2 canvasUnitWorldSize)
        {
            RootPosition = rootPosition;
            UpperLinkLength = upperLinkLength;
            LowerLinkLength = lowerLinkLength;
            UpperLinkAngleDeg = upperLinkAngleDeg;
            LowerLinkRelativeAngleDeg = lowerLinkRelativeAngleDeg;
            Hammer = hammer;
            CanvasUnitWorldSize = canvasUnitWorldSize;
        }

        /// <summary>
        /// ログ出力用の文字列を返す。
        /// </summary>
        /// <returns>主要な値を並べた文字列。</returns>
        public override string ToString()
        {
            return "root=" + RootPosition
                + " upperLength=" + UpperLinkLength.ToString("F3")
                + " lowerLength=" + LowerLinkLength.ToString("F3")
                + " upperAngle=" + UpperLinkAngleDeg.ToString("F2")
                + " lowerRelativeAngle=" + LowerLinkRelativeAngleDeg.ToString("F2")
                + " hammer=" + Hammer.Id;
        }
    }
}

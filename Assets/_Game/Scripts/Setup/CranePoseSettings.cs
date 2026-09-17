using Crane.Data;
using UnityEngine;

namespace Crane.Setup
{
    /// <summary>
    /// <see cref="CranePoseModel"/> が使う初期配置と操作制約。
    /// </summary>
    /// <remarks>
    /// ScriptableObject(<see cref="CranePoseEditorData"/>)から必要な値だけを取り出した値型。
    /// テストでは直接組み立てて使う。
    /// </remarks>
    public struct CranePoseSettings
    {
        /// <summary>ブームの回転軸。</summary>
        public Vector2 BoomPivot;

        /// <summary>第一リンクの根元の初期位置。</summary>
        public Vector2 UpperLinkPivot;

        /// <summary>第二リンクの根元の初期位置。</summary>
        public Vector2 LowerLinkPivot;

        /// <summary>ハンマーの初期位置。</summary>
        public Vector2 HammerPosition;

        /// <summary>ブーム角度の下限(度)。</summary>
        public float BoomMinAngleDeg;

        /// <summary>ブーム角度の上限(度)。</summary>
        public float BoomMaxAngleDeg;

        /// <summary>ブームの最大回転速度(度/秒)。</summary>
        public float BoomMaxAngularSpeedDeg;

        /// <summary>ブームの当たり判定の太さ。</summary>
        public float BoomHitThickness;

        /// <summary>第一リンク先端の操作判定サイズ。</summary>
        public Vector2 UpperLinkHandleSize;

        /// <summary>第二リンク先端の操作判定サイズ。</summary>
        public Vector2 LowerLinkHandleSize;

        /// <summary>リンクの最小長。</summary>
        public float MinimumLinkLength;

        /// <summary>第二リンクの相対角の上限を適用するか。</summary>
        public bool UseLowerLinkRelativeAngleLimit;

        /// <summary>第二リンクの相対角の上限(度)。</summary>
        public float LowerLinkMaxRelativeAngleDeg;

        /// <summary>
        /// エディタ用データから設定を作る。
        /// </summary>
        /// <param name="data">Setup 画面のデータ。</param>
        /// <returns>モデル用の設定。</returns>
        public static CranePoseSettings FromData(CranePoseEditorData data)
        {
            CranePoseSettings settings = new CranePoseSettings();
            settings.BoomPivot = data.BoomPivot;
            settings.UpperLinkPivot = data.UpperLinkPivot;
            settings.LowerLinkPivot = data.LowerLinkPivot;
            settings.HammerPosition = data.LowerLinkEndPoint;
            settings.BoomMinAngleDeg = data.BoomMinAngle;
            settings.BoomMaxAngleDeg = data.BoomMaxAngle;
            settings.BoomMaxAngularSpeedDeg = data.BoomMaxAngularSpeed;
            settings.BoomHitThickness = data.BoomThickness + data.BoomHitPadding;
            settings.UpperLinkHandleSize = data.UpperLinkHandleSize;
            settings.LowerLinkHandleSize = data.LowerLinkHandleSize;
            settings.MinimumLinkLength = data.MinimumLinkLength;
            settings.UseLowerLinkRelativeAngleLimit = data.UseLowerLinkRelativeAngleLimit;
            settings.LowerLinkMaxRelativeAngleDeg = data.LowerLinkMaxRelativeAngle;
            return settings;
        }
    }
}

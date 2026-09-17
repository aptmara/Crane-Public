using UnityEngine;

namespace Crane.Data
{
    /// <summary>
    /// Setup 画面(クレーンの初期姿勢エディタ)の初期配置・操作範囲・見た目。
    /// </summary>
    /// <remarks>
    /// 座標はすべて Setup Canvas のローカル座標(px、中心原点)で持つ。
    /// 初期配置を変える時は、このアセットを Inspector で直接編集する
    /// (<see cref="Crane.Setup.CranePoseEditorController"/> が Edit Mode 中もプレビューに反映する)。
    /// </remarks>
    [CreateAssetMenu(fileName = "CranePoseEditorData", menuName = "Crane/Crane Pose Editor Data")]
    public class CranePoseEditorData : ScriptableObject
    {
        /// <summary>座標の基準にする Canvas の解像度。</summary>
        [Header("Canvas")]
        [Tooltip("座標の基準にする Canvas の解像度。")]
        public Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        /// <summary>ブーム(a-mu)の回転軸。</summary>
        [Header("Initial points (Canvas local)")]
        [Tooltip("ブーム(a-mu)の回転軸。")]
        public Vector2 BoomPivot = new Vector2(283.39f, -384.95f);

        /// <summary>第一リンク(bou1)の根元。ブームの先端でもあり、振り子全体の支点になる。</summary>
        [Tooltip("第一リンク(bou1)の根元。ブームの先端でもあり、振り子全体の支点になる。")]
        public Vector2 UpperLinkPivot = new Vector2(-2.90f, 54.38f);

        /// <summary>第二リンク(bou2)の根元。第一リンクの先端。</summary>
        [Tooltip("第二リンク(bou2)の根元。第一リンクの先端。")]
        public Vector2 LowerLinkPivot = new Vector2(215.35f, 67.71f);

        /// <summary>第二リンクの先端。ハンマーの位置。</summary>
        [Tooltip("第二リンクの先端。ハンマーの位置。")]
        public Vector2 LowerLinkEndPoint = new Vector2(-175f, 95f);

        /// <summary>鐘の中心位置。</summary>
        [Header("Bell (Canvas local, 見た目のみ・ドラッグ対象外)")]
        [Tooltip("鐘の中心位置。")]
        public Vector2 BellPosition = new Vector2(-300f, 100f);

        /// <summary>鐘の大きさ。</summary>
        [Tooltip("鐘の大きさ。")]
        public Vector2 BellSize = new Vector2(220f, 260f);

        /// <summary>鐘の色。</summary>
        [Tooltip("鐘の色。")]
        public Color BellColor = new Color(0.85f, 0.65f, 0.2f, 1f);

        /// <summary>ブーム角度の下限(度)。</summary>
        [Header("Boom range (degrees)")]
        [Tooltip("ブーム角度の下限(度)。")]
        public float BoomMinAngle = 110.5717f;

        /// <summary>ブーム角度の上限(度)。</summary>
        [Tooltip("ブーム角度の上限(度)。")]
        public float BoomMaxAngle = 145.1259f;

        /// <summary>ブームをドラッグした時の最大回転速度(度/秒)。</summary>
        [Tooltip("ブームをドラッグした時の最大回転速度(度/秒)。")]
        public float BoomMaxAngularSpeed = 45f;

        /// <summary>リンクの最小長(px)。長さ 0 付近では物理が破綻するため、これより短くはできない。</summary>
        [Header("Link constraints")]
        [Tooltip("リンクの最小長(px)。長さ0付近では物理が破綻するため、これより短くはできない。")]
        public float MinimumLinkLength = 10f;

        /// <summary>第二リンクの第一リンクに対する相対角の上限を適用するか。</summary>
        [Tooltip("第二リンクの第一リンクに対する相対角の上限を適用するか。原作の実測値はあるが、適用するかは要確認のため既定は無効。")]
        public bool UseLowerLinkRelativeAngleLimit = false;

        /// <summary>第二リンクの相対角の上限(度)。原作の実測値は 21.961 度。</summary>
        [Tooltip("第二リンクの相対角(θ2-θ1)の上限(度)。原作の実測値は21.961度。下限は未測定。")]
        public float LowerLinkMaxRelativeAngle = 21.961f;

        /// <summary>ブームの太さ(px)。</summary>
        [Header("Dimensions (px)")]
        [Tooltip("ブームの太さ(px)。")]
        public float BoomThickness = 72f;

        /// <summary>リンクの太さ(px)。</summary>
        [Tooltip("リンクの太さ(px)。")]
        public float LinkThickness = 28f;

        /// <summary>ハンマーの大きさ(px)。</summary>
        [Tooltip("ハンマーの大きさ(px)。")]
        public Vector2 HammerSize = new Vector2(150f, 82f);

        /// <summary>第二リンクの向きに対するハンマー画像の回転(度)。</summary>
        [Tooltip("第二リンクの向きに対するハンマー画像の回転(度)。")]
        public float HammerRotationOffset = 90f;

        /// <summary>ブームの当たり判定を太さより広げる量(px)。</summary>
        [Tooltip("ブームの当たり判定を太さより広げる量(px)。")]
        public float BoomHitPadding = 18f;

        /// <summary>第一リンク先端の操作判定サイズ(px)。原作の実測値は 227.25 四方。</summary>
        [Tooltip("第一リンク先端の操作判定サイズ(px)。原作の実測値は227.25四方。")]
        public Vector2 UpperLinkHandleSize = new Vector2(227.25f, 227.25f);

        /// <summary>第二リンク先端の操作判定サイズ(px)。原作の実測値は 227.25 四方。</summary>
        [Tooltip("第二リンク先端の操作判定サイズ(px)。原作の実測値は227.25四方。")]
        public Vector2 LowerLinkHandleSize = new Vector2(227.25f, 227.25f);

        /// <summary>関節の表示サイズ(px)。</summary>
        [Tooltip("関節の表示サイズ(px)。")]
        public float PivotSize = 22f;

        /// <summary>エディタ背景の色。</summary>
        [Header("Colors")]
        [Tooltip("エディタ背景の色。")]
        public Color BackgroundColor = new Color(0.2078f, 0.2745f, 0.4f, 1f);

        /// <summary>ブームの色。</summary>
        [Tooltip("ブームの色。")]
        public Color BoomColor = new Color(1f, 0.7412f, 0f, 1f);

        /// <summary>リンクの色。</summary>
        [Tooltip("リンクの色。")]
        public Color LinkColor = new Color(0.2667f, 0.2863f, 0.2941f, 1f);

        /// <summary>ハンマーの色。</summary>
        [Tooltip("ハンマーの色。")]
        public Color HammerColor = new Color(0.749f, 0.769f, 0.780f, 1f);

        /// <summary>操作判定の可視化色。</summary>
        [Tooltip("操作判定の可視化色。")]
        public Color HandleColor = new Color(0.149f, 0.561f, 0.835f, 0.24f);

        /// <summary>関節の色。</summary>
        [Tooltip("関節の色。")]
        public Color PivotColor = new Color(0.96f, 0.96f, 0.96f, 1f);
    }
}

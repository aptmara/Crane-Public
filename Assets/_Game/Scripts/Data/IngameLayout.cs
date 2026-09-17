using UnityEngine;

namespace Crane.Data
{
    /// <summary>
    /// Setup 画面と Ingame 画面の位置関係。
    /// </summary>
    /// <remarks>
    /// 原作では PLAY を押すとカメラが寄り、Setup 画面の絵が拡大されて左上方向へずれた状態で表示される。
    /// 原作の Setup 画面と Ingame 画面を同じ縮尺で重ね、画面と Setup 画像の大きさ・位置を実測した値から、
    /// 「Setup 画面(Canvas)の正規化座標 → Ingame 画面の Viewport 座標」の対応を求めている。
    /// 実測値の単位は測定時の任意の長さで、比だけに意味がある。
    ///
    /// この対応は次の 2 か所で使い、Setup で見た位置と物理の初期位置を一致させる。
    /// <list type="bullet">
    /// <item><description>Canvas 上の Setup 画像を Ingame 用に拡大・移動する(<see cref="Crane.Presentation.IngameCanvasLayout"/>)</description></item>
    /// <item><description>Setup の座標を物理ワールド座標へ変換する(<see cref="Crane.PhysicsSim.SetupToPhysicsConverter"/>)</description></item>
    /// </list>
    /// </remarks>
    public static class IngameLayout
    {
        /// <summary>Ingame 画面の幅(実測値)。</summary>
        private const float MeasuredScreenWidth = 23.57f;

        /// <summary>Ingame 画面の高さ(実測値)。</summary>
        private const float MeasuredScreenHeight = 13.22f;

        /// <summary>Ingame 画面に映る Setup 画像全体の幅(実測値)。</summary>
        private const float MeasuredSetupImageWidth = 33.87f;

        /// <summary>Ingame 画面に映る Setup 画像全体の高さ(実測値)。</summary>
        private const float MeasuredSetupImageHeight = 18.96f;

        /// <summary>画面左端から Setup 画像左端までの距離(実測値)。負の値は画面外へはみ出していることを表す。</summary>
        private const float MeasuredSetupImageLeft = -5.04f;

        /// <summary>Setup 画像の上端が画面上端からはみ出している量(実測値)。</summary>
        private const float MeasuredSetupImageTopOverflow = 5.83f;

        /// <summary>
        /// Setup 画像全体が Ingame 画面上で占める範囲(Viewport 座標、左下原点)。
        /// </summary>
        /// <remarks>
        /// 下端は「画面の高さ + 上端のはみ出し - 画像の高さ」で求める。
        /// 幅と高さは、そのまま Setup 画像の拡大率でもある。
        /// </remarks>
        public static readonly Rect SetupImageViewportRect = new Rect(
            MeasuredSetupImageLeft / MeasuredScreenWidth,
            (MeasuredScreenHeight + MeasuredSetupImageTopOverflow - MeasuredSetupImageHeight) / MeasuredScreenHeight,
            MeasuredSetupImageWidth / MeasuredScreenWidth,
            MeasuredSetupImageHeight / MeasuredScreenHeight);

        /// <summary>
        /// Setup Canvas 上の正規化座標(左下 0, 右上 1)を、Ingame 画面の Viewport 座標へ変換する。
        /// </summary>
        /// <param name="normalizedCanvasPoint">Setup Canvas 上の正規化座標。</param>
        /// <returns>Ingame 画面の Viewport 座標。</returns>
        public static Vector2 NormalizedCanvasToViewport(Vector2 normalizedCanvasPoint)
        {
            Rect rect = SetupImageViewportRect;
            return new Vector2(
                rect.x + normalizedCanvasPoint.x * rect.width,
                rect.y + normalizedCanvasPoint.y * rect.height);
        }

        /// <summary>
        /// Ingame 表示時に、Canvas 中心基準で Setup 画像をずらす量を求める。
        /// </summary>
        /// <param name="canvasSize">Canvas の現在のサイズ。</param>
        /// <returns>anchoredPosition に加算するオフセット。</returns>
        public static Vector2 GetCanvasOffset(Vector2 canvasSize)
        {
            Rect rect = SetupImageViewportRect;
            Vector2 normalizedCenterOffset = new Vector2(
                rect.x + rect.width * 0.5f - 0.5f,
                rect.y + rect.height * 0.5f - 0.5f);
            return Vector2.Scale(normalizedCenterOffset, canvasSize);
        }

        /// <summary>
        /// Ingame 表示時の Setup 画像の拡大率。
        /// </summary>
        /// <returns>localScale に掛ける拡大率(z は 1)。</returns>
        public static Vector3 GetScale()
        {
            Rect rect = SetupImageViewportRect;
            return new Vector3(rect.width, rect.height, 1f);
        }
    }
}

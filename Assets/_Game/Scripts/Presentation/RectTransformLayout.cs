using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// 2 点や中心座標から RectTransform を配置する。
    /// </summary>
    /// <remarks>
    /// すべて親の中心をアンカーにして配置する。
    /// </remarks>
    public static class RectTransformLayout
    {
        /// <summary>中心アンカー。</summary>
        private static readonly Vector2 CenterAnchor = new Vector2(0.5f, 0.5f);

        /// <summary>左端中央ピボット。</summary>
        private static readonly Vector2 LeftCenterPivot = new Vector2(0f, 0.5f);

        /// <summary>
        /// 始点から終点へ伸びる棒として配置する。
        /// </summary>
        /// <param name="target">配置する RectTransform。</param>
        /// <param name="start">始点。</param>
        /// <param name="end">終点。</param>
        /// <param name="thickness">棒の太さ。</param>
        public static void PlaceBar(RectTransform target, Vector2 start, Vector2 end, float thickness)
        {
            Vector2 direction = end - start;
            target.anchorMin = CenterAnchor;
            target.anchorMax = CenterAnchor;
            target.pivot = LeftCenterPivot;
            target.anchoredPosition = start;
            target.sizeDelta = new Vector2(direction.magnitude, thickness);
            target.localRotation = Quaternion.Euler(0f, 0f, GetAngleDeg(direction));
        }

        /// <summary>
        /// 指定位置を中心に、回転なしで配置する。
        /// </summary>
        /// <param name="target">配置する RectTransform。</param>
        /// <param name="center">中心位置。</param>
        /// <param name="size">大きさ。</param>
        public static void PlaceCentered(RectTransform target, Vector2 center, Vector2 size)
        {
            PlaceCentered(target, center, size, 0f);
        }

        /// <summary>
        /// 指定位置を中心に、指定角度で配置する。
        /// </summary>
        /// <param name="target">配置する RectTransform。</param>
        /// <param name="center">中心位置。</param>
        /// <param name="size">大きさ。</param>
        /// <param name="angleDeg">Z 軸周りの回転(度)。</param>
        public static void PlaceCentered(RectTransform target, Vector2 center, Vector2 size, float angleDeg)
        {
            target.anchorMin = CenterAnchor;
            target.anchorMax = CenterAnchor;
            target.pivot = CenterAnchor;
            target.anchoredPosition = center;
            target.sizeDelta = size;
            target.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        /// <summary>
        /// ベクトルの向きを角度にする。
        /// </summary>
        /// <param name="direction">向き。</param>
        /// <returns>X 軸正方向を 0 とする反時計回りの角度(度)。</returns>
        public static float GetAngleDeg(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }
}

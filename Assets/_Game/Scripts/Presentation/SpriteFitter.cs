using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// SpriteRenderer を指定サイズ・指定ピボットに合わせて配置する。
    /// </summary>
    /// <remarks>
    /// Setup 画面の UI Image(RectTransform のサイズとピボット)と同じ見た目を、
    /// ワールドの SpriteRenderer で再現するために使う。
    /// </remarks>
    public static class SpriteFitter
    {
        /// <summary>幅・高さに許す最小値。0 除算と見えない Sprite を防ぐ。</summary>
        private const float MinimumSize = 0.01f;

        /// <summary>中心ピボットの白い Sprite のキャッシュ。</summary>
        private static Sprite centeredWhiteSprite;

        /// <summary>左端中央ピボットの白い Sprite のキャッシュ。</summary>
        private static Sprite leftPivotWhiteSprite;

        /// <summary>
        /// 中心ピボットの白い Sprite を返す。画像が未設定の時の代替に使う。
        /// </summary>
        /// <returns>共有の Sprite。</returns>
        public static Sprite GetCenteredWhiteSprite()
        {
            if (centeredWhiteSprite == null)
            {
                centeredWhiteSprite = CreateWhiteSprite(new Vector2(0.5f, 0.5f));
            }

            return centeredWhiteSprite;
        }

        /// <summary>
        /// 左端中央ピボットの白い Sprite を返す。根元から伸びる棒の代替画像に使う。
        /// </summary>
        /// <returns>共有の Sprite。</returns>
        public static Sprite GetLeftPivotWhiteSprite()
        {
            if (leftPivotWhiteSprite == null)
            {
                leftPivotWhiteSprite = CreateWhiteSprite(new Vector2(0f, 0.5f));
            }

            return leftPivotWhiteSprite;
        }

        /// <summary>
        /// Sprite が指定サイズで表示され、指定ピボットが親の原点に来るように Transform を設定する。
        /// </summary>
        /// <param name="target">SpriteRenderer を持つ Transform。localPosition と localScale を上書きする。</param>
        /// <param name="sprite">表示する Sprite。</param>
        /// <param name="size">表示サイズ(親のローカル単位)。</param>
        /// <param name="pivot">サイズに対する正規化ピボット(左下 0, 右上 1)。</param>
        public static void Fit(Transform target, Sprite sprite, Vector2 size, Vector2 pivot)
        {
            float width = Mathf.Max(size.x, MinimumSize);
            float height = Mathf.Max(size.y, MinimumSize);

            Bounds bounds = sprite.bounds;
            Vector3 scale = new Vector3(width / bounds.size.x, height / bounds.size.y, 1f);

            Vector2 desiredMinimum = new Vector2(-pivot.x * width, -pivot.y * height);
            Vector2 scaledMinimum = new Vector2(bounds.min.x * scale.x, bounds.min.y * scale.y);

            target.localPosition = desiredMinimum - scaledMinimum;
            target.localScale = scale;
        }

        /// <summary>
        /// 白一色の Sprite を作る。
        /// </summary>
        /// <param name="pivot">Sprite のピボット。</param>
        /// <returns>1 ワールド単位四方の Sprite。</returns>
        private static Sprite CreateWhiteSprite(Vector2 pivot)
        {
            Texture2D texture = Texture2D.whiteTexture;
            Rect rect = new Rect(0f, 0f, texture.width, texture.height);
            return Sprite.Create(texture, rect, pivot, texture.width);
        }
    }
}

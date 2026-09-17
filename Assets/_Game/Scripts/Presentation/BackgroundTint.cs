using Crane.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Crane.Presentation
{
    /// <summary>
    /// 背景画像群の色をまとめて変え、元の色へ戻す。
    /// </summary>
    /// <remarks>
    /// スーパーご利益タイムの点滅(<see cref="Crane.UI.BenefitTimePresenter"/>)と、
    /// 失敗時の灰色化(<see cref="Crane.UI.ResultPresenter"/>)が共有する。
    /// 各画像のアルファは元の値を保ち、RGB だけを変える。
    /// </remarks>
    public class BackgroundTint : MonoBehaviour
    {
        /// <summary>色を変える画像群の親。子孫の Image すべてが対象になる。</summary>
        [SerializeField] private GameObject backgroundRoot;

        /// <summary>対象の画像。</summary>
        private Image[] images;

        /// <summary>各画像の元の色。<see cref="images"/> と同じ並び。</summary>
        private Color[] originalColors;

        /// <summary>
        /// 対象画像と元の色を記録する。
        /// </summary>
        private void Awake()
        {
            if (RequiredReference.IsAssigned(backgroundRoot, nameof(backgroundRoot), this))
            {
                images = backgroundRoot.GetComponentsInChildren<Image>(true);
            }
            else
            {
                images = new Image[0];
            }

            originalColors = new Color[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                originalColors[i] = images[i].color;
            }
        }

        /// <summary>
        /// すべての画像を指定色にする(アルファは元の値を保つ)。
        /// </summary>
        /// <param name="color">適用する色。</param>
        public void Apply(Color color)
        {
            for (int i = 0; i < images.Length; i++)
            {
                Color appliedColor = color;
                appliedColor.a = originalColors[i].a;
                images[i].color = appliedColor;
            }
        }

        /// <summary>
        /// すべての画像を元の色へ戻す。
        /// </summary>
        public void Restore()
        {
            for (int i = 0; i < images.Length; i++)
            {
                images[i].color = originalColors[i];
            }
        }
    }
}

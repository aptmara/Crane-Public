using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// CanvasGroup の取得処理。
    /// </summary>
    public static class CanvasGroupUtility
    {
        /// <summary>
        /// CanvasGroup を取得し、なければ追加する。
        /// </summary>
        /// <param name="target">対象の GameObject。</param>
        /// <returns>対象の CanvasGroup。</returns>
        public static CanvasGroup GetOrAdd(GameObject target)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }

            return canvasGroup;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Crane.Presentation
{
    /// <summary>
    /// 子 Canvas の描画順の上書きを、UI が表示されている間に適用する。
    /// </summary>
    /// <remarks>
    /// 非表示(非アクティブ)の Canvas は自分をルート Canvas とみなすため、
    /// その間に overrideSorting を変更しても無視される。
    /// そこで上書きの有無は「適用待ち」として保持し、表示中の LateUpdate で適用する。
    /// LateUpdate は同じフレームの描画より前に呼ばれるため、表示された最初のフレームから正しい順で描画される。
    /// <see cref="IngameCanvasLayout"/> が実行時に追加する。
    /// </remarks>
    [RequireComponent(typeof(Canvas))]
    public class CanvasSortingApplier : MonoBehaviour
    {
        /// <summary>描画順を上書きする Canvas。</summary>
        private Canvas canvas;

        /// <summary>Ingame 中だけ有効にするクリック判定。クリックを受け付けない UI では null。</summary>
        private GraphicRaycaster pointerInput;

        /// <summary>Ingame 中に設定する描画順。</summary>
        private int ingameSortingOrder;

        /// <summary>Ingame 用の描画順を適用すべきなら true、Setup 用(上書きなし)なら false。</summary>
        private bool isIngame;

        /// <summary>まだ Canvas に反映していない変更があれば true。</summary>
        private bool isApplyPending;

        /// <summary><see cref="Initialize"/> 済みなら true。</summary>
        /// <remarks>
        /// この UI を Instantiate で複製すると、初期化されていない適用役も一緒に複製される。
        /// 複製は元の Canvas の描画順をそのまま引き継いでいるので、初期化されていなければ何もしない。
        /// </remarks>
        private bool isInitialized;

        /// <summary>
        /// 対象の Canvas と Ingame 中の設定を登録する。
        /// </summary>
        /// <param name="sortingOrder">Ingame 中に設定する描画順。</param>
        /// <param name="raycaster">Ingame 中だけ有効にするクリック判定。不要なら null。</param>
        public void Initialize(int sortingOrder, GraphicRaycaster raycaster)
        {
            canvas = GetComponent<Canvas>();
            ingameSortingOrder = sortingOrder;
            pointerInput = raycaster;
            isApplyPending = true;
            isInitialized = true;
        }

        /// <summary>
        /// Ingame 用と Setup 用のどちらの描画順にするかを設定する。反映は表示中の LateUpdate で行う。
        /// </summary>
        /// <param name="value">Ingame 用にするなら true。</param>
        public void SetIngame(bool value)
        {
            isIngame = value;
            isApplyPending = true;
        }

        /// <summary>
        /// 表示されたら、非表示の間に受けた変更を反映する。
        /// </summary>
        private void OnEnable()
        {
            isApplyPending = true;
        }

        /// <summary>
        /// 適用待ちの変更があれば Canvas へ反映する。
        /// </summary>
        private void LateUpdate()
        {
            if (!isInitialized || !isApplyPending)
            {
                return;
            }

            isApplyPending = false;
            canvas.overrideSorting = isIngame;
            if (isIngame)
            {
                canvas.sortingOrder = ingameSortingOrder;
            }

            if (pointerInput != null)
            {
                pointerInput.enabled = isIngame;
            }
        }
    }
}

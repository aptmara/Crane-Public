using System;
using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// Ingame 時に描画順を上書きする UI の 1 行分の設定。
    /// </summary>
    [Serializable]
    public class CanvasSortingOverride
    {
        /// <summary>描画順を上書きする UI。</summary>
        [SerializeField] private GameObject target;

        /// <summary>描画順の種類。値は <see cref="SortingOrders.GetCanvasOrder"/> で決まる。</summary>
        [SerializeField] private CanvasSortingSlot slot;

        /// <summary>Ingame 中もクリックを受け付けるか(STOP ボタンなど)。</summary>
        [SerializeField] private bool receivesPointerInput;

        /// <summary>描画順を上書きする UI。</summary>
        public GameObject Target
        {
            get
            {
                return target;
            }
        }

        /// <summary>描画順の種類。</summary>
        public CanvasSortingSlot Slot
        {
            get
            {
                return slot;
            }
        }

        /// <summary>Ingame 中もクリックを受け付けるか。</summary>
        public bool ReceivesPointerInput
        {
            get
            {
                return receivesPointerInput;
            }
        }
    }
}

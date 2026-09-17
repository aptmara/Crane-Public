namespace Crane.Presentation
{
    /// <summary>
    /// 描画順(sortingOrder)の一覧。
    /// </summary>
    /// <remarks>
    /// Ingame では、Setup Canvas(Screen Space - Camera)の一部と、ワールドの SpriteRenderer が
    /// 同じカメラ上で前後に重なる。前後関係を 1 か所で把握できるよう、両方の値をここに集める。
    /// 値が小さいほど奥に描画される。
    /// <code>
    ///  -1000  Setup Canvas 本体(Ingame 時)
    ///    -30  背景(Back)
    ///    -20  第一リンク … -7 第一リンクの重ね画像
    ///     30  ブーム
    ///     39  ハンマーの残像
    ///     40  ハンマー
    ///     70  鐘 / おめでとう
    ///     80  前景(flont)
    ///     90  煩悩ゲージ
    ///    100  ヒットエフェクト / スコア / 残り時間
    ///    101  ヒット数値
    ///    110  スーパーご利益タイム帯 / 111 ご利益
    ///    200  結果画面 / 破損エンド
    ///    300  STOP ボタン
    /// </code>
    /// </remarks>
    public static class SortingOrders
    {
        /// <summary>Ingame 時の Setup Canvas 本体。すべての上書き Canvas より奥にする。</summary>
        public const int IngameSetupCanvas = -1000;

        /// <summary>第一リンク本体。</summary>
        public const int UpperLink = -20;

        /// <summary>第一リンクの関節。</summary>
        public const int UpperLinkPivot = -18;

        /// <summary>第二リンク本体。</summary>
        public const int LowerLink = -10;

        /// <summary>第二リンクの関節。</summary>
        public const int LowerLinkPivot = -8;

        /// <summary>第一リンクの上に重ねる画像。</summary>
        public const int UpperLinkOverlay = -7;

        /// <summary>ハンマー。</summary>
        public const int Hammer = 40;

        /// <summary>ハンマーの残像。ハンマーのすぐ奥。</summary>
        public const int HammerAfterimage = Hammer - 1;

        /// <summary>Ingame の鐘。</summary>
        public const int Bell = 70;

        /// <summary>ヒットエフェクト画像。</summary>
        public const int HitEffect = 100;

        /// <summary>ヒット時の数値テキスト。</summary>
        public const int HitText = 101;

        /// <summary>
        /// Ingame 時に Canvas の描画順を上書きする UI の値を返す。
        /// </summary>
        /// <param name="slot">上書き対象の UI の種類。</param>
        /// <returns>sortingOrder に設定する値。</returns>
        public static int GetCanvasOrder(CanvasSortingSlot slot)
        {
            switch (slot)
            {
                case CanvasSortingSlot.Back:
                    return -30;

                case CanvasSortingSlot.Boom:
                    return 30;

                case CanvasSortingSlot.Congratulations:
                    return 70;

                case CanvasSortingSlot.Front:
                    return 80;

                case CanvasSortingSlot.Graph:
                    return 90;

                case CanvasSortingSlot.Score:
                    return 100;

                case CanvasSortingSlot.Time:
                    return 100;

                case CanvasSortingSlot.SuperBenefit:
                    return 110;

                case CanvasSortingSlot.Benefit:
                    return 111;

                case CanvasSortingSlot.ClearEnd:
                    return 200;

                case CanvasSortingSlot.OverEnd:
                    return 200;

                case CanvasSortingSlot.BrokenEnd:
                    return 200;

                case CanvasSortingSlot.StopButton:
                    return 300;

                default:
                    return 0;
            }
        }
    }
}

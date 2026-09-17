namespace Crane.Presentation
{
    /// <summary>
    /// Ingame 時に描画順を上書きする UI の種類。
    /// </summary>
    /// <remarks>
    /// 具体的な値は <see cref="SortingOrders.GetCanvasOrder"/> で決める。
    /// </remarks>
    public enum CanvasSortingSlot
    {
        /// <summary>背景。</summary>
        Back,

        /// <summary>ブーム。</summary>
        Boom,

        /// <summary>前景。</summary>
        Front,

        /// <summary>煩悩ゲージ。</summary>
        Graph,

        /// <summary>スコア表示。</summary>
        Score,

        /// <summary>残り時間表示。</summary>
        Time,

        /// <summary>スーパーご利益タイムの帯。</summary>
        SuperBenefit,

        /// <summary>ヒット時に跳ねる「ご利益」。</summary>
        Benefit,

        /// <summary>クリア時の結果画面。</summary>
        ClearEnd,

        /// <summary>失敗時の結果画面。</summary>
        OverEnd,

        /// <summary>アームが折れた時の特殊エンド。</summary>
        BrokenEnd,

        /// <summary>「おめでとう」表示。</summary>
        Congratulations,

        /// <summary>STOP ボタン。</summary>
        StopButton,
    }
}

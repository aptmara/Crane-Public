namespace Crane.Game
{
    /// <summary>
    /// ゲーム全体の進行状態。
    /// </summary>
    /// <remarks>
    /// 遷移は <see cref="GameController"/> だけが行う。
    /// <code>
    /// Setup ──PLAY──▶ Playing ──煩悩0──▶ BenefitTime ──時間切れ──▶ Result
    ///                   │ └──────────時間切れ──────────────────────▶ Result
    ///                   └──異常加速──▶ BrokenEnding  (BenefitTime からも遷移する)
    /// Setup 以外 ──STOP──▶ Setup
    /// </code>
    /// </remarks>
    public enum GameState
    {
        /// <summary>クレーンと振り子の初期姿勢を決める。</summary>
        Setup,

        /// <summary>物理シミュレーション中。鐘を叩くと煩悩が減る。</summary>
        Playing,

        /// <summary>煩悩を祓い切った後の追加得点フェーズ。鐘を叩くとご利益が増える。</summary>
        BenefitTime,

        /// <summary>時間切れ後の結果表示。</summary>
        Result,

        /// <summary>異常加速でアームが折れた特殊失敗ルート。スコアは表示しない。</summary>
        BrokenEnding,
    }
}

namespace Crane.Bell
{
    /// <summary>
    /// ヒットの威力の計算式。
    /// </summary>
    /// <remarks>
    /// 原作の計算式は非公開のため、候補を切り替えて観測結果に近いものを選べるようにしている。
    /// </remarks>
    public enum HitPowerFormula
    {
        /// <summary>撃力の大きさ。</summary>
        Impulse,

        /// <summary>相対速度の大きさ。</summary>
        RelativeVelocity,

        /// <summary>運動量(質量 × 相対速度)。</summary>
        Momentum,

        /// <summary>運動エネルギー(1/2 × 質量 × 相対速度²)。</summary>
        KineticEnergy,

        /// <summary>接触点でのハンマーの速さ。</summary>
        PointVelocity,
    }
}

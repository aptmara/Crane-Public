namespace Crane.Data
{
    /// <summary>
    /// ヒットの威力を煩悩・ご利益の整数量へ換算する時の丸め方。
    /// </summary>
    public enum HitBonnoRoundingMode
    {
        /// <summary>切り捨て。</summary>
        Floor,

        /// <summary>四捨五入(偶数丸め)。</summary>
        Round,

        /// <summary>切り上げ。</summary>
        Ceil,
    }
}

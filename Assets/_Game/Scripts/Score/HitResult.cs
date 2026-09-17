namespace Crane.Score
{
    /// <summary>
    /// 1 回のヒットをスコアへ反映した結果。
    /// </summary>
    /// <remarks>
    /// スコアのルールは <see cref="ScoreBoard"/> だけが知っており、
    /// 演出側はこの結果を受け取って表示するだけにする(ルールの二重実装を防ぐ)。
    /// </remarks>
    public readonly struct HitResult
    {
        /// <summary>煩悩とご利益のどちらに反映されたか。</summary>
        public readonly HitResultKind Kind;

        /// <summary>減った煩悩、または増えたご利益の量。</summary>
        public readonly int Amount;

        /// <summary>ヒットの威力。演出の大きさに使う。</summary>
        public readonly float Power;

        /// <summary>
        /// ヒット結果を作る。
        /// </summary>
        /// <param name="kind">煩悩とご利益のどちらに反映されたか。</param>
        /// <param name="amount">減った煩悩、または増えたご利益の量。</param>
        /// <param name="power">ヒットの威力。</param>
        public HitResult(HitResultKind kind, int amount, float power)
        {
            Kind = kind;
            Amount = amount;
            Power = power;
        }
    }
}

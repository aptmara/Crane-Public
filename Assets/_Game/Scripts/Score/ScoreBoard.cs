using UnityEngine;

namespace Crane.Score
{
    /// <summary>
    /// 煩悩とご利益の集計ルール本体。
    /// </summary>
    /// <remarks>
    /// Unity に依存しない純粋な C# クラスとして切り出し、EditMode テストで検証できるようにしている。
    /// ルールは次の通り。
    /// <list type="bullet">
    /// <item><description>煩悩が残っている間のヒットは煩悩を減らす(0 未満にはならない)。</description></item>
    /// <item><description>煩悩を祓い切った後のヒットはご利益を増やす。</description></item>
    /// <item><description>煩悩を 0 にしたヒットの余剰分はご利益へ繰り越さない。</description></item>
    /// </list>
    /// </remarks>
    public sealed class ScoreBoard
    {
        /// <summary>残り煩悩。</summary>
        public int RemainingBonno { get; private set; }

        /// <summary>獲得したご利益。</summary>
        public int Benefit { get; private set; }

        /// <summary>今回のゲームでのヒット回数。</summary>
        public int TotalHitCount { get; private set; }

        /// <summary>今回のゲームで最も強かったヒットの威力。</summary>
        public float BestHitPower { get; private set; }

        /// <summary>
        /// 煩悩を祓い切っていれば true。
        /// </summary>
        public bool IsBonnoCleared
        {
            get
            {
                return RemainingBonno <= 0;
            }
        }

        /// <summary>
        /// 指定した煩悩数で集計を始める。
        /// </summary>
        /// <param name="startingBonno">開始時の煩悩数。</param>
        public ScoreBoard(int startingBonno)
        {
            Reset(startingBonno);
        }

        /// <summary>
        /// 集計を開始時の状態へ戻す。
        /// </summary>
        /// <param name="startingBonno">開始時の煩悩数。</param>
        public void Reset(int startingBonno)
        {
            RemainingBonno = startingBonno;
            Benefit = 0;
            TotalHitCount = 0;
            BestHitPower = 0f;
        }

        /// <summary>
        /// 1 回のヒットを集計に反映する。
        /// </summary>
        /// <param name="hitPower">ヒットの威力。</param>
        /// <param name="amount">威力から換算した増減量。</param>
        /// <returns>何にどれだけ反映されたか。</returns>
        public HitResult ApplyHit(float hitPower, int amount)
        {
            TotalHitCount++;
            BestHitPower = Mathf.Max(BestHitPower, hitPower);

            if (IsBonnoCleared)
            {
                Benefit += amount;
                return new HitResult(HitResultKind.Benefit, amount, hitPower);
            }

            RemainingBonno = Mathf.Max(0, RemainingBonno - amount);
            return new HitResult(HitResultKind.Bonno, amount, hitPower);
        }
    }
}

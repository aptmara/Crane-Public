using Crane.Score;
using NUnit.Framework;

namespace Crane.Tests.EditMode
{
    /// <summary>
    /// <see cref="ScoreBoard"/> の集計ルールのテスト。
    /// </summary>
    public class ScoreBoardTests
    {
        /// <summary>開始時の煩悩数(原作値)。</summary>
        private const int StartingBonno = 108;

        /// <summary>
        /// 開始直後は煩悩が満タンで、ご利益とヒット数は 0。
        /// </summary>
        [Test]
        public void Constructor_StartsWithFullBonnoAndNoBenefit()
        {
            ScoreBoard board = new ScoreBoard(StartingBonno);

            Assert.AreEqual(StartingBonno, board.RemainingBonno);
            Assert.AreEqual(0, board.Benefit);
            Assert.AreEqual(0, board.TotalHitCount);
            Assert.IsFalse(board.IsBonnoCleared);
        }

        /// <summary>
        /// 煩悩が残っている間のヒットは煩悩を減らす。
        /// </summary>
        [Test]
        public void ApplyHit_WhileBonnoRemains_ReducesBonno()
        {
            ScoreBoard board = new ScoreBoard(StartingBonno);

            HitResult result = board.ApplyHit(12.3f, 12);

            Assert.AreEqual(HitResultKind.Bonno, result.Kind);
            Assert.AreEqual(12, result.Amount);
            Assert.AreEqual(StartingBonno - 12, board.RemainingBonno);
            Assert.AreEqual(0, board.Benefit);
        }

        /// <summary>
        /// 残りより大きいヒットでも煩悩は 0 で止まり、余剰分はご利益へ繰り越さない。
        /// </summary>
        [Test]
        public void ApplyHit_LargerThanRemaining_ClampsToZeroWithoutCarryOver()
        {
            ScoreBoard board = new ScoreBoard(10);

            board.ApplyHit(50f, 50);

            Assert.AreEqual(0, board.RemainingBonno);
            Assert.IsTrue(board.IsBonnoCleared);
            Assert.AreEqual(0, board.Benefit);
        }

        /// <summary>
        /// 煩悩を祓い切った後のヒットはご利益を増やす。
        /// </summary>
        [Test]
        public void ApplyHit_AfterCleared_AddsBenefit()
        {
            ScoreBoard board = new ScoreBoard(10);
            board.ApplyHit(10f, 10);

            HitResult result = board.ApplyHit(7f, 7);

            Assert.AreEqual(HitResultKind.Benefit, result.Kind);
            Assert.AreEqual(7, board.Benefit);
            Assert.AreEqual(0, board.RemainingBonno);
        }

        /// <summary>
        /// ヒット数と最大威力は、煩悩とご利益のどちらのヒットでも記録する。
        /// </summary>
        [Test]
        public void ApplyHit_RecordsHitCountAndBestPower()
        {
            ScoreBoard board = new ScoreBoard(10);

            board.ApplyHit(3f, 3);
            board.ApplyHit(20f, 20);
            board.ApplyHit(5f, 5);

            Assert.AreEqual(3, board.TotalHitCount);
            Assert.AreEqual(20f, board.BestHitPower);
        }

        /// <summary>
        /// リセットすると開始時の状態へ戻る。
        /// </summary>
        [Test]
        public void Reset_RestoresInitialState()
        {
            ScoreBoard board = new ScoreBoard(10);
            board.ApplyHit(10f, 10);
            board.ApplyHit(4f, 4);

            board.Reset(StartingBonno);

            Assert.AreEqual(StartingBonno, board.RemainingBonno);
            Assert.AreEqual(0, board.Benefit);
            Assert.AreEqual(0, board.TotalHitCount);
            Assert.AreEqual(0f, board.BestHitPower);
        }
    }
}

using Crane.Common;
using NUnit.Framework;

namespace Crane.Tests.EditMode
{
    /// <summary>
    /// <see cref="FixedStepTime"/> の秒数とステップ数の換算のテスト。
    /// </summary>
    public class FixedStepTimeTests
    {
        /// <summary>プロジェクトの固定ステップ幅(秒)。</summary>
        private const float StepSeconds = 0.02f;

        /// <summary>
        /// 秒数は最も近いステップ数になり、浮動小数点の誤差で 1 ステップ増えない。
        /// </summary>
        /// <param name="seconds">秒数。</param>
        /// <param name="expectedSteps">期待するステップ数。</param>
        [TestCase(30f, 1500)]
        [TestCase(10f, 500)]
        [TestCase(0.3f, 15)]
        [TestCase(0.02f, 1)]
        public void SecondsToSteps_RoundsToNearestStep(float seconds, int expectedSteps)
        {
            int steps = FixedStepTime.SecondsToSteps(seconds, StepSeconds);

            Assert.AreEqual(expectedSteps, steps);
        }

        /// <summary>
        /// 0 以下の秒数は 0 ステップになる。
        /// </summary>
        /// <param name="seconds">0 以下の秒数。</param>
        [TestCase(0f)]
        [TestCase(-1f)]
        public void SecondsToSteps_NonPositiveIsZero(float seconds)
        {
            int steps = FixedStepTime.SecondsToSteps(seconds, StepSeconds);

            Assert.AreEqual(0, steps);
        }
    }
}

using Crane.Bell;
using Crane.Data;
using Crane.Presentation;
using Crane.Score;
using NUnit.Framework;
using UnityEngine;

namespace Crane.Tests.EditMode
{
    /// <summary>
    /// ヒットの威力計算(<see cref="HitPowerCalculator"/>)と、
    /// 増減量への換算(<see cref="HitAmountCalculator"/>)、演出の強さ(<see cref="HitStrength"/>)のテスト。
    /// </summary>
    public class HitCalculatorTests
    {
        /// <summary>浮動小数点の比較の許容誤差。</summary>
        private const float Tolerance = 0.0001f;

        /// <summary>
        /// 各計算式が、対応する物理量を返す。
        /// </summary>
        /// <param name="formula">計算式。</param>
        /// <param name="expected">期待する威力(倍率 1、威力倍率 1 の時)。</param>
        [TestCase(HitPowerFormula.Impulse, 6f)]
        [TestCase(HitPowerFormula.RelativeVelocity, 4f)]
        [TestCase(HitPowerFormula.Momentum, 8f)]
        [TestCase(HitPowerFormula.KineticEnergy, 16f)]
        [TestCase(HitPowerFormula.PointVelocity, 5f)]
        public void Calculate_ReturnsValueOfSelectedFormula(HitPowerFormula formula, float expected)
        {
            BellHitData data = CreateHitData(1f);

            float power = HitPowerCalculator.Calculate(data, formula, 1f);

            Assert.AreEqual(expected, power, Tolerance);
        }

        /// <summary>
        /// ハンマーの威力倍率と設定の倍率が掛け合わされる。
        /// </summary>
        [Test]
        public void Calculate_AppliesDamageBonusAndMultiplier()
        {
            BellHitData data = CreateHitData(1.5f);

            float power = HitPowerCalculator.Calculate(data, HitPowerFormula.RelativeVelocity, 2f);

            Assert.AreEqual(4f * 1.5f * 2f, power, Tolerance);
        }

        /// <summary>
        /// 丸め方ごとに換算結果が変わる。
        /// </summary>
        /// <param name="mode">丸め方。</param>
        /// <param name="expected">威力 2.4 の時の期待値。</param>
        [TestCase(HitBonnoRoundingMode.Floor, 2)]
        [TestCase(HitBonnoRoundingMode.Round, 2)]
        [TestCase(HitBonnoRoundingMode.Ceil, 3)]
        public void HitAmount_UsesRoundingMode(HitBonnoRoundingMode mode, int expected)
        {
            int amount = HitAmountCalculator.Calculate(2.4f, 1f, mode);

            Assert.AreEqual(expected, amount);
        }

        /// <summary>
        /// 負の威力でも増減量は 0 未満にならない。
        /// </summary>
        [Test]
        public void HitAmount_NeverNegative()
        {
            int amount = HitAmountCalculator.Calculate(-5f, 1f, HitBonnoRoundingMode.Ceil);

            Assert.AreEqual(0, amount);
        }

        /// <summary>
        /// 演出の強さは威力に比例し、最大になる威力で 1 に達してそれ以上は増えない。
        /// </summary>
        [Test]
        public void HitStrength_IsLinearInPowerAndClamped()
        {
            GameSettings settings = ScriptableObject.CreateInstance<GameSettings>();
            settings.HitEffectPowerForMaxScale = 200f;

            Assert.AreEqual(0.25f, HitStrength.FromPower(50f, settings), Tolerance);
            Assert.AreEqual(0.5f, HitStrength.FromPower(100f, settings), Tolerance);
            Assert.AreEqual(1f, HitStrength.FromPower(400f, settings), Tolerance);

            Object.DestroyImmediate(settings);
        }

        /// <summary>
        /// 拡大率は強さに応じて最小値から最大値まで線形に変わる。
        /// </summary>
        [Test]
        public void HitStrength_EffectScaleInterpolatesBetweenMinAndMax()
        {
            GameSettings settings = ScriptableObject.CreateInstance<GameSettings>();
            settings.HitEffectMinScale = 0.2f;
            settings.HitEffectMaxScale = 1.2f;

            Assert.AreEqual(0.2f, HitStrength.ToEffectScale(0f, settings), Tolerance);
            Assert.AreEqual(0.7f, HitStrength.ToEffectScale(0.5f, settings), Tolerance);
            Assert.AreEqual(1.2f, HitStrength.ToEffectScale(1f, settings), Tolerance);

            Object.DestroyImmediate(settings);
        }

        /// <summary>
        /// テスト用のヒット情報を作る。
        /// </summary>
        /// <param name="damageBonus">ハンマーの威力倍率。</param>
        /// <returns>相対速度 4、撃力 6、質量 2、接触点速度 (3, 4) のヒット情報。</returns>
        private static BellHitData CreateHitData(float damageBonus)
        {
            return new BellHitData(
                0f,
                4f,
                6f,
                2f,
                damageBonus,
                Vector3.zero,
                new Vector3(3f, 4f, 0f),
                null);
        }
    }
}

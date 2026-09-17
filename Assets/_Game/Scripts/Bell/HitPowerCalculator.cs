namespace Crane.Bell
{
    /// <summary>
    /// ヒット情報から威力を計算する。
    /// </summary>
    public static class HitPowerCalculator
    {
        /// <summary>
        /// 指定した計算式で威力を求め、ハンマーの威力倍率と設定の倍率を掛ける。
        /// </summary>
        /// <param name="data">ヒット情報。</param>
        /// <param name="formula">使う計算式。</param>
        /// <param name="multiplier">設定の倍率。</param>
        /// <returns>ヒットの威力。</returns>
        public static float Calculate(BellHitData data, HitPowerFormula formula, float multiplier)
        {
            float rawPower = CalculateRawPower(data, formula);
            return rawPower * data.DamageBonus * multiplier;
        }

        /// <summary>
        /// 倍率を掛ける前の威力を求める。
        /// </summary>
        /// <param name="data">ヒット情報。</param>
        /// <param name="formula">使う計算式。</param>
        /// <returns>倍率を掛ける前の威力。</returns>
        private static float CalculateRawPower(BellHitData data, HitPowerFormula formula)
        {
            switch (formula)
            {
                case HitPowerFormula.RelativeVelocity:
                    return data.RelativeVelocity;

                case HitPowerFormula.Momentum:
                    return data.HammerMass * data.RelativeVelocity;

                case HitPowerFormula.KineticEnergy:
                    return 0.5f * data.HammerMass * data.RelativeVelocity * data.RelativeVelocity;

                case HitPowerFormula.PointVelocity:
                    return data.HammerPointVelocity.magnitude;

                case HitPowerFormula.Impulse:
                default:
                    return data.Impulse;
            }
        }
    }
}

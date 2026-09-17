using UnityEngine;

namespace Crane.Bell
{
    /// <summary>
    /// ハンマーが鐘に当たった瞬間の物理的な情報。
    /// </summary>
    public readonly struct BellHitData
    {
        /// <summary>当たった物理ステップの時刻(Time.fixedTime)。</summary>
        public readonly float Time;

        /// <summary>接触点での鐘に対するハンマーの相対速度の大きさ。</summary>
        public readonly float RelativeVelocity;

        /// <summary>鐘に与える撃力の大きさ。</summary>
        public readonly float Impulse;

        /// <summary>ハンマー(第二リンク)の質量。</summary>
        public readonly float HammerMass;

        /// <summary>ハンマーの威力倍率。</summary>
        public readonly float DamageBonus;

        /// <summary>接触点のワールド座標。</summary>
        public readonly Vector3 ContactPoint;

        /// <summary>接触点でのハンマーの速度。</summary>
        public readonly Vector3 HammerPointVelocity;

        /// <summary>ハンマーの効果音。未設定なら null。</summary>
        public readonly AudioClip HitSound;

        /// <summary>
        /// ヒット情報を作る。
        /// </summary>
        /// <param name="time">当たった時刻。</param>
        /// <param name="relativeVelocity">相対速度の大きさ。</param>
        /// <param name="impulse">撃力の大きさ。</param>
        /// <param name="hammerMass">ハンマーの質量。</param>
        /// <param name="damageBonus">ハンマーの威力倍率。</param>
        /// <param name="contactPoint">接触点。</param>
        /// <param name="hammerPointVelocity">接触点でのハンマーの速度。</param>
        /// <param name="hitSound">ハンマーの効果音。</param>
        public BellHitData(
            float time,
            float relativeVelocity,
            float impulse,
            float hammerMass,
            float damageBonus,
            Vector3 contactPoint,
            Vector3 hammerPointVelocity,
            AudioClip hitSound)
        {
            Time = time;
            RelativeVelocity = relativeVelocity;
            Impulse = impulse;
            HammerMass = hammerMass;
            DamageBonus = damageBonus;
            ContactPoint = contactPoint;
            HammerPointVelocity = hammerPointVelocity;
            HitSound = hitSound;
        }
    }
}

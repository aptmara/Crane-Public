using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// 鐘に当たったオブジェクトがハンマーであることと、その性質を示す目印。
    /// </summary>
    /// <remarks>
    /// <see cref="Crane.Bell.BellHitDetector"/> が接触したコライダーの親からこのコンポーネントを探す。
    /// </remarks>
    public class HammerMarker : MonoBehaviour
    {
        /// <summary>先端(第二リンク)の質量。</summary>
        public float Mass { get; private set; }

        /// <summary>ヒットの威力に掛ける倍率。</summary>
        public float DamageBonus { get; private set; }

        /// <summary>鐘に当たった時の効果音。</summary>
        public AudioClip HitSound { get; private set; }

        /// <summary>
        /// ハンマーの性質を設定する。
        /// </summary>
        /// <param name="mass">先端の質量。</param>
        /// <param name="damageBonus">ヒットの威力に掛ける倍率。</param>
        /// <param name="hitSound">鐘に当たった時の効果音。</param>
        public void Initialize(float mass, float damageBonus, AudioClip hitSound)
        {
            Mass = mass;
            DamageBonus = damageBonus;
            HitSound = hitSound;
        }
    }
}

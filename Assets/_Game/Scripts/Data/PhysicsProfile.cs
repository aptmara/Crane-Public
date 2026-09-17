using UnityEngine;

namespace Crane.Data
{
    /// <summary>
    /// 二重振り子(ArticulationBody)の物理パラメータ。
    /// </summary>
    /// <remarks>
    /// 原作では減速する配置もゲーム性の一部なので、
    /// 「常に派手に動かすために減衰を極端に小さくする」調整はしないこと。
    /// </remarks>
    [CreateAssetMenu(fileName = "PhysicsProfile", menuName = "Crane/Physics Profile")]
    public class PhysicsProfile : ScriptableObject
    {
        /// <summary>第一リンク(中間部分)の質量。原作では重い値で固定。</summary>
        [Header("Mass")]
        [Tooltip("第一リンク(中間部分)の質量。軽すぎると振り子が止まりやすくなる。")]
        public float ElbowMass = 8f;

        /// <summary>重力の倍率。</summary>
        [Header("Gravity")]
        [Tooltip("腕の振り子運動の速さ。1で通常重力、大きいほど振りが速くなる。")]
        public float GravityMultiplier = 3f;

        /// <summary>並進の減衰。</summary>
        [Header("Damping")]
        [Tooltip("並進の減衰。")]
        public float LinearDamping = 0f;

        /// <summary>回転の減衰。</summary>
        [Tooltip("回転の減衰。")]
        public float AngularDamping = 0f;

        /// <summary>関節の摩擦。</summary>
        [Tooltip("関節の摩擦。")]
        public float JointFriction = 0f;

        /// <summary>この運動量を下回るとスリープする閾値。</summary>
        [Tooltip("この運動量を下回るとスリープする閾値。")]
        public float SleepThreshold = 0f;
    }
}

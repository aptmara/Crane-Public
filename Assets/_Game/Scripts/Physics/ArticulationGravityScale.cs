using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// ArticulationBody に倍率付きの重力を加える。
    /// </summary>
    /// <remarks>
    /// ArticulationBody には重力倍率の設定がないため、組み込みの重力を切り(useGravity = false)、
    /// 代わりに毎物理ステップで「重力 × 倍率 × 質量」の力を加える。
    /// </remarks>
    [RequireComponent(typeof(ArticulationBody))]
    public class ArticulationGravityScale : MonoBehaviour
    {
        /// <summary>重力の倍率。1 で通常の重力。</summary>
        [SerializeField] private float multiplier = 1f;

        /// <summary>力を加える対象。</summary>
        private ArticulationBody body;

        /// <summary>
        /// 重力の倍率を設定する。
        /// </summary>
        /// <param name="value">重力の倍率。</param>
        public void SetMultiplier(float value)
        {
            multiplier = value;
        }

        /// <summary>
        /// 対象の ArticulationBody を取得する。
        /// </summary>
        private void Awake()
        {
            body = GetComponent<ArticulationBody>();
        }

        /// <summary>
        /// 物理ステップごとに重力を加える。
        /// </summary>
        private void FixedUpdate()
        {
            Vector3 gravityForce = Physics.gravity * multiplier * body.mass;
            body.AddForce(gravityForce, ForceMode.Force);
        }
    }
}

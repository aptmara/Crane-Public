using System;
using Crane.Common;
using Crane.PhysicsSim;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crane.Bell
{
    /// <summary>
    /// ハンマーが鐘に当たったことを検出し、鐘を押し返してヒットを通知する。
    /// </summary>
    /// <remarks>
    /// 鐘の当たり判定はトリガーで、物理的な反発は自前で加える。
    /// 1 回の接触が連続ヒットにならないよう、ヒット後は一定時間ヒットを受け付けない(無敵時間)。
    /// <code>
    /// 接触 ──(押し返しまでの遅延)──▶ 押し返し + Hit 通知 ──(無敵時間)──▶ 次のヒットを受付
    /// </code>
    /// 遅延と無敵時間は物理の固定ステップ数で数える(<see cref="FixedStepTime"/>)。
    /// 押し返しは鐘の揺れを変え、その後のヒットすべてに影響するため、フレームの長さで前後させない。
    /// 音や演出、スコアは <see cref="Hit"/> の購読側が扱う。
    /// </remarks>
    [RequireComponent(typeof(Collider))]
    public class BellHitDetector : MonoBehaviour
    {
        /// <summary>押し返す鐘の剛体。</summary>
        [SerializeField] private Rigidbody bellRigidbody;

        /// <summary>相対速度に掛けて撃力にする倍率。</summary>
        [SerializeField] private float impulseRate = 1f;

        /// <summary>接触してから押し返すまでの遅延(秒)。固定ステップ数に換算して使う。</summary>
        [FormerlySerializedAs("knockBackDelay")]
        [SerializeField] private float knockBackDelaySeconds = 0.02f;

        /// <summary>ヒット後に次のヒットを受け付けない時間(秒)。固定ステップ数に換算して使う。</summary>
        [FormerlySerializedAs("invincibleTime")]
        [SerializeField] private float invincibleSeconds = 0.3f;

        /// <summary>押し返し待ちのヒットがあれば true。</summary>
        private bool isKnockBackPending;

        /// <summary>押し返しまでの残りステップ数。</summary>
        private int knockBackStepsRemaining;

        /// <summary>無敵時間の残りステップ数。</summary>
        private int invincibleStepsRemaining;

        /// <summary>押し返し待ちのヒット情報。</summary>
        private BellHitData pendingHitData;

        /// <summary>押し返し待ちの撃力。接触した瞬間の相対速度から決まる。</summary>
        private Vector3 pendingImpulse;

        /// <summary>鐘が押し返された時に通知される。</summary>
        public event Action<BellHitData> Hit;

        /// <summary>
        /// ヒットを処理中(押し返し待ちまたは無敵時間中)なら true。
        /// </summary>
        private bool IsHitInProgress
        {
            get
            {
                if (isKnockBackPending)
                {
                    return true;
                }

                return invincibleStepsRemaining > 0;
            }
        }

        /// <summary>
        /// 必須参照を検証する。
        /// </summary>
        private void Awake()
        {
            if (!RequiredReference.IsAssigned(bellRigidbody, nameof(bellRigidbody), this))
            {
                enabled = false;
            }
        }

        /// <summary>
        /// 無効化時に処理中のヒットを打ち切り、次に有効になった時にすぐ受け付けられるようにする。
        /// </summary>
        private void OnDisable()
        {
            isKnockBackPending = false;
            knockBackStepsRemaining = 0;
            invincibleStepsRemaining = 0;
        }

        /// <summary>
        /// ハンマーとの接触を検出し、押し返しを予約する。
        /// </summary>
        /// <param name="other">接触したコライダー。</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!enabled || IsHitInProgress)
            {
                return;
            }

            HammerMarker hammer = other.GetComponentInParent<HammerMarker>();
            if (hammer == null)
            {
                return;
            }

            pendingHitData = MeasureHit(other, hammer, out pendingImpulse);
            knockBackStepsRemaining = Mathf.Max(1, FixedStepTime.SecondsToSteps(knockBackDelaySeconds));
            isKnockBackPending = true;
        }

        /// <summary>
        /// 物理ステップごとに、押し返しまでの遅延と無敵時間を進める。
        /// </summary>
        private void FixedUpdate()
        {
            if (isKnockBackPending)
            {
                knockBackStepsRemaining--;
                if (knockBackStepsRemaining > 0)
                {
                    return;
                }

                isKnockBackPending = false;
                invincibleStepsRemaining = FixedStepTime.SecondsToSteps(invincibleSeconds);
                ResolveHit();
                return;
            }

            if (invincibleStepsRemaining > 0)
            {
                invincibleStepsRemaining--;
            }
        }

        /// <summary>
        /// 接触点での相対速度から、ヒット情報と鐘に与える撃力を計測する。
        /// </summary>
        /// <param name="hammerCollider">接触したハンマーのコライダー。</param>
        /// <param name="hammer">ハンマーの性質。</param>
        /// <param name="impulse">鐘に与える撃力。接触した瞬間の相対速度から決まる。</param>
        /// <returns>ヒット情報。</returns>
        private BellHitData MeasureHit(Collider hammerCollider, HammerMarker hammer, out Vector3 impulse)
        {
            Vector3 contactPoint = hammerCollider.ClosestPoint(transform.position);

            Vector3 hammerPointVelocity = Vector3.zero;
            ArticulationBody hammerBody = hammerCollider.attachedArticulationBody;
            if (hammerBody != null)
            {
                hammerPointVelocity = hammerBody.GetPointVelocity(contactPoint);
            }

            Vector3 bellPointVelocity = bellRigidbody.GetPointVelocity(contactPoint);
            Vector3 relativeVelocity = hammerPointVelocity - bellPointVelocity;
            impulse = relativeVelocity * impulseRate;

            return new BellHitData(
                Time.fixedTime,
                relativeVelocity.magnitude,
                impulse.magnitude,
                hammer.Mass,
                hammer.DamageBonus,
                contactPoint,
                hammerPointVelocity,
                hammer.HitSound);
        }

        /// <summary>
        /// 予約していた撃力で鐘を押し返し、ヒットを通知する。
        /// </summary>
        private void ResolveHit()
        {
            bellRigidbody.AddForce(pendingImpulse, ForceMode.Impulse);

            CraneLog.Info(nameof(BellHitDetector), "Hit: relativeVelocity=" + pendingHitData.RelativeVelocity.ToString("F2") + " mass=" + pendingHitData.HammerMass.ToString("F2"));
            if (Hit != null)
            {
                Hit.Invoke(pendingHitData);
            }
        }
    }
}

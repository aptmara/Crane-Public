using Crane.Common;
using Crane.Data;
using Crane.Game;
using Crane.Presentation;
using Crane.Score;
using UnityEngine;

namespace Crane.Bell
{
    /// <summary>
    /// 鐘へのヒットを、スコアと演出へつなぐ。
    /// </summary>
    /// <remarks>
    /// 処理の流れ:
    /// <code>
    /// BellHitDetector.Hit ─▶ 威力を計算 ─▶ ScoreController.ApplyHit ─▶ HitResult
    ///                                                                  ├▶ HitEffectPresenter.Show
    ///                                                                  └▶ CameraShaker.Shake
    /// </code>
    /// スコア計測中(Playing / BenefitTime)以外のヒットは、スコアにも演出にも反映しない。
    /// 効果音は <see cref="BellHitAudioPlayer"/> が状態に関係なく鳴らす。
    /// </remarks>
    public class HitScoreBridge : MonoBehaviour
    {
        /// <summary>ヒットを通知する検出器。</summary>
        [SerializeField] private BellHitDetector bellHitDetector;

        /// <summary>威力の計算設定。</summary>
        [SerializeField] private HitPowerProfile hitPowerProfile;

        /// <summary>演出の強さの換算設定。</summary>
        [SerializeField] private GameSettings settings;

        /// <summary>スコア計測中かどうかを判断するための進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>スコア管理。</summary>
        [SerializeField] private ScoreController scoreController;

        /// <summary>ヒットの演出。</summary>
        [SerializeField] private HitEffectPresenter hitEffectPresenter;

        /// <summary>ヒット時の画面揺れ。</summary>
        [SerializeField] private CameraShaker cameraShaker;

        /// <summary>
        /// 必須参照を検証する。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
            }
        }

        /// <summary>
        /// ヒットの通知を購読する。
        /// </summary>
        private void OnEnable()
        {
            bellHitDetector.Hit += HandleHit;
        }

        /// <summary>
        /// ヒットの通知の購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            bellHitDetector.Hit -= HandleHit;
        }

        /// <summary>
        /// ヒットをスコアへ反映し、その結果で演出を表示する。
        /// </summary>
        /// <param name="hitData">ヒット情報。</param>
        private void HandleHit(BellHitData hitData)
        {
            if (!gameController.IsScoringActive)
            {
                return;
            }

            float power = HitPowerCalculator.Calculate(hitData, hitPowerProfile.Formula, hitPowerProfile.Multiplier);
            HitResult result = scoreController.ApplyHit(power);
            CraneLog.Info(nameof(HitScoreBridge), result.Kind + " +" + result.Amount + " (power=" + power.ToString("F2") + ")");

            hitEffectPresenter.Show(hitData.HammerPointVelocity, result);
            float strength = HitStrength.FromPower(result.Power, settings);
            cameraShaker.Shake(hitData.HammerPointVelocity, strength);
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(bellHitDetector, nameof(bellHitDetector), this);
            isValid &= RequiredReference.IsAssigned(hitPowerProfile, nameof(hitPowerProfile), this);
            isValid &= RequiredReference.IsAssigned(settings, nameof(settings), this);
            isValid &= RequiredReference.IsAssigned(gameController, nameof(gameController), this);
            isValid &= RequiredReference.IsAssigned(scoreController, nameof(scoreController), this);
            isValid &= RequiredReference.IsAssigned(hitEffectPresenter, nameof(hitEffectPresenter), this);
            isValid &= RequiredReference.IsAssigned(cameraShaker, nameof(cameraShaker), this);
            return isValid;
        }
    }
}

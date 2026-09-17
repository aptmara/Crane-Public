using Crane.Common;
using Crane.Game;
using Crane.Presentation;
using Crane.Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crane.UI
{
    /// <summary>
    /// 残り煩悩(スーパーご利益タイム中はご利益)の数値とゲージを表示する。
    /// </summary>
    /// <remarks>
    /// ゲージは 2 本重ねで、手前のゲージは即座に減り、奥のゲージは少し遅れて追いかける。
    /// 減った量が一瞬残って見えることで、ヒットの手応えを強調する。
    /// </remarks>
    public class BonnoHudController : MonoBehaviour
    {
        /// <summary>ゲームの進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>煩悩とご利益の変化を通知するスコア管理。</summary>
        [SerializeField] private ScoreController scoreController;

        /// <summary>「残り煩悩」「ご利益」のラベル。</summary>
        [SerializeField] private TMP_Text labelText;

        /// <summary>数値。</summary>
        [SerializeField] private TMP_Text scoreText;

        /// <summary>即座に減るゲージ。</summary>
        [SerializeField] private Slider mainSlider;

        /// <summary>遅れて追いかけるゲージ。</summary>
        [SerializeField] private Slider delayedSlider;

        /// <summary>遅れゲージが追いかけ始めるまでの待ち時間(秒)。</summary>
        [SerializeField] private float delayedStartSeconds = 0.25f;

        /// <summary>遅れゲージが追いつくまでの時間(秒)。</summary>
        [SerializeField] private float delayedFollowSeconds = 0.5f;

        /// <summary>シーンに設定された通常時のラベル文言。</summary>
        private string normalLabel;

        /// <summary>通常時のラベルの色。</summary>
        private Color normalLabelColor;

        /// <summary>通常時の数値の色。</summary>
        private Color normalScoreColor;

        /// <summary>ご利益を表示中なら true。</summary>
        private bool isShowingBenefit;

        /// <summary>遅れゲージが追いかけ中なら true。</summary>
        private bool isDelayedFollowing;

        /// <summary>遅れゲージが追いかけ始める時の値。</summary>
        private float delayedStartValue;

        /// <summary>遅れゲージが追いつく先の値。</summary>
        private float delayedTargetValue;

        /// <summary>追いかけ始めるまでの残り待ち時間(秒)。</summary>
        private float delayedWaitRemainingSeconds;

        /// <summary>追いかけ始めてからの経過時間(秒)。</summary>
        private float delayedFollowElapsedSeconds;

        /// <summary>
        /// 必須参照を検証し、通常時の文言と色を記録する。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            normalLabel = labelText.text;
            normalLabelColor = labelText.color;
            normalScoreColor = scoreText.color;
        }

        /// <summary>
        /// 通知を購読し、現在の状態で表示を初期化する。
        /// </summary>
        /// <remarks>
        /// この HUD はゲーム中だけ表示されるため、開始時の通知を受け取れない。
        /// そのため表示されるたびに現在の値から初期化する。
        /// </remarks>
        private void OnEnable()
        {
            scoreController.BonnoChanged += HandleBonnoChanged;
            scoreController.BenefitChanged += HandleBenefitChanged;
            gameController.StateChanged += HandleStateChanged;

            ConfigureSlider(mainSlider, scoreController.StartingBonno);
            ConfigureSlider(delayedSlider, scoreController.StartingBonno);
            ShowBonnoImmediately(scoreController.RemainingBonno);

            if (gameController.CurrentState == GameState.BenefitTime)
            {
                SwitchToBenefitDisplay();
            }
            else
            {
                SwitchToBonnoDisplay();
            }
        }

        /// <summary>
        /// 通知の購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            scoreController.BonnoChanged -= HandleBonnoChanged;
            scoreController.BenefitChanged -= HandleBenefitChanged;
            gameController.StateChanged -= HandleStateChanged;
        }

        /// <summary>
        /// 遅れゲージを追いかけさせる。
        /// </summary>
        private void Update()
        {
            if (!isDelayedFollowing)
            {
                return;
            }

            if (delayedWaitRemainingSeconds > 0f)
            {
                delayedWaitRemainingSeconds -= Time.unscaledDeltaTime;
                return;
            }

            delayedFollowElapsedSeconds += Time.unscaledDeltaTime;
            float duration = Mathf.Max(delayedFollowSeconds, Mathf.Epsilon);
            float progress = Mathf.Clamp01(delayedFollowElapsedSeconds / duration);
            delayedSlider.value = Mathf.Lerp(delayedStartValue, delayedTargetValue, progress);

            if (progress >= 1f)
            {
                isDelayedFollowing = false;
            }
        }

        /// <summary>
        /// 残り煩悩が変わったら数値とゲージを更新する。
        /// </summary>
        /// <param name="remainingBonno">変更後の残り煩悩。</param>
        private void HandleBonnoChanged(int remainingBonno)
        {
            if (isShowingBenefit)
            {
                return;
            }

            scoreText.text = remainingBonno.ToString();
            mainSlider.value = remainingBonno;
            StartDelayedFollow(remainingBonno);
        }

        /// <summary>
        /// ご利益が変わったら数値を更新する。
        /// </summary>
        /// <param name="benefit">変更後のご利益。</param>
        private void HandleBenefitChanged(int benefit)
        {
            if (!isShowingBenefit)
            {
                return;
            }

            scoreText.text = benefit.ToString();
        }

        /// <summary>
        /// スーパーご利益タイムに入ったらご利益表示に切り替える。
        /// </summary>
        /// <param name="previous">変更前の状態。</param>
        /// <param name="next">変更後の状態。</param>
        private void HandleStateChanged(GameState previous, GameState next)
        {
            if (next == GameState.BenefitTime)
            {
                SwitchToBenefitDisplay();
            }
        }

        /// <summary>
        /// ラベルと数値を「ご利益」の表示にする。
        /// </summary>
        private void SwitchToBenefitDisplay()
        {
            isShowingBenefit = true;
            isDelayedFollowing = false;

            labelText.text = ScoreStyle.BenefitLabel;
            labelText.color = ScoreStyle.BenefitColor;
            scoreText.color = ScoreStyle.BenefitColor;
            scoreText.text = scoreController.Benefit.ToString();
        }

        /// <summary>
        /// ラベルと数値を通常(残り煩悩)の表示にする。
        /// </summary>
        private void SwitchToBonnoDisplay()
        {
            isShowingBenefit = false;

            labelText.text = normalLabel;
            labelText.color = normalLabelColor;
            scoreText.color = normalScoreColor;
            scoreText.text = scoreController.RemainingBonno.ToString();
        }

        /// <summary>
        /// 数値と両方のゲージを、アニメーションなしで指定値にする。
        /// </summary>
        /// <param name="remainingBonno">表示する残り煩悩。</param>
        private void ShowBonnoImmediately(int remainingBonno)
        {
            scoreText.text = remainingBonno.ToString();
            mainSlider.value = remainingBonno;
            delayedSlider.value = remainingBonno;
            isDelayedFollowing = false;
        }

        /// <summary>
        /// 遅れゲージの追いかけを開始する。値が増えた時は追いかけず即座に合わせる。
        /// </summary>
        /// <param name="targetValue">追いつく先の値。</param>
        private void StartDelayedFollow(int targetValue)
        {
            if (targetValue >= delayedSlider.value)
            {
                delayedSlider.value = targetValue;
                isDelayedFollowing = false;
                return;
            }

            delayedStartValue = delayedSlider.value;
            delayedTargetValue = targetValue;
            delayedWaitRemainingSeconds = Mathf.Max(0f, delayedStartSeconds);
            delayedFollowElapsedSeconds = 0f;
            isDelayedFollowing = true;
        }

        /// <summary>
        /// ゲージを表示専用に設定する。
        /// </summary>
        /// <param name="slider">対象のゲージ。</param>
        /// <param name="maximum">ゲージの最大値。</param>
        private static void ConfigureSlider(Slider slider, int maximum)
        {
            slider.interactable = false;
            slider.wholeNumbers = false;
            slider.minValue = 0f;
            slider.maxValue = maximum;
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(gameController, nameof(gameController), this);
            isValid &= RequiredReference.IsAssigned(scoreController, nameof(scoreController), this);
            isValid &= RequiredReference.IsAssigned(labelText, nameof(labelText), this);
            isValid &= RequiredReference.IsAssigned(scoreText, nameof(scoreText), this);
            isValid &= RequiredReference.IsAssigned(mainSlider, nameof(mainSlider), this);
            isValid &= RequiredReference.IsAssigned(delayedSlider, nameof(delayedSlider), this);
            return isValid;
        }
    }
}

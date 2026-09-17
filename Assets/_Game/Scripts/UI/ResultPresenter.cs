using Crane.Common;
using Crane.Game;
using Crane.Presentation;
using Crane.Score;
using TMPro;
using UnityEngine;

namespace Crane.UI
{
    /// <summary>
    /// 時間切れ後の結果画面を表示する。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>煩悩を祓い切った: 獲得したご利益を表示する(年賀状として共有できる明るい画面)。</description></item>
    /// <item><description>祓い切れなかった: 残った煩悩を表示し、背景を灰色にする。</description></item>
    /// </list>
    /// 結果画面以外の状態では両方を隠し、背景を元の色へ戻す。
    /// </remarks>
    public class ResultPresenter : MonoBehaviour
    {
        /// <summary>ゲームの進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>結果の値を持つスコア管理。</summary>
        [SerializeField] private ScoreController scoreController;

        /// <summary>背景の色を変える担当。</summary>
        [SerializeField] private BackgroundTint backgroundTint;

        /// <summary>クリア時の結果画面。</summary>
        [SerializeField] private GameObject clearEnd;

        /// <summary>クリア時に獲得したご利益を表示するテキスト。</summary>
        [SerializeField] private TMP_Text clearEndScoreText;

        /// <summary>失敗時の結果画面。</summary>
        [SerializeField] private GameObject overEnd;

        /// <summary>失敗時に残った煩悩を表示するテキスト。</summary>
        [SerializeField] private TMP_Text overEndScoreText;

        /// <summary>クリア時の結果画面の表示を切り替える CanvasGroup。</summary>
        private CanvasGroup clearEndCanvasGroup;

        /// <summary>失敗時の結果画面の表示を切り替える CanvasGroup。</summary>
        private CanvasGroup overEndCanvasGroup;

        /// <summary>
        /// 必須参照を検証し、結果画面を隠す。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            clearEndCanvasGroup = CanvasGroupUtility.GetOrAdd(clearEnd);
            overEndCanvasGroup = CanvasGroupUtility.GetOrAdd(overEnd);
            Hide();
        }

        /// <summary>
        /// 状態の変化を購読する。
        /// </summary>
        private void OnEnable()
        {
            gameController.StateChanged += HandleStateChanged;
        }

        /// <summary>
        /// 状態の変化の購読を解除し、結果画面を隠す。
        /// </summary>
        private void OnDisable()
        {
            gameController.StateChanged -= HandleStateChanged;
            Hide();
        }

        /// <summary>
        /// 結果画面に入ったら表示し、抜けたら隠す。
        /// </summary>
        /// <param name="previous">変更前の状態。</param>
        /// <param name="next">変更後の状態。</param>
        private void HandleStateChanged(GameState previous, GameState next)
        {
            if (next == GameState.Result)
            {
                Show();
                return;
            }

            if (previous == GameState.Result)
            {
                Hide();
                backgroundTint.Restore();
            }
        }

        /// <summary>
        /// 煩悩を祓い切ったかどうかで、クリアと失敗のどちらかの結果画面を表示する。
        /// </summary>
        private void Show()
        {
            if (scoreController.IsBonnoCleared)
            {
                ShowClearEnd();
            }
            else
            {
                ShowOverEnd();
            }
        }

        /// <summary>
        /// クリア時の結果画面を表示する。
        /// </summary>
        private void ShowClearEnd()
        {
            backgroundTint.Restore();
            clearEndScoreText.text = scoreController.Benefit.ToString();
            overEndCanvasGroup.alpha = 0f;
            clearEndCanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 失敗時の結果画面を表示し、背景を灰色にする。
        /// </summary>
        private void ShowOverEnd()
        {
            backgroundTint.Apply(ScoreStyle.FailedBackgroundColor);
            overEndScoreText.text = scoreController.RemainingBonno.ToString();
            clearEndCanvasGroup.alpha = 0f;
            overEndCanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 両方の結果画面を隠す。
        /// </summary>
        private void Hide()
        {
            clearEndCanvasGroup.alpha = 0f;
            overEndCanvasGroup.alpha = 0f;
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
            isValid &= RequiredReference.IsAssigned(backgroundTint, nameof(backgroundTint), this);
            isValid &= RequiredReference.IsAssigned(clearEnd, nameof(clearEnd), this);
            isValid &= RequiredReference.IsAssigned(clearEndScoreText, nameof(clearEndScoreText), this);
            isValid &= RequiredReference.IsAssigned(overEnd, nameof(overEnd), this);
            isValid &= RequiredReference.IsAssigned(overEndScoreText, nameof(overEndScoreText), this);
            return isValid;
        }
    }
}

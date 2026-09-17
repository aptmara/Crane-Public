using Crane.Common;
using Crane.Data;
using Crane.Game;
using TMPro;
using UnityEngine;

namespace Crane.UI
{
    /// <summary>
    /// 「年明けまで」の残り秒数を表示する。
    /// </summary>
    /// <remarks>
    /// タイマーが動いていない間は、制限時間をそのまま表示する。
    /// 秒は切り上げで表示し、0.1 秒でも残っていれば 1 と表示する。
    /// </remarks>
    public class TimeHudController : MonoBehaviour
    {
        /// <summary>制限時間の設定。</summary>
        [SerializeField] private GameSettings settings;

        /// <summary>残り時間を持つタイマー。</summary>
        [SerializeField] private GameTimer gameTimer;

        /// <summary>残り秒数を表示するテキスト。</summary>
        [SerializeField] private TMP_Text timeText;

        /// <summary>表示中の秒数。変化した時だけテキストを書き換えるために使う。</summary>
        private int displayedSeconds = -1;

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
        /// 表示された時点の値ですぐに更新する。
        /// </summary>
        private void OnEnable()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 毎フレーム残り時間を反映する。
        /// </summary>
        private void Update()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 残り秒数が変わっていればテキストを更新する。
        /// </summary>
        private void UpdateDisplay()
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, GetRemainingSeconds()));
            if (seconds == displayedSeconds)
            {
                return;
            }

            displayedSeconds = seconds;
            timeText.text = seconds.ToString();
        }

        /// <summary>
        /// 表示する残り秒数を求める。
        /// </summary>
        /// <returns>タイマーが動いているか終了していればその残り時間、それ以外は制限時間。</returns>
        private float GetRemainingSeconds()
        {
            if (gameTimer.IsRunning || gameTimer.IsFinished)
            {
                return gameTimer.RemainingSeconds;
            }

            return settings.PlayDurationSeconds;
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(settings, nameof(settings), this);
            isValid &= RequiredReference.IsAssigned(gameTimer, nameof(gameTimer), this);
            isValid &= RequiredReference.IsAssigned(timeText, nameof(timeText), this);
            return isValid;
        }
    }
}

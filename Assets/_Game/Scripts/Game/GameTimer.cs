using System;
using Crane.Common;
using UnityEngine;

namespace Crane.Game
{
    /// <summary>
    /// 「年明けまで」の残り時間を数えるカウントダウンタイマー。
    /// </summary>
    /// <remarks>
    /// 時間の長さは知らず、開始時に <see cref="GameController"/> から秒数を受け取る。
    /// 残り時間は物理の固定ステップ数で数える(<see cref="FixedStepTime"/>)。
    /// フレームの長さに左右されず、同じ配置なら毎回同じ物理ステップで時間切れになる。
    /// 0 に達したら <see cref="Finished"/> を一度だけ通知する。
    /// </remarks>
    [DefaultExecutionOrder(ExecutionOrders.CoreSystems)]
    public class GameTimer : MonoBehaviour
    {
        /// <summary>残りの固定ステップ数。</summary>
        private int remainingSteps;

        /// <summary>残り時間が 0 になった時に通知される。</summary>
        public event Action Finished;

        /// <summary>残り秒数。停止中も最後の値を保持する。</summary>
        public float RemainingSeconds
        {
            get
            {
                return FixedStepTime.StepsToSeconds(remainingSteps);
            }
        }

        /// <summary>カウントダウン中なら true。</summary>
        public bool IsRunning { get; private set; }

        /// <summary>0 に達して終了したなら true。</summary>
        public bool IsFinished { get; private set; }

        /// <summary>
        /// 指定秒数からカウントダウンを開始する。
        /// </summary>
        /// <param name="durationSeconds">開始時の残り秒数。</param>
        public void StartCountdown(float durationSeconds)
        {
            remainingSteps = FixedStepTime.SecondsToSteps(durationSeconds);
            IsRunning = true;
            IsFinished = false;
        }

        /// <summary>
        /// 残り時間を保持したままカウントダウンを止める。
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// 停止して、開始前の状態へ戻す。
        /// </summary>
        public void ResetCountdown()
        {
            remainingSteps = 0;
            IsRunning = false;
            IsFinished = false;
        }

        /// <summary>
        /// 残り時間が指定秒数より短ければ、その秒数まで延長してカウントダウンを続ける。
        /// </summary>
        /// <param name="minimumSeconds">保証する最低残り秒数。</param>
        public void ExtendToAtLeast(float minimumSeconds)
        {
            remainingSteps = Mathf.Max(remainingSteps, FixedStepTime.SecondsToSteps(minimumSeconds));
            IsRunning = true;
            IsFinished = false;
        }

        /// <summary>
        /// 物理ステップごとに残り時間を減らし、0 に達したら終了を通知する。
        /// </summary>
        private void FixedUpdate()
        {
            if (!IsRunning)
            {
                return;
            }

            remainingSteps--;
            if (remainingSteps > 0)
            {
                return;
            }

            remainingSteps = 0;
            IsRunning = false;
            IsFinished = true;
            if (Finished != null)
            {
                Finished.Invoke();
            }
        }
    }
}

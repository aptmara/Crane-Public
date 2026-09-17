using System;
using Crane.Common;
using Crane.Data;
using Crane.Score;
using Crane.Setup;
using UnityEngine;

namespace Crane.Game
{
    /// <summary>
    /// ゲームの進行状態(<see cref="GameState"/>)を管理する状態機械。
    /// </summary>
    /// <remarks>
    /// 状態遷移を行うのはこのクラスだけで、他のコンポーネントは
    /// <see cref="StateChanged"/> などのイベントを購読して自分の表示や処理を切り替える。
    /// 遷移のきっかけは次の通り。
    /// <list type="bullet">
    /// <item><description>PLAY ボタン: <see cref="StartGame"/></description></item>
    /// <item><description>STOP ボタン: <see cref="StopGameAndReturnToSetup"/></description></item>
    /// <item><description>煩悩を祓い切った: <see cref="ScoreController.BonnoCleared"/></description></item>
    /// <item><description>時間切れ: <see cref="GameTimer.Finished"/></description></item>
    /// <item><description>異常加速: <see cref="NotifyCraneBroken"/></description></item>
    /// </list>
    /// </remarks>
    [DefaultExecutionOrder(ExecutionOrders.CoreSystems)]
    public class GameController : MonoBehaviour
    {
        /// <summary>制限時間などのゲームルール設定。</summary>
        [SerializeField] private GameSettings settings;

        /// <summary>残り時間を数えるタイマー。</summary>
        [SerializeField] private GameTimer gameTimer;

        /// <summary>煩悩とご利益を集計するスコア管理。</summary>
        [SerializeField] private ScoreController scoreController;

        /// <summary>状態が変わった時に (変更前, 変更後) で通知される。</summary>
        public event Action<GameState, GameState> StateChanged;

        /// <summary>PLAY で物理シミュレーションを開始する時に、確定した初期姿勢付きで通知される。</summary>
        public event Action<CraneSetupSnapshot> GameStarted;

        /// <summary>STOP で Setup へ戻る時に通知される。</summary>
        public event Action GameStopped;

        /// <summary>現在の進行状態。</summary>
        public GameState CurrentState { get; private set; }

        /// <summary>直近の PLAY で確定した初期姿勢。一度も開始していなければ null。</summary>
        public CraneSetupSnapshot CurrentSnapshot { get; private set; }

        /// <summary>
        /// 鐘へのヒットをスコアとして数える状態なら true。
        /// </summary>
        public bool IsScoringActive
        {
            get
            {
                if (CurrentState == GameState.Playing)
                {
                    return true;
                }

                if (CurrentState == GameState.BenefitTime)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 必須参照を検証し、フレームレートを固定する。
        /// </summary>
        private void Awake()
        {
            CurrentState = GameState.Setup;
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = settings.TargetFrameRate;
        }

        /// <summary>
        /// タイマーとスコアの通知を購読する。
        /// </summary>
        private void OnEnable()
        {
            gameTimer.Finished += HandleTimerFinished;
            scoreController.BonnoCleared += HandleBonnoCleared;
        }

        /// <summary>
        /// タイマーとスコアの通知の購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            gameTimer.Finished -= HandleTimerFinished;
            scoreController.BonnoCleared -= HandleBonnoCleared;
        }

        /// <summary>
        /// Setup で確定した初期姿勢からゲームを開始する。
        /// </summary>
        /// <param name="snapshot">PLAY を押した瞬間のクレーンと振り子の姿勢。</param>
        public void StartGame(CraneSetupSnapshot snapshot)
        {
            if (CurrentState != GameState.Setup)
            {
                CraneLog.Info(nameof(GameController), "Setup 以外の状態で StartGame が呼ばれたため無視します。state=" + CurrentState);
                return;
            }

            CraneLog.Info(nameof(GameController), "StartGame: " + snapshot);

            CurrentSnapshot = snapshot;
            scoreController.ResetScore();
            gameTimer.StartCountdown(settings.PlayDurationSeconds);
            ChangeState(GameState.Playing);

            if (GameStarted != null)
            {
                GameStarted.Invoke(snapshot);
            }
        }

        /// <summary>
        /// 実行中のゲームを中断して Setup へ戻る。
        /// </summary>
        public void StopGameAndReturnToSetup()
        {
            if (CurrentState == GameState.Setup)
            {
                return;
            }

            gameTimer.ResetCountdown();
            scoreController.ResetScore();
            ChangeState(GameState.Setup);

            if (GameStopped != null)
            {
                GameStopped.Invoke();
            }
        }

        /// <summary>
        /// 異常加速でアームが折れたことを通知し、特殊失敗ルートへ移る。
        /// </summary>
        /// <remarks>
        /// 原作では永久機関によるスコア稼ぎを防ぐため、この状態をミスとして扱いスコアを表示しない。
        /// </remarks>
        public void NotifyCraneBroken()
        {
            if (!IsScoringActive)
            {
                return;
            }

            gameTimer.Stop();
            ChangeState(GameState.BrokenEnding);
        }

        /// <summary>
        /// 煩悩を祓い切った時にスーパーご利益タイムへ移る。
        /// </summary>
        /// <remarks>
        /// 残り時間が短すぎるとご利益タイムを楽しめないため、最低秒数まで延長する。
        /// 物理シミュレーションは止めずにそのまま続ける。
        /// </remarks>
        private void HandleBonnoCleared()
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }

            gameTimer.ExtendToAtLeast(settings.BenefitTimeMinimumSeconds);
            ChangeState(GameState.BenefitTime);
        }

        /// <summary>
        /// 時間切れで結果表示へ移る。
        /// </summary>
        private void HandleTimerFinished()
        {
            if (!IsScoringActive)
            {
                return;
            }

            ChangeState(GameState.Result);
        }

        /// <summary>
        /// 状態を変更し、変化があれば <see cref="StateChanged"/> を通知する。
        /// </summary>
        /// <param name="next">変更後の状態。</param>
        private void ChangeState(GameState next)
        {
            if (CurrentState == next)
            {
                return;
            }

            GameState previous = CurrentState;
            CurrentState = next;
            CraneLog.Info(nameof(GameController), "State: " + previous + " -> " + next);

            if (StateChanged != null)
            {
                StateChanged.Invoke(previous, next);
            }
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
            isValid &= RequiredReference.IsAssigned(scoreController, nameof(scoreController), this);
            return isValid;
        }
    }
}

using System;
using Crane.Common;
using Crane.Data;
using UnityEngine;

namespace Crane.Score
{
    /// <summary>
    /// シーン上でスコア集計(<see cref="ScoreBoard"/>)を保持し、変化を通知する。
    /// </summary>
    /// <remarks>
    /// ゲームの進行状態は知らない。ヒットをスコアとして数えるかどうかは呼び出し側
    /// (<see cref="Crane.Bell.HitScoreBridge"/>)が判断する。
    /// </remarks>
    [DefaultExecutionOrder(ExecutionOrders.CoreSystems)]
    public class ScoreController : MonoBehaviour
    {
        /// <summary>開始時の煩悩数と、威力から増減量への換算設定。</summary>
        [SerializeField] private GameSettings settings;

        /// <summary>集計ルール本体。</summary>
        private ScoreBoard board;

        /// <summary>残り煩悩が変わった時に、変更後の値で通知される。</summary>
        public event Action<int> BonnoChanged;

        /// <summary>ご利益が変わった時に、変更後の値で通知される。</summary>
        public event Action<int> BenefitChanged;

        /// <summary>煩悩を祓い切った瞬間に一度だけ通知される。</summary>
        public event Action BonnoCleared;

        /// <summary>開始時の煩悩数。</summary>
        public int StartingBonno
        {
            get
            {
                return settings.StartingBonno;
            }
        }

        /// <summary>残り煩悩。</summary>
        public int RemainingBonno
        {
            get
            {
                return board.RemainingBonno;
            }
        }

        /// <summary>獲得したご利益。</summary>
        public int Benefit
        {
            get
            {
                return board.Benefit;
            }
        }

        /// <summary>煩悩を祓い切っていれば true。</summary>
        public bool IsBonnoCleared
        {
            get
            {
                return board.IsBonnoCleared;
            }
        }

        /// <summary>
        /// 必須参照を検証し、集計を初期化する。
        /// </summary>
        private void Awake()
        {
            if (!RequiredReference.IsAssigned(settings, nameof(settings), this))
            {
                enabled = false;
                return;
            }

            board = new ScoreBoard(settings.StartingBonno);
        }

        /// <summary>
        /// 集計を開始時の状態へ戻す。
        /// </summary>
        public void ResetScore()
        {
            board.Reset(settings.StartingBonno);
            RaiseBonnoChanged();
            RaiseBenefitChanged();
        }

        /// <summary>
        /// 1 回のヒットをスコアへ反映する。
        /// </summary>
        /// <param name="hitPower">ヒットの威力。</param>
        /// <returns>何にどれだけ反映されたか。演出に渡す。</returns>
        public HitResult ApplyHit(float hitPower)
        {
            int amount = HitAmountCalculator.Calculate(
                hitPower,
                settings.HitBonnoScoreMultiplier,
                settings.HitBonnoRoundingMode);

            bool wasCleared = board.IsBonnoCleared;
            HitResult result = board.ApplyHit(hitPower, amount);

            if (result.Kind == HitResultKind.Benefit)
            {
                RaiseBenefitChanged();
                return result;
            }

            RaiseBonnoChanged();
            if (!wasCleared && board.IsBonnoCleared)
            {
                RaiseBonnoCleared();
            }

            return result;
        }

        /// <summary><see cref="BonnoChanged"/> を通知する。</summary>
        private void RaiseBonnoChanged()
        {
            if (BonnoChanged != null)
            {
                BonnoChanged.Invoke(board.RemainingBonno);
            }
        }

        /// <summary><see cref="BenefitChanged"/> を通知する。</summary>
        private void RaiseBenefitChanged()
        {
            if (BenefitChanged != null)
            {
                BenefitChanged.Invoke(board.Benefit);
            }
        }

        /// <summary><see cref="BonnoCleared"/> を通知する。</summary>
        private void RaiseBonnoCleared()
        {
            if (BonnoCleared != null)
            {
                BonnoCleared.Invoke();
            }
        }
    }
}

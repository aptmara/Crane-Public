using Crane.Common;
using Crane.Game;
using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// ゲームの状態に応じて、GameObject の表示とコンポーネントの有効・無効を切り替える。
    /// </summary>
    /// <remarks>
    /// 「どの状態で何を見せるか」はコードではなく Inspector の表(ルールの配列)で持つ。
    /// 表示を追加・変更する時はルールを 1 行足すだけでよい。
    /// </remarks>
    public class StateVisibilityController : MonoBehaviour
    {
        /// <summary>状態の変化を通知する進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>GameObject の表示ルール。</summary>
        [SerializeField] private StateVisibilityRule[] visibilityRules = new StateVisibilityRule[0];

        /// <summary>コンポーネントの有効・無効ルール。</summary>
        [SerializeField] private StateBehaviourRule[] behaviourRules = new StateBehaviourRule[0];

        /// <summary>
        /// 必須参照を検証する。
        /// </summary>
        private void Awake()
        {
            if (!RequiredReference.IsAssigned(gameController, nameof(gameController), this))
            {
                enabled = false;
            }
        }

        /// <summary>
        /// 状態の変化を購読する。
        /// </summary>
        private void OnEnable()
        {
            gameController.StateChanged += HandleStateChanged;
        }

        /// <summary>
        /// 状態の変化の購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            gameController.StateChanged -= HandleStateChanged;
        }

        /// <summary>
        /// 起動時の状態を反映する。
        /// </summary>
        private void Start()
        {
            Apply(gameController.CurrentState);
        }

        /// <summary>
        /// 状態が変わったらルールを適用し直す。
        /// </summary>
        /// <param name="previous">変更前の状態。</param>
        /// <param name="next">変更後の状態。</param>
        private void HandleStateChanged(GameState previous, GameState next)
        {
            Apply(next);
        }

        /// <summary>
        /// すべてのルールを指定状態に対して適用する。
        /// </summary>
        /// <param name="state">適用する状態。</param>
        private void Apply(GameState state)
        {
            foreach (StateVisibilityRule rule in visibilityRules)
            {
                ApplyVisibilityRule(rule, state);
            }

            foreach (StateBehaviourRule rule in behaviourRules)
            {
                ApplyBehaviourRule(rule, state);
            }
        }

        /// <summary>
        /// GameObject の表示ルールを 1 件適用する。
        /// </summary>
        /// <param name="rule">適用するルール。</param>
        /// <param name="state">現在の状態。</param>
        private void ApplyVisibilityRule(StateVisibilityRule rule, GameState state)
        {
            if (rule.Target == null)
            {
                CraneLog.Warning(nameof(StateVisibilityController), "対象が未設定の表示ルールがあります。", this);
                return;
            }

            bool isInStates = GameStateMaskUtility.Contains(rule.States, state);

            if (rule.Mode == StateVisibilityMode.ShowOnlyInStates)
            {
                rule.Target.SetActive(isInStates);
                return;
            }

            if (isInStates)
            {
                rule.Target.SetActive(false);
            }
        }

        /// <summary>
        /// コンポーネントの有効・無効ルールを 1 件適用する。
        /// </summary>
        /// <param name="rule">適用するルール。</param>
        /// <param name="state">現在の状態。</param>
        private void ApplyBehaviourRule(StateBehaviourRule rule, GameState state)
        {
            if (rule.Target == null)
            {
                CraneLog.Warning(nameof(StateVisibilityController), "対象が未設定のコンポーネントルールがあります。", this);
                return;
            }

            rule.Target.enabled = GameStateMaskUtility.Contains(rule.EnabledStates, state);
        }
    }
}

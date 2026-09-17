using System;
using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// 「どのコンポーネントを、どの状態で有効にするか」の 1 行分の設定。
    /// </summary>
    [Serializable]
    public class StateBehaviourRule
    {
        /// <summary>有効・無効を切り替える対象。</summary>
        [SerializeField] private Behaviour target;

        /// <summary>有効にする状態。</summary>
        [SerializeField] private GameStateMask enabledStates;

        /// <summary>有効・無効を切り替える対象。</summary>
        public Behaviour Target
        {
            get
            {
                return target;
            }
        }

        /// <summary>有効にする状態。</summary>
        public GameStateMask EnabledStates
        {
            get
            {
                return enabledStates;
            }
        }
    }
}

using System;
using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// 「どの GameObject を、どの状態で表示するか」の 1 行分の設定。
    /// </summary>
    [Serializable]
    public class StateVisibilityRule
    {
        /// <summary>表示を切り替える対象。</summary>
        [SerializeField] private GameObject target;

        /// <summary>対象の状態。</summary>
        [SerializeField] private GameStateMask states;

        /// <summary>適用方法。</summary>
        [SerializeField] private StateVisibilityMode mode;

        /// <summary>表示を切り替える対象。</summary>
        public GameObject Target
        {
            get
            {
                return target;
            }
        }

        /// <summary>対象の状態。</summary>
        public GameStateMask States
        {
            get
            {
                return states;
            }
        }

        /// <summary>適用方法。</summary>
        public StateVisibilityMode Mode
        {
            get
            {
                return mode;
            }
        }
    }
}

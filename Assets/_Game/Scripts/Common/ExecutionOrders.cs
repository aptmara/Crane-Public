namespace Crane.Common
{
    /// <summary>
    /// スクリプトの実行順(DefaultExecutionOrder)の一覧。
    /// </summary>
    /// <remarks>
    /// 表示側のコンポーネントは OnEnable でスコアや状態を読むことがあるため、
    /// それらを持つ中核のコンポーネントの Awake を先に済ませておく。
    /// 値が小さいほど先に実行される。未指定のスクリプトは 0。
    /// </remarks>
    public static class ExecutionOrders
    {
        /// <summary>進行管理・タイマー・スコアなど、他から参照される中核のコンポーネント。</summary>
        public const int CoreSystems = -100;
    }
}

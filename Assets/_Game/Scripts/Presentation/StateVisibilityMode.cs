namespace Crane.Presentation
{
    /// <summary>
    /// <see cref="StateVisibilityRule"/> の適用方法。
    /// </summary>
    public enum StateVisibilityMode
    {
        /// <summary>指定した状態でだけ表示し、それ以外では非表示にする。</summary>
        ShowOnlyInStates,

        /// <summary>指定した状態で非表示にするだけで、それ以外では表示を変更しない(表示は別のコンポーネントが決める)。</summary>
        HideInStates,
    }
}

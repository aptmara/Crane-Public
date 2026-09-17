namespace Crane.Common
{
    /// <summary>
    /// Inspector で設定する必須参照の検証を行う。
    /// </summary>
    /// <remarks>
    /// 必須参照は各コンポーネントの Awake で一度だけ検証し、未設定なら即座にエラーを出して
    /// コンポーネントを無効化する(fail fast)。
    /// これにより、処理の途中に null チェックを散らばらせて設定漏れを握りつぶすことを避ける。
    /// </remarks>
    public static class RequiredReference
    {
        /// <summary>
        /// 参照が設定されているかを検証し、未設定ならエラーログを出す。
        /// </summary>
        /// <param name="reference">検証する参照。</param>
        /// <param name="fieldName">エラーメッセージに表示するフィールド名。nameof 演算子で渡す。</param>
        /// <param name="owner">参照を持つコンポーネント。</param>
        /// <returns>設定されていれば true。</returns>
        /// <remarks>
        /// UnityEngine.Object の == 演算子は破棄済みオブジェクトも null とみなすため、
        /// 引数を UnityEngine.Object として受けて比較している。
        /// </remarks>
        public static bool IsAssigned(UnityEngine.Object reference, string fieldName, UnityEngine.Object owner)
        {
            if (reference != null)
            {
                return true;
            }

            string ownerName = owner.GetType().Name;
            CraneLog.Error(ownerName, "必須参照 '" + fieldName + "' が設定されていません。", owner);
            return false;
        }
    }
}

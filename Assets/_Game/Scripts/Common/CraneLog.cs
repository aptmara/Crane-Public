using System.Diagnostics;

namespace Crane.Common
{
    /// <summary>
    /// ゲーム全体で使うログ出力の窓口。
    /// </summary>
    /// <remarks>
    /// 情報ログ(<see cref="Info"/>)はエディタと Development Build でのみ出力し、
    /// リリースビルドでは呼び出しごとコンパイル時に除去される。
    /// 警告とエラーは設定ミスや異常の検知に使うため、常に出力する。
    /// </remarks>
    public static class CraneLog
    {
        /// <summary>全ログに付ける接頭辞。Console の検索用。</summary>
        private const string Prefix = "[Crane]";

        /// <summary>
        /// 動作確認用の情報ログを出力する。
        /// </summary>
        /// <param name="category">出力元を表す名前。通常はクラス名を渡す。</param>
        /// <param name="message">出力する本文。</param>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Info(string category, string message)
        {
            UnityEngine.Debug.Log(Format(category, message));
        }

        /// <summary>
        /// 処理は継続できるが、想定外の状態であることを警告する。
        /// </summary>
        /// <param name="category">出力元を表す名前。通常はクラス名を渡す。</param>
        /// <param name="message">出力する本文。</param>
        /// <param name="context">Console でクリックした時に選択されるオブジェクト。</param>
        public static void Warning(string category, string message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(Format(category, message), context);
        }

        /// <summary>
        /// 処理を継続できない異常を出力する。
        /// </summary>
        /// <param name="category">出力元を表す名前。通常はクラス名を渡す。</param>
        /// <param name="message">出力する本文。</param>
        /// <param name="context">Console でクリックした時に選択されるオブジェクト。</param>
        public static void Error(string category, string message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(Format(category, message), context);
        }

        /// <summary>
        /// 接頭辞とカテゴリを付けた出力文字列を作る。
        /// </summary>
        /// <param name="category">出力元を表す名前。</param>
        /// <param name="message">出力する本文。</param>
        /// <returns>「[Crane][カテゴリ] 本文」形式の文字列。</returns>
        private static string Format(string category, string message)
        {
            return Prefix + "[" + category + "] " + message;
        }
    }
}

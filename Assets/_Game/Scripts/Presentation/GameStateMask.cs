using System;
using Crane.Game;

namespace Crane.Presentation
{
    /// <summary>
    /// 複数の <see cref="GameState"/> をまとめて指定するためのビットフラグ。
    /// </summary>
    /// <remarks>
    /// Inspector で「どの状態で表示するか」を複数選択できるようにするために使う。
    /// </remarks>
    [Flags]
    public enum GameStateMask
    {
        /// <summary>どの状態も含まない。</summary>
        None = 0,

        /// <summary><see cref="GameState.Setup"/>。</summary>
        Setup = 1 << 0,

        /// <summary><see cref="GameState.Playing"/>。</summary>
        Playing = 1 << 1,

        /// <summary><see cref="GameState.BenefitTime"/>。</summary>
        BenefitTime = 1 << 2,

        /// <summary><see cref="GameState.Result"/>。</summary>
        Result = 1 << 3,

        /// <summary><see cref="GameState.BrokenEnding"/>。</summary>
        BrokenEnding = 1 << 4,
    }

    /// <summary>
    /// <see cref="GameStateMask"/> の判定処理。
    /// </summary>
    public static class GameStateMaskUtility
    {
        /// <summary>
        /// マスクが指定した状態を含むかを判定する。
        /// </summary>
        /// <param name="mask">判定するマスク。</param>
        /// <param name="state">含まれているか調べる状態。</param>
        /// <returns>含まれていれば true。</returns>
        public static bool Contains(GameStateMask mask, GameState state)
        {
            GameStateMask flag = ToMask(state);
            return (mask & flag) == flag;
        }

        /// <summary>
        /// 状態を対応するフラグへ変換する。
        /// </summary>
        /// <param name="state">変換する状態。</param>
        /// <returns>対応するフラグ。</returns>
        public static GameStateMask ToMask(GameState state)
        {
            switch (state)
            {
                case GameState.Setup:
                    return GameStateMask.Setup;

                case GameState.Playing:
                    return GameStateMask.Playing;

                case GameState.BenefitTime:
                    return GameStateMask.BenefitTime;

                case GameState.Result:
                    return GameStateMask.Result;

                case GameState.BrokenEnding:
                    return GameStateMask.BrokenEnding;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, "未対応の GameState です。");
            }
        }
    }
}

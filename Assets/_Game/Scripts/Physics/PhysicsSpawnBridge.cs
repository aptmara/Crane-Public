using Crane.Common;
using Crane.Data;
using Crane.Game;
using Crane.Setup;
using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// ゲームの進行に合わせて、物理リグの生成・破棄・破損を行う。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>ゲーム開始: 確定した姿勢をワールド座標へ変換してリグを生成する。</description></item>
    /// <item><description>Setup へ戻る: リグを破棄する。</description></item>
    /// <item><description>BrokenEnding へ移る: アームを折る。</description></item>
    /// </list>
    /// </remarks>
    public class PhysicsSpawnBridge : MonoBehaviour
    {
        /// <summary>ゲームの進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>リグの生成担当。</summary>
        [SerializeField] private CraneSpawner spawner;

        /// <summary>座標変換に使う Setup Canvas の基準解像度を持つデータ。</summary>
        [SerializeField] private CranePoseEditorData poseEditorData;

        /// <summary>物理ワールドを映すカメラ。</summary>
        [SerializeField] private Camera gameplayCamera;

        /// <summary>
        /// 必須参照を検証する。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
            }
        }

        /// <summary>
        /// ゲームの進行を購読する。
        /// </summary>
        private void OnEnable()
        {
            gameController.GameStarted += HandleGameStarted;
            gameController.GameStopped += HandleGameStopped;
            gameController.StateChanged += HandleStateChanged;
        }

        /// <summary>
        /// ゲームの進行の購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            gameController.GameStarted -= HandleGameStarted;
            gameController.GameStopped -= HandleGameStopped;
            gameController.StateChanged -= HandleStateChanged;
        }

        /// <summary>
        /// 確定した姿勢からリグを生成する。
        /// </summary>
        /// <param name="snapshot">PLAY で確定した姿勢。</param>
        private void HandleGameStarted(CraneSetupSnapshot snapshot)
        {
            CranePhysicsSetup setup = SetupToPhysicsConverter.Convert(
                snapshot,
                poseEditorData.ReferenceResolution,
                gameplayCamera);
            spawner.Spawn(setup);
        }

        /// <summary>
        /// Setup へ戻る時にリグを破棄する。
        /// </summary>
        private void HandleGameStopped()
        {
            spawner.DestroyCurrentRig();
        }

        /// <summary>
        /// 特殊失敗ルートへ移ったらアームを折る。
        /// </summary>
        /// <param name="previous">変更前の状態。</param>
        /// <param name="next">変更後の状態。</param>
        private void HandleStateChanged(GameState previous, GameState next)
        {
            if (next != GameState.BrokenEnding)
            {
                return;
            }

            if (spawner.CurrentRig == null)
            {
                return;
            }

            spawner.CurrentRig.Break();
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(gameController, nameof(gameController), this);
            isValid &= RequiredReference.IsAssigned(spawner, nameof(spawner), this);
            isValid &= RequiredReference.IsAssigned(poseEditorData, nameof(poseEditorData), this);
            isValid &= RequiredReference.IsAssigned(gameplayCamera, nameof(gameplayCamera), this);
            return isValid;
        }
    }
}

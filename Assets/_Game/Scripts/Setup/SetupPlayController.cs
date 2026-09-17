using Crane.Common;
using Crane.Game;
using Crane.Hammer;
using UnityEngine;

namespace Crane.Setup
{
    /// <summary>
    /// PLAY ボタンと STOP ボタンの処理。
    /// </summary>
    /// <remarks>
    /// Button の OnClick から <see cref="OnPlayButtonPressed"/> / <see cref="OnStopButtonPressed"/> を呼ぶ。
    /// </remarks>
    public class SetupPlayController : MonoBehaviour
    {
        /// <summary>現在の姿勢を持つ姿勢エディタ。</summary>
        [SerializeField] private CranePoseEditorController poseEditor;

        /// <summary>選択中のハンマーを持つ切り替えボタン。</summary>
        [SerializeField] private HammerSwitchController hammerSwitch;

        /// <summary>ゲームの進行管理。</summary>
        [SerializeField] private GameController gameController;

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
        /// 現在の姿勢と選択中のハンマーを初期条件として確定し、ゲームを開始する。
        /// </summary>
        public void OnPlayButtonPressed()
        {
            HammerDefinition hammer = hammerSwitch.SelectedHammer;
            CraneLog.Info(nameof(SetupPlayController), "PLAY: hammer=" + hammer.Id);

            CraneSetupSnapshot snapshot = poseEditor.BuildSnapshot(hammer);
            gameController.StartGame(snapshot);
        }

        /// <summary>
        /// ゲームを中断して Setup へ戻る。
        /// </summary>
        public void OnStopButtonPressed()
        {
            hammerSwitch.ResetSelection();
            gameController.StopGameAndReturnToSetup();
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(poseEditor, nameof(poseEditor), this);
            isValid &= RequiredReference.IsAssigned(hammerSwitch, nameof(hammerSwitch), this);
            isValid &= RequiredReference.IsAssigned(gameController, nameof(gameController), this);
            return isValid;
        }
    }
}

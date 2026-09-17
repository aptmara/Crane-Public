using Crane.Common;
using Crane.Hammer;
using UnityEngine;
using UnityEngine.UI;

namespace Crane.Setup
{
    /// <summary>
    /// Setup 画面の金槌・木槌の切り替えボタン。
    /// </summary>
    /// <remarks>
    /// 先端の重さは振り子の運動そのものを変える、重要な攻略要素。
    /// 起動時と STOP で Setup へ戻った時は金槌を選択した状態にする。
    /// </remarks>
    public class HammerSwitchController : MonoBehaviour
    {
        /// <summary>金槌を選ぶボタン。</summary>
        [SerializeField] private Button metalButton;

        /// <summary>木槌を選ぶボタン。</summary>
        [SerializeField] private Button woodButton;

        /// <summary>金槌が選択中の時に表示する画像。</summary>
        [SerializeField] private Image metalOnImage;

        /// <summary>金槌が未選択の時に表示する画像。</summary>
        [SerializeField] private Image metalOffImage;

        /// <summary>木槌が選択中の時に表示する画像。</summary>
        [SerializeField] private Image woodOnImage;

        /// <summary>木槌が未選択の時に表示する画像。</summary>
        [SerializeField] private Image woodOffImage;

        /// <summary>金槌の定義。</summary>
        [SerializeField] private HammerDefinition metalHammer;

        /// <summary>木槌の定義。</summary>
        [SerializeField] private HammerDefinition woodHammer;

        /// <summary>選択したハンマーの画像を反映する姿勢エディタ。</summary>
        [SerializeField] private CranePoseEditorController poseEditor;

        /// <summary>選択中のハンマー。</summary>
        public HammerDefinition SelectedHammer { get; private set; }

        /// <summary>
        /// 必須参照を検証し、ボタンを登録して金槌を選択する。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            metalButton.onClick.AddListener(HandleMetalButtonClicked);
            woodButton.onClick.AddListener(HandleWoodButtonClicked);
            Select(metalHammer);
        }

        /// <summary>
        /// ボタンの登録を解除する。
        /// </summary>
        private void OnDestroy()
        {
            if (metalButton != null)
            {
                metalButton.onClick.RemoveListener(HandleMetalButtonClicked);
            }

            if (woodButton != null)
            {
                woodButton.onClick.RemoveListener(HandleWoodButtonClicked);
            }
        }

        /// <summary>
        /// 選択を初期状態(金槌)へ戻す。
        /// </summary>
        public void ResetSelection()
        {
            Select(metalHammer);
        }

        /// <summary>
        /// 金槌ボタンが押された時の処理。
        /// </summary>
        private void HandleMetalButtonClicked()
        {
            Select(metalHammer);
        }

        /// <summary>
        /// 木槌ボタンが押された時の処理。
        /// </summary>
        private void HandleWoodButtonClicked()
        {
            Select(woodHammer);
        }

        /// <summary>
        /// ハンマーを選択し、Setup 画面とボタンの見た目へ反映する。
        /// </summary>
        /// <param name="hammer">選択するハンマー。</param>
        private void Select(HammerDefinition hammer)
        {
            SelectedHammer = hammer;
            poseEditor.ApplyHammerVisual(hammer);

            bool isMetalSelected = hammer == metalHammer;
            metalOnImage.gameObject.SetActive(isMetalSelected);
            metalOffImage.gameObject.SetActive(!isMetalSelected);
            woodOnImage.gameObject.SetActive(!isMetalSelected);
            woodOffImage.gameObject.SetActive(isMetalSelected);
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(metalButton, nameof(metalButton), this);
            isValid &= RequiredReference.IsAssigned(woodButton, nameof(woodButton), this);
            isValid &= RequiredReference.IsAssigned(metalOnImage, nameof(metalOnImage), this);
            isValid &= RequiredReference.IsAssigned(metalOffImage, nameof(metalOffImage), this);
            isValid &= RequiredReference.IsAssigned(woodOnImage, nameof(woodOnImage), this);
            isValid &= RequiredReference.IsAssigned(woodOffImage, nameof(woodOffImage), this);
            isValid &= RequiredReference.IsAssigned(metalHammer, nameof(metalHammer), this);
            isValid &= RequiredReference.IsAssigned(woodHammer, nameof(woodHammer), this);
            isValid &= RequiredReference.IsAssigned(poseEditor, nameof(poseEditor), this);
            return isValid;
        }
    }
}

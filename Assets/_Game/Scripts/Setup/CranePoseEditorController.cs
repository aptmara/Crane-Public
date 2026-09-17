using Crane.Common;
using Crane.Data;
using Crane.Hammer;
using Crane.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Crane.Setup
{
    /// <summary>
    /// Setup 画面で、クレーンの初期姿勢をドラッグ操作で決める。
    /// </summary>
    /// <remarks>
    /// 役割は「入力を <see cref="CranePoseModel"/> へ渡す」と「モデルの姿勢を UI へ反映する」の 2 つだけで、
    /// 操作ルールはモデル側が持つ。
    ///
    /// <c>[ExecuteAlways]</c> は、<see cref="CranePoseEditorData"/> を Inspector で編集した時に
    /// Edit Mode 中もシーンビューへ即座に反映するために付けている。
    /// ドラッグ操作とブーム回転は UI の EventSystem を使うため Play Mode 中だけ動く。
    /// 初期配置を変える時はデータアセットを直接編集する。
    /// </remarks>
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class CranePoseEditorController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        /// <summary>初期配置・操作範囲・見た目のデータ。</summary>
        [SerializeField] private CranePoseEditorData data;

        /// <summary>ブームの画像。</summary>
        [Header("Parts")]
        [SerializeField] private RectTransform boomSprite;

        /// <summary>ブームの操作判定の可視化。</summary>
        [SerializeField] private RectTransform boomHandle;

        /// <summary>ブームの回転軸の表示。</summary>
        [SerializeField] private RectTransform boomPivot;

        /// <summary>第一リンクの画像。</summary>
        [SerializeField] private RectTransform upperLinkSprite;

        /// <summary>第一リンクに重ねる画像。第一リンクと一緒に回転する。</summary>
        [SerializeField] private RectTransform upperLinkOverlay;

        /// <summary>第一リンク先端の操作判定の可視化。</summary>
        [SerializeField] private RectTransform upperLinkHandle;

        /// <summary>第一リンクの根元の表示。</summary>
        [SerializeField] private RectTransform upperLinkPivot;

        /// <summary>第二リンクの画像。</summary>
        [SerializeField] private RectTransform lowerLinkSprite;

        /// <summary>第二リンク先端の操作判定の可視化。</summary>
        [SerializeField] private RectTransform lowerLinkHandle;

        /// <summary>第二リンクの根元の表示。</summary>
        [SerializeField] private RectTransform lowerLinkPivot;

        /// <summary>ハンマーの画像。</summary>
        [SerializeField] private RectTransform hammerSprite;

        /// <summary>ハンマーの操作判定。</summary>
        [SerializeField] private RectTransform hammerHandle;

        /// <summary>鐘の画像。</summary>
        [SerializeField] private RectTransform bellSprite;

        /// <summary>姿勢と操作ルール。データ未設定の間は null。</summary>
        private CranePoseModel model;

        /// <summary>このコンポーネントの RectTransform。ポインター座標の変換に使う。</summary>
        private RectTransform editorRect;

        /// <summary>ドラッグ中の部位。</summary>
        private CranePosePart activePart;

        /// <summary>ブームをドラッグ中の最新のポインター位置。</summary>
        private Vector2 lastPointerPosition;

        /// <summary>第一リンクの画像に対する重ね画像の回転差。初期化時の配置から求める。</summary>
        private Quaternion upperLinkOverlayRotationOffset = Quaternion.identity;

        /// <summary>
        /// 有効化時に、未初期化ならデータから初期化し、初期化済みなら表示だけ更新する。
        /// </summary>
        /// <remarks>
        /// Play Mode 中に一時的に非表示になっても、再表示で姿勢が初期値へ戻らないようにしている。
        /// </remarks>
        private void OnEnable()
        {
            editorRect = GetComponent<RectTransform>();

            if (!Application.isPlaying || model == null)
            {
                InitializeFromData();
                return;
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Inspector で値が変わった時に、データから初期化し直してプレビューへ反映する。
        /// </summary>
        private void OnValidate()
        {
            editorRect = GetComponent<RectTransform>();
            InitializeFromData();
        }

        /// <summary>
        /// コンポーネント追加時とコンテキストメニューから、子階層の部位を名前で探して設定する。
        /// </summary>
        [ContextMenu("Assign Parts From Hierarchy")]
        private void Reset()
        {
            boomSprite = FindChildRect("Boom/Sprite");
            boomHandle = FindChildRect("Boom/Handle");
            boomPivot = FindChildRect("Boom/Pivot");
            upperLinkSprite = FindChildRect("UpperLink/Sprite");
            upperLinkOverlay = FindChildRect("UpperLink/Handle/Image");
            upperLinkHandle = FindChildRect("UpperLink/Handle");
            upperLinkPivot = FindChildRect("UpperLink/Pivot");
            lowerLinkSprite = FindChildRect("LowerLink/Sprite");
            lowerLinkHandle = FindChildRect("LowerLink/Handle");
            lowerLinkPivot = FindChildRect("LowerLink/Pivot");
            hammerSprite = FindChildRect("Hammer/Sprite");
            hammerHandle = FindChildRect("Hammer/Handle");
            bellSprite = FindChildRect("Bell/Sprite");
        }

        /// <summary>
        /// ブームをドラッグ中なら、ポインターのある側へ回転させる。
        /// </summary>
        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (model == null || activePart != CranePosePart.Boom)
            {
                return;
            }

            model.RotateBoomToward(lastPointerPosition.x, Time.unscaledDeltaTime);
            UpdateVisuals();
        }

        /// <summary>
        /// 選択したハンマーの画像を Setup 画面へ反映する。
        /// </summary>
        /// <param name="hammer">選択したハンマー。</param>
        public void ApplyHammerVisual(HammerDefinition hammer)
        {
            if (hammer.Sprite == null)
            {
                return;
            }

            Image image = hammerSprite.GetComponent<Image>();
            image.sprite = hammer.Sprite;
        }

        /// <summary>
        /// 現在の姿勢を初期条件として確定する。
        /// </summary>
        /// <param name="hammer">選択中のハンマー。</param>
        /// <returns>物理シミュレーションへ渡す初期姿勢。</returns>
        public CraneSetupSnapshot BuildSnapshot(HammerDefinition hammer)
        {
            return model.CreateSnapshot(hammer);
        }

        /// <summary>
        /// ポインターを押した位置の部位を操作対象にする。
        /// </summary>
        /// <param name="eventData">ポインターのイベント情報。</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 pointerPosition;
            if (!TryGetLocalPointer(eventData, out pointerPosition))
            {
                return;
            }

            activePart = model.FindPartAt(pointerPosition);
            ApplyPointer(pointerPosition);
        }

        /// <summary>
        /// ドラッグ中の部位をポインターに追従させる。
        /// </summary>
        /// <param name="eventData">ポインターのイベント情報。</param>
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 pointerPosition;
            if (!TryGetLocalPointer(eventData, out pointerPosition))
            {
                return;
            }

            ApplyPointer(pointerPosition);
        }

        /// <summary>
        /// ポインターを離したら操作を終える。
        /// </summary>
        /// <param name="eventData">ポインターのイベント情報。</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            activePart = CranePosePart.None;
        }

        /// <summary>
        /// スクリーン座標のポインター位置を、エディタ内のローカル座標へ変換する。
        /// </summary>
        /// <param name="eventData">ポインターのイベント情報。</param>
        /// <param name="pointerPosition">変換後のローカル座標。</param>
        /// <returns>変換でき、かつエディタの範囲内なら true。</returns>
        private bool TryGetLocalPointer(PointerEventData eventData, out Vector2 pointerPosition)
        {
            bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                editorRect,
                eventData.position,
                eventData.pressEventCamera,
                out pointerPosition);
            if (!isConverted)
            {
                return false;
            }

            return editorRect.rect.Contains(pointerPosition);
        }

        /// <summary>
        /// 操作中の部位に応じてポインター位置をモデルへ渡し、表示を更新する。
        /// </summary>
        /// <param name="pointerPosition">ローカル座標のポインター位置。</param>
        private void ApplyPointer(Vector2 pointerPosition)
        {
            switch (activePart)
            {
                case CranePosePart.Boom:
                    lastPointerPosition = pointerPosition;
                    break;

                case CranePosePart.UpperLink:
                    model.MoveUpperLinkTip(pointerPosition);
                    break;

                case CranePosePart.LowerLink:
                    model.MoveLowerLinkTip(pointerPosition);
                    break;

                case CranePosePart.None:
                default:
                    break;
            }

            UpdateVisuals();
        }

        /// <summary>
        /// データから姿勢を作り直し、色と表示を反映する。
        /// </summary>
        private void InitializeFromData()
        {
            if (data == null || !HasAllParts())
            {
                model = null;
                return;
            }

            model = new CranePoseModel(CranePoseSettings.FromData(data));
            activePart = CranePosePart.None;
            lastPointerPosition = model.BoomPivot;
            upperLinkOverlayRotationOffset = Quaternion.Inverse(upperLinkSprite.localRotation) * upperLinkOverlay.localRotation;

            ApplyColors();
            UpdateVisuals();
        }

        /// <summary>
        /// データの色を各部位へ反映し、部位の画像がポインター入力を奪わないようにする。
        /// </summary>
        /// <remarks>
        /// 入力はこのコンポーネントの Image(エディタ背景)だけで受け、部位の判定はモデルが計算する。
        /// </remarks>
        private void ApplyColors()
        {
            Image editorImage = GetComponent<Image>();
            editorImage.color = data.BackgroundColor;
            editorImage.raycastTarget = true;

            SetImageColor(boomSprite, data.BoomColor);
            SetImageColor(upperLinkSprite, data.LinkColor);
            SetImageColor(lowerLinkSprite, data.LinkColor);
            SetImageColor(hammerSprite, data.HammerColor);
            SetImageColor(boomHandle, data.HandleColor);
            SetImageColor(upperLinkHandle, data.HandleColor);
            SetImageColor(lowerLinkHandle, data.HandleColor);
            SetImageColor(hammerHandle, Color.clear);
            SetImageColor(boomPivot, data.PivotColor);
            SetImageColor(upperLinkPivot, data.PivotColor);
            SetImageColor(lowerLinkPivot, data.PivotColor);
            SetImageColor(bellSprite, data.BellColor);
        }

        /// <summary>
        /// モデルの姿勢を各部位の RectTransform へ反映する。
        /// </summary>
        private void UpdateVisuals()
        {
            if (model == null)
            {
                return;
            }

            RectTransformLayout.PlaceBar(boomSprite, model.BoomPivot, model.UpperLinkPivot, data.BoomThickness);
            RectTransformLayout.PlaceBar(upperLinkSprite, model.UpperLinkPivot, model.LowerLinkPivot, data.LinkThickness);
            RectTransformLayout.PlaceBar(lowerLinkSprite, model.LowerLinkPivot, model.HammerPosition, data.LinkThickness);
            upperLinkOverlay.localRotation = upperLinkSprite.localRotation * upperLinkOverlayRotationOffset;

            RectTransformLayout.PlaceBar(boomHandle, model.BoomPivot, model.UpperLinkPivot, data.BoomThickness + data.BoomHitPadding);
            RectTransformLayout.PlaceCentered(upperLinkHandle, model.LowerLinkPivot, data.UpperLinkHandleSize);
            RectTransformLayout.PlaceCentered(lowerLinkHandle, model.HammerPosition, data.LowerLinkHandleSize);

            float hammerAngle = model.LowerLinkAngleDeg + data.HammerRotationOffset;
            RectTransformLayout.PlaceCentered(hammerSprite, model.HammerPosition, data.HammerSize, hammerAngle);
            RectTransformLayout.PlaceCentered(hammerHandle, model.HammerPosition, data.LowerLinkHandleSize, hammerAngle);

            Vector2 pivotSize = Vector2.one * data.PivotSize;
            RectTransformLayout.PlaceCentered(boomPivot, model.BoomPivot, pivotSize, model.BoomAngleDeg);
            RectTransformLayout.PlaceCentered(upperLinkPivot, model.UpperLinkPivot, pivotSize, model.UpperLinkAngleDeg);
            RectTransformLayout.PlaceCentered(lowerLinkPivot, model.LowerLinkPivot, pivotSize, model.LowerLinkAngleDeg);

            RectTransformLayout.PlaceCentered(bellSprite, data.BellPosition, data.BellSize);
        }

        /// <summary>
        /// すべての部位の参照が設定されているかを確認する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        /// <remarks>
        /// Edit Mode ではコンポーネント追加直後など未設定の瞬間があるため、Play Mode 中だけエラーを出す。
        /// </remarks>
        private bool HasAllParts()
        {
            RectTransform[] parts =
            {
                boomSprite, boomHandle, boomPivot,
                upperLinkSprite, upperLinkOverlay, upperLinkHandle, upperLinkPivot,
                lowerLinkSprite, lowerLinkHandle, lowerLinkPivot,
                hammerSprite, hammerHandle, bellSprite,
            };

            foreach (RectTransform part in parts)
            {
                if (part != null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    CraneLog.Error(nameof(CranePoseEditorController), "部位の参照が不足しています。コンテキストメニューの Assign Parts From Hierarchy で設定してください。", this);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 子階層から RectTransform をパスで探す。
        /// </summary>
        /// <param name="path">このオブジェクトからの相対パス。</param>
        /// <returns>見つかった RectTransform。なければ null。</returns>
        private RectTransform FindChildRect(string path)
        {
            Transform child = transform.Find(path);
            if (child == null)
            {
                return null;
            }

            return child.GetComponent<RectTransform>();
        }

        /// <summary>
        /// 部位の Image の色を設定し、ポインター入力を受けないようにする。
        /// </summary>
        /// <param name="target">部位の RectTransform。</param>
        /// <param name="color">設定する色。</param>
        private static void SetImageColor(RectTransform target, Color color)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            image.color = color;
            image.raycastTarget = false;
        }
    }
}

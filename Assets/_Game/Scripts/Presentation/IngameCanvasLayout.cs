using Crane.Common;
using Crane.Data;
using Crane.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Crane.Presentation
{
    /// <summary>
    /// Setup 用 Canvas を、Ingame では物理ワールドと重ねて描画できるように切り替える。
    /// </summary>
    /// <remarks>
    /// 原作では Setup 画面の絵(背景・ブーム・前景)が Ingame でもそのまま拡大表示され、
    /// その手前と奥に物理ワールドの振り子や鐘が描かれる。これを再現するため、Ingame 中は次の処理を行う。
    /// <list type="number">
    /// <item><description>Canvas を Screen Space - Camera にして、ワールドと同じカメラで描画する。</description></item>
    /// <item><description>一部の UI に子 Canvas を持たせ、描画順を <see cref="SortingOrders"/> の値で上書きする(反映は <see cref="CanvasSortingApplier"/> が表示中に行う)。</description></item>
    /// <item><description>Setup 画像を <see cref="IngameLayout"/> に従って拡大・移動する。</description></item>
    /// </list>
    /// Setup へ戻る時は、起動時に記録した設定へすべて戻す。
    /// </remarks>
    public class IngameCanvasLayout : MonoBehaviour
    {
        /// <summary>Ingame 時に Canvas を描画するカメラからの距離。ワールドの奥行き範囲より奥に置く。</summary>
        private const float IngameCanvasPlaneDistance = 100f;

        /// <summary>状態の変化を通知する進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>Setup 画面の Canvas。</summary>
        [SerializeField] private Canvas setupCanvas;

        /// <summary>物理ワールドを映すカメラ。</summary>
        [SerializeField] private Camera gameplayCamera;

        /// <summary>Ingame で拡大・移動する Setup 画像(背景・ブーム・前景)。</summary>
        [SerializeField] private GameObject[] scaledSetupImages = new GameObject[0];

        /// <summary>Ingame で描画順を上書きする UI。</summary>
        [SerializeField] private CanvasSortingOverride[] sortingOverrides = new CanvasSortingOverride[0];

        /// <summary>ゲームプレイ用カメラの画面揺れ。カメラに付いていなければ null。</summary>
        private CameraShaker cameraShaker;

        /// <summary>画面揺れで絵をずらす量(ワールド単位)。</summary>
        private Vector2 shakeImageOffset;

        /// <summary>各 UI の描画順の適用役。<see cref="sortingOverrides"/> と同じ並び。</summary>
        private CanvasSortingApplier[] sortingAppliers;

        /// <summary>Setup Canvas の RectTransform。サイズの変化を監視する。</summary>
        private RectTransform setupCanvasRect;

        /// <summary>起動時の Canvas 設定。Setup へ戻る時に復元する。</summary>
        private RenderMode initialRenderMode;

        /// <summary>起動時の Canvas のカメラ。</summary>
        private Camera initialWorldCamera;

        /// <summary>起動時の Canvas の平面距離。</summary>
        private float initialPlaneDistance;

        /// <summary>起動時の Canvas の描画順上書き設定。</summary>
        private bool initialOverrideSorting;

        /// <summary>起動時の Canvas の描画順。</summary>
        private int initialSortingOrder;

        /// <summary><see cref="scaledSetupImages"/> の RectTransform。</summary>
        private RectTransform[] scaledRects;

        /// <summary>各 Setup 画像の起動時の anchoredPosition。</summary>
        private Vector2[] initialAnchoredPositions;

        /// <summary>各 Setup 画像の起動時の localScale。</summary>
        private Vector3[] initialLocalScales;

        /// <summary>Ingame 用のレイアウトを適用中なら true。</summary>
        private bool isIngameLayoutActive;

        /// <summary>最後にレイアウトを計算した時の Canvas サイズ。</summary>
        private Vector2 lastAppliedCanvasSize;

        /// <summary>
        /// 必須参照を検証し、上書き用の子 Canvas を用意して、起動時の設定を記録する。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            setupCanvasRect = setupCanvas.GetComponent<RectTransform>();
            cameraShaker = gameplayCamera.GetComponent<CameraShaker>();
            RecordInitialCanvasSettings();
            PrepareSortingCanvases();
            RecordInitialImageLayout();
        }

        /// <summary>
        /// 状態の変化と画面揺れを購読する。
        /// </summary>
        private void OnEnable()
        {
            gameController.StateChanged += HandleStateChanged;
            if (cameraShaker != null)
            {
                cameraShaker.ImageOffsetChanged += HandleShakeImageOffsetChanged;
            }
        }

        /// <summary>
        /// 状態の変化と画面揺れの購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            gameController.StateChanged -= HandleStateChanged;
            if (cameraShaker != null)
            {
                cameraShaker.ImageOffsetChanged -= HandleShakeImageOffsetChanged;
            }
        }

        /// <summary>
        /// 起動時の状態を反映する。
        /// </summary>
        private void Start()
        {
            ApplyMode(gameController.CurrentState);
        }

        /// <summary>
        /// ウィンドウサイズが変わった時に、Ingame レイアウトを計算し直す。
        /// </summary>
        private void Update()
        {
            if (!isIngameLayoutActive)
            {
                return;
            }

            Vector2 currentCanvasSize = setupCanvasRect.rect.size;
            if (currentCanvasSize == lastAppliedCanvasSize)
            {
                return;
            }

            ApplyIngameImageLayout(currentCanvasSize);
        }

        /// <summary>
        /// 状態が変わったら Canvas の描画方式を切り替える。
        /// </summary>
        /// <param name="previous">変更前の状態。</param>
        /// <param name="next">変更後の状態。</param>
        private void HandleStateChanged(GameState previous, GameState next)
        {
            ApplyMode(next);
        }

        /// <summary>
        /// 画面揺れに合わせて、Setup 画像をワールドの絵と同じ量だけずらす。
        /// </summary>
        /// <param name="imageOffset">画面上で絵をずらす量(ワールド単位)。</param>
        /// <remarks>
        /// Ingame 中の Canvas はカメラに追従するため、カメラを動かす画面揺れでは揺れない。
        /// ずらさないと、ブーム(Canvas)と振り子(ワールド)の付け根が離れて見える。
        /// </remarks>
        private void HandleShakeImageOffsetChanged(Vector2 imageOffset)
        {
            shakeImageOffset = imageOffset;
            if (isIngameLayoutActive)
            {
                ApplyIngameImageLayout(lastAppliedCanvasSize);
            }
        }

        /// <summary>
        /// 状態に応じて Setup 用と Ingame 用の描画方式を切り替える。
        /// </summary>
        /// <param name="state">現在の状態。</param>
        private void ApplyMode(GameState state)
        {
            if (state == GameState.Setup)
            {
                ApplySetupMode();
            }
            else
            {
                ApplyIngameMode();
            }
        }

        /// <summary>
        /// 起動時の Canvas 設定と画像配置へ戻す。
        /// </summary>
        private void ApplySetupMode()
        {
            isIngameLayoutActive = false;

            setupCanvas.renderMode = initialRenderMode;
            setupCanvas.worldCamera = initialWorldCamera;
            setupCanvas.planeDistance = initialPlaneDistance;
            setupCanvas.overrideSorting = initialOverrideSorting;
            setupCanvas.sortingOrder = initialSortingOrder;

            foreach (CanvasSortingApplier applier in sortingAppliers)
            {
                applier.SetIngame(false);
            }

            for (int i = 0; i < scaledRects.Length; i++)
            {
                scaledRects[i].anchoredPosition = initialAnchoredPositions[i];
                scaledRects[i].localScale = initialLocalScales[i];
            }
        }

        /// <summary>
        /// Canvas をカメラ描画にして描画順を上書きし、Setup 画像を Ingame 用に配置する。
        /// </summary>
        private void ApplyIngameMode()
        {
            setupCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            setupCanvas.worldCamera = gameplayCamera;
            setupCanvas.planeDistance = IngameCanvasPlaneDistance;
            setupCanvas.overrideSorting = true;
            setupCanvas.sortingOrder = SortingOrders.IngameSetupCanvas;

            foreach (CanvasSortingApplier applier in sortingAppliers)
            {
                applier.SetIngame(true);
            }

            isIngameLayoutActive = true;
            ApplyIngameImageLayout(setupCanvasRect.rect.size);
        }

        /// <summary>
        /// Setup 画像を <see cref="IngameLayout"/> に従って拡大・移動する。
        /// </summary>
        /// <param name="canvasSize">現在の Canvas サイズ。</param>
        private void ApplyIngameImageLayout(Vector2 canvasSize)
        {
            lastAppliedCanvasSize = canvasSize;

            Vector2 offset = IngameLayout.GetCanvasOffset(canvasSize) + ConvertWorldOffsetToCanvas(shakeImageOffset);
            Vector3 scale = IngameLayout.GetScale();
            for (int i = 0; i < scaledRects.Length; i++)
            {
                scaledRects[i].anchoredPosition = initialAnchoredPositions[i] + offset;
                scaledRects[i].localScale = Vector3.Scale(initialLocalScales[i], scale);
            }
        }

        /// <summary>
        /// ワールド単位のずれを、Canvas 上のずれへ換算する。
        /// </summary>
        /// <param name="worldOffset">ワールド単位のずれ。</param>
        /// <returns>Canvas 直下の anchoredPosition に加えるずれ。</returns>
        private Vector2 ConvertWorldOffsetToCanvas(Vector2 worldOffset)
        {
            Vector3 canvasScale = setupCanvas.transform.lossyScale;
            if (Mathf.Approximately(canvasScale.x, 0f) || Mathf.Approximately(canvasScale.y, 0f))
            {
                return Vector2.zero;
            }

            return new Vector2(worldOffset.x / canvasScale.x, worldOffset.y / canvasScale.y);
        }

        /// <summary>
        /// Canvas の起動時の設定を記録する。
        /// </summary>
        private void RecordInitialCanvasSettings()
        {
            initialRenderMode = setupCanvas.renderMode;
            initialWorldCamera = setupCanvas.worldCamera;
            initialPlaneDistance = setupCanvas.planeDistance;
            initialOverrideSorting = setupCanvas.overrideSorting;
            initialSortingOrder = setupCanvas.sortingOrder;
        }

        /// <summary>
        /// 描画順を上書きする UI に、子 Canvas・クリック用の GraphicRaycaster・適用役を用意する。
        /// </summary>
        /// <remarks>
        /// Canvas を追加すると Transform が RectTransform に置き換わる。
        /// 置き換え前の Transform を保持していると参照が切れるため、
        /// 画像配置を記録する(<see cref="RecordInitialImageLayout"/>)より前に行う。
        /// 追加直後は描画順を上書きしないため、Setup 画面の見た目は変わらない。
        /// </remarks>
        private void PrepareSortingCanvases()
        {
            sortingAppliers = new CanvasSortingApplier[sortingOverrides.Length];

            for (int i = 0; i < sortingOverrides.Length; i++)
            {
                CanvasSortingOverride sortingOverride = sortingOverrides[i];
                GameObject target = sortingOverride.Target;
                if (target.GetComponent<Canvas>() == null)
                {
                    target.AddComponent<Canvas>();
                }

                GraphicRaycaster raycaster = null;
                if (sortingOverride.ReceivesPointerInput)
                {
                    raycaster = target.GetComponent<GraphicRaycaster>();
                    if (raycaster == null)
                    {
                        raycaster = target.AddComponent<GraphicRaycaster>();
                    }
                }

                CanvasSortingApplier applier = target.GetComponent<CanvasSortingApplier>();
                if (applier == null)
                {
                    applier = target.AddComponent<CanvasSortingApplier>();
                }

                applier.Initialize(SortingOrders.GetCanvasOrder(sortingOverride.Slot), raycaster);
                sortingAppliers[i] = applier;
            }
        }

        /// <summary>
        /// 拡大・移動する Setup 画像の起動時の配置を記録する。
        /// </summary>
        private void RecordInitialImageLayout()
        {
            scaledRects = new RectTransform[scaledSetupImages.Length];
            initialAnchoredPositions = new Vector2[scaledSetupImages.Length];
            initialLocalScales = new Vector3[scaledSetupImages.Length];

            for (int i = 0; i < scaledSetupImages.Length; i++)
            {
                GameObject image = scaledSetupImages[i];
                if (image.GetComponent<RectTransform>() == null)
                {
                    image.AddComponent<RectTransform>();
                }

                RectTransform rect = image.GetComponent<RectTransform>();
                scaledRects[i] = rect;
                initialAnchoredPositions[i] = rect.anchoredPosition;
                initialLocalScales[i] = rect.localScale;
            }
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(gameController, nameof(gameController), this);
            isValid &= RequiredReference.IsAssigned(setupCanvas, nameof(setupCanvas), this);
            isValid &= RequiredReference.IsAssigned(gameplayCamera, nameof(gameplayCamera), this);

            for (int i = 0; i < scaledSetupImages.Length; i++)
            {
                isValid &= RequiredReference.IsAssigned(scaledSetupImages[i], nameof(scaledSetupImages) + "[" + i + "]", this);
            }

            for (int i = 0; i < sortingOverrides.Length; i++)
            {
                isValid &= RequiredReference.IsAssigned(sortingOverrides[i].Target, nameof(sortingOverrides) + "[" + i + "]", this);
            }

            return isValid;
        }
    }
}

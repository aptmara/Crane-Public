using Crane.Common;
using Crane.Data;
using Crane.Hammer;
using Crane.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// 物理リグに、Setup 画面と同じ見た目の SpriteRenderer を付ける。
    /// </summary>
    /// <remarks>
    /// Setup 画面の UI Image(画像・色・大きさ・ピボット)を写し取ることで、
    /// PLAY を押した瞬間に Setup の絵と物理リグの絵が入れ替わっても見た目が変わらないようにしている。
    /// UI への依存はこのクラスに閉じ込め、物理構造(<see cref="CraneRigBuilder"/>)は UI を知らない。
    /// 写し取り元の Image が未設定の部位は、<see cref="CranePoseEditorData"/> の色と大きさで代替表示する。
    /// </remarks>
    public class CraneRigVisualBuilder : MonoBehaviour
    {
        /// <summary>ハンマーの残像が消えるまでの時間(秒)。</summary>
        private const float HammerTrailDurationSeconds = 0.5f;

        /// <summary>代替表示に使う色と大きさ。</summary>
        [SerializeField] private CranePoseEditorData poseEditorData;

        /// <summary>第一リンクの写し取り元。</summary>
        [Header("Setup image sources (optional)")]
        [SerializeField] private Image upperLinkVisualSource;

        /// <summary>第一リンクに重ねる画像の写し取り元。</summary>
        [SerializeField] private Image upperLinkOverlaySource;

        /// <summary>第一リンクの関節の写し取り元。</summary>
        [SerializeField] private Image upperLinkPivotSource;

        /// <summary>第二リンクの写し取り元。</summary>
        [SerializeField] private Image lowerLinkVisualSource;

        /// <summary>第二リンクの関節の写し取り元。</summary>
        [SerializeField] private Image lowerLinkPivotSource;

        /// <summary>ハンマーの写し取り元。</summary>
        [SerializeField] private Image hammerVisualSource;

        /// <summary>
        /// 必須参照を検証する。
        /// </summary>
        private void Awake()
        {
            if (!RequiredReference.IsAssigned(poseEditorData, nameof(poseEditorData), this))
            {
                enabled = false;
            }
        }

        /// <summary>
        /// リグのリンク・関節・ハンマーに見た目を付ける。
        /// </summary>
        /// <param name="rig">見た目を付けるリグ。</param>
        /// <param name="setup">リンクの長さとサイズ変換に使う初期条件。</param>
        /// <returns>ハンマーの SpriteRenderer。当たり判定の大きさに使う。</returns>
        public SpriteRenderer Decorate(CraneRig rig, CranePhysicsSetup setup)
        {
            Transform upperLink = rig.UpperLinkBody.transform;
            Transform lowerLink = rig.LowerLinkBody.transform;
            Vector2 canvasUnit = setup.CanvasUnitWorldSize;

            CreateLinkVisual(upperLink, setup.UpperLinkLength, canvasUnit.y, upperLinkVisualSource, SortingOrders.UpperLink);
            CreateLinkVisual(lowerLink, setup.LowerLinkLength, canvasUnit.y, lowerLinkVisualSource, SortingOrders.LowerLink);

            CreateCopiedVisual(upperLink, "Overlay", upperLinkOverlaySource, upperLinkVisualSource, canvasUnit, SortingOrders.UpperLinkOverlay);
            CreateCopiedVisual(upperLink, "Pivot", upperLinkPivotSource, null, canvasUnit, SortingOrders.UpperLinkPivot);
            CreateCopiedVisual(lowerLink, "Pivot", lowerLinkPivotSource, null, canvasUnit, SortingOrders.LowerLinkPivot);

            SpriteRenderer hammerRenderer = DecorateHammer(rig.HammerTransform, setup.LowerLinkLength, setup.Hammer, canvasUnit);
            CreateHammerTrail(rig.transform, hammerRenderer);
            return hammerRenderer;
        }

        /// <summary>
        /// リンクの根元から先端へ伸びる棒の見た目を作る。
        /// </summary>
        /// <param name="link">対象のリンク。</param>
        /// <param name="length">リンクの長さ(ワールド単位)。</param>
        /// <param name="canvasUnitWorldHeight">Canvas の 1px のワールドでの高さ。</param>
        /// <param name="source">写し取り元。null なら代替表示。</param>
        /// <param name="sortingOrder">描画順。</param>
        private void CreateLinkVisual(Transform link, float length, float canvasUnitWorldHeight, Image source, int sortingOrder)
        {
            GameObject visual = new GameObject("Sprite");
            visual.transform.SetParent(link, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;

            if (source == null || source.sprite == null)
            {
                renderer.sprite = SpriteFitter.GetLeftPivotWhiteSprite();
                renderer.color = poseEditorData.LinkColor;
                Vector2 fallbackSize = new Vector2(length, poseEditorData.LinkThickness * canvasUnitWorldHeight);
                SpriteFitter.Fit(visual.transform, renderer.sprite, fallbackSize, new Vector2(0f, 0.5f));
                return;
            }

            RectTransform sourceRect = source.rectTransform;
            renderer.sprite = source.sprite;
            renderer.color = source.color;
            Vector2 size = new Vector2(
                length * Mathf.Abs(sourceRect.localScale.x),
                sourceRect.rect.height * Mathf.Abs(sourceRect.localScale.y) * canvasUnitWorldHeight);
            SpriteFitter.Fit(visual.transform, renderer.sprite, size, sourceRect.pivot);
        }

        /// <summary>
        /// Setup 画面の Image を写し取った見た目を、リンクに固定して作る。
        /// </summary>
        /// <param name="parent">見た目を固定するリンク。</param>
        /// <param name="name">オブジェクト名。</param>
        /// <param name="source">写し取り元。null なら何も作らない。</param>
        /// <param name="coordinateFrame">
        /// 位置と回転の基準にする Setup 側の Image。
        /// null なら写し取り元をリンクの原点に置き、自身の回転と拡大率をそのまま使う。
        /// </param>
        /// <param name="canvasUnitWorldSize">Canvas の 1px のワールドでの大きさ。</param>
        /// <param name="sortingOrder">描画順。</param>
        private static void CreateCopiedVisual(
            Transform parent,
            string name,
            Image source,
            Image coordinateFrame,
            Vector2 canvasUnitWorldSize,
            int sortingOrder)
        {
            if (source == null)
            {
                return;
            }

            RectTransform sourceRect = source.rectTransform;
            Vector2 localPosition = Vector2.zero;
            Vector2 scale = new Vector2(Mathf.Abs(sourceRect.localScale.x), Mathf.Abs(sourceRect.localScale.y));
            float rotationDeg = sourceRect.localEulerAngles.z;

            if (coordinateFrame != null)
            {
                RectTransform frameRect = coordinateFrame.rectTransform;
                Transform frameParent = frameRect.parent;

                Vector3 sourceInFrameParent = frameParent.InverseTransformPoint(sourceRect.position);
                Vector3 frameInFrameParent = frameParent.InverseTransformPoint(frameRect.position);
                localPosition = Quaternion.Inverse(frameRect.localRotation) * (sourceInFrameParent - frameInFrameParent);

                Vector3 frameParentScale = frameParent.lossyScale;
                Vector3 sourceWorldScale = sourceRect.lossyScale;
                scale = new Vector2(
                    Mathf.Abs(sourceWorldScale.x / frameParentScale.x),
                    Mathf.Abs(sourceWorldScale.y / frameParentScale.y));

                rotationDeg = (Quaternion.Inverse(frameRect.rotation) * sourceRect.rotation).eulerAngles.z;
            }

            GameObject visual = new GameObject(name);
            visual.transform.SetParent(parent, false);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            if (source.sprite == null)
            {
                renderer.sprite = SpriteFitter.GetCenteredWhiteSprite();
            }
            else
            {
                renderer.sprite = source.sprite;
            }

            renderer.color = source.color;
            renderer.sortingOrder = sortingOrder;

            Vector2 worldSize = Vector2.Scale(Vector2.Scale(sourceRect.rect.size, scale), canvasUnitWorldSize);
            SpriteFitter.Fit(visual.transform, renderer.sprite, worldSize, sourceRect.pivot);

            Vector3 worldOffset = new Vector3(
                localPosition.x * canvasUnitWorldSize.x,
                localPosition.y * canvasUnitWorldSize.y,
                0f);
            visual.transform.localPosition += worldOffset;
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, rotationDeg);
        }

        /// <summary>
        /// ハンマーの見た目を付ける。
        /// </summary>
        /// <param name="hammer">ハンマーのオブジェクト(第二リンクの子)。</param>
        /// <param name="lowerLinkLength">第二リンクの長さ。ハンマーは第二リンクの先端に置く。</param>
        /// <param name="definition">ハンマーの定義。画像を使う。</param>
        /// <param name="canvasUnitWorldSize">Canvas の 1px のワールドでの大きさ。</param>
        /// <returns>ハンマーの SpriteRenderer。</returns>
        private SpriteRenderer DecorateHammer(
            Transform hammer,
            float lowerLinkLength,
            HammerDefinition definition,
            Vector2 canvasUnitWorldSize)
        {
            Vector2 size = Vector2.Scale(poseEditorData.HammerSize, canvasUnitWorldSize);
            Color color = poseEditorData.HammerColor;
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            if (hammerVisualSource != null)
            {
                RectTransform sourceRect = hammerVisualSource.rectTransform;
                Vector2 sourceScale = new Vector2(Mathf.Abs(sourceRect.localScale.x), Mathf.Abs(sourceRect.localScale.y));
                size = Vector2.Scale(Vector2.Scale(sourceRect.rect.size, sourceScale), canvasUnitWorldSize);
                color = hammerVisualSource.color;
                pivot = sourceRect.pivot;
            }

            SpriteRenderer renderer = hammer.gameObject.AddComponent<SpriteRenderer>();
            if (definition.Sprite == null)
            {
                renderer.sprite = SpriteFitter.GetCenteredWhiteSprite();
            }
            else
            {
                renderer.sprite = definition.Sprite;
            }

            renderer.color = color;
            renderer.sortingOrder = SortingOrders.Hammer;

            hammer.localRotation = Quaternion.Euler(0f, 0f, poseEditorData.HammerRotationOffset);
            SpriteFitter.Fit(hammer, renderer.sprite, size, pivot);
            hammer.localPosition += new Vector3(lowerLinkLength, 0f, 0f);
            return renderer;
        }

        /// <summary>
        /// ハンマーの残像を作る。
        /// </summary>
        /// <param name="rigRoot">残像を置くリグのルート。</param>
        /// <param name="hammerRenderer">残像の元にするハンマーの見た目。</param>
        private static void CreateHammerTrail(Transform rigRoot, SpriteRenderer hammerRenderer)
        {
            GameObject trailObject = new GameObject("HammerTrail");
            trailObject.transform.SetParent(rigRoot, false);
            HammerAfterimageTrail trail = trailObject.AddComponent<HammerAfterimageTrail>();
            trail.Initialize(hammerRenderer, HammerTrailDurationSeconds);
        }
    }
}

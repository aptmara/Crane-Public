using Crane.Common;
using Crane.Data;
using Crane.PhysicsSim;
using Crane.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Crane.Bell
{
    /// <summary>
    /// Ingame の鐘の位置・大きさ・見た目を、Setup 画面の鐘に合わせる。
    /// </summary>
    /// <remarks>
    /// 鐘の位置の正は Setup 画面側(<see cref="CranePoseEditorData.BellPosition"/>)にある。
    /// 起動時に一度だけワールド座標へ変換し、当たり判定・揺れの戻り位置・見た目をそろえる。
    /// 前回のゲームの揺れが次のゲームに持ち越されると、同じ配置でも結果が変わってしまうため、
    /// Ingame 表示になるたびに鐘を静止した元の位置へ戻す。
    /// </remarks>
    public class BellPoseSync : MonoBehaviour
    {
        /// <summary>鐘の位置と基準解像度を持つデータ。</summary>
        [SerializeField] private CranePoseEditorData poseEditorData;

        /// <summary>鐘の当たり判定。</summary>
        [SerializeField] private BoxCollider bellCollider;

        /// <summary>物理ワールドを映すカメラ。</summary>
        [SerializeField] private Camera gameplayCamera;

        /// <summary>見た目の写し取り元(Setup 画面の鐘の Image)。未設定ならデータの大きさと色で代替表示する。</summary>
        [SerializeField] private Image visualSource;

        /// <summary>鐘の剛体。なければ位置だけを戻す。</summary>
        private Rigidbody bellRigidbody;

        /// <summary>静止時の位置。</summary>
        private Vector3 homePosition;

        /// <summary>静止時の回転。</summary>
        private Quaternion homeRotation;

        /// <summary>配置が済んでいれば true。</summary>
        private bool isPlaced;

        /// <summary>
        /// 鐘をワールドへ配置する。
        /// </summary>
        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                enabled = false;
                return;
            }

            transform.position = SetupToPhysicsConverter.CanvasToWorldPoint(
                poseEditorData.BellPosition,
                poseEditorData.ReferenceResolution,
                gameplayCamera);

            Vector2 worldSize = SetupToPhysicsConverter.CanvasToWorldSize(
                GetCanvasSize(),
                poseEditorData.ReferenceResolution,
                gameplayCamera);

            bellCollider.size = new Vector3(worldSize.x, worldSize.y, bellCollider.size.z);
            bellCollider.isTrigger = true;

            AnchorHomeJoint();
            CreateVisual(worldSize);

            bellRigidbody = GetComponent<Rigidbody>();
            homePosition = transform.position;
            homeRotation = transform.rotation;
            isPlaced = true;

            CraneLog.Info(nameof(BellPoseSync), "鐘を配置しました: position=" + transform.position + " size=" + bellCollider.size);
        }

        /// <summary>
        /// Ingame 表示になるたびに、鐘を静止した元の位置へ戻す。
        /// </summary>
        private void OnEnable()
        {
            if (!isPlaced)
            {
                return;
            }

            transform.SetPositionAndRotation(homePosition, homeRotation);
            if (bellRigidbody == null)
            {
                return;
            }

            bellRigidbody.position = homePosition;
            bellRigidbody.rotation = homeRotation;
            bellRigidbody.linearVelocity = Vector3.zero;
            bellRigidbody.angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Setup 画面上の鐘の大きさを求める。
        /// </summary>
        /// <returns>Canvas 上の大きさ(px)。</returns>
        private Vector2 GetCanvasSize()
        {
            if (visualSource == null)
            {
                return poseEditorData.BellSize;
            }

            RectTransform sourceRect = visualSource.rectTransform;
            Vector2 sourceScale = new Vector2(Mathf.Abs(sourceRect.localScale.x), Mathf.Abs(sourceRect.localScale.y));
            return Vector2.Scale(sourceRect.rect.size, sourceScale);
        }

        /// <summary>
        /// 鐘を元の位置へ引き戻すバネの固定点を、配置後の位置にする。
        /// </summary>
        private void AnchorHomeJoint()
        {
            SpringJoint homeJoint = GetComponent<SpringJoint>();
            if (homeJoint == null)
            {
                return;
            }

            homeJoint.autoConfigureConnectedAnchor = false;
            homeJoint.anchor = Vector3.zero;
            homeJoint.connectedAnchor = transform.position;
        }

        /// <summary>
        /// 鐘の見た目を子オブジェクトとして作る。
        /// </summary>
        /// <param name="worldSize">ワールドでの大きさ。</param>
        private void CreateVisual(Vector2 worldSize)
        {
            GameObject visual = new GameObject("Sprite");
            visual.transform.SetParent(transform, false);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = SortingOrders.Bell;

            Vector2 pivot = new Vector2(0.5f, 0.5f);
            if (visualSource == null)
            {
                renderer.sprite = SpriteFitter.GetCenteredWhiteSprite();
                renderer.color = poseEditorData.BellColor;
            }
            else
            {
                renderer.sprite = visualSource.sprite;
                renderer.color = visualSource.color;
                pivot = visualSource.rectTransform.pivot;
            }

            if (renderer.sprite == null)
            {
                renderer.sprite = SpriteFitter.GetCenteredWhiteSprite();
            }

            SpriteFitter.Fit(visual.transform, renderer.sprite, worldSize, pivot);
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(poseEditorData, nameof(poseEditorData), this);
            isValid &= RequiredReference.IsAssigned(bellCollider, nameof(bellCollider), this);
            isValid &= RequiredReference.IsAssigned(gameplayCamera, nameof(gameplayCamera), this);
            return isValid;
        }
    }
}

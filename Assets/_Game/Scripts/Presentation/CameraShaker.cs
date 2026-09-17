using System;
using Crane.Common;
using Crane.Game;
using UnityEngine;

namespace Crane.Presentation
{
    /// <summary>
    /// ヒット時に画面を短時間揺らす。
    /// </summary>
    /// <remarks>
    /// 揺れは「打たれた方向へ押されて、減衰しながら元へ戻る」1 方向の振動にする。
    /// ランダムな方向へ毎フレーム飛ばすと振り子の動きを目で追えなくなるため、方向は固定し、乱数も使わない。
    /// <code>
    /// 振幅     = 最小振幅 + (最大振幅 - 最小振幅) × 強さ
    /// 絵のずれ = 打撃方向 × 振幅 × 減衰(1 - t)² × sin(2π × 周波数 × 経過時間)
    /// </code>
    /// カメラの位置を絵のずれと逆向きに動かして、物理ワールド(振り子・鐘)を揺らす。
    /// Ingame 中の Setup Canvas はカメラに追従するため揺れないので、
    /// <see cref="ImageOffsetChanged"/> を受けた <see cref="IngameCanvasLayout"/> が背景(配下の HUD を含む)・ブーム・前景を同じ量だけずらす。
    ///
    /// カメラは Setup の座標を物理のワールド座標へ変換する時にも使う。
    /// 揺れたまま次のゲームを始めると振り子の初期位置がずれて再現性が崩れるため、Setup へ戻った時点で揺れを止める。
    ///
    /// 投影行列を書き換える方式は、Screen Space - Camera の Canvas の配置計算が狂い背景が画面外へ飛ぶため使わない。
    /// </remarks>
    [RequireComponent(typeof(Camera))]
    public class CameraShaker : MonoBehaviour
    {
        /// <summary>Setup へ戻ったことを知るための進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>揺れ続ける時間(秒)。</summary>
        [SerializeField] private float shakeDurationSeconds = 0.25f;

        /// <summary>最も弱いヒット(強さ 0)での振幅(ワールド単位)。画面の高さは約 10.8。</summary>
        [SerializeField] private float minAmplitude = 0.08f;

        /// <summary>最も強いヒット(強さ 1)での振幅(ワールド単位)。画面の高さは約 10.8。</summary>
        [SerializeField] private float maxAmplitude = 0.3f;

        /// <summary>振動の周波数(Hz)。</summary>
        [SerializeField] private float frequency = 12f;

        /// <summary>揺れていない時のカメラの localPosition。</summary>
        private Vector3 baseLocalPosition;

        /// <summary>揺れの向き(長さ 1)。</summary>
        private Vector2 shakeDirection = Vector2.right;

        /// <summary>今回の揺れの振幅(ワールド単位)。</summary>
        private float amplitude;

        /// <summary>揺れ始めてからの経過時間(秒)。</summary>
        private float elapsedSeconds;

        /// <summary>揺れている最中なら true。</summary>
        private bool isShaking;

        /// <summary>
        /// 画面上で絵をずらす量(ワールド単位)が変わった時に通知される。揺れが終わると 0 で通知される。
        /// </summary>
        public event Action<Vector2> ImageOffsetChanged;

        /// <summary>
        /// 必須参照を検証し、揺れていない時のカメラ位置を記録する。
        /// </summary>
        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
            if (!RequiredReference.IsAssigned(gameController, nameof(gameController), this))
            {
                enabled = false;
            }
        }

        /// <summary>
        /// 状態の変化を購読する。
        /// </summary>
        private void OnEnable()
        {
            gameController.StateChanged += HandleStateChanged;
        }

        /// <summary>
        /// 購読を解除し、揺れを止めてカメラを元の位置へ戻す。
        /// </summary>
        /// <remarks>
        /// シーン終了時は通知先が先に破棄されている場合があるため、ここでは通知しない。
        /// </remarks>
        private void OnDisable()
        {
            gameController.StateChanged -= HandleStateChanged;
            isShaking = false;
            elapsedSeconds = 0f;
            transform.localPosition = baseLocalPosition;
        }

        /// <summary>
        /// 揺れを開始する。揺れている途中なら新しいヒットで始め直す。
        /// </summary>
        /// <param name="hitVelocity">打撃の速度。絵はこの向きに押される。</param>
        /// <param name="strength">ヒットの強さ(0〜1、<see cref="HitStrength"/>)。振幅は最小値から最大値までこれに比例して大きくなる。</param>
        public void Shake(Vector2 hitVelocity, float strength)
        {
            if (hitVelocity.sqrMagnitude > Mathf.Epsilon)
            {
                shakeDirection = hitVelocity.normalized;
            }

            amplitude = Mathf.Lerp(minAmplitude, maxAmplitude, Mathf.Clamp01(strength));
            elapsedSeconds = 0f;
            isShaking = true;
        }

        /// <summary>
        /// 描画の直前に、経過時間に応じてカメラの位置をずらす。
        /// </summary>
        private void LateUpdate()
        {
            if (!isShaking)
            {
                return;
            }

            elapsedSeconds += Time.unscaledDeltaTime;
            if (elapsedSeconds >= shakeDurationSeconds)
            {
                StopShake();
                return;
            }

            float progress = elapsedSeconds / shakeDurationSeconds;
            float decay = (1f - progress) * (1f - progress);
            float wave = Mathf.Sin(2f * Mathf.PI * frequency * elapsedSeconds);
            Vector2 imageOffset = shakeDirection * (amplitude * decay * wave);

            ApplyImageOffset(imageOffset);
        }

        /// <summary>
        /// Setup へ戻ったら揺れを止める。
        /// </summary>
        /// <param name="previous">変更前の状態。</param>
        /// <param name="next">変更後の状態。</param>
        private void HandleStateChanged(GameState previous, GameState next)
        {
            if (next == GameState.Setup)
            {
                StopShake();
            }
        }

        /// <summary>
        /// 揺れを止め、カメラを元の位置へ戻す。
        /// </summary>
        private void StopShake()
        {
            isShaking = false;
            elapsedSeconds = 0f;
            ApplyImageOffset(Vector2.zero);
        }

        /// <summary>
        /// 絵が指定量だけずれて見えるようにカメラを動かし、ずれを通知する。
        /// </summary>
        /// <param name="imageOffset">画面上で絵をずらす量(ワールド単位)。</param>
        /// <remarks>
        /// カメラを +d 動かすと、ワールドの絵は画面上で -d ずれて見えるため、カメラは逆向きに動かす。
        /// </remarks>
        private void ApplyImageOffset(Vector2 imageOffset)
        {
            transform.localPosition = baseLocalPosition - new Vector3(imageOffset.x, imageOffset.y, 0f);

            if (ImageOffsetChanged != null)
            {
                ImageOffsetChanged.Invoke(imageOffset);
            }
        }
    }
}

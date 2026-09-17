using Crane.Presentation;
using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// 高速で動くハンマーの残像を描く。
    /// </summary>
    /// <remarks>
    /// 毎フレーム、前フレームとの間を補間した位置に複数の残像を置き、時間とともに薄くして消す。
    /// 残像は起動時に必要数をまとめて作ってプールし、古いものから使い回す(リングバッファ)。
    /// 毎フレームの GameObject の生成・破棄によるガベージを出さないため。
    /// 残像はワールドの姿勢をそのまま写すため、親の拡大率の影響を受けないようシーン直下に置く。
    /// </remarks>
    public sealed class HammerAfterimageTrail : MonoBehaviour
    {
        /// <summary>残像の出始めの不透明度。</summary>
        private const float InitialAlpha = 0.65f;

        /// <summary>1 フレームの移動の間に置く残像の数。</summary>
        private const int SamplesPerFrame = 4;

        /// <summary>プールの大きさの見積もりに使う想定フレームレート。これより高いと古い残像が早めに再利用される。</summary>
        private const int AssumedFramesPerSecond = 60;

        /// <summary>残像のシェーダー名(Resources 配下)。</summary>
        private const string ShaderResourceName = "HammerAfterimage";

        /// <summary>残像の元にするハンマーの見た目。</summary>
        private SpriteRenderer sourceRenderer;

        /// <summary>残像が消えるまでの時間(秒)。</summary>
        private float durationSeconds;

        /// <summary>残像用のマテリアル。このコンポーネントの破棄時に破棄する。</summary>
        private Material afterimageMaterial;

        /// <summary>残像のプール。</summary>
        private SpriteRenderer[] pool;

        /// <summary>各残像を置いた時刻。<see cref="pool"/> と同じ並び。</summary>
        private float[] createdTimes;

        /// <summary>次に使う残像の添え字。</summary>
        private int nextIndex;

        /// <summary>前フレームのハンマーの位置。</summary>
        private Vector3 lastPosition;

        /// <summary>前フレームのハンマーの回転。</summary>
        private Quaternion lastRotation;

        /// <summary>前フレームのハンマーの拡大率。</summary>
        private Vector3 lastScale;

        /// <summary>前フレームの時刻。</summary>
        private float lastSampleTime;

        /// <summary>
        /// 残像の元と表示時間を設定し、残像のプールを作る。
        /// </summary>
        /// <param name="source">残像の元にするハンマーの見た目。</param>
        /// <param name="trailDurationSeconds">残像が消えるまでの時間(秒)。</param>
        public void Initialize(SpriteRenderer source, float trailDurationSeconds)
        {
            sourceRenderer = source;
            durationSeconds = trailDurationSeconds;

            Shader shader = Resources.Load<Shader>(ShaderResourceName);
            afterimageMaterial = new Material(shader);
            afterimageMaterial.name = "Hammer Afterimage Material";

            CreatePool();

            Transform sourceTransform = sourceRenderer.transform;
            lastPosition = sourceTransform.position;
            lastRotation = sourceTransform.rotation;
            lastScale = sourceTransform.lossyScale;
            lastSampleTime = Time.time;
        }

        /// <summary>
        /// 残像を薄くし、ハンマーが動いていれば新しい残像を置く。
        /// </summary>
        private void LateUpdate()
        {
            FadeAfterimages();

            if (sourceRenderer == null)
            {
                return;
            }

            Transform sourceTransform = sourceRenderer.transform;
            Vector3 currentPosition = sourceTransform.position;
            Quaternion currentRotation = sourceTransform.rotation;
            Vector3 currentScale = sourceTransform.lossyScale;

            bool hasMoved = currentPosition != lastPosition
                || currentRotation != lastRotation
                || currentScale != lastScale;
            if (!hasMoved)
            {
                return;
            }

            float currentTime = Time.time;
            for (int i = 1; i <= SamplesPerFrame; i++)
            {
                float t = i / (float)SamplesPerFrame;
                PlaceAfterimage(
                    Vector3.Lerp(lastPosition, currentPosition, t),
                    Quaternion.Slerp(lastRotation, currentRotation, t),
                    Vector3.Lerp(lastScale, currentScale, t),
                    Mathf.Lerp(lastSampleTime, currentTime, t));
            }

            lastPosition = currentPosition;
            lastRotation = currentRotation;
            lastScale = currentScale;
            lastSampleTime = currentTime;
        }

        /// <summary>
        /// 残像のプールを作る。すべて非表示の状態で用意する。
        /// </summary>
        private void CreatePool()
        {
            int capacity = Mathf.CeilToInt(durationSeconds * AssumedFramesPerSecond) * SamplesPerFrame;
            capacity = Mathf.Max(capacity, SamplesPerFrame);

            pool = new SpriteRenderer[capacity];
            createdTimes = new float[capacity];

            for (int i = 0; i < capacity; i++)
            {
                GameObject afterimage = new GameObject("HammerAfterimage");

                SpriteRenderer renderer = afterimage.AddComponent<SpriteRenderer>();
                renderer.sharedMaterial = afterimageMaterial;
                renderer.sortingOrder = SortingOrders.HammerAfterimage;
                renderer.enabled = false;

                pool[i] = renderer;
            }
        }

        /// <summary>
        /// プールから残像を 1 つ取り出し、指定の姿勢で表示する。
        /// </summary>
        /// <param name="position">ワールド位置。</param>
        /// <param name="rotation">ワールド回転。</param>
        /// <param name="scale">拡大率。</param>
        /// <param name="createdTime">残像を置いたとみなす時刻。</param>
        private void PlaceAfterimage(Vector3 position, Quaternion rotation, Vector3 scale, float createdTime)
        {
            SpriteRenderer renderer = pool[nextIndex];
            createdTimes[nextIndex] = createdTime;
            nextIndex = (nextIndex + 1) % pool.Length;

            renderer.transform.SetPositionAndRotation(position, rotation);
            renderer.transform.localScale = scale;
            renderer.sprite = sourceRenderer.sprite;
            renderer.flipX = sourceRenderer.flipX;
            renderer.flipY = sourceRenderer.flipY;
            renderer.spriteSortPoint = sourceRenderer.spriteSortPoint;
            renderer.sortingLayerID = sourceRenderer.sortingLayerID;
            renderer.color = new Color(1f, 1f, 1f, InitialAlpha);
            renderer.enabled = true;
        }

        /// <summary>
        /// 表示中の残像を経過時間に応じて薄くし、表示時間を過ぎたものを隠す。
        /// </summary>
        private void FadeAfterimages()
        {
            for (int i = 0; i < pool.Length; i++)
            {
                SpriteRenderer renderer = pool[i];
                if (!renderer.enabled)
                {
                    continue;
                }

                float progress = (Time.time - createdTimes[i]) / durationSeconds;
                if (progress >= 1f)
                {
                    renderer.enabled = false;
                    continue;
                }

                renderer.color = new Color(1f, 1f, 1f, InitialAlpha * (1f - progress));
            }
        }

        /// <summary>
        /// プールした残像と、実行時に作ったマテリアルを破棄する。
        /// </summary>
        private void OnDestroy()
        {
            if (pool != null)
            {
                foreach (SpriteRenderer renderer in pool)
                {
                    if (renderer != null)
                    {
                        Destroy(renderer.gameObject);
                    }
                }
            }

            if (afterimageMaterial != null)
            {
                Destroy(afterimageMaterial);
            }
        }
    }
}

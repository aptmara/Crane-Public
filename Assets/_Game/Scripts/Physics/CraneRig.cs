using System.Collections;
using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// 生成済みの二重振り子 1 組への窓口。
    /// </summary>
    /// <remarks>
    /// <see cref="CraneSpawner"/> が組み立てたリグのルートに付く。
    /// 外部からは、ハンマーの速度の取得と、異常加速時にアームを折る操作だけを公開する。
    /// </remarks>
    public class CraneRig : MonoBehaviour
    {
        /// <summary>第一リンク。</summary>
        private ArticulationBody upperLinkBody;

        /// <summary>第二リンク(ハンマーが付く)。</summary>
        private ArticulationBody lowerLinkBody;

        /// <summary>ハンマーのオブジェクト。</summary>
        private Transform hammerTransform;

        /// <summary>折れてリグから外れた第二リンク。リグの破棄時に一緒に破棄する。</summary>
        private GameObject detachedLowerLink;

        /// <summary>第一リンク。</summary>
        public ArticulationBody UpperLinkBody
        {
            get
            {
                return upperLinkBody;
            }
        }

        /// <summary>第二リンク。</summary>
        public ArticulationBody LowerLinkBody
        {
            get
            {
                return lowerLinkBody;
            }
        }

        /// <summary>ハンマーのオブジェクト。</summary>
        public Transform HammerTransform
        {
            get
            {
                return hammerTransform;
            }
        }

        /// <summary>アームが折れていれば true。</summary>
        public bool IsBroken { get; private set; }

        /// <summary>
        /// リグを構成するリンクとハンマーを設定する。
        /// </summary>
        /// <param name="upperLink">第一リンク。</param>
        /// <param name="lowerLink">第二リンク。</param>
        /// <param name="hammer">ハンマーのオブジェクト。</param>
        public void Initialize(ArticulationBody upperLink, ArticulationBody lowerLink, Transform hammer)
        {
            upperLinkBody = upperLink;
            lowerLinkBody = lowerLink;
            hammerTransform = hammer;
        }

        /// <summary>
        /// 異常加速の判定に使う、第二リンクの速さを返す。
        /// </summary>
        /// <returns>第二リンクの ArticulationBody の速さ(m/s)。</returns>
        /// <remarks>
        /// ハンマー位置の点速度は、二重振り子の先端がムチのようにしなるため通常のスイングでも大きくなる。
        /// <see cref="Crane.Data.OverSpeedProfile.SpeedThreshold"/> はこの値を基準に調整しているので、測り方を変えないこと。
        /// </remarks>
        public float GetLowerLinkSpeed()
        {
            return lowerLinkBody.linearVelocity.magnitude;
        }

        /// <summary>
        /// アームを折り、第二リンクをリグから切り離す。
        /// </summary>
        /// <remarks>
        /// 原作では異常加速(永久機関)が起きるとアームが折れ、特殊な寸劇へ移る。
        /// 第二リンクをリグの外へ付け替えると、関節を持たない独立した物体として飛んでいく。
        /// </remarks>
        public void Break()
        {
            if (IsBroken)
            {
                return;
            }

            IsBroken = true;
            StartCoroutine(DetachLowerLink());
        }

        /// <summary>
        /// 第二リンクを切り離し、折れる直前の速度を引き継がせる。
        /// </summary>
        /// <returns>コルーチン。</returns>
        /// <remarks>
        /// 付け替えた ArticulationBody は次の物理ステップで作り直されるため、
        /// 速度はその後に設定する。
        /// </remarks>
        private IEnumerator DetachLowerLink()
        {
            Vector3 linearVelocity = lowerLinkBody.linearVelocity;
            Vector3 angularVelocity = lowerLinkBody.angularVelocity;

            detachedLowerLink = lowerLinkBody.gameObject;
            detachedLowerLink.transform.SetParent(transform.parent, true);

            yield return new WaitForFixedUpdate();

            if (lowerLinkBody == null)
            {
                yield break;
            }

            lowerLinkBody.linearVelocity = linearVelocity;
            lowerLinkBody.angularVelocity = angularVelocity;
        }

        /// <summary>
        /// 切り離した第二リンクも一緒に破棄する。
        /// </summary>
        private void OnDestroy()
        {
            if (detachedLowerLink != null)
            {
                Destroy(detachedLowerLink);
            }
        }
    }
}

using UnityEngine;

namespace Crane.Data
{
    /// <summary>
    /// 異常加速(永久機関)を判定する閾値。
    /// </summary>
    [CreateAssetMenu(fileName = "OverSpeedProfile", menuName = "Crane/OverSpeed Profile")]
    public class OverSpeedProfile : ScriptableObject
    {
        /// <summary>第二リンクの速度(m/s)がこの値を超えたら異常加速とみなす。</summary>
        [Tooltip("第二リンク(ArticulationBody)の速度(m/s)がこの値を超えたら異常加速とみなし、アームを折る。")]
        public float SpeedThreshold = 50f;
    }
}

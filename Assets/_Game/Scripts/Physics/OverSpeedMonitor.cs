using Crane.Common;
using Crane.Data;
using Crane.Game;
using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// ハンマーの異常加速(永久機関)を検知し、特殊失敗ルートへ移す。
    /// </summary>
    /// <remarks>
    /// 原作の ArticulationBody には特定条件で振り子が加速し続ける現象がある。
    /// 作者はこれを修正せず、一定以上の速度になったらアームが折れる特殊ルートとしてゲームに取り込んでいる。
    /// そのため速度を制限して通常プレイへ戻すのではなく、検知したら <see cref="GameController.NotifyCraneBroken"/> を呼ぶ。
    /// </remarks>
    public class OverSpeedMonitor : MonoBehaviour
    {
        /// <summary>異常加速の閾値。</summary>
        [SerializeField] private OverSpeedProfile profile;

        /// <summary>異常加速を検知した時に表示する警告。</summary>
        [SerializeField] private GameObject overSpeedIndicator;

        /// <summary>ゲームの進行管理。</summary>
        [SerializeField] private GameController gameController;

        /// <summary>監視対象のリグを生成する担当。</summary>
        [SerializeField] private CraneSpawner spawner;

        /// <summary>監視中のリグ。存在しなければ null。</summary>
        private CraneRig monitoredRig;

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
        /// リグの生成・破棄とゲームの中断を購読する。
        /// </summary>
        private void OnEnable()
        {
            spawner.RigSpawned += HandleRigSpawned;
            spawner.RigDestroying += HandleRigDestroying;
            gameController.GameStopped += HandleGameStopped;
        }

        /// <summary>
        /// 購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            spawner.RigSpawned -= HandleRigSpawned;
            spawner.RigDestroying -= HandleRigDestroying;
            gameController.GameStopped -= HandleGameStopped;
        }

        /// <summary>
        /// スコア計測中のみ、物理ステップごとに第二リンクの速さを閾値と比べる。
        /// </summary>
        /// <remarks>
        /// アームが折れる瞬間をフレームの長さで前後させないよう、Update ではなく FixedUpdate で判定する。
        /// </remarks>
        private void FixedUpdate()
        {
            if (monitoredRig == null || monitoredRig.IsBroken)
            {
                return;
            }

            if (!gameController.IsScoringActive)
            {
                return;
            }

            float speed = monitoredRig.GetLowerLinkSpeed();
            if (speed > profile.SpeedThreshold)
            {
                HandleOverSpeed(speed);
            }
        }

        /// <summary>
        /// 異常加速を検知した時の処理。警告を表示し、特殊失敗ルートへ移す。
        /// </summary>
        /// <param name="speed">検知した時の速さ(m/s)。</param>
        private void HandleOverSpeed(float speed)
        {
            CraneLog.Info(nameof(OverSpeedMonitor), "異常加速を検知しました。speed=" + speed.ToString("F2") + " threshold=" + profile.SpeedThreshold.ToString("F2"));
            overSpeedIndicator.SetActive(true);
            gameController.NotifyCraneBroken();
        }

        /// <summary>
        /// 生成されたリグを監視対象にする。
        /// </summary>
        /// <param name="rig">生成されたリグ。</param>
        private void HandleRigSpawned(CraneRig rig)
        {
            monitoredRig = rig;
        }

        /// <summary>
        /// 破棄されるリグを監視対象から外す。
        /// </summary>
        private void HandleRigDestroying()
        {
            monitoredRig = null;
        }

        /// <summary>
        /// Setup へ戻る時に警告を消す。
        /// </summary>
        private void HandleGameStopped()
        {
            overSpeedIndicator.SetActive(false);
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(profile, nameof(profile), this);
            isValid &= RequiredReference.IsAssigned(overSpeedIndicator, nameof(overSpeedIndicator), this);
            isValid &= RequiredReference.IsAssigned(gameController, nameof(gameController), this);
            isValid &= RequiredReference.IsAssigned(spawner, nameof(spawner), this);
            return isValid;
        }
    }
}

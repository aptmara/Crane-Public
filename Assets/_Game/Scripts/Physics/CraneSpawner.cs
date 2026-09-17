using System;
using Crane.Common;
using Crane.Data;
using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// 二重振り子のリグを生成・破棄する。
    /// </summary>
    /// <remarks>
    /// 生成は「物理構造 → 見た目 → 見た目に合わせた当たり判定」の順に行う。
    /// 同時に存在するリグは常に 1 組だけ。
    /// </remarks>
    public class CraneSpawner : MonoBehaviour
    {
        /// <summary>物理パラメータ。</summary>
        [SerializeField] private PhysicsProfile physicsProfile;

        /// <summary>リグを置く親。</summary>
        [SerializeField] private Transform spawnParent;

        /// <summary>リグに見た目を付ける担当。</summary>
        [SerializeField] private CraneRigVisualBuilder visualBuilder;

        /// <summary>リグを生成した時に通知される。</summary>
        public event Action<CraneRig> RigSpawned;

        /// <summary>リグを破棄する直前に通知される。</summary>
        public event Action RigDestroying;

        /// <summary>現在のリグ。存在しなければ null。</summary>
        public CraneRig CurrentRig { get; private set; }

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
        /// 既存のリグを破棄し、初期条件から新しいリグを生成する。
        /// </summary>
        /// <param name="setup">ワールド座標系の初期条件。</param>
        public void Spawn(CranePhysicsSetup setup)
        {
            DestroyCurrentRig();

            CraneRig rig = CraneRigBuilder.BuildLinks(setup, physicsProfile, spawnParent);
            SpriteRenderer hammerRenderer = visualBuilder.Decorate(rig, setup);
            CraneRigBuilder.AttachHammerCollider(rig, hammerRenderer);

            CurrentRig = rig;
            CraneLog.Info(nameof(CraneSpawner), "リグを生成しました: " + setup);

            if (RigSpawned != null)
            {
                RigSpawned.Invoke(rig);
            }
        }

        /// <summary>
        /// 現在のリグを破棄する。リグがなければ何もしない。
        /// </summary>
        public void DestroyCurrentRig()
        {
            if (CurrentRig == null)
            {
                return;
            }

            if (RigDestroying != null)
            {
                RigDestroying.Invoke();
            }

            Destroy(CurrentRig.gameObject);
            CurrentRig = null;
            CraneLog.Info(nameof(CraneSpawner), "リグを破棄しました。");
        }

        /// <summary>
        /// Inspector で設定する必須参照がすべて揃っているかを検証する。
        /// </summary>
        /// <returns>すべて設定されていれば true。</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;
            isValid &= RequiredReference.IsAssigned(physicsProfile, nameof(physicsProfile), this);
            isValid &= RequiredReference.IsAssigned(spawnParent, nameof(spawnParent), this);
            isValid &= RequiredReference.IsAssigned(visualBuilder, nameof(visualBuilder), this);
            return isValid;
        }
    }
}

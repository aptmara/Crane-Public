using Crane.Data;
using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// 二重振り子の物理構造(ArticulationBody の階層と当たり判定)を組み立てる。
    /// </summary>
    /// <remarks>
    /// 見た目は扱わない(<see cref="CraneRigVisualBuilder"/> の担当)。
    /// 組み立てる階層は次の通り。各リンクはローカル X 軸方向へ伸び、根元の Z 軸周りに自由回転する。
    /// <code>
    /// Root          … 固定された支点(immovable)
    /// └ UpperLink  … 第一リンク。先端に小さな球の当たり判定を持つ
    ///   └ LowerLink … 第二リンク。HammerMarker を持つ
    ///     └ Hammer  … 先端。当たり判定は見た目の大きさに合わせて後から付ける
    /// </code>
    /// 作者が一般的な方式では減速しすぎる問題に直面し、ArticulationBody を採用したことに合わせている。
    /// </remarks>
    public static class CraneRigBuilder
    {
        /// <summary>第一リンク先端の当たり判定の半径。</summary>
        private const float JointColliderRadius = 0.03f;

        /// <summary>ハンマーの当たり判定の奥行き。</summary>
        private const float HammerColliderDepth = 0.1f;

        /// <summary>
        /// リンクとハンマーの物理構造を組み立てる。
        /// </summary>
        /// <param name="setup">ワールド座標系の初期条件。</param>
        /// <param name="profile">物理パラメータ。</param>
        /// <param name="parent">リグを置く親。</param>
        /// <returns>組み立てたリグ。ハンマーの当たり判定はまだ付いていない。</returns>
        public static CraneRig BuildLinks(CranePhysicsSetup setup, PhysicsProfile profile, Transform parent)
        {
            GameObject root = new GameObject("Root");
            root.transform.SetParent(parent, false);
            root.transform.position = setup.RootPosition;

            ArticulationBody rootBody = root.AddComponent<ArticulationBody>();
            rootBody.immovable = true;

            ArticulationBody upperLink = CreateLink(
                root.transform,
                "UpperLink",
                Vector3.zero,
                setup.UpperLinkAngleDeg,
                profile.ElbowMass,
                profile);
            AddJointCollider(upperLink.gameObject, setup.UpperLinkLength);

            ArticulationBody lowerLink = CreateLink(
                upperLink.transform,
                "LowerLink",
                new Vector3(setup.UpperLinkLength, 0f, 0f),
                setup.LowerLinkRelativeAngleDeg,
                setup.Hammer.Mass,
                profile);

            HammerMarker marker = lowerLink.gameObject.AddComponent<HammerMarker>();
            marker.Initialize(lowerLink.mass, setup.Hammer.DamageBonus, setup.Hammer.HitSound);

            GameObject hammer = new GameObject("Hammer");
            hammer.transform.SetParent(lowerLink.transform, false);
            hammer.transform.localPosition = new Vector3(setup.LowerLinkLength, 0f, 0f);

            CraneRig rig = root.AddComponent<CraneRig>();
            rig.Initialize(upperLink, lowerLink, hammer.transform);
            return rig;
        }

        /// <summary>
        /// ハンマーの見た目に合わせた当たり判定を付ける。
        /// </summary>
        /// <param name="rig">対象のリグ。</param>
        /// <param name="hammerRenderer">ハンマーの見た目。大きさと位置が確定している必要がある。</param>
        /// <remarks>
        /// 第一リンク先端の球とハンマーが干渉しないよう、両者の衝突を無効にする。
        /// </remarks>
        public static void AttachHammerCollider(CraneRig rig, SpriteRenderer hammerRenderer)
        {
            Bounds spriteBounds = hammerRenderer.sprite.bounds;
            BoxCollider hammerCollider = rig.HammerTransform.gameObject.AddComponent<BoxCollider>();
            hammerCollider.center = spriteBounds.center;
            hammerCollider.size = new Vector3(spriteBounds.size.x, spriteBounds.size.y, HammerColliderDepth);

            SphereCollider jointCollider = rig.UpperLinkBody.GetComponent<SphereCollider>();
            Physics.IgnoreCollision(jointCollider, hammerCollider, true);
        }

        /// <summary>
        /// Z 軸周りに回転するリンクを 1 本作る。
        /// </summary>
        /// <param name="parent">親のリンク。</param>
        /// <param name="name">オブジェクト名。</param>
        /// <param name="localOrigin">親に対する根元の位置。</param>
        /// <param name="localAngleDeg">親に対する初期角度(度)。</param>
        /// <param name="mass">リンクの質量。</param>
        /// <param name="profile">物理パラメータ。</param>
        /// <returns>作ったリンクの ArticulationBody。</returns>
        private static ArticulationBody CreateLink(
            Transform parent,
            string name,
            Vector3 localOrigin,
            float localAngleDeg,
            float mass,
            PhysicsProfile profile)
        {
            GameObject link = new GameObject(name);
            link.transform.SetParent(parent, false);
            link.transform.localPosition = localOrigin;
            link.transform.localRotation = Quaternion.Euler(0f, 0f, localAngleDeg);

            ArticulationBody body = link.AddComponent<ArticulationBody>();
            body.jointType = ArticulationJointType.RevoluteJoint;
            body.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
            body.twistLock = ArticulationDofLock.FreeMotion;
            body.useGravity = false;
            body.mass = mass;
            body.linearDamping = profile.LinearDamping;
            body.angularDamping = profile.AngularDamping;
            body.jointFriction = profile.JointFriction;
            body.sleepThreshold = profile.SleepThreshold;

            ArticulationGravityScale gravityScale = link.AddComponent<ArticulationGravityScale>();
            gravityScale.SetMultiplier(profile.GravityMultiplier);

            return body;
        }

        /// <summary>
        /// リンクの先端に小さな球の当たり判定を付ける。
        /// </summary>
        /// <param name="link">対象のリンク。</param>
        /// <param name="length">リンクの長さ。</param>
        private static void AddJointCollider(GameObject link, float length)
        {
            SphereCollider collider = link.AddComponent<SphereCollider>();
            collider.radius = JointColliderRadius;
            collider.center = new Vector3(length, 0f, 0f);
        }
    }
}

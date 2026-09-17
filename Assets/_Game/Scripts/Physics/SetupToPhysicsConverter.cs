using Crane.Data;
using Crane.Setup;
using UnityEngine;

namespace Crane.PhysicsSim
{
    /// <summary>
    /// Setup Canvas の座標を、物理ワールドの座標へ変換する。
    /// </summary>
    /// <remarks>
    /// 座標の正は Setup Canvas 側にあり、物理はそこから変換して作る。
    /// 変換は「Canvas 座標 → Canvas 正規化座標 → Ingame の Viewport 座標(<see cref="IngameLayout"/>) → ワールド座標」の順。
    /// ワールドの奥行きは z = 0 の平面とする。
    /// </remarks>
    public static class SetupToPhysicsConverter
    {
        /// <summary>
        /// Setup の姿勢から、物理の初期条件を作る。
        /// </summary>
        /// <param name="snapshot">PLAY で確定した姿勢と選択したハンマー。</param>
        /// <param name="referenceResolution">Setup Canvas の基準解像度。</param>
        /// <param name="gameplayCamera">物理ワールドを映すカメラ。</param>
        /// <returns>ワールド座標系の初期条件。</returns>
        public static CranePhysicsSetup Convert(
            CraneSetupSnapshot snapshot,
            Vector2 referenceResolution,
            Camera gameplayCamera)
        {
            Vector3 root = CanvasToWorldPoint(snapshot.UpperLinkPivot, referenceResolution, gameplayCamera);
            Vector3 joint = CanvasToWorldPoint(snapshot.LowerLinkPivot, referenceResolution, gameplayCamera);
            Vector3 tip = CanvasToWorldPoint(snapshot.HammerPosition, referenceResolution, gameplayCamera);

            Vector2 upperLink = joint - root;
            Vector2 lowerLink = tip - joint;
            float upperAngle = Mathf.Atan2(upperLink.y, upperLink.x) * Mathf.Rad2Deg;
            float lowerAngle = Mathf.Atan2(lowerLink.y, lowerLink.x) * Mathf.Rad2Deg;
            Vector2 canvasUnitWorldSize = CanvasToWorldSize(Vector2.one, referenceResolution, gameplayCamera);

            return new CranePhysicsSetup(
                root,
                upperLink.magnitude,
                lowerLink.magnitude,
                upperAngle,
                lowerAngle - upperAngle,
                snapshot.Hammer,
                canvasUnitWorldSize);
        }

        /// <summary>
        /// Setup Canvas 上の点を、ワールド座標(z = 0 の平面上)へ変換する。
        /// </summary>
        /// <param name="canvasPoint">Canvas ローカル座標(中心原点、px)。</param>
        /// <param name="referenceResolution">Setup Canvas の基準解像度。</param>
        /// <param name="gameplayCamera">物理ワールドを映すカメラ。</param>
        /// <returns>ワールド座標。</returns>
        public static Vector3 CanvasToWorldPoint(Vector2 canvasPoint, Vector2 referenceResolution, Camera gameplayCamera)
        {
            Vector2 normalizedPoint = new Vector2(
                canvasPoint.x / referenceResolution.x + 0.5f,
                canvasPoint.y / referenceResolution.y + 0.5f);
            Vector2 viewportPoint = IngameLayout.NormalizedCanvasToViewport(normalizedPoint);
            float worldPlaneDepth = gameplayCamera.WorldToViewportPoint(Vector3.zero).z;

            return gameplayCamera.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, worldPlaneDepth));
        }

        /// <summary>
        /// Setup Canvas 上の大きさを、ワールドでの大きさへ変換する。
        /// </summary>
        /// <param name="canvasSize">Canvas 上の大きさ(px)。</param>
        /// <param name="referenceResolution">Setup Canvas の基準解像度。</param>
        /// <param name="gameplayCamera">物理ワールドを映すカメラ。</param>
        /// <returns>ワールドでの幅と高さ。</returns>
        public static Vector2 CanvasToWorldSize(Vector2 canvasSize, Vector2 referenceResolution, Camera gameplayCamera)
        {
            Vector3 origin = CanvasToWorldPoint(Vector2.zero, referenceResolution, gameplayCamera);
            Vector3 horizontalEnd = CanvasToWorldPoint(new Vector2(canvasSize.x, 0f), referenceResolution, gameplayCamera);
            Vector3 verticalEnd = CanvasToWorldPoint(new Vector2(0f, canvasSize.y), referenceResolution, gameplayCamera);

            return new Vector2(
                Vector3.Distance(origin, horizontalEnd),
                Vector3.Distance(origin, verticalEnd));
        }
    }
}

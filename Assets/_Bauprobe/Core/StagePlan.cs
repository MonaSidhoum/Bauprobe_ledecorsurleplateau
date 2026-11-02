using UnityEngine;

namespace Bauprobe.Core
{
    /// <summary>
    /// Passage entre le repère de Unity et le repère « plan » du bureau d'études (tableau CSV).
    ///
    /// Unity : X à droite, Y en haut, Z devant (repère « main gauche »).
    /// Plan  : X à droite, Y devant (profondeur), Z en haut (repère « main droite », comme AutoCAD / SketchUp).
    ///
    /// Sur le plateau, « devant » est l'avant du repère du QR code (voir StageCalibrator) : pour un QR code
    /// vertical (mur, pied), la direction dans laquelle on regarde quand on le lit ; pour un QR code à plat
    /// au sol, la direction de son bord du haut. QR code lu depuis la salle : X+ = cour, Y+ = lointain, Z+ = hauteur.
    ///
    /// Angles du plan, en degrés :
    ///   Angle     = rotation autour de la verticale (Z), sens inverse des aiguilles d'une montre vu de dessus ;
    ///   Bascule X = rotation autour de X ; Bascule Y = rotation autour de Y.
    /// L'objet est d'abord basculé autour de Y, puis autour de X, puis tourné de « Angle » autour de la verticale.
    /// Pour un objet qui n'est pas basculé, seul « Angle » est différent de 0.
    /// </summary>
    public static class StagePlan
    {
        /// <summary>Position Unity (repère du plateau) → position plan (X, Y profondeur, Z hauteur).</summary>
        public static Vector3 ToPlan(Vector3 stage) => new Vector3(stage.x, stage.z, stage.y);

        /// <summary>Position plan (X, Y profondeur, Z hauteur) → position Unity (repère du plateau).</summary>
        public static Vector3 FromPlan(Vector3 plan) => new Vector3(plan.x, plan.z, plan.y);

        /// <summary>
        /// Rotation Unity → angles du plan (x = Bascule X, y = Bascule Y, z = Angle), en degrés, entre -180 et 180.
        /// </summary>
        public static Vector3 ToPlanAngles(Quaternion stageRotation)
        {
            // Unity applique ses angles dans l'ordre Z, X, Y. En échangeant Y et Z (et en changeant le sens de
            // rotation, car on passe d'un repère main gauche à main droite), on obtient l'ordre Y, X, Z du plan.
            var e = stageRotation.eulerAngles;
            return new Vector3(Normalize(-e.x), Normalize(-e.z), Normalize(-e.y));
        }

        /// <summary>Angles du plan (x = Bascule X, y = Bascule Y, z = Angle), en degrés → rotation Unity.</summary>
        public static Quaternion FromPlanAngles(Vector3 planAngles) =>
            Quaternion.Euler(-planAngles.x, -planAngles.z, -planAngles.y);

        /// <summary>
        /// Position de StageRoot (le point zéro du bureau d'études) dans le repère du QR code, à partir de la
        /// position du QR code (le point du sol sous son centre) mesurée depuis le point zéro, en mètres.
        /// Les axes sont ceux du QR code : X+ à droite, Y+ vers l'avant (voir StageCalibrator).
        /// Offset nul → StageRoot est exactement sur le QR code.
        /// </summary>
        public static Pose ReferenceInQrFrame(float qrOffsetX, float qrOffsetY)
        {
            // Le QR code est en (X, Y) depuis le point zéro : le point zéro est en (-X, -Y) depuis le QR code.
            return new Pose(-FromPlan(new Vector3(qrOffsetX, qrOffsetY, 0f)), Quaternion.identity);
        }

        /// <summary>Ramène un angle entre -180 et 180 degrés (et évite d'écrire « -0 »).</summary>
        public static float Normalize(float degrees)
        {
            var a = Mathf.Repeat(degrees + 180f, 360f) - 180f;
            if (a <= -180f + 1e-4f) a = 180f;
            return Mathf.Abs(a) < 1e-4f ? 0f : a;
        }
    }
}

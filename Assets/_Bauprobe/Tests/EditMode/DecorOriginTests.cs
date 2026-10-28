using System.Collections.Generic;
using Bauprobe.Decor;
using Bauprobe.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    public class DecorOriginTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
            _created.Clear();
        }

        // Un cube de 1 m dont le pivot est au centre (comme le cube de test du catalogue).
        private GameObject CreateCube(string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            _created.Add(go);
            return go;
        }

        [Test]
        public void Sol_PointOrigine_EstLeBasAuCentre_SansEchelleDuParent()
        {
            var cube = CreateCube("Cube");
            cube.transform.localScale = new Vector3(2f, 2f, 2f); // l'échelle de l'objet ne compte pas

            var origin = DecorOrigin.ComputeLocalOrigin(cube.transform, DecorAttachment.Sol);

            Assert.That(Vector3.Distance(origin, new Vector3(0f, -0.5f, 0f)), Is.LessThan(1e-4f));
        }

        [Test]
        public void Cintre_PointOrigine_EstLeHautAuCentre_AvecModeleDecale()
        {
            var root = new GameObject("Frise");
            _created.Add(root);

            // Modèle décalé et étiré : boîte de x 0,5..1,5 ; y 0,5..3,5 ; z -0,5..0,5
            var model = CreateCube("Modele");
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = new Vector3(1f, 2f, 0f);
            model.transform.localScale = new Vector3(1f, 3f, 1f);

            var origin = DecorOrigin.ComputeLocalOrigin(root.transform, DecorAttachment.Cintre);

            Assert.That(Vector3.Distance(origin, new Vector3(1f, 3.5f, 0f)), Is.LessThan(1e-4f));
        }

        [Test]
        public void SansModele_PointOrigine_EstLePivot()
        {
            var empty = new GameObject("Vide");
            _created.Add(empty);

            Assert.That(DecorOrigin.ComputeLocalOrigin(empty.transform, DecorAttachment.Sol), Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Scale_GardeLeBasDeLObjetAuSol()
        {
            var stage = new GameObject("StageRoot");
            _created.Add(stage);
            stage.transform.SetPositionAndRotation(new Vector3(3f, 1f, -2f), Quaternion.Euler(0f, 40f, 0f));

            var cube = CreateCube("Cube");
            cube.transform.SetParent(stage.transform, false);
            var instance = cube.AddComponent<DecorInstance>();

            instance.SetOriginStagePosition(new Vector3(1f, 0f, 2f));
            Assert.That(cube.transform.localPosition.y, Is.EqualTo(0.5f).Within(1e-4f), "le pivot est au centre du cube");

            instance.SetScaleAroundOrigin(new Vector3(3f, 3f, 3f));

            Assert.That(Vector3.Distance(instance.OriginStagePosition, new Vector3(1f, 0f, 2f)), Is.LessThan(1e-4f),
                "le point d'origine (bas au centre) n'a pas bougé");
            Assert.That(cube.transform.localPosition.y, Is.EqualTo(1.5f).Within(1e-4f), "le cube a grandi vers le haut");
        }

        [Test]
        public void Sauvegarde_EcritLePointOrigine_EnVersion2()
        {
            var stage = new GameObject("StageRoot");
            _created.Add(stage);

            var cube = CreateCube("Cube");
            cube.transform.SetParent(stage.transform, false);
            var instance = cube.AddComponent<DecorInstance>();
            instance.Initialize(ScriptableObject.CreateInstance<DecorItemDefinition>());
            instance.SetOriginStagePosition(new Vector3(1f, 0f, 2f));

            var data = LayoutSerializer.Capture(new[] { instance });

            Assert.That(data.version, Is.EqualTo(LayoutData.CurrentVersion));
            Assert.That(Vector3.Distance(data.items[0].position, new Vector3(1f, 0f, 2f)), Is.LessThan(1e-4f));
        }
    }
}

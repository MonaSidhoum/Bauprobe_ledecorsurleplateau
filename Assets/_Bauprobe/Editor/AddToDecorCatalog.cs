#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Outil d'éditeur : clic droit sur un ou plusieurs prefabs dans Project >
/// "Bauprobe > Ajouter au catalogue".
///  1. Recopie sur le prefab les composants d'interaction d'un objet déjà au catalogue
///     (Rigidbody, Grabbable, Decor Constraints, Interactable Object...) : c'est ce qui permet
///     de saisir l'objet et d'afficher son petit menu (déplacer, agrandir...).
///  2. Crée la fiche (Decor Item Definition) à côté du catalogue et l'ajoute à la liste.
/// Si le prefab est déjà au catalogue, les composants manquants sont simplement ajoutés.
/// Ne dépend d'aucun autre script du projet (tout passe par les noms de champs Unity).
/// </summary>
public static class AddToDecorCatalog
{
    private const string MenuPath = "Assets/Bauprobe/Ajouter au catalogue";

    [MenuItem(MenuPath, true)]
    private static bool Validate() =>
        Selection.objects.Any(o => o is GameObject && PrefabUtility.IsPartOfPrefabAsset(o));

    [MenuItem(MenuPath)]
    private static void Run()
    {
        var catalog = FindCatalog();
        if (catalog == null) { Debug.LogError("[Catalogue] Aucun asset Decor Catalog trouvé dans le projet."); return; }

        var defType = FindType("DecorItemDefinition");
        if (defType == null) { Debug.LogError("[Catalogue] Type DecorItemDefinition introuvable."); return; }

        var folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(catalog)).Replace('\\', '/');
        var catalogSO = new SerializedObject(catalog);
        var items = FindItemsArray(catalogSO);
        if (items == null) { Debug.LogError("[Catalogue] Liste des objets introuvable dans le catalogue."); return; }

        int added = 0;
        foreach (var obj in Selection.objects)
        {
            var prefab = obj as GameObject;
            if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab)) continue;

            // Modèle de référence : un objet déjà au catalogue (autre que celui-ci).
            var template = FirstDefinition(items, prefab);
            var templatePrefab = template != null ? GetPrefab(template) : null;

            if (AlreadyInCatalog(items, prefab))
            {
                if (templatePrefab != null && PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.Model)
                    UpgradePrefab(prefab, templatePrefab);
                Debug.Log("[Catalogue] « " + prefab.name + " » est déjà dans le catalogue (composants vérifiés).");
                continue;
            }

            // 1. Prefab prêt à être manipulé
            var finalPrefab = prefab;
            if (templatePrefab != null)
                finalPrefab = PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Model
                    ? WrapModel(prefab, templatePrefab)
                    : UpgradePrefab(prefab, templatePrefab);

            // 2. Fiche
            var def = template != null ? UnityEngine.Object.Instantiate(template) : ScriptableObject.CreateInstance(defType);
            def.name = prefab.name;
            var defSO = new SerializedObject(def);
            SetString(defSO, "id", UniqueId(items, prefab.name.ToLowerInvariant().Replace(' ', '_')));
            SetString(defSO, "displayname", prefab.name);
            SetObject(defSO, "prefab", finalPrefab);
            SetObject(defSO, "thumbnail", null);
            defSO.ApplyModifiedPropertiesWithoutUndo();

            var path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + prefab.name + ".asset");
            AssetDatabase.CreateAsset(def, path);

            items.arraySize++;
            items.GetArrayElementAtIndex(items.arraySize - 1).objectReferenceValue = def;
            added++;
            Debug.Log("[Catalogue] « " + prefab.name + " » ajouté au catalogue (" + path + ").");
        }

        catalogSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        if (added > 0) EditorGUIUtility.PingObject(catalog);
    }

    // ------------------------------------------------------------------ composants d'interaction
    /// <summary>Ajoute au prefab (en place) les composants du modèle qu'il n'a pas encore.</summary>
    private static GameObject UpgradePrefab(GameObject prefab, GameObject templatePrefab)
    {
        var path = AssetDatabase.GetAssetPath(prefab);
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            EnsureCollider(root);
            CopyMissingComponents(templatePrefab, root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    /// <summary>Un modèle importé (.fbx, .glb) n'est pas modifiable : on l'enveloppe dans un nouveau prefab.</summary>
    private static GameObject WrapModel(GameObject model, GameObject templatePrefab)
    {
        var root = new GameObject(model.name);
        try
        {
            var child = (GameObject)PrefabUtility.InstantiatePrefab(model);
            child.transform.SetParent(root.transform, false);
            EnsureCollider(root);
            CopyMissingComponents(templatePrefab, root);

            var folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(model)).Replace('\\', '/');
            var path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + model.name + "_Decor.prefab");
            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log("[Catalogue] Modèle importé : prefab créé (" + path + ").");
            return saved;
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static void CopyMissingComponents(GameObject templatePrefab, GameObject target)
    {
        foreach (var c in templatePrefab.GetComponents<Component>())
        {
            if (c == null || c is Transform || c is MeshFilter || c is MeshRenderer || c is Collider) continue;
            if (target.GetComponent(c.GetType()) != null) continue;

            if (ComponentUtility.CopyComponent(c) && ComponentUtility.PasteComponentAsNew(target))
                Debug.Log("[Catalogue]   + " + c.GetType().Name + " ajouté à « " + target.name + " ».");
            else
                Debug.LogWarning("[Catalogue] Impossible de copier " + c.GetType().Name + ".");
        }

        // Les références vers le modèle (ex. Grabbable > Rigidbody) doivent viser le nouvel objet.
        foreach (var comp in target.GetComponents<Component>())
        {
            if (comp == null || comp is Transform) continue;
            var so = new SerializedObject(comp);
            var it = so.GetIterator();
            bool changed = false;
            for (bool enter = true; it.Next(enter); enter = false)
            {
                if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                var vc = it.objectReferenceValue as Component;
                if (vc == null || vc.gameObject != templatePrefab) continue;
                var local = target.GetComponent(vc.GetType());
                if (local != null) { it.objectReferenceValue = local; changed = true; }
            }
            if (changed) so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // Sans collider, l'objet ne peut pas être saisi : on en ajoute un qui entoure le modèle.
    private static void EnsureCollider(GameObject root)
    {
        if (root.GetComponentInChildren<Collider>() != null) return;
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        var box = root.AddComponent<BoxCollider>();
        box.center = root.transform.InverseTransformPoint(b.center);
        box.size = b.size;
    }

    // ------------------------------------------------------------------ outils
    private static ScriptableObject FindCatalog()
    {
        var guid = AssetDatabase.FindAssets("t:DecorCatalog").FirstOrDefault();
        return guid == null ? null : AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid));
    }

    private static Type FindType(string name) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
            .FirstOrDefault(t => t.Name == name && typeof(ScriptableObject).IsAssignableFrom(t));

    private static SerializedProperty FindItemsArray(SerializedObject so)
    {
        var it = so.GetIterator();
        for (bool enter = true; it.NextVisible(enter); enter = false)
            if (it.isArray && it.propertyType == SerializedPropertyType.Generic && it.name != "m_Script")
                return so.FindProperty(it.propertyPath);
        return null;
    }

    private static GameObject GetPrefab(ScriptableObject def)
    {
        var p = FindField(new SerializedObject(def), "prefab");
        return p != null ? p.objectReferenceValue as GameObject : null;
    }

    // Première fiche du catalogue dont le prefab existe et n'est pas celui qu'on traite.
    private static ScriptableObject FirstDefinition(SerializedProperty items, GameObject except)
    {
        for (int i = 0; i < items.arraySize; i++)
        {
            var d = items.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
            if (d == null) continue;
            var p = GetPrefab(d);
            if (p != null && p != except) return d;
        }
        return null;
    }

    private static bool AlreadyInCatalog(SerializedProperty items, GameObject prefab)
    {
        for (int i = 0; i < items.arraySize; i++)
        {
            var d = items.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableObject;
            if (d == null) continue;
            if (GetPrefab(d) == prefab) return true;
        }
        return false;
    }

    private static string UniqueId(SerializedProperty items, string wanted)
    {
        var used = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < items.arraySize; i++)
        {
            var d = items.GetArrayElementAtIndex(i).objectReferenceValue;
            if (d == null) continue;
            var p = FindField(new SerializedObject(d), "id");
            if (p != null) used.Add(p.stringValue);
        }
        var id = wanted; int n = 2;
        while (used.Contains(id)) id = wanted + "_" + n++;
        return id;
    }

    // Cherche un champ sans tenir compte de la casse ni des préfixes "_" / "m_".
    private static SerializedProperty FindField(SerializedObject so, string wanted)
    {
        var it = so.GetIterator();
        for (bool enter = true; it.NextVisible(enter); enter = false)
        {
            var n = it.name.TrimStart('_').ToLowerInvariant();
            if (n.StartsWith("m_")) n = n.Substring(2);
            if (n == wanted) return so.FindProperty(it.propertyPath);
        }
        return null;
    }

    private static void SetString(SerializedObject so, string field, string value)
    {
        var p = FindField(so, field);
        if (p != null && p.propertyType == SerializedPropertyType.String) p.stringValue = value;
        else Debug.LogWarning("[Catalogue] Champ texte « " + field + " » introuvable dans la fiche.");
    }

    private static void SetObject(SerializedObject so, string field, UnityEngine.Object value)
    {
        var p = FindField(so, field);
        if (p != null && p.propertyType == SerializedPropertyType.ObjectReference) p.objectReferenceValue = value;
        else Debug.LogWarning("[Catalogue] Champ « " + field + " » introuvable dans la fiche.");
    }
}
#endif

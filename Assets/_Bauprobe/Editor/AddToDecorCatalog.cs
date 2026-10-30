#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Outil d'éditeur : clic droit sur un ou plusieurs prefabs dans Project >
/// "Bauprobe > Ajouter au catalogue". Crée la fiche (Decor Item Definition) à côté du catalogue
/// et l'ajoute à la liste du catalogue. Le prefab apparaît ensuite dans le menu Objects.
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

            if (AlreadyInCatalog(items, prefab)) { Debug.Log("[Catalogue] « " + prefab.name + " » est déjà dans le catalogue."); continue; }

            var def = ScriptableObject.CreateInstance(defType);
            def.name = prefab.name;
            var defSO = new SerializedObject(def);
            SetString(defSO, "id", UniqueId(items, prefab.name.ToLowerInvariant().Replace(' ', '_')));
            SetString(defSO, "displayname", prefab.name);
            SetObject(defSO, "prefab", prefab);
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

    private static bool AlreadyInCatalog(SerializedProperty items, GameObject prefab)
    {
        for (int i = 0; i < items.arraySize; i++)
        {
            var d = items.GetArrayElementAtIndex(i).objectReferenceValue;
            if (d == null) continue;
            var so = new SerializedObject(d);
            var p = FindField(so, "prefab");
            if (p != null && p.objectReferenceValue == prefab) return true;
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

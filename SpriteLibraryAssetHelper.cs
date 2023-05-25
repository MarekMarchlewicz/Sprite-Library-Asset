using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;

public static class SpriteLibraryAssetHelper
{
    public const string assetExtension = ".spriteLib";

    const string k_AnimationRuntimeNamespace = "UnityEngine.U2D.Animation";
    const string k_AnimationRuntimeAssembly = "Unity.2D.Animation.Runtime";

    const string k_AnimationEditorNamespace = "UnityEditor.U2D.Animation";
    const string k_AnimationEditorAssembly = "Unity.2D.Animation.Editor";

    static readonly Assembly k_RuntimeAssembly = Assembly.Load(k_AnimationRuntimeAssembly);
    static readonly Assembly k_EditorAssembly = Assembly.Load(k_AnimationEditorAssembly);

    static class SpriteLibraryTypes
    {
        public static readonly Type spriteLibCategoryType = k_RuntimeAssembly.GetType($"{k_AnimationRuntimeNamespace}.SpriteLibCategory");
        public static readonly Type sourceAssetType = k_RuntimeAssembly.GetType($"{k_AnimationRuntimeNamespace}.SpriteLibrarySourceAsset");
        public static readonly Type spriteLibCategoryOverrideType = k_RuntimeAssembly.GetType($"{k_AnimationRuntimeNamespace}.SpriteLibCategoryOverride");
        public static readonly Type labelEntryType = k_RuntimeAssembly.GetType($"{k_AnimationRuntimeNamespace}.SpriteCategoryEntry");
        public static readonly Type labelOverrideEntryType = k_RuntimeAssembly.GetType($"{k_AnimationRuntimeNamespace}.SpriteCategoryEntryOverride");
        public static readonly Type spriteLibrarySourceAssetImporterName = k_EditorAssembly.GetType($"{k_AnimationEditorNamespace}.SpriteLibrarySourceAssetImporter");
    }

    static class SpriteLibraryFields
    {
        public static readonly FieldInfo overrideEntriesField = SpriteLibraryTypes.spriteLibCategoryOverrideType.GetField("m_OverrideEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo categoryNameField = SpriteLibraryTypes.spriteLibCategoryType.GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo mainAssetGuidField = SpriteLibraryTypes.sourceAssetType.GetField("m_PrimaryLibraryGUID", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo spriteOverrideField = SpriteLibraryTypes.labelOverrideEntryType.GetField("m_SpriteOverride", BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly FieldInfo labelNameField = SpriteLibraryTypes.labelEntryType.GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    static class SpriteLibraryMethods
    {
        public static readonly MethodInfo setLibraryMethod = SpriteLibraryTypes.sourceAssetType.GetMethod("SetLibrary");
        public static readonly MethodInfo saveSpriteLibrarySourceAssetMethod = SpriteLibraryTypes.spriteLibrarySourceAssetImporterName.GetMethod("SaveSpriteLibrarySourceAsset", BindingFlags.NonPublic | BindingFlags.Static);
    }

    public static SpriteLibraryAsset SaveAsSpriteLibrarySourceAsset(SpriteLibraryAsset asset, string path, SpriteLibraryAsset mainAsset)
    {
        if (asset == null || string.IsNullOrEmpty(path) || !path.StartsWith("Assets/") || Path.GetExtension(path) != assetExtension)
            return null;

        var sourceAsset = ScriptableObject.CreateInstance(SpriteLibraryTypes.sourceAssetType);

        var categoryListType = typeof(List<>).MakeGenericType(SpriteLibraryTypes.spriteLibCategoryOverrideType);
        var categoryList = Activator.CreateInstance(categoryListType);
        var categoryAddMethod = categoryListType.GetMethod("Add");
        Debug.Assert(categoryAddMethod != null);

        var labelListType = typeof(List<>).MakeGenericType(SpriteLibraryTypes.labelOverrideEntryType);
        var labelAddMethod = labelListType.GetMethod("Add");
        Debug.Assert(labelAddMethod != null);

        var categories = asset.GetCategoryNames();
        if (categories != null)
        {
            foreach (var category in categories)
            {
                var labelList = Activator.CreateInstance(labelListType);

                var labels = asset.GetCategoryLabelNames(category);
                if (labels != null)
                {
                    foreach (var label in labels)
                    {
                        var sprite = asset.GetSprite(category, label);
                        var spriteLibLabel = Activator.CreateInstance(SpriteLibraryTypes.labelOverrideEntryType);
                        SpriteLibraryFields.spriteOverrideField.SetValue(spriteLibLabel, sprite);
                        SpriteLibraryFields.labelNameField.SetValue(spriteLibLabel, label);

                        labelAddMethod.Invoke(labelList, new[] { spriteLibLabel });
                    }
                }

                var spriteLibCategory = Activator.CreateInstance(SpriteLibraryTypes.spriteLibCategoryOverrideType);
                SpriteLibraryFields.categoryNameField.SetValue(spriteLibCategory, category);
                SpriteLibraryFields.overrideEntriesField.SetValue(spriteLibCategory, labelList);

                categoryAddMethod.Invoke(categoryList, new[] { spriteLibCategory });
            }
        }

        SpriteLibraryMethods.setLibraryMethod.Invoke(sourceAsset, new[] { categoryList });

        var mainAssetGuid = mainAsset != null ? AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(mainAsset)).ToString() : string.Empty;
        SpriteLibraryFields.mainAssetGuidField.SetValue(sourceAsset, mainAssetGuid);

        SpriteLibraryMethods.saveSpriteLibrarySourceAssetMethod.Invoke(null, new object[] { sourceAsset, path });
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        return AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(path);
    }
}

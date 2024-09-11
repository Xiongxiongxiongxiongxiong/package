using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UnusedMash : EditorWindow
{
    private List<string> resourceNames = new List<string>();
    private List<string> sceneObjectNames = new List<string>();


    public Object NewMesh;
    public Object NewMaterial;

    public Object NewTexture;
    [MenuItem("Tool/检查场景多余资源")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<UnusedMash>("查找未使用的模型");
    }

    private void OnGUI()
    {
        NewMesh = EditorGUILayout.ObjectField("拖一个mesh文件夹", NewMesh, typeof(Object), true);
        GUILayout.Label(AssetDatabase.GetAssetPath(NewMesh));
        if (GUILayout.Button("查找未使用的模型"))
        {
            ExamineMesh();
        }
        NewMaterial = EditorGUILayout.ObjectField("拖一个mat文件夹", NewMaterial, typeof(Object), true);
        GUILayout.Label(AssetDatabase.GetAssetPath(NewMaterial));

        if (GUILayout.Button("查找未使用的材质球"))
        {
            ExamineMaterial();
        }
        NewTexture = EditorGUILayout.ObjectField("拖一个Texture文件夹", NewTexture, typeof(Object), true);
        GUILayout.Label(AssetDatabase.GetAssetPath(NewTexture));

        if (GUILayout.Button("查找未使用的贴图"))
        {
            ExamineTexture();
        }

    }


    private void ExamineMesh()
    {
        resourceNames.Clear();
        sceneObjectNames.Clear();

        // 获取指定路径中的资源名称
        var resourcePaths = AssetDatabase.FindAssets("t:GameObject", new[] { AssetDatabase.GetAssetPath(NewMesh) });
        foreach (var path in resourcePaths)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(path);
            var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            resourceNames.Add(assetName);
        }

        // 获取场景中的游戏对象名称的一部分
        var gameObjects = FindObjectsOfType<GameObject>();
        foreach (var go in gameObjects)
        {
            if (go.GetComponent<MeshRenderer>() != null)
            {
                var sceneObjectPartName = go.name.Split(' ')[0]; // 提取名称的第一部分
                sceneObjectNames.Add(sceneObjectPartName);
            }
        }

        // 比较资源名称和场景对象名称的一部分，找到不匹配的对象
        foreach (var resourceName in resourceNames)
        {
            if (!sceneObjectNames.Contains(resourceName))
            {
                Debug.Log("未使用的模型: " + resourceName);
            }
        }
    }

    private void ExamineMaterial()
    {
        resourceNames.Clear();
        sceneObjectNames.Clear();

        string newMaterialPath = AssetDatabase.GetAssetPath(NewMaterial);
        if (!string.IsNullOrEmpty(newMaterialPath))
        {
            var resourcePaths = AssetDatabase.FindAssets("t:Material", new[] { newMaterialPath });
            foreach (var path in resourcePaths)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(path);
                var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                resourceNames.Add(assetName);
            }
        }

        var gameObjects = FindObjectsOfType<GameObject>();
        foreach (var go in gameObjects)
        {
            var meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var sceneObjectMats = meshRenderer.sharedMaterials;
                foreach (var mat in sceneObjectMats)
                {
                    if (mat != null) // 检查材质是否为 null
                    {
                        // 忽略 "(Instance)" 后缀
                        var matName = mat.name.Replace("(Instance)", "").Trim();
                        sceneObjectNames.Add(matName);
                    }
                }
            }
        }

        foreach (var resourceName in resourceNames)
        {
            if (!sceneObjectNames.Contains(resourceName))
            {
                Debug.Log("未使用的材质: " + resourceName);
            }
        }
    }

    private void ExamineTexture()
    {
        resourceNames.Clear();
        sceneObjectNames.Clear();

        string newTexturePath = AssetDatabase.GetAssetPath(NewTexture);
        string newMaterialPath = AssetDatabase.GetAssetPath(NewMaterial);
        if (!string.IsNullOrEmpty(newMaterialPath))
        {
            var resourcePaths = AssetDatabase.FindAssets("t:Texture", new[] { newTexturePath });
            var scenePaths = AssetDatabase.FindAssets("t:Material", new[] { newMaterialPath });
            foreach (var path in resourcePaths)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(path);
                var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                resourceNames.Add(assetName);
            }
            foreach (var path in scenePaths)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(path);
                var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                sceneObjectNames.Add(assetName);
            }
        }
        foreach (var resourceName in resourceNames)
        {
            if (!sceneObjectNames.Contains(resourceName))
            {
                Debug.Log("未使用的材质: " + resourceName);
            }
        }
    }

}
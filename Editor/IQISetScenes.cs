using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using IQIGame.Extension;



public class IQISetScenes : EditorWindow
{
    Object gress;
    Object NewScene;
    Object NewModel;
    Object NewMaterial;
    Object NewPrefab;
    Object NewTexture;
    Object NewVolume;
    Object M;
    private int MapNumber = 20;
    private string MapName = "";
    private List<GameObject> emptyObject = new List<GameObject>();
   

    [MenuItem("关卡插件/一键制作关卡包")]

    static void Init()
    {
        GetWindow(typeof(IQISetScenes));

    }

    private void OnGUI()
    {
        NewScene = EditorGUILayout.ObjectField("关卡文件夹", NewScene, typeof(Object), true);
        if (GUILayout.Button("生成对应文件夹"))
        {
            CreateModelFolder();
        }

        NewMaterial = EditorGUILayout.ObjectField("材质球文件夹", NewMaterial, typeof(Object), true);
        NewModel = EditorGUILayout.ObjectField("模型文件夹", NewModel, typeof(Object), true);
        NewPrefab = EditorGUILayout.ObjectField("预制体文件夹", NewPrefab, typeof(Object), true);
        NewTexture = EditorGUILayout.ObjectField("纹理文件夹", NewTexture, typeof(Object), true);
        if (GUILayout.Button("一键贴图改名"))
        {
            CreateTextureName();
        }

        if (GUILayout.Button("生成对应材质球"))
        {
            CreateMaterials();
        }

        if (GUILayout.Button("为材质球赋予贴图"))
        {
            AssignTexturesToMaterials();
        }

        //if (GUILayout.Button("生成对应预制体"))
        //{
        //    GeneratePrefabs();
        //}
        if (GUILayout.Button("将于模型生成在场景中"))
        {
            InstantiateModels();
        }

        //  MapNumber = EditorGUILayout.IntField("地块数量", MapNumber, typeof(Int), true);
        MapNumber = EditorGUILayout.IntField("地块数量", MapNumber);
        if (GUILayout.Button("合成地块"))
        {
            OrganizeModels();
        }

        // MapName = EditorGUILayout.TextField("名字前缀", MapName);
        // if (GUILayout.Button("地块命名"))
        // {
        //     RenameParentObjects();
        // }
        if (GUILayout.Button("将于场景模型设置对应的材质球"))
        {
            SetPrefabsMaterial();
        }

        if (GUILayout.Button("生成预制体"))
        {
            SevaPrefab();
        }

        M = EditorGUILayout.ObjectField("模型文件夹", M, typeof(Object), true);
        if (GUILayout.Button("一键设置初始模型和材质"))
        {
            EnableReadWriteForModels();
        }

        gress = EditorGUILayout.ObjectField("切割植被文件夹", gress, typeof(Object), true);

        if (GUILayout.Button("设置植被"))
        {
           // SetGress();
        }


    }

    //创建对应文件夹
    private void CreateModelFolder()
    {
        if (NewScene != null)
        {
            string Modelfolder = Path.Combine(AssetDatabase.GetAssetPath(NewScene), "Mesh");
            string Texturefolder = Path.Combine(AssetDatabase.GetAssetPath(NewScene), "Texture");
            string Materialfolder = Path.Combine(AssetDatabase.GetAssetPath(NewScene), "Material");
            string Prefabfolder = Path.Combine(AssetDatabase.GetAssetPath(NewScene), "Prefab");
            string Volumefolder = Path.Combine(AssetDatabase.GetAssetPath(NewScene), "Volume");
            Directory.CreateDirectory(Modelfolder);
            Directory.CreateDirectory(Texturefolder);
            Directory.CreateDirectory(Materialfolder);
            Directory.CreateDirectory(Prefabfolder);
            Directory.CreateDirectory(Volumefolder);
            AssetDatabase.Refresh(); // 刷新AssetDatabase，使得新创建的文件夹在Unity中可见
        }
    }



    //贴图改名
    private void CreateTextureName()
    {
        string modelFolderPath = AssetDatabase.GetAssetPath(NewTexture);
        // 获取Model文件夹下的所有文件
        string[] modelFiles =
            AssetDatabase.FindAssets("t:Texture",
                new string[] { modelFolderPath }); //Directory.GetFiles(modelFolderPath);

        for (int i = 0; i < modelFiles.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(modelFiles[i]);
            var model = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            //  Debug.Log(model.name);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            string[] nameParts = model.name.Split('_');



            if (textureImporter != null)
            {
                // 修改TextureImporter设置
                textureImporter.textureType = TextureImporterType.Default;
                textureImporter.mipmapEnabled = false;

                // 设置Android平台的Max Size
                TextureImporterPlatformSettings androidSettings = textureImporter.GetPlatformTextureSettings("Android");
                androidSettings.overridden = true;
                androidSettings.maxTextureSize = 2048;
                textureImporter.SetPlatformTextureSettings(androidSettings);

                // 设置iOS平台的Max Size
                TextureImporterPlatformSettings iOSSettings = textureImporter.GetPlatformTextureSettings("iPhone");
                iOSSettings.overridden = true;
                iOSSettings.maxTextureSize = 2048;
                textureImporter.SetPlatformTextureSettings(iOSSettings);

                // 保存并重新导入
                textureImporter.SaveAndReimport();

                Debug.Log($"Properties set for: {model.name}");
            }

            if (nameParts.Length >= 3)
            {
                string newName = nameParts[0] + "_" + nameParts[1]; //+ "_" + nameParts[2];
                // string newFilePath = Path.Combine(modelFolderPath, newName + ".exr"); // 替换为实际的文件类型


                AssetDatabase.RenameAsset(path, newName);

            }

        }

        AssetDatabase.Refresh();
    }

    //根据模型名字创建材质球
    private void CreateMaterials()
    {
        string modelFolderPath = AssetDatabase.GetAssetPath(NewTexture);
        string materialFolderPath = AssetDatabase.GetAssetPath(NewMaterial);

        // 获取Model文件夹下的所有文件
        string[] modelFiles =
            AssetDatabase.FindAssets("t:Texture",
                new string[] { modelFolderPath }); //Directory.GetFiles(modelFolderPath);
        Debug.Log(modelFiles.Length);
        for (int i = 0; i < modelFiles.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(modelFiles[i]);
            var model = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            //  Debug.Log(model.name);
            // 在Material文件夹中创建相同名称的Material文件
            string materialFilePath = Path.Combine(materialFolderPath, model.name + ".mat");

            if (!File.Exists(materialFilePath))
            {
                // 创建Material文件
                Material material = new Material(Shader.Find("IQIRendering/CelToneShading/Scene(烘焙场景)"));
                AssetDatabase.CreateAsset(material, materialFilePath);
            }

            AssetDatabase.Refresh();
        }


    }

    private void CreateMaterialsFromTextures()
    {
        string textureFolderPath = Path.Combine(Application.dataPath, "Textures");
        string materialFolderPath = Path.Combine(Application.dataPath, "Materials");

        if (!Directory.Exists(textureFolderPath))
        {
            Debug.LogWarning("Textures folder not found at path: " + textureFolderPath);
            return;
        }

        // 获取所有纹理文件名
        string[] textureFiles = Directory.GetFiles(textureFolderPath, "*.png");

        // 创建相同数量和名称的材质文件
        foreach (string textureFile in textureFiles)
        {
            string textureFileName = Path.GetFileNameWithoutExtension(textureFile);
            string materialFilePath = Path.Combine(materialFolderPath, textureFileName + ".mat");

            Material material = new Material(Shader.Find("IQIRendering/CelToneShading/Scene"));
            AssetDatabase.CreateAsset(material, materialFilePath);
            Debug.Log("Material created at path: " + materialFilePath);
        }
    }

    //将贴图赋予对应的材质球
    private void AssignTexturesToMaterials()
    {
        string materialFolderPath = AssetDatabase.GetAssetPath(NewMaterial);
        string textureFolderPath = AssetDatabase.GetAssetPath(NewTexture);

        // 获取Material文件夹下的所有文件
        string[] materialFiles = AssetDatabase.FindAssets("t:Material", new string[] { materialFolderPath });
        string[] textureFiles = AssetDatabase.FindAssets("t:Texture", new[] { textureFolderPath });
        Debug.Log(textureFiles.Length);
        Debug.Log(materialFiles.Length);
        for (int i = 0; i < textureFiles.Length; i++)
        {
            var texpath = AssetDatabase.GUIDToAssetPath(textureFiles[i]);
            var texture = AssetDatabase.LoadAssetAtPath<Texture>(texpath);



            for (int j = 0; j < materialFiles.Length; j++)
            {
                var matpath = AssetDatabase.GUIDToAssetPath(materialFiles[j]);
                var material = AssetDatabase.LoadAssetAtPath<Material>(matpath);

                string[] nameParts = material.name.Split('_');

                if (nameParts.Length >= 3)
                {
                    string newName = nameParts[0] + "_" + nameParts[1] + "_" + nameParts[2];

                    if (newName == texture.name)
                    {
                        material.SetTexture("_LightMap", texture);

                    }
                }
                else
                {
                    if (material.name == texture.name)
                    {
                        material.SetTexture("_LightMap", texture);

                    }
                }


            }



        }
    }


    private void GeneratePrefabs()
    {
        string modelFolderPath = AssetDatabase.GetAssetPath(NewModel);
        string prefabFolderPath = AssetDatabase.GetAssetPath(NewPrefab);
        string[] modelFiles = AssetDatabase.FindAssets("t:Model", new string[] { modelFolderPath });

        for (int i = 0; i < modelFiles.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(modelFiles[i]);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (model != null)
            {
                // 调试输出，确保模型成功加载
                Debug.Log("Model loaded: " + model.name);

                // 生成预制体路径
                string prefabFilePath = Path.Combine(prefabFolderPath, model.name + ".prefab");

                // 调试输出，确保路径正确
                Debug.Log("PrefabFilePath: " + prefabFilePath);

                // 创建预制体
                PrefabUtility.SaveAsPrefabAsset(model, prefabFilePath);
                Debug.Log("Prefab created for model: " + model.name);
            }
            else
            {
                Debug.LogWarning("Failed to load model at path: " + path);
            }
        }

        AssetDatabase.Refresh();
    }

    //将预制体文件生成在场景当中
    private void InstantiateModels()
    {
        string modelFolderPath = AssetDatabase.GetAssetPath(NewModel);
        string[] models = AssetDatabase.FindAssets("t:Model", new[] { modelFolderPath });
        Debug.Log(models.Length);

        for (int i = 0; i < models.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(models[i]);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instantiatedPrefab = PrefabUtility.InstantiatePrefab(model) as GameObject;
            //  Instantiate(model);
        }
    }


    private void SetPrefabsMaterial()
    {
        string materialFolderPath = AssetDatabase.GetAssetPath(NewMaterial);
        string[] materialFiles = AssetDatabase.FindAssets("t:Material", new[] { materialFolderPath });
        var meshs = FindObjectsOfType<MeshRenderer>();

        for (int i = 0; i < materialFiles.Length; i++)
        {

            var path = AssetDatabase.GUIDToAssetPath(materialFiles[i]);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);


            for (int j = 0; j < meshs.Length; j++)
            {
                var mesh = meshs[j];

                // 获取当前物体及其所有子物体的Transform组件
                Transform[] allTransforms = mesh.GetComponentsInChildren<Transform>(true);

                // 遍历每个Transform
                foreach (Transform childTransform in allTransforms)
                {
                    // 获取当前Transform的路径
                    string objectPath = GetObjectPathName(childTransform);

                    // 分割路径，获取第二层的名字
                    string[] pathSegments = objectPath.Split('/');
                    if (pathSegments.Length >= 2)
                    {
                        string secondLayerName = pathSegments[1];



                        if (secondLayerName == mat.name)
                        {

                            var meshrender = mesh.GetComponent<MeshRenderer>();
                            var meshMats = meshrender.sharedMaterials;
                            for (int k = 0; k < meshMats.Length; k++)
                            {
                                meshMats[k] = mat;
                            }

                            meshrender.sharedMaterials = meshMats;
                        }



                    }
                    else if (pathSegments.Length == 1)
                    {
                        string secondLayerName = pathSegments[0];



                        if (secondLayerName == mat.name)
                        {

                            var meshrender = mesh.GetComponent<MeshRenderer>();
                            var meshMats = meshrender.sharedMaterials;
                            for (int k = 0; k < meshMats.Length; k++)
                            {
                                meshMats[k] = mat;
                            }

                            meshrender.sharedMaterials = meshMats;
                        }
                    }
                }


                string GetObjectPathName(Transform objTransform)
                {
                    if (objTransform == null)
                        return "";

                    string path = objTransform.name;

                    while (objTransform.parent != null)
                    {
                        objTransform = objTransform.parent;
                        path = objTransform.name + "/" + path;
                    }

                    return path;
                }
            }


        }
    }



    private void EnableReadWriteForModels()
    {
        string materiPath = AssetDatabase.GetAssetPath(M);
        string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { materiPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (modelImporter != null)
            {
                modelImporter.isReadable = true;
                modelImporter.animationType = ModelImporterAnimationType.Generic; // 将Animation Type改为Generic
                modelImporter.materialLocation =
                    ModelImporterMaterialLocation.External; // 将Materials Location改为UseExternalMaterials(Legacy)
                AssetDatabase.ImportAsset(assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }

    //合成地块
    private void OrganizeModels()
    {
        // 清空之前的空物体列表
        emptyObject.Clear();

        // 查找场景中的所有对象
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // 创建10个空物体并命名为"01"到"10"
        for (int i = 1; i <= MapNumber; i++)
        {
            GameObject empty = new GameObject(i.ToString("D2"));
            emptyObject.Add(empty);
        }

        // 遍历场景中的所有对象
        foreach (GameObject obj in allObjects)
        {
            // 忽略空物体和Editor Window自身的对象
            if (obj.name.Length >= 2 && !emptyObject.Contains(obj))
            {
                // 获取模型名字的最后两个字符
                string lastTwoChars = obj.name.Substring(obj.name.Length - 2);

                // 遍历所有空物体并匹配名字
                for (int i = 0; i < emptyObject.Count; i++)
                {
                    if (lastTwoChars == emptyObject[i].name)
                    {
                        // 将模型设置为匹配空物体的子物体
                        obj.transform.SetParent(emptyObject[i].transform, true);
                        break; // 一旦找到匹配的空物体，就不需要继续循环
                    }
                }
            }
        }

    }

    //保存预制体
    private void SevaPrefab()
    {
        // 为每个空物体创建预制体
        string PrefabPath = AssetDatabase.GetAssetPath(NewPrefab);
        //  string prefabFolderPath = PrefabPath;//"Assets/Prefabs"; // 定义预制体保存路径

        // 获取场景中的所有根物体
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        // 创建预制体文件夹（如果不存在）
        if (!Directory.Exists(PrefabPath))
        {
            Directory.CreateDirectory(PrefabPath);
        }

        foreach (GameObject rootObj in rootObjects)
        {
            // 为每个根物体创建一个预制体
            string prefabPath = Path.Combine(PrefabPath, rootObj.name + ".prefab");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);

            if (prefab != null)
            {
                Debug.Log("Created Prefab: " + prefab.name);
            }
            else
            {
                Debug.LogWarning("Failed to create Prefab for: " + rootObj.name);
            }

            // 替换场景中的对象为预制体实例
            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            prefabInstance.transform.position = rootObj.transform.position;
            prefabInstance.transform.rotation = rootObj.transform.rotation;
            prefabInstance.transform.localScale = rootObj.transform.localScale;

            // 删除原始的根物体
            DestroyImmediate(rootObj);
        }

        // 刷新资源
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }


    //地块命名
    private void RenameParentObjects()
    {
        // 获取场景中所有的根物体（父物体）
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        // 遍历每一个根物体
        foreach (GameObject parentObject in rootObjects)
        {
            // 确保父物体有子物体
            if (parentObject.transform.childCount > 0)
            {
                // 获取第一个子物体
                Transform firstChild = parentObject.transform.GetChild(0);

                // 计算X轴和Z轴的整数值
                int xValue = Mathf.FloorToInt(firstChild.position.x / 20);
                int zValue = Mathf.FloorToInt(firstChild.position.z / 20);

                // 生成新的名字
                string newName = $"{MapName}_{xValue}_{zValue}";

                // 设置父物体的新名字
                parentObject.name = newName;

                // 标记场景为脏，以便保存时 Unity 记录更改
                EditorUtility.SetDirty(parentObject);
            }
        }

        // 刷新场景以显示更改
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    // private void SetGress()
    // {
    //             // 查找所有以 ".asset" 类型结尾的文件
    //     var gresspath = AssetDatabase.GetAssetPath(gress);
    //     string[] assetGUIDs = AssetDatabase.FindAssets("t:Object", new[] { gresspath });
    //   //  var block = (VegetationBlock)ScriptableObject.CreateInstance(typeof(VegetationBlock));
    //     foreach (string guid in assetGUIDs)
    //     {
    //         string assetPath = AssetDatabase.GUIDToAssetPath(guid);
    //         VegetationBlock asset = (VegetationBlock)AssetDatabase.LoadAssetAtPath<Object>(assetPath);
    //
    //         if (asset != null)
    //         {
    //             // 遍历所有场景中的对象
    //             GameObject[] allObjects = FindObjectsOfType<GameObject>();
    //
    //             foreach (GameObject obj in allObjects)
    //             {
    //                 if (obj.name == asset.name)
    //                 {
    //                     // 如果物体名字和 asset 名字相同，添加 VegetationLoader 脚本
    //                     VegetationLoader vegetationLoader = obj.GetComponent<VegetationLoader>();
    //
    //                     if (vegetationLoader == null)
    //                     {
    //                         vegetationLoader = obj.AddComponent<VegetationLoader>();
    //                         vegetationLoader.vegetaion= new VegetationBlock[1] { asset };
    //                     }
    //
    //                     
    //                     
    //                //     为脚本分配 asset 文件
    //                    SerializedObject serializedObject = new SerializedObject(vegetationLoader);
    //                    SerializedProperty assetProperty = serializedObject.FindProperty("vegetaion"); // 确保属性名正确
    //
    //                     // if (assetProperty != null && asset is VegetationBlock)
    //                     // {
    //                     //     // 确保 asset 是正确的 VegetationBlock 类型
    //                     //     List<VegetationBlock> vegetationList = new List<VegetationBlock>(vegetationLoader.vegetaion ?? new VegetationBlock[0]);
    //                     //     vegetationList.Add((VegetationBlock)asset);
    //                     //     vegetationLoader.vegetaion = vegetationList.ToArray();
    //                     //     serializedObject.ApplyModifiedProperties();
    //                     // }
    //
    //                     Debug.Log($"Attached VegetationLoader to {obj.name} and set asset to {asset.name}");
    //                 }
    //             }
    //         }
    //     }
    //
    //     // 刷新资源
    //     AssetDatabase.Refresh();
    // }

}
   
    
    
    
    
    









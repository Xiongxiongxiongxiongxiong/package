using UnityEngine;
using UnityEditor;
using System.IO;

public class IQISetScenes : EditorWindow
{
    Object NewScene;
    Object NewModel;
    Object NewMaterial;
    Object NewPrefab;
    Object NewTexture;
    Object NewVolume;
    Object M;
    [MenuItem("关卡插件/一键制作关卡")]
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
        //  NewPrefab = EditorGUILayout.ObjectField("预制体文件夹", NewPrefab, typeof(Object), true);
        NewTexture = EditorGUILayout.ObjectField("纹理文件夹", NewTexture, typeof(Object), true);
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
        if (GUILayout.Button("将于场景模型设置对应的材质球"))
        {
            SetPrefabsMaterial();
        }
        if (GUILayout.Button("一键贴图改名"))
        {
            CreateTextureName();
        }
        M = EditorGUILayout.ObjectField("模型文件夹", M, typeof(Object), true);
        if (GUILayout.Button("文件夹"))
        {
            EnableReadWriteForModels();
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
        string[] modelFiles = AssetDatabase.FindAssets("t:Texture", new string[] { modelFolderPath });        //Directory.GetFiles(modelFolderPath);
        Debug.Log(modelFiles.Length);
        for (int i = 0; i < modelFiles.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(modelFiles[i]);
            var model = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            //  Debug.Log(model.name);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            string[] nameParts = model.name.Split('_');

            if (nameParts.Length >= 3)
            {
                string newName = nameParts[0] + "_" + nameParts[1] + "_" + nameParts[2];
                string newFilePath = Path.Combine(modelFolderPath, newName + ".exr"); // 替换为实际的文件类型

                AssetDatabase.RenameAsset(path, newName);
                AssetDatabase.ImportAsset(path);

                // 更新TextureImporter设置以确保贴图正确加载
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

    }
    //根据模型名字创建材质球
    private void CreateMaterials()
    {
        string modelFolderPath = AssetDatabase.GetAssetPath(NewTexture);
        string materialFolderPath = AssetDatabase.GetAssetPath(NewMaterial);

        // 获取Model文件夹下的所有文件
        string[] modelFiles = AssetDatabase.FindAssets("t:Texture", new string[] { modelFolderPath });        //Directory.GetFiles(modelFolderPath);
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
                Material material = new Material(Shader.Find("IQIRendering/CelToneShading/Scene"));
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
        Debug.Log(textureFiles.Length); Debug.Log(materialFiles.Length);
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

    //根据模型文件创建对应的预制体文件
    //private void GeneratePrefabs()
    //{
    //    string modelFolderPath = AssetDatabase.GetAssetPath(NewModel);
    //    string prefabFolderPath = AssetDatabase.GetAssetPath(NewPrefab); //       Path.Combine(Application.dataPath, "Prefab");
    //    string[] modelFiles = AssetDatabase.FindAssets("t:Model", new string[] {modelFolderPath}); 
    //    // 获取Model文件夹下的所有文件
    //  //  string[] modelFiles = Directory.GetFiles(modelFolderPath, "*.fbx");

    //  for (int i = 0; i < modelFiles.Length; i++)
    //  {
    //      var path = AssetDatabase.GUIDToAssetPath(modelFiles[i]);
    //      var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);

    //      if (model != null)
    //      {
    //          // 生成预制体路径
    //          string prefabFilePath = Path.Combine(prefabFolderPath, model.name + ".prefab");

    //          // 创建预制体
    //          PrefabUtility.SaveAsPrefabAsset(model, prefabFilePath);
    //          Debug.Log("Prefab created for model: " + model.name);
    //      }


    //  }
    //  AssetDatabase.Refresh();
    //}

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
    //将场景中的预制体文件中的材质球修改为正确
    //private void SetPrefabsMaterial()
    //{
    //    string materialFolderPath = AssetDatabase.GetAssetPath(NewMaterial);
    //    string[] materialFiles = AssetDatabase.FindAssets("t:Material", new[] { materialFolderPath });
    //    var  meshs = FindObjectsOfType<MeshRenderer>();

    //    for (int i = 0; i < materialFiles.Length; i++)
    //    {

    //        var path = AssetDatabase.GUIDToAssetPath(materialFiles[i]);
    //        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);


    //        for (int j = 0; j < meshs.Length; j++)
    //        {
    //            var mesh = meshs[j];
    //            //Debug.Log(mesh.transform.root.GetChild(0).name);
    //            if (mesh.transform.root.GetChild(0).name == mat.name)
    //            {

    //                var meshrender = mesh.GetComponent<MeshRenderer>();
    //                var meshMats = meshrender.sharedMaterials;
    //                for (int k = 0; k < meshMats.Length; k++)
    //                {
    //                    meshMats[k] = mat;
    //                }

    //                meshrender.sharedMaterials = meshMats;
    //            }

    //            //if ( mesh.transform.root.GetChild(1).name == mat.name)
    //            //{
    //            //    Debug.Log(mesh.transform.root.GetChild(1).name);
    //            //    var mseh01 = mesh.transform.root.GetChild(0).GetComponent<MeshRenderer>();
    //            //     var mseh01mat = mseh01.sharedMaterials;
    //            //    for (int l = 0; l < mseh01mat.Length; l++)
    //            //    {
    //            //        mseh01mat[l] = mat;
    //            //    }


    //            //    var meshrender = mesh.GetComponent<MeshRenderer>();
    //            //    var meshMats = meshrender.sharedMaterials;
    //            //    for (int k = 0; k < meshMats.Length; k++)
    //            //    {
    //            //        meshMats[k] = mat;
    //            //    }

    //            //    meshrender.sharedMaterials = meshMats;
    //            //}
    //        }
    //    }
    //}



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



    private  void EnableReadWriteForModels()
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
                modelImporter.materialLocation = ModelImporterMaterialLocation.External; // 将Materials Location改为UseExternalMaterials(Legacy)
                AssetDatabase.ImportAsset(assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }


}








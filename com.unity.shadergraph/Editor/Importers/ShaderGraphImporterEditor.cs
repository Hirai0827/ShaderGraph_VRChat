using System;
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{

    [CustomEditor(typeof(ShaderGraphImporter))]
    class ShaderGraphImporterEditor : ScriptedImporterEditor
    {
        protected override bool needsApplyRevert => false;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Shader Editor"))
            {
                AssetImporter importer = target as AssetImporter;
                Debug.Assert(importer != null, "importer != null");
                ShowGraphEditWindow(importer.assetPath);
            }
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Logging Shader"))
            {
                var importer = target as ShaderGraphImporter;
                Debug.Log(importer.assetPath);
                //Debug.Log(importer.ShaderText);
                Debug.Log(ShaderGraphImporter.GetShaderText(importer.assetPath, out var _));
            }
            
            
            if (GUILayout.Button("Export Shader"))
            {
                var importer = target as ShaderGraphImporter;
                Debug.Log(importer.assetPath);
                //Debug.Log(importer.ShaderText);
                var shaderText = ShaderGraphImporter.GetShaderText(importer.assetPath, out var _);
                var path = OpenAndSelectPath("", "shader");
                System.IO.File.WriteAllBytes(path, System.Text.Encoding.UTF8.GetBytes(shaderText));
                AssetDatabase.Refresh();
            }

            GUILayout.EndHorizontal();

            ApplyRevertGUI();
        }

        internal static bool ShowGraphEditWindow(string path)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
                return false;
            // Path.GetExtension returns the extension prefixed with ".", so we remove it. We force lower case such that
            // the comparison will be case-insensitive.
            extension = extension.Substring(1).ToLowerInvariant();
            if (extension != ShaderGraphImporter.Extension && extension != ShaderSubGraphImporter.Extension)
                return false;

            foreach (var w in Resources.FindObjectsOfTypeAll<MaterialGraphEditWindow>())
            {
                if (w.selectedGuid == guid)
                {
                    w.Focus();
                    return true;
                }
            }

            var window = EditorWindow.CreateWindow<MaterialGraphEditWindow>(typeof(MaterialGraphEditWindow), typeof(SceneView));
            window.Initialize(guid);
            window.Focus();
            return true;
        }

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var path = AssetDatabase.GetAssetPath(instanceID);
            return ShowGraphEditWindow(path);
        }
        
        public string OpenAndSelectPath(string initName,string extName)
        {
        
    
            // 推奨するディレクトリがあればpathに入れておく
            var path = "";
            string ext = extName;
        
            if (string.IsNullOrEmpty(path) || System.IO.Path.GetExtension(path) != "." + ext)
            {
                // 推奨する保存パスがないときはシーンのディレクトリをとってきたりする（用途次第）
                if (string.IsNullOrEmpty(path)) {
                    path = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
                    if (!string.IsNullOrEmpty(path)) {
                        path = System.IO.Path.GetDirectoryName(path);
                    }
                }
                if (string.IsNullOrEmpty(path)) {
                    path = "Assets";
                }
            }

            // ディレクトリがなければ作る
            else if (System.IO.Directory.Exists(path) == false) {
                System.IO.Directory.CreateDirectory(path);
            }

            // ファイル保存パネルを表示
            var fileName = initName +"." + ext;
            fileName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(path, fileName)));
            path = EditorUtility.SaveFilePanelInProject("Save Some Asset", fileName, ext, "", path);
            return path;
        }
    }
}

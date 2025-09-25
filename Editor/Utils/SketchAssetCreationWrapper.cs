using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SketchRenderer.Editor.Utils
{
    public static class SketchAssetCreationWrapper
    {
        public static string GetAssetPath(UnityEngine.Object asset)
        {
            if(AssetDatabase.Contains(asset))
            {
                return AssetDatabase.GetAssetPath(asset);
            }
            
            return null;
        }
        
        internal static bool TryValidateOrCreateAssetPath(string path)
        {
            if(string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            
            if (AssetDatabase.IsValidFolder(path))
                return true;

            string pathRoot = string.Empty;
            if (Path.IsPathRooted(path))
                pathRoot = Path.GetPathRoot(path);
            string[] directories = path.Split(new[] { Path.PathSeparator, Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);
            if(directories.Length == 0 || string.IsNullOrEmpty(directories[0]))
                throw new ArgumentNullException(nameof(path));
            
            if(pathRoot != string.Empty)
                directories[0] = pathRoot;
            
            if(directories[0] != "Assets")
                throw new UnityException($"Invalid path, must begin at Assets folder. Given path: {path}");
            
            //create the full path until it exists
            string currentDirectoryPath = directories[0];
            for (int i = 1; i < directories.Length; i++)
            {
                string nextDirectoryPath = Path.Combine(currentDirectoryPath, directories[i]);
                
                if (!AssetDatabase.IsValidFolder(nextDirectoryPath))
                {
                    AssetDatabase.CreateFolder(currentDirectoryPath, directories[i]);
                    AssetDatabase.Refresh();
                }
   
                currentDirectoryPath = nextDirectoryPath;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            //final validation
            return AssetDatabase.IsValidFolder(currentDirectoryPath);
        }

        internal static string ConvertToAssetsPath(string path)
        {
            if(string.IsNullOrEmpty(path))
                return string.Empty;
            
            string pathRoot = string.Empty;
            if (Path.IsPathRooted(path))
                pathRoot = Path.GetPathRoot(path);
            string[] directories = path.Split(new[] { Path.PathSeparator, Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);
            if(directories.Length == 0 || string.IsNullOrEmpty(directories[0]))
                throw new ArgumentNullException(nameof(path));
            
            if(pathRoot != string.Empty)
                directories[0] = pathRoot;
            
            if(directories[0] == "Assets")
                return path;
            
            string finalPath = string.Empty;
            for (int i = 0; i < directories.Length; i++)
            {
                if (string.IsNullOrEmpty(finalPath) && directories[i] == "Assets")
                {
                    finalPath = directories[i];
                }
                else if (!string.IsNullOrEmpty(finalPath))
                {
                    finalPath = Path.Combine(finalPath, directories[i]);
                }
            }

            if (string.IsNullOrEmpty(finalPath))
                throw new UnityException($"Invalid path, couldn't find Assets directory: {path}");
            
            return finalPath;
        }

        internal static T CreateScriptableInstance<T>(string path, bool forceFocus = true) where T : ScriptableObject
        {
            try
            {
                string validatedPath = ConvertToAssetsPath(path);
                TryValidateOrCreateAssetPath(validatedPath);

                T asset = ScriptableObject.CreateInstance<T>();
                
                string typeName = typeof(T).Name;
                
                string assetName = typeName + ".asset";
                int copiesCount = 0;
                while (AssetDatabase.AssetPathExists(validatedPath + "/" +  assetName))
                {
                    copiesCount++;
                    assetName = $"{typeName}_{copiesCount}.asset";
                }
                string assetPath = validatedPath + "/" +  assetName;
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                T assetInProject = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (forceFocus)
                {
                    Selection.activeObject = assetInProject;
                    EditorGUIUtility.PingObject(assetInProject);
                    EditorUtility.FocusProjectWindow();
                }
                return assetInProject;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}
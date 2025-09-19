namespace SketchRenderer.Runtime.Data
{
    public static class SketchRendererData
    {
        public static readonly string PackagePath = "Packages/com.lorenzogrando.sketchrenderer";
        public static readonly string PackageDisplayName = "Sketch Renderer";
        
        //Unity attribute paths
        public const string PackageMenuItemPath = "Tools/Sketch Renderer/";
        public const string PackageMenuTextureToolSubPath = "Texture Tools/";
        
        public const string PackageAssetItemPath = "Sketch Renderer/Scriptable Objects/";
        public const string PackageInspectorVolumePath = "Sketch Renderer/";
        
        public const string PackageProjectSettingsPath = "Project/Sketch Renderer";
        
        //Default data holders
        public static readonly string DefaultSketchRendererContextPackagePath = PackagePath + "/Runtime/Data/DefaultScriptables/SketchRendererContext.asset"; 
        public static readonly string DefaultSketchResourceAssetPackagePath = PackagePath + "/Runtime/Data/DefaultScriptables/SketchResourceAsset.asset";
        
        public static readonly string DefaultSketchManagerSettingsPackagePath = PackagePath + "/Editor/Data/DefaultScriptables/SketchManagerSettings.asset";
        

        public static readonly string DefaultPackageAssetDirectoryPath = "Assets/" + PackageDisplayName;
    }
}
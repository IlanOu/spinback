using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.ShortcutManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx.Editor
{
    public static class Installer
    {
#if VOL_FX
        // [InitializeOnLoadMethod]
        public static void Restore()
        {
            // if (Application.isPlaying)
            //     return;
            
            // restore function - delete keyword if needed
            RemoveDefines("VOL_FX");
        }

        public class ScriptCatcherCallback : AssetPostprocessor
        {
            public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (deletedAssets != null && deletedAssets.Length != 0)
                {
                    if (HasVolFx() == false && HasVolFxPackage() == false && deletedAssets.Any(n => n.EndsWith("VolFx.asmdef")) && HasVolFxPackageAsm() == false)
                        EditorApplication.delayCall += Restore;
                }
            }
        }
#endif

#if !VOL_FX
        public static Type _urpFeatureType = typeof(FlowFx);

        [InitializeOnLoadMethod]
        public static void PackageTracker()
        {
            AssetDatabase.importPackageCompleted += n =>
            {
                EditorApplication.delayCall += Supress;
            };
        }
        
        public static void Supress()
        {
            if (Application.isPlaying)
                return;
            
            // restore function - add keyword if needed
            if (HasVolFx())
                AddDefine("VOL_FX");
        }

        [DidReloadScripts]
        public static void UrpCheckDelayed()
        {
            EditorApplication.delayCall += UrpCheck;
        }
        
        public static void UrpCheck()
        {
            if (Application.isPlaying)
                return;
            
            if (FileLatch.Has("UrpHelpOff"))
                return;
            
            if (UrpBuilder.HasPipeline())
                return;

            var answer = EditorUtility.DisplayDialogComplex($"• {_urpFeatureType.Name} by NullTale",
                                                            //$"{_urpFeatureType.Name} can setup Urp for you /\n" +
                                                            $"The Project do not configured for Universal Render Pipeline, " +
                                                            $"it required for {_urpFeatureType.Name} in order to work \n \n" +
                                                            "• Do you want to configure project to Urp automatically?",
                                                            "Yes, setup Urp", "Not now, I configure project myself", "Do not show again");
            
            switch (answer)
            {
                // setup Urp
                case 0:
                {
                    UrpBuilder.CreateUrp((asset) =>
                    {
                        GraphicsSettings.defaultRenderPipeline = asset;
                        QualitySettings.renderPipeline       = null;
#pragma warning disable CS4014
                        _delayedCheck();
#pragma warning restore CS4014
                        
                        Debug.Log("Urp asset was created", asset);
                        
                        async Task _delayedCheck()
                        {
                            // delayed check for an render feature
                            await Task.Yield();
                            
                            AssetDatabase.SaveAssets();

                            await Task.Yield();
                            await Task.Yield();
                            
                            FeatureCheck();
                        }
                    });
                } break;
                // delayed
                case 1:
                {
                    // pass, do nothing
                } break;
                // no
                case 2:
                {
                    // never again
                    FileLatch.Lock("UrpHelpOff");
                } break;
            }
        }
        
        [DidReloadScripts]
        public static void FeatureCheck()
        {
            if (FileLatch.Has("FeatHelpOff"))
                return;

            if (UrpBuilder.HasPipeline() == false)
                return;

            if (UrpBuilder.HasFeature(_urpFeatureType))
                return;
            

            var answer = EditorUtility.DisplayDialogComplex($"• {_urpFeatureType.Name} by NullTale ☄",
                                                            $"Active Urp Renderer does not contain {_urpFeatureType.Name} render feature. It required to perform effect logic. \n \n" +
                                                            $"• Do you want to add {_urpFeatureType.Name} to current UrpRenderer?",
                                                            "Yes", "Not now", "Do not show again");
            switch (answer)
            {
                // add feature
                case 0:
                {
                    try
                    {
#pragma warning disable CS4014
                        _delayedCheck();
#pragma warning restore CS4014
                        
                        async Task _delayedCheck()
                        {
                            // delayed check for an render feature
                            await Task.Yield();
                            
                            AssetDatabase.SaveAssets();

                            await Task.Yield();
                            await Task.Yield();
                            
                            UrpBuilder.CreateFeature(_urpFeatureType, n =>
                            {
                                var pipline = UrpBuilder.GetDefaultPipline();
                                var data    = UrpBuilder._getDefaultRenderer(pipline as UniversalRenderPipelineAsset);
                                Selection.activeObject = data;
                                EditorGUIUtility.PingObject(data);
                                
                                n.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                                //EditorUtility.DisplayDialog($"• {_urpFeatureType.Name} by NullTale", $"{_urpFeatureType.Name} was added to the Urp asset.\nYou can control it via Volume profile like common post processing effect.", "Ok");
                                Debug.Log($"<color=white>• {_urpFeatureType.Name} was added to the Urp asset. \nNow, you can control it via Volume profile like common post processing effect.</color>", n);
                            });
                            
                            await Task.Yield();
                            await Task.Yield();
                            AssetDatabase.SaveAssets();
                        }
                    }
                    catch (Exception e)
                    {
                        EditorUtility.DisplayDialog($"• {_urpFeatureType.Name} by NullTale", $"Oops, something goes wrong :\n" +
                                                                                             $"It can be related to the Unity Api changes or specific project environment, please configure Urp manually", "Ok");
                        
                        Debug.LogError(e.Message);
                        throw;
                    }
                } break;
                // skip for now
                case 1:
                {
                    // pass, do nothing
                } break;
                // never again
                case 2:
                {
                    FileLatch.Lock("FeatHelpOff");
                } break;
            }
        }
#endif
        
        private static bool HasVolFx()
        {
            var sep = Path.DirectorySeparatorChar;
            var asm = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>($"Assets{sep}VolFx{sep}VolFx{sep}Runtime{sep}VolFx.asmdef");
            return asm != null;
            // var asmName = "VolFx, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            // var asm     = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == asmName);
            // return asm != null;
        }

        public static bool HasVolFxPackage()
        {
#if !UNITY_2021_1_OR_NEWER
            return false;
#else
            return UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Any(n => n.name == "VolFx");
#endif
        }
        
        public static bool HasVolFxPackageAsm()
        {
            return AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")
                         .Select(n => AssetDatabase.GUIDToAssetPath(n))
                         .Select(n => AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(n))
                         .Any(n => n.text == "VolFx");
        }
        
        private static void RemoveDefines(params string[] defines)
        {
            var definesList = GetDefines().ToList();
            foreach (var def in defines)
            {
                if (definesList.Contains(def))
                {
                    definesList.Remove(def);
                    SetDefines(definesList);
                }
            }
        }

        private static List<string> GetDefines()
        {
            var target           = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var defines          = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return defines.Split(';').ToList();
        }

        private static void AddDefine(string define)
        {
            var defs = GetDefines();
            if (defs.Contains(define))
                return;
            
            defs.Add(define);
            SetDefines(defs);
        }

        private static void SetDefines(List<string> definesList)
        {
            var target           = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var defines          = string.Join(";", definesList.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }
    }

    public static class UrpBuilder
    {
        public static void CreateFeature(Type type, Action<ScriptableRendererFeature> onComplete)
        {
            var feature  = ScriptableObject.CreateInstance(type) as ScriptableRendererFeature;
            feature.name = type.Name;
                
            AddRenderFeature(feature);
            
            var so        = new SerializedObject(feature);
            var property  = so.FindProperty("_pass");
            var fieldType = feature.GetType().GetField("_pass").FieldType;
            var pass      = ScriptableObject.CreateInstance(fieldType);
            pass.hideFlags    = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            feature.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            AssetDatabase.AddObjectToAsset(pass, property.serializedObject.targetObject);
            property.objectReferenceValue = pass;

            EditorUtility.SetDirty(property.serializedObject.targetObject);
            EditorUtility.SetDirty(pass);

            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            feature.Create();
            
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.SaveAssets();
                onComplete?.Invoke(feature);
            };
        }
        
        public static void CreateUrp(Action<UniversalRenderPipelineAsset> onComplete)
        {
            var pathName = $"Assets{Path.DirectorySeparatorChar}UrpAsset.asset";
            
#if !UNITY_2021_1_OR_NEWER
            var asset = Create(CreateRendererAsset(pathName, RendererType.ForwardRenderer));
#else
            var asset = Create(CreateRendererAsset(pathName, RendererType.UniversalRenderer));
#endif
            AssetDatabase.CreateAsset(asset, pathName);
            onComplete?.Invoke(asset);
            
            // var builder = ScriptableObject.CreateInstance<CreateUniversalPipelineAsset>();
            // builder._onComplete = onComplete;
            // ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, builder, "UrpAsset.asset", null, null);
        }
        
        public static bool HasPipeline()
        {
            return GetPipline() as UniversalRenderPipelineAsset != null;
        }
        
        public static bool HasFeature(Type type)
        {
            var asset = GetPipline() as UniversalRenderPipelineAsset;
            if (asset == null)
                return false;

            // Do NOT use asset.LoadBuiltinRendererData().
            // It's a trap, see: https://github.com/Unity-Technologies/Graphics/blob/b57fcac51bb88e1e589b01e32fd610c991f16de9/Packages/com.unity.render-pipelines.universal/Runtime/Data/UniversalRenderPipelineAsset.cs#L719
            var data = GetDefaultRenderer(asset);
            
            return data.rendererFeatures.Any(type.IsInstanceOfType);
        }
        
        private static ScriptableRendererData GetDefaultRenderer(UniversalRenderPipelineAsset asset)
        {
            if (asset)
            {
                var rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                                                                 .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance)
                                                                 .GetValue(asset);
                var defaultRendererIndex = _getDefaultRendererIndex(asset);

                return rendererDataList[defaultRendererIndex];
            }
            
            Debug.LogError("No Universal Render Pipeline is currently active.");
            return null;

            // -----------------------------------------------------------------------
            int _getDefaultRendererIndex(UniversalRenderPipelineAsset asset)
            {
                return (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asset);
            }
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        private class CreateUniversalPipelineAsset : EndNameEditAction
        {
            public Action<UniversalRenderPipelineAsset> _onComplete;
            
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                
                Debug.Log(pathName);
                
#if !UNITY_2021_1_OR_NEWER
                var asset = Create(CreateRendererAsset(pathName, RendererType.ForwardRenderer));
#else
                var asset = Create(CreateRendererAsset(pathName, RendererType.UniversalRenderer));
#endif
                AssetDatabase.CreateAsset(asset, pathName);
                _onComplete?.Invoke(asset);
            }
        }
        
        public static RenderPipelineAsset GetPipline()
        {
            var result = QualitySettings.renderPipeline;
            if (result == null)
                result = GetDefaultPipline();
            
            return result;
        }
        
        public static RenderPipelineAsset GetDefaultPipline()
        {
#if UNITY_6000_0_OR_NEWER
            
            return GraphicsSettings.defaultRenderPipeline;
#else
            return  GraphicsSettings.renderPipelineAsset;
#endif
        }
        
        private static UniversalRenderPipelineAsset Create(ScriptableRendererData data = null)
        {
            // Create Universal RP Asset
            var urpAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            var field = urpAsset.GetType().GetField("m_RendererDataList", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
            if (data != null)
                field.SetValue(urpAsset, new ScriptableRendererData[] { data });
            else
            {
#if !UNITY_2021_1_OR_NEWER
                field.SetValue(urpAsset, new ScriptableRendererData[] { ScriptableObject.CreateInstance<ForwardRendererData>() });
#else
                field.SetValue(urpAsset, new ScriptableRendererData[] { ScriptableObject.CreateInstance<UniversalRendererData>() });
#endif
            }

            // Initialize default Renderer
            //instance.m_EditorResourcesAsset = instance.editorResources;

            // Only enable for new URP assets by default
            //instance.m_ConservativeEnclosingSphere = true;

            return urpAsset;
        }
        
        private static ScriptableRendererData CreateRendererAsset(string path, RendererType type, bool relativePath = true, string suffix = "Renderer")
        {
            var data     = CreateRendererData(type);
            var dataPath = relativePath ? $"{Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))}_{suffix}{Path.GetExtension(path)}" : path;

            Debug.Log(dataPath);
            AssetDatabase.CreateAsset(data, dataPath);
                
            return data;
        }
        
        private static ScriptableRendererData CreateRendererData(RendererType type)
        {
            
#if !UNITY_2021_1_OR_NEWER
            switch (type)
            {
                case RendererType.ForwardRenderer:
                default:
                {
                    var rendererData = ScriptableObject.CreateInstance<ForwardRendererData>();
                    rendererData.postProcessData = GetDefaultPostProcessData();
                    return rendererData;
                }
            }
#else
            switch (type)
            {
                case RendererType.UniversalRenderer:
                default:
                {
                    var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                    rendererData.postProcessData = GetDefaultPostProcessData();
                    return rendererData;
                }
            }
#endif
        }
        
        private static void AddRenderFeature(ScriptableRendererFeature feature)
        {
            var pipline = GetDefaultPipline();
            var data    = _getDefaultRenderer(pipline as UniversalRenderPipelineAsset);
            if (data == null)
                return;

            // Let's mirror what Unity does.
            var serializedObject = new SerializedObject(data);

            var renderFeaturesProp = serializedObject.FindProperty("m_RendererFeatures"); // Let's hope they don't change these.
            var renderFeaturesMapProp = serializedObject.FindProperty("m_RendererFeatureMap");

            serializedObject.Update();

            // Store this new effect as a sub-asset so we can reference it safely afterwards.
            // Only when we're not dealing with an instantiated asset
            if (EditorUtility.IsPersistent(data))
                AssetDatabase.AddObjectToAsset(feature, data);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out var guid, out long localId);

            // Grow the list first, then add - that's how serialized lists work in Unity
            renderFeaturesProp.arraySize++;
            var componentProp = renderFeaturesProp.GetArrayElementAtIndex(renderFeaturesProp.arraySize - 1);
            componentProp.objectReferenceValue = feature;

            // Update GUID Map
            renderFeaturesMapProp.arraySize++;
            var guidProp = renderFeaturesMapProp.GetArrayElementAtIndex(renderFeaturesMapProp.arraySize - 1);
            guidProp.longValue = localId;

            // Force save / refresh
            if (EditorUtility.IsPersistent(data))
                AssetDatabase.SaveAssetIfDirty(data);

            serializedObject.ApplyModifiedProperties();
            
            // Debug.Log($"The feature {feature.GetType().Name} was created at {data.name} (click to inspect)", data);
        }

        public static ScriptableRendererData _getDefaultRenderer(UniversalRenderPipelineAsset asset)
        {
            if (asset == null)
                return null;
                    
            var rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                                                             .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance)
                                                             .GetValue(asset);
            var defaultRendererIndex = _getDefaultRendererIndex(asset);

            return rendererDataList[defaultRendererIndex];
            
            int _getDefaultRendererIndex(UniversalRenderPipelineAsset asset)
            {
                return (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asset);
            }
        }
        
        private static PostProcessData GetDefaultPostProcessData()
        {
            var path = Path.Combine(UniversalRenderPipelineAsset.packagePath, "Runtime/Data/PostProcessData.asset");
            return AssetDatabase.LoadAssetAtPath<PostProcessData>(path);
        }
    }
    
#if !VOL_FX
    public static class FileLatch
    {
        public static string k_FilePrefix  = "Latch";
        public static string k_FilePostfix = $"{Installer._urpFeatureType.Name}.json";
        public static string k_PrefsPath   = $"ProjectSettings{Path.DirectorySeparatorChar}";
        
        // =======================================================================
        public static void Lock(string name)
        {
            File.Create($"{k_PrefsPath}{k_FilePrefix}{name}_{k_FilePostfix}");
        }
        
        public static bool Has(string name)
        {
            return File.Exists($"{k_PrefsPath}{k_FilePrefix}{name}_{k_FilePostfix}");
        }
    }
#endif
}
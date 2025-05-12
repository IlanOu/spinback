#if !VOL_FX

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    public static partial class VolFx
    {
        [Serializable]
        public abstract class Pass : ScriptableObject
        {
            [SerializeField] [HideInInspector]
            private  Shader            _shader;
            protected Material         _material;
            private   bool             _isActive;
            
            public            VolumeStack Stack   => VolumeManager.instance.stack;
            protected virtual bool        Invert  => false;
            protected virtual int         MatPass => 0;
            
            // =======================================================================
            internal bool IsActiveCheck
            {
                get => _isActive && _material != null;
                set => _isActive = value;
            }
            
            public abstract string ShaderName { get; }
            
            internal void _init()
            {
#if UNITY_EDITOR
                if (_shader == null || _material == null)
                {
                    var sna = (GetType().GetCustomAttributes(typeof(ShaderNameAttribute), true).FirstOrDefault() as ShaderNameAttribute);
                    var shaderName = sna == null ? ShaderName : sna._name;
                    if (string.IsNullOrEmpty(shaderName) == false)
                    {
                        _shader   = Shader.Find(shaderName);
                        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_shader);
                        if (_editorValidate && string.IsNullOrEmpty(assetPath) == false) 
                            _editorSetup(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath));

                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
#endif
                
                if (_shader != null)
                    _material = new Material(_shader);
                
                Init();
            }

            /// <summary>
            /// called to init resources
            /// </summary>
            public virtual void Init(InitApi initApi)
            {
            }
            
            /// <summary>
            /// called to perform rendering
            /// </summary>
            public virtual void Invoke(RTHandle source, RTHandle dest, CallApi callApi)
            {
                callApi.Blit(source, dest, _material, MatPass);
            }

            public void Validate()
            {
#if UNITY_EDITOR
                if (_shader == null || _editorValidate)
                {
                    var shaderName = GetType().GetCustomAttributes(typeof(ShaderNameAttribute), true).FirstOrDefault() as ShaderNameAttribute;
                    if (shaderName != null)
                    {
                        _shader = Shader.Find(shaderName._name);
                        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(_shader);
                        if (string.IsNullOrEmpty(assetPath) == false)
                            _editorSetup(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath));
                        
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
                
                if ((_material == null || _material.shader != _shader) && _shader != null)
                {
                    _material = new Material(_shader);
                    Init();
                }
#endif
                
                IsActiveCheck = Validate(_material);
            }

            /// <summary>
            /// called to initialize pass when material is created
            /// </summary>
            public virtual void Init()
            {
            }

            /// <summary>
            /// called each frame to check is render is required and setup render material
            /// </summary>
            public abstract bool Validate(Material mat);
            
            /// <summary>
            /// frame clean up function used if implemented custom Invoke function to release resources
            /// </summary>
            public virtual void Cleanup(CommandBuffer cmd)
            {
            }
            
            /// <summary>
            /// used for optimization purposes, returns true if we need to call _editorSetup function
            /// </summary>
            protected virtual bool _editorValidate => false;
            
            /// <summary>
            /// editor validation function, used to gather additional references 
            /// </summary>
            protected virtual void _editorSetup(string folder, string asset)
            {
            }
        }
    }
}

#endif
#if !VOL_FX

#if UNITY_6000_0_OR_NEWER
#define UNITY_RENDER_GRAPH
#else
#define UNITY_LEGACY
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_RENDER_GRAPH
using UnityEngine.Rendering.RenderGraphModule;
#endif

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
#if !UNITY_2021_1_OR_NEWER
    [DisallowMultipleRendererFeature()]
#else
    [DisallowMultipleRendererFeature("FlowFx")]
#endif
    public class FlowFx : ScriptableRendererFeature
    {
        protected static List<ShaderTagId> k_ShaderTags;
        
        public static int s_BlitTexId       = Shader.PropertyToID("_BlitTexture");
        public static int s_BlitScaleBiasId = Shader.PropertyToID("_BlitScaleBias");
        
#if !UNITY_2021_1_OR_NEWER
        [Tooltip("When to execute")]
        public RenderPassEvent _event  = RenderPassEvent.BeforeRenderingPostProcessing;
#else
        [Tooltip("When to execute")]
        public RenderPassEvent _event  = RenderPassEvent.AfterRenderingPostProcessing;
#endif
        
        public FlowPass _pass;
        
        [HideInInspector]
        public Shader _blitShader;

        [NonSerialized]
        public Material _blit;
        
        [NonSerialized]
        public PassExecution _execution;

        // =======================================================================
        public class PassExecution : ScriptableRenderPass
        {
            public  FlowFx        _owner;
            private RenderTarget  _output;
            
#if UNITY_RENDER_GRAPH
            private VolFx.InitApiRg _initApiRg;
            private VolFx.CallApiRg _callApiRg;
#endif
            
            private VolFx.InitApiLeg _initApiLeg;
            private VolFx.CallApiLeg _callApiLeg;
            private ProfilingSampler _profiler;
            
            // =======================================================================
#if UNITY_RENDER_GRAPH
            public class PassData
            {
                public TextureHandle _camera;
                public TextureHandle _buffer;
            }
#endif
            // =======================================================================
            public void Init()
            {
                renderPassEvent = _owner._event;
                
                _output = new RenderTarget().Allocate(_owner.name);
                
#if UNITY_RENDER_GRAPH
                _initApiRg = new VolFx.InitApiRg();
                _callApiRg = new VolFx.CallApiRg();
#endif
                _initApiLeg = new VolFx.InitApiLeg();
                _callApiLeg = new VolFx.CallApiLeg();
                
                _profiler = new ProfilingSampler(_owner.name);
            }
            
#if UNITY_RENDER_GRAPH
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var camData = frameData.Get<UniversalCameraData>();
                var resData = frameData.Get<UniversalResourceData>();
                    
                ref var camRtDesc = ref camData.cameraTargetDescriptor;
                var     width     = camRtDesc.width;
                var     height    = camRtDesc.height;

                var _initApi = _initApiRg;
                var _callApi = _callApiRg;
                
                _initApi.Width  = width;
                _initApi.Height = height;
                
                _owner._pass.Validate();
                if (_owner._pass.IsActiveCheck == false)
                    return;
                
                using (var builder = renderGraph.AddUnsafePass<PassData>(passName, out var passData, _profiler))
                {
                    var camDesc = new TextureDesc(camRtDesc.width, camRtDesc.height, false, false);
                    camDesc.format = camData.cameraTargetDescriptor.graphicsFormat;
                    camDesc.depthBufferBits = 0;
                    
                    // command buffer
                    _initApi._builder = builder;
                    _initApi._frameData = frameData;
                    _initApi._renderGraph = renderGraph;
                    
                    _owner._pass.Init(_initApi);
                    
                    _callApi._cam = camData.camera;
                    _callApi._blit = _owner._blit;
                    _callApi._cam = camData.camera;
                    
                    passData._camera = resData.cameraColor;
                    passData._buffer = builder.CreateTransientTexture(in camDesc);
                    builder.UseTexture(passData._buffer, AccessFlags.ReadWrite);
                    
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => Execute(data, context));
                }
            }
            
            private void Execute(PassData data, UnsafeGraphContext context)
            {
                var cmd = context.cmd;
                var _callApi = _callApiRg;
                
                _callApi._cmd  = context.cmd;
                
                var pass = _owner._pass;
                pass.Invoke(data._camera, data._buffer, _callApi);
                _callApi.Blit(data._buffer, data._camera);
            }
            
            [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                _owner._pass.Validate();
                if (_owner._pass.IsActiveCheck == false)
                    return;
                
                // allocate stuff
                var cmd = CommandBufferPool.Get(_owner.name);
                ref var cameraData = ref renderingData.cameraData;
                ref var desc = ref cameraData.cameraTargetDescriptor;
                _output.Get(cmd, in desc);
                
                var _initApi = _initApiLeg;
                var _callApi = _callApiLeg;
                
                _initApi.Width  = cameraData.cameraTargetDescriptor.width;
                _initApi.Height = cameraData.cameraTargetDescriptor.height;
                
                _initApi._cmd = cmd;
                _callApi._cmd = cmd;

                var source = _getCameraTex(ref renderingData);
                
                // init pass content
                _owner._pass.Init(_initApi);
                
                // draw post process
                _owner._pass.Invoke(source, _output.Handle, _callApi);
                _owner.Blit(cmd, _output.Handle, source);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                // -----------------------------------------------------------------------
                RTHandle _getCameraTex(ref RenderingData renderingData)
                {
                    ref var cameraData = ref renderingData.cameraData;
#if UNITY_2022_1_OR_NEWER                
                    return cameraData.renderer.cameraColorTargetHandle;
#else
                    return RTHandles.Alloc(cameraData.renderer.cameraColorTarget);
#endif
                }
            }
#endif
            
#if UNITY_LEGACY
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                _owner._pass.Validate();
                if (_owner._pass.IsActiveCheck == false)
                    return;
                
                // allocate stuff
                var cmd = CommandBufferPool.Get(_owner.name);
                ref var cameraData = ref renderingData.cameraData;
                ref var desc = ref cameraData.cameraTargetDescriptor;
                _output.Get(cmd, in desc);
                
                var _initApi = _initApiLeg;
                var _callApi = _callApiLeg;
                
                _initApi.Width  = cameraData.cameraTargetDescriptor.width;
                _initApi.Height = cameraData.cameraTargetDescriptor.height;
                
                _initApi._cmd = cmd;
                _callApi._cmd = cmd;

                var source = _getCameraTex(ref renderingData);
                
                // init pass content
                _owner._pass.Init(_initApi);
                
                // draw post process
                _owner._pass.Invoke(source, _output.Handle, _callApi);
#if !UNITY_2022_1_OR_NEWER
                cmd.SetGlobalVector(s_BlitScaleBiasId, new Vector4(1, 1, 0));
                cmd.SetGlobalTexture("_SourceTex", _output.Handle);
                cmd.SetRenderTarget(source, 0);
                var invVp = (renderingData.cameraData.GetProjectionMatrix() * renderingData.cameraData.GetViewMatrix()).inverse;
                if (renderingData.cameraData.IsCameraProjectionMatrixFlipped())
                    invVp.m11 = -invVp.m11;
                cmd.DrawMesh(Utils.FullscreenMesh, invVp, _owner._blit, 0, 0);
#else
                _owner.Blit(cmd, _output.Handle, source);
#endif

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                // -----------------------------------------------------------------------
                RTHandle _getCameraTex(ref RenderingData renderingData)
                {
                    ref var cameraData = ref renderingData.cameraData;
#if UNITY_2022_1_OR_NEWER                
                    return cameraData.renderer.cameraColorTargetHandle;
#else
                    return RTHandles.Alloc(cameraData.renderer.cameraColorTarget);
#endif
                }
            }
#endif
            
            public override void FrameCleanup(CommandBuffer cmd)
            {
                _output.Release(cmd);
                _output.Release(cmd);
                _owner._pass.Cleanup(cmd);
            }
        }
        
        // =======================================================================
        public void Blit(CommandBuffer cmd, RTHandle source, RTHandle destination)
        {
            cmd.SetGlobalVector(s_BlitScaleBiasId, new Vector4(1, 1, 0));
            cmd.SetGlobalTexture(s_BlitTexId, source);
            cmd.SetRenderTarget(destination, 0);
            cmd.DrawMesh(Utils.FullscreenMesh, Matrix4x4.identity, _blit, 0, 0);
        }
        
        public override void Create()
        {
#if UNITY_EDITOR
            _blitShader = Shader.Find("Hidden/Universal Render Pipeline/Blit");
            
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            _blit      = new Material(_blitShader);
            _execution = new PassExecution(){ _owner = this };
            _execution.Init();
            
            if (_pass != null)
                _pass._init();
            
            if (k_ShaderTags == null)
            {
                k_ShaderTags = new List<ShaderTagId>(new[]
                {
                    new ShaderTagId("SRPDefaultUnlit"),
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly")
                });
            }
        }
        private void Reset()
        {
#if UNITY_EDITOR
            if (_pass != null)
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(_pass);
                UnityEditor.AssetDatabase.SaveAssets();
                _pass = null;
            }
#endif
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;
            
            if (_blit == null)
                _blit = new Material(_blitShader);
            
            if (_pass == null)
                return;
#endif
            renderer.EnqueuePass(_execution);
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (_pass != null)
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(_pass);
                UnityEditor.AssetDatabase.SaveAssets();
                _pass = null;
            }
#endif
        }
    }
}

#endif
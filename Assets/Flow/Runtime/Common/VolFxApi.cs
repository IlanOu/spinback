#if !VOL_FX
#if UNITY_6000_0_OR_NEWER
#define UNITY_RENDER_GRAPH
#else
#define UNITY_LEGACY
#endif

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
    public partial class VolFx
    {
        private static readonly int s_BlitTexture = Shader.PropertyToID("_BlitTexture");
        private static readonly int s_BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");

        public abstract class InitApi 
        {
            public int Width  { get; internal set; }
            public int Height { get; internal set; }
            
            public abstract void Allocate(RenderTarget rt, int width, int height, GraphicsFormat format, TextureWrapMode wrap = TextureWrapMode.Repeat, FilterMode filter = FilterMode.Bilinear);
        }
        
        public abstract class CallApi 
        {
            public abstract RTHandle              CamColor { get; }
            public abstract MaterialPropertyBlock Mat      { get; }
            public abstract CameraType            CamType  { get; }
            
            public abstract void Blit(RTHandle source, RTHandle dest, Material mat, int pass = 0);
            public abstract void Blit(RTHandle source, RTHandle dest);
            public abstract void EndSample(ProfilingSampler sampler);
            public abstract void BeginSample(ProfilingSampler sampler);
        }
        
#if UNITY_RENDER_GRAPH
        public class InitApiRg : InitApi
        {
            internal IUnsafeRenderGraphBuilder _builder;
            internal RenderGraph               _renderGraph;
            internal ContextContainer          _frameData;
            
            // =======================================================================
            public override void Allocate(RenderTarget rt, int width, int height, GraphicsFormat format, TextureWrapMode wrap = TextureWrapMode.Repeat, FilterMode filter = FilterMode.Bilinear)
            {
                var rtd = new RenderTextureDescriptor(width, height, format, GraphicsFormat.None, 0);
                RenderingUtils.ReAllocateHandleIfNeeded(ref rt.Handle, rtd, filter, wrap);
                //var tex = _renderGraph.ImportTexture(rt, new RenderTargetInfo() { width = width, height = height, format = format, msaaSamples = 1, volumeDepth = 1, bindMS = false});
                var tex = _renderGraph.ImportTexture(rt.Handle);
                _builder.UseTexture(tex, AccessFlags.ReadWrite);
            }
        }
        
        public class CallApiRg : CallApi
        {
            internal static MaterialPropertyBlock _mat = new MaterialPropertyBlock();
            internal        UnsafeCommandBuffer   _cmd;
            internal        Camera                _cam;
            public          Material              _blit;
            internal        RTHandle              _camColor;
            
            public override CameraType CamType => _cam.cameraType;
            
            // =======================================================================
            public override MaterialPropertyBlock Mat => _mat;
            public override RTHandle CamColor => _camColor;

            public override void Blit(RTHandle source, RTHandle dest, Material mat, int pass = 0)
            {
                _mat.SetTexture(Utils.s_MainTexId, source);
                _cmd.SetRenderTarget(dest, 0);
                _cmd.DrawMesh(Utils.FullscreenMesh, Matrix4x4.identity, mat, 0, pass, _mat);
            }

            public override void Blit(RTHandle source, RTHandle dest)
            {
                _mat.SetTexture(Utils.s_MainTexId, source);
                _mat.SetTexture(s_BlitTexture, source);
                _mat.SetVector(s_BlitScaleBias, Vector4.one);

                _cmd.SetRenderTarget(dest, 0);
                _cmd.DrawMesh(Utils.FullscreenMesh, Matrix4x4.identity, _blit, 0, 0, _mat);
            }

            public override void BeginSample(ProfilingSampler sampler)
            {
            }
            
            public override void EndSample(ProfilingSampler sampler)
            {
            }
        }
        
        public class InitApiLeg : InitApi
        {
            internal CommandBuffer _cmd;
            
            // =======================================================================
            public override void Allocate(RenderTarget rt, int width, int height, GraphicsFormat format, TextureWrapMode wrap = TextureWrapMode.Repeat, FilterMode filter = FilterMode.Bilinear)
            {
                var rtd = new RenderTextureDescriptor(width, height, format, GraphicsFormat.None, 0);
                RenderingUtils.ReAllocateHandleIfNeeded(ref rt.Handle, rtd, filter, wrap);
                _cmd.GetTemporaryRT(rt.Id, width, height, 0, filter, format);
            }
        }
        
        public class CallApiLeg : CallApi
        {
            internal static MaterialPropertyBlock _mat = new MaterialPropertyBlock();
            internal        CommandBuffer         _cmd;
            internal        Camera                _cam;
            public          Material              _blit;
            internal        RTHandle              _camColor;
        private ProfilingSampler _sampler;

            public override CameraType CamType => _cam.cameraType;
            
            // =======================================================================
            public override MaterialPropertyBlock Mat      => _mat;
            public override RTHandle              CamColor => _camColor;
            
            public override void Blit(RTHandle source, RTHandle dest, Material mat, int pass = 0)
            {
                //_mat.SetTexture(Utils.s_MainTexId, source);
                _cmd.SetGlobalTexture(Utils.s_MainTexId, source);
                _cmd.SetRenderTarget(dest, 0);
                _cmd.DrawMesh(Utils.FullscreenMesh, Matrix4x4.identity, mat, 0, pass, _mat);
            }
            
            public override void Blit(RTHandle source, RTHandle dest)
            {
                Blit(source, dest, _blit);
            }
            
            public override void EndSample(ProfilingSampler sampler)
            {
            }

            public override void BeginSample(ProfilingSampler sampler)
            {
            }
        }
#else
        public class InitApiLeg : InitApi
        {
            internal CommandBuffer _cmd;
            
            // =======================================================================
            public override void Allocate(RenderTarget rt, int width, int height, GraphicsFormat format, TextureWrapMode wrap = TextureWrapMode.Repeat, FilterMode filter = FilterMode.Bilinear)
            {
#if !UNITY_2021_1_OR_NEWER
                var rtd = new RenderTextureDescriptor(width, height, format, 0, 0);
#else
                var rtd = new RenderTextureDescriptor(width, height, format, GraphicsFormat.None, 0);
#endif
                
                
#if !UNITY_2022_1_OR_NEWER
                _reAllocateIfNeededImpl(ref rt.Handle, rtd, filter, wrap);
#else
                RenderingUtils.ReAllocateIfNeeded(ref rt.Handle, rtd, filter, wrap);
#endif
                
                _cmd.GetTemporaryRT(rt.Id, width, height, 0, filter, format);
            }
            
            private void _reAllocateIfNeededImpl(ref RTHandle handle, in RenderTextureDescriptor desc, FilterMode filterMode, TextureWrapMode wrapMode)
            {
                if (handle == null 
                    || handle.rt == null 
                    || handle.rt.width != desc.width 
                    || handle.rt.height != desc.height
                    || handle.rt.graphicsFormat != desc.graphicsFormat
                    || handle.rt.filterMode != filterMode
                    || handle.rt.wrapMode != wrapMode)
                {
                    if (handle != null && handle.rt != null)
                        RTHandles.Release(handle);
                    
                    handle = RTHandles.Alloc(desc.width, desc.height, colorFormat: desc.graphicsFormat, filterMode: filterMode, wrapMode: wrapMode);
                }
            }
        }
        
        public class CallApiLeg : CallApi
        {
            internal static MaterialPropertyBlock _mat = new MaterialPropertyBlock();
            internal        CommandBuffer         _cmd;
            internal        Camera                _cam;
            public          Material              _blit;
            internal        RTHandle              _camColor;

            public override CameraType CamType => _cam.cameraType;
            
            // =======================================================================
            public override MaterialPropertyBlock Mat      => _mat;
            public override RTHandle              CamColor => _camColor;
            
            public override void Blit(RTHandle source, RTHandle dest, Material mat, int pass = 0)
            {
#if UNITY_EDITOR
                if (Application.isPlaying == false && mat == null)
                    return;
#endif
                if (mat == null)
                    return;

                //_mat.SetTexture(Utils.s_MainTexId, source);
                _cmd.SetGlobalTexture(Utils.s_MainTexId, source);
                _cmd.SetRenderTarget(dest, 0);
                
                _cmd.DrawMesh(Utils.FullscreenMesh, Matrix4x4.identity, mat, 0, pass, _mat);
            }
            
            public override void Blit(RTHandle source, RTHandle dest)
            {
                Blit(source, dest, _blit);
            }
            
            public override void EndSample(ProfilingSampler sampler)
            {
                // sampler.Begin(_cmd);
            }

            public override void BeginSample(ProfilingSampler sampler)
            {
                // sampler.End(_cmd);
            }
        }
#endif
    }
}
#endif
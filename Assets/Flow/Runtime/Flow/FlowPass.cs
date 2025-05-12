using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

//  FlowFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/Vol/Flow")]
    public class FlowPass : VolFx.Pass
    {
        private static readonly int s_FlowTex = Shader.PropertyToID("_FlowTex");
        private static readonly int s_Weight  = Shader.PropertyToID("_Weight");
        private static readonly int s_Tiling  = Shader.PropertyToID("_Tiling");
        private static readonly int s_Tint    = Shader.PropertyToID("_Tint");
        
        public override string ShaderName => string.Empty;
        
        [Tooltip("Desired fps")]
        public float  _fps = 60f;
        
        [Range(0, 1f)]
        [Tooltip("Flow texture resolution (parameter can be used for performance reasons or to blur an image)")]
        public  float _scale = .5f;
        [Tooltip("Use HDR texture format, it keeps color quality, but can loose alpha")]
        public bool _hdr;
        
        private RenderTargetFlip _flow;
        private bool             _clear;
        private float            _main;
        private float            _fade;
        private Vector2          _offsetUv;
        private float            _scaleUv;
        private float            _rotUv;
        private float            _lastDraw;
        private float            _print;
        private float            _adaptive;
        private int              _fadeFrames;
        
        private                 ProfilingSampler _sampler;
        private                 int              _warmup;
        private                 int              _sampling;
        private                 GraphicsFormat   _format;

        // =======================================================================
        public override void Init()
        {
            _flow   = new RenderTargetFlip(new RenderTarget().Allocate($"{name}_a"),
                                           new RenderTarget().Allocate($"{name}_b"));

            _sampler = new ProfilingSampler(name);
            _lastDraw = -1f;
            _warmup = 0;
            
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<FlowVol>();

            if (settings.IsActive() == false)
            {
                if (_fadeFrames < 7)
                {
                    _fadeFrames++;
                    return true;
                }
                
                _clear = true;
                return false;
            }

            _fadeFrames = 0;
            _print = settings.m_Print.value;
            
            _main = 1f - settings.m_Fade.value;
            _fade = 1f - _main + settings.m_Strain.value * .5f;
            
            _sampling = settings.m_Samples.value;
            _adaptive = settings.m_Adaptive.value;
            
            var flow  = settings.m_Flow.value; 
            _offsetUv = new Vector2(0.01f * flow.x, 0.01f * flow.y);
            _scaleUv  = 1f + 0.12f * flow.z;
            _rotUv    = settings.m_Angle.value * 0.01f;
            
            mat.SetColor(s_Tint, settings.m_Tint.value);
            
            return true;
        }

        public override void Init(VolFx.InitApi initApi)
        {
            _format = _hdr ? GraphicsFormat.B10G11R11_UFloatPack32 : GraphicsFormat.R8G8B8A8_UNorm;
            
            initApi.Allocate(_flow.From, Mathf.Max((int)(Screen.width * _scale), 16), Mathf.Max((int)(Screen.height * _scale), 16), _format);
            initApi.Allocate(_flow.To,   Mathf.Max((int)(Screen.width * _scale), 16), Mathf.Max((int)(Screen.height * _scale), 16), _format);
        }

        public override void Invoke(RTHandle source, RTHandle dest, VolFx.CallApi callApi)
        {
            // warmup textures allocation to avoid bиg on a new unity versions
            _warmup ++;
            if (_warmup < 2)
			{
				callApi.Blit(source, dest);
				return;
			}
            
            callApi.BeginSample(_sampler);

            if (_clear)
            {
                callApi.Blit(source, _flow.From.Handle, _material, 1);
                callApi.Blit(_flow.From.Handle, dest, _material, 1);
                _clear = false;
                
                _lastDraw = getDrawTime();
                
                callApi.EndSample(_sampler);
                return;
            }

            var drawTime = getDrawTime();
            
            
            callApi.Mat.SetVector(s_Weight, new Vector4(_main, _fade, _print, _adaptive));
            _draw();
            if (drawTime - _lastDraw > 1f / _fps)
            {
                _flow.Flip();
                _lastDraw = drawTime;
                
                for (var n = 1; n < _sampling; n++)
                {
                    _draw();
                    _flow.Flip();
                }
            }
            
            callApi.Mat.SetTexture(s_FlowTex, _flow.From.Handle);
            callApi.Blit(source, dest, _material, 0);
            
            if (_print > 0f)
                callApi.Blit(source, dest, _material, 2);
                
            callApi.EndSample(_sampler);

            void _draw()
            {
                callApi.Mat.SetTexture(s_FlowTex, _flow.From.Handle);
                callApi.Mat.SetVector(s_Tiling, new Vector4(_offsetUv.x, _offsetUv.y, _scaleUv, _rotUv));
                callApi.Blit(source, _flow.To.Handle, _material, 0);
            }
        }

        // -----------------------------------------------------------------------
        float getDrawTime()
        {
            var drawTime = Time.time;
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                drawTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
#endif
            
            return drawTime;
        }
    }
}
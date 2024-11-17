Shader "Hiddend/RaymarchMain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #pragma exclude_renderers d3d11_9x
            #pragma exclude_renderers d3d9

            #include "UnityCG.cginc"
            #include "DistanceFunctions.cginc"

            // how to draw
            sampler2D _MainTex;
            uniform sampler2D _CameraDepthTexture;
            uniform float4x4 _CamFrustum, _CamToWorld;
            uniform int _MaxIteration;
            uniform float _Accuracy, _minDistance, _maxDistance;
            uniform float _AoStepSize, _AoIntensity;
            uniform int _AoIterations;
            uniform float3 _lightOrigin;
            uniform fixed4 _lightCol;
            uniform float _lightIntensity;
            uniform float _ShadowIntensity, _ShadowPenumbra;
            uniform float2 _ShadowDistance;
            
            
            // COMMON
            fixed4 _planetPR[256];
            uniform int _NumPlanets;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                half index = v.vertex.z;
                v.vertex.z = 0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.ray = _CamFrustum[(int) index].xyz;

                o.ray /= abs(o.ray.z);

                o.ray = mul(_CamToWorld, o.ray);

                return o;
            }

            float2 opU(float2 d1, float2 d2)
            {
                return d1.x < d2.x ? d1 : d2;
            }

            
            float2 map(float3 p)
            {
                // s = 10000
                //scale(x)=Si(x>s, -0.5 x+1.5 s, s)- s + 1
                //tdst(x)=Si(x>s, -1 x+2 s, s)
                //if star cap min scale, draw between [s; s+5] if p >= 10005 and star_r = 600 scaled star should be 60 and star center => in same direction than before but at a dist 100005
                //else don't draw if scale<=0 
                // nothing is drawned after s+5



                //return a map of distance => behaviour(int);
                if (_NumPlanets == 0)
                {
                    return float2(0, 0);
                }
                fixed4 pPR = _planetPR[0];
                float2 res = float2(sdSphere(p - pPR.rgb, pPR.a), 0);
                
                for (int i = 1; i < _NumPlanets; i++)
                {
                    pPR = _planetPR[i];
                    res = opU(res, float2(sdSphere(p - pPR.rgb, pPR.a), 0));
                }
                return res;
            }
 


            float3 getNormal(float3 p)
            {
                const float2 offset = float2(0.001, 0.0);
                float3 n = float3(
                    map(p + offset.xyy).x - map(p - offset.xyy).x,
                    map(p + offset.yxy).x - map(p - offset.yxy).x,
                    map(p + offset.yyx).x - map(p - offset.yyx).x                    
                );
                return normalize(n);
            }




            float softShadows(float3 ro, float3 rd, float mint, float maxt, float k)
            {
                float result = 1.0;
                for (float t = mint; t < maxt;)
                {
                    float h = map(ro+rd*t).x;
                    if (h<0.001)
                    {
                        return 0.0;
                    }
                    result = min(result, k*h/t);
                    t+=h;
                }
                return result;
            }

            float AmbientOcclusion(float3 p, float3 n)
            {
                float step = _AoStepSize;
                float ao = 0.0;
                float dist;
                for (int i = 1; i <= _AoIterations; i++)
                {
                    dist = step * i;
                    ao += max(0.0, (dist - map(p + n * dist).x) / dist);
                }
                return (1.0 - ao * _AoIntensity);
            }

            float3 GetCol(float3 p, float3 n, int col)
            {
                return n;
            }

            float3 Shading(float3 p, float3 n, int col)
            {
                float3 result;
                //Diffuse Color
                float3 color = (col == 0 )? n : GetCol(p, n, col);
                
                float3 lightDirection = normalize(p - _lightOrigin); // pointlight
                //Directional Light
                float3 light = (_lightCol * dot(-lightDirection, n) * 0.5 + 0.5) * _lightIntensity;

                //Shadows
                float shadow = softShadows(p, -lightDirection, _ShadowDistance.x, _ShadowDistance.y, _ShadowPenumbra) * 0.5 + 0.5;
                shadow = max(0.0, pow(shadow, _ShadowIntensity));

                //Ambient Occlusion
                float ao = AmbientOcclusion(p,n);

                result = color * light * shadow * ao;


                return result;
            }

            fixed4 raymarching(float3 ro, float3 rd, float depth)
            {
                fixed4 result = fixed4(1,1,1,0);
                const int max_iteration = _MaxIteration;
                float t = 0; // distance travelled along the ray direction
                for (int i = 0; i < max_iteration; i++)
                {
                    if (t > _maxDistance || t >= depth)
                    {
                        //Environment
                        result = fixed4(rd, 0);
                        
                        break;
                    }

                    float3 p = ro + (rd * t);
                    //check for hit in distance field
                    float2 d = map(p);
                    if (d.x < _Accuracy && t > _minDistance) // hit something
                    {
                        //shading !
                        float3 n = getNormal(p);
                        float3 s = Shading(p, n, (int)d.y);
                        result = fixed4(s,1);
                        break;
                    }
                    t += d.x;
                }

                return result; 
            }


            fixed4 frag (v2f i) : SV_Target
            {
                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                depth *= length(i.ray);
                fixed3 col = tex2D(_MainTex, i.uv);
                float3 rayDirection = normalize(i.ray.xyz);
                float3 rayOrigin = _WorldSpaceCameraPos;
                fixed4 result = raymarching(rayOrigin, rayDirection, depth);
                return fixed4(col * (1.0 - result.w) + result.xyz * result.w,1.0);
            }
            ENDCG
        }
    }
}
Shader "Hiddend/RaymarchMain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
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
            #include "FractalNoise.cginc"

            // how to draw
            sampler2D _MainTex;
            uniform sampler2D _CameraDepthTexture;
            uniform float4x4 _CamFrustum, _CamToWorld;
            uniform int _MaxIteration;
            uniform float _Accuracy, _minDistance, _maxDistance;
            uniform float _AoStepSize, _AoIntensity;
            uniform int _AoIterations;
            uniform fixed4 _lightCol;
            uniform float _lightIntensity;
            uniform float _ShadowIntensity, _ShadowPenumbra;
            uniform float2 _ShadowDistance;
            uniform float _TangentCorrection;
            
            
            // PLANET
            uniform fixed4 _planetPR[256];
            uniform fixed4 _planetMinstr[256];
            uniform fixed4 _planetM10[256];
            uniform fixed4 _planetM11[256];
            uniform fixed4 _planetM20[256];
            uniform fixed4 _planetM21[256];
            uniform fixed4 _planetM30[256];
            uniform fixed4 _planetM31[256];
            // LOW OVERRIDE
            uniform float _LODThreshold;
            uniform float _HighAccuracy;
            uniform float2 _HIGHShadowDistance;

            uniform fixed4 _planetHighMinstr[256];
            uniform fixed4 _planetHighM10[256];
            uniform fixed4 _planetHighM11[256];
            uniform fixed4 _planetHighM20[256];
            uniform fixed4 _planetHighM21[256];
            uniform fixed4 _planetHighM30[256];
            uniform fixed4 _planetHighM31[256];
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

            v2f vert(appdata v)
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

            float3 opU(float3 d1, float3 d2)
            {
                return d1.x < d2.x ? d1 : d2;
            }

            float3 unPackModifiers(int mm)
            {
                float base = 2;
                float a = floor(mm / pow(base, 2));
                float b = floor((mm - (a * pow(base, 2))) / pow(base, 1));
                float c = floor((mm - (a * pow(base, 2)) - (b * pow(base, 1))) / pow(base, 0));
                return float3(c, b, a);
            }

            
            float getD(float3 p, int i, int modif, fixed4 arg0, fixed4 arg1)
            {
                //TODO switch logic here
                if (modif == 1)
                {
                    fixed4 pPR = _planetPR[i];
                    return arg0.r + simpleNoise(normalize(p - pPR.rgb), arg0.g, arg0.b);
                }
                else if (modif == 2)
                {

                }
                return 0;
            }

            float3 GetCol(float3 p, float3 n, int col)
            {
                float3 c = unPackModifiers(col);
                if (c.z == 0)
                {
                    return float3(1, 1, 1);
                }
                return n;
            }
            

            //lowMap
            float3 map(float3 p, int mode)
            {
                if (_NumPlanets == 0)
                {
                    return float3(0, 0, 0);
                }
                
                fixed4 pPR = _planetPR[0];
                //modif params
                int m;
                float3 mm;
                fixed4 M10;
                fixed4 M11;
                fixed4 M20;
                fixed4 M21;
                fixed4 M30;
                fixed4 M31;
                if (mode == 0)
                {
                    m = (int)_planetMinstr[0].r
                    mm = unPackModifiers((int)_planetMinstr[0].r);
                    M10 = _planetM10[0];
                    M11 = _planetM11[0];
                    M20 = _planetM20[0];
                    M21 = _planetM21[0];
                    M30 = _planetM30[0];
                    M31 = _planetM31[0];
                }
                else
                {
                    m = (int)_planetHighMinstr[0].r
                    mm = unPackModifiers((int)_planetHighMinstr[0].r);
                    M10 = _planetHighM10[0];
                    M11 = _planetHighM11[0];
                    M20 = _planetHighM20[0];
                    M21 = _planetHighM21[0];
                    M30 = _planetHighM30[0];
                    M31 = _planetHighM31[0];
                }
                float3 res = float3(sdSphere(p - pPR.rgb, pPR.a) + getD(p, 0, (int)mm.x, M10, M11) + getD(p, 0, (int)mm.y, M20, M21) + getD(p, 0, (int)mm.z, M30, M31), m, 0);
                for (int i = 0; i < _NumPlanets; i++)
                {
                    if (mode == 0)
                    {
                        m = (int)_planetMinstr[0].r
                        mm = unPackModifiers((int)_planetMinstr[0].r);
                        M10 = _planetM10[0];
                        M11 = _planetM11[0];
                        M20 = _planetM20[0];
                        M21 = _planetM21[0];
                        M30 = _planetM30[0];
                        M31 = _planetM31[0];
                    }
                    else
                    {
                        m = (int)_planetHighMinstr[i].r
                        mm = unPackModifiers((int)_planetHighMinstr[i].r);
                        M10 = _planetHighM10[i];
                        M11 = _planetHighM11[i];
                        M20 = _planetHighM20[i];
                        M21 = _planetHighM21[i];
                        M30 = _planetHighM30[i];
                        M31 = _planetHighM31[i];
                    }
                    pPR = _planetPR[i];
                    res = opU(res, float3(sdSphere(p - pPR.rgb, pPR.a) + getD(p, i, (int)mm.x, M10, M11) + getD(p, 1, (int)mm.y, M20, M21) + getD(p, i, (int)mm.z, M30, M31), m, i));
                }
                return res;
            }
            

            //low Normals
            float3 getNormal(float3 p, int mode)
            {
                const float2 offset = float2(0.01, 0.0);
                float3 n = float3(
                    map(p + offset.xyy, mode).x - map(p - offset.xyy, mode).x,
                    map(p + offset.yxy, mode).x - map(p - offset.yxy, mode).x,
                    map(p + offset.yyx, mode).x - map(p - offset.yyx, mode).x
                );
                return normalize(n);
            }
            //low shadows
            float softShadows(float3 ro, float3 rd, float mint, float maxt, float k, int mode)
            {
                float result = 1.0;
                for (float t = mint; t < maxt; )
                {
                    float h = map(ro + rd * t, mode).x;
                    if (h < 0.001)
                    {
                        return 0.0;
                    }
                    result = min(result, k * h / t);
                    t += h;
                }
                return result;
            }

            //low AO
            float AmbientOcclusion(float3 p, float3 n, int mode)
            {
                float step = _AoStepSize;
                float ao = 0.0;
                float dist;
                for (int i = 1; i <= _AoIterations; i++)
                {
                    dist = step * i;
                    ao += max(0.0, (dist - map(p + n * dist, mode).x) / dist);
                }
                return (1.0 - ao * _AoIntensity);
            }

            
            //Low shading
            float3 Shading(float3 p, float3 n, int col, int id, int mode)
            {
                float3 result;
                //Diffuse Color
                float3 color = (col == 0) ? n : GetCol(p, n, col);
                
                float3 lightDirection = normalize(_planetPR[id].rgb - _planetMinstr[id].gba); // pointlight
                //Directional Light
                float3 light = (_lightCol * dot(-lightDirection, n) * 0.5 + 0.5) * _lightIntensity;

                //Shadows
                float shadow = softShadows(p, -lightDirection, _ShadowDistance.x, _ShadowDistance.y, _ShadowPenumbra, mode) * 0.5 + 0.5;
                shadow = max(0.0, pow(shadow, _ShadowIntensity));

                //Ambient Occlusion
                float ao = AmbientOcclusion(p, n, mode);

                result = color * light * shadow * ao;


                return result;
            }

            fixed4 raymarching(float3 ro, float3 rd, float depth)
            {
                //TODO ADD LOD, SHADOWS NOT DRAWN FOR DETAIL ON BIG PLANETS(change shadow params scale?)
                fixed4 result = fixed4(1, 1, 1, 0);
                const int max_iteration = _MaxIteration;
                float accuracy = _Accuracy;
                float hAccuracy = _HighAccuracy;
                float t = 0; // distance travelled along the ray direction
                for (int i = 0; i < max_iteration; i++)
                {
                    // dynamic hit threshold after half it.
                    if (i >= max_iteration / 2)
                    {
                        accuracy += _TangentCorrection;
                        hAccuracy += _TangentCorrection;
                    }

                    if (t > _maxDistance || t >= depth)
                    {
                        //Environment
                        result = fixed4(rd, 0);
                        
                        break;
                    }

                    float3 p = ro + (rd * t);
                    //check for hit in distance field
                    //redo clean and check zone before each following test
                    float3 d;
                    if (length(p) <= (_maxDistance * _LODThreshold))
                    {
                        //HIGH RES VOL
                        d = map(p, 1);
                        if (d.x < accuracy && length(p) > _minDistance)
                        {
                            float3 n = getNormal(p, 1);
                            float3 s = Shading(p, n, (int)d.y, (int)d.z, 1);
                            result = fixed4(s, 1);
                            break;
                        }
                    }
                    else
                    {
                        //LOW RES VOL
                        d = map(p, 0);
                        if (d.x < accuracy && length(p) > _minDistance)
                        {
                            float3 n = getNormal(p, 0);
                            float3 s = Shading(p, n, (int)d.y, (int)d.z, 0);
                            result = fixed4(s, 1);
                            break;
                        }
                    }
                    t += d.x;
                }
                return result;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                depth *= length(i.ray);
                fixed3 col = tex2D(_MainTex, i.uv);
                float3 rayDirection = normalize(i.ray.xyz);
                float3 rayOrigin = _WorldSpaceCameraPos;
                fixed4 result = raymarching(rayOrigin, rayDirection, depth);
                return fixed4(col * (1.0 - result.w) + result.xyz * result.w, 1.0);
            }
            ENDCG
        }
    }
}
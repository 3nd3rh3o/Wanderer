using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(Camera))]
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class RaymarchCamera : SceneViewFilter
{
    [SerializeField]
    public Shader _shader;
    private Material _rayMarchMat;
    private Camera _cam;

    public Material _rayMarchMaterial
    {
        get
        {
            if (!_rayMarchMat && _shader)
            {
                _rayMarchMat = new Material(_shader);
                _rayMarchMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return _rayMarchMat;
        }
    }

    public Camera _camera
    {
        get
        {
            if (!_cam)
            {
                _cam = GetComponent<Camera>();
            }
            return _cam;
        }
    }
    public Vector2 _Distance;
    public int _MaxIteration;
    public float _Accuracy;

    public float _HighAccuracy;

    public float _LODThreshold;
    public Color _lightCol;
    public Vector3 _StarPosition;
    public float _lightIntensity;
    public float _ShadowPenumbra;
    public float _ShadowIntensity;
    public Vector2 _ShadowDistance;
    public Vector2 _HIGHShadowDistance;
    public float _AoStepSize;
    public float _AoIntensity;
    public int _AoIterations;
    public int _MaxPlanets = 10;
    public float _TangentCorrection = 0f;
    public UniverseDataHolder _dataHolder = new();

    private Vector2 _playerLook;
    public InputActionReference _MOUSE;
    public Vector3 _playerPos = new();
    public InputActionReference _MOVE;

    private List<Vector4[]> _planetBuffer;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_rayMarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }
        UpdateShaderBuffer();
        if (_MOUSE && _cam)
        {
            _playerLook = _MOUSE.action.ReadValue<Vector2>();
            _cam.transform.localRotation *= Quaternion.Euler(_playerLook.y * -0.1f, _playerLook.x * 0.1f, 0);
            _cam.transform.localRotation = _cam.transform.localRotation.normalized;
            if (_MOVE)
            {
                Vector2 movement = _MOVE.action.ReadValue<Vector2>();
                _playerPos += _cam.transform.localRotation * new Vector3(movement.x, 0, movement.y) * 1f;
            }
        }

        _rayMarchMaterial.SetMatrix("_CamFrustum", CamFrustum(_camera));
        _rayMarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);
        _rayMarchMaterial.SetFloat("_minDistance", _Distance.x);
        _rayMarchMaterial.SetFloat("_maxDistance", _Distance.y);
        _rayMarchMaterial.SetInt("_MaxIteration", _MaxIteration);
        _rayMarchMaterial.SetFloat("_Accuracy", _Accuracy);
        _rayMarchMaterial.SetFloat("_HighAccuracy", _HighAccuracy);
        _rayMarchMaterial.SetFloat("_LODThreshold", _LODThreshold);
        _rayMarchMaterial.SetColor("_lightCol", _lightCol);
        _rayMarchMaterial.SetFloat("_lightIntensity", _lightIntensity);
        _rayMarchMaterial.SetFloat("_ShadowPenumbra", _ShadowPenumbra);
        _rayMarchMaterial.SetFloat("_ShadowIntensity", _ShadowIntensity);
        _rayMarchMaterial.SetVector("_ShadowDistance", _ShadowDistance);
        _rayMarchMaterial.SetVector("_HIGHShadowDistance", _HIGHShadowDistance);
        _rayMarchMaterial.SetFloat("_AoStepSize", _AoStepSize);
        _rayMarchMaterial.SetFloat("_AoIntensity", _AoIntensity);
        _rayMarchMaterial.SetInt("_AoIterations", _AoIterations);
        _rayMarchMaterial.SetInt("_NumPlanets", _dataHolder.NumElem());
        _rayMarchMaterial.SetFloat("_TangentCorrection", _TangentCorrection);


        _rayMarchMat.SetVectorArray("_planetPR", _planetBuffer[0]);
        _rayMarchMat.SetVectorArray("_planetMinstr", _planetBuffer[1]);
        _rayMarchMat.SetVectorArray("_planetM10", _planetBuffer[2]);
        _rayMarchMat.SetVectorArray("_planetM11", _planetBuffer[3]);
        _rayMarchMat.SetVectorArray("_planetM20", _planetBuffer[4]);
        _rayMarchMat.SetVectorArray("_planetM21", _planetBuffer[5]);
        _rayMarchMat.SetVectorArray("_planetM30", _planetBuffer[6]);
        _rayMarchMat.SetVectorArray("_planetM31", _planetBuffer[7]);
        _rayMarchMat.SetVectorArray("_planetHighMinstr", _planetBuffer[8]);
        _rayMarchMat.SetVectorArray("_planetHighM10", _planetBuffer[9]);
        _rayMarchMat.SetVectorArray("_planetHighM11", _planetBuffer[10]);
        _rayMarchMat.SetVectorArray("_planetHighM20", _planetBuffer[11]);
        _rayMarchMat.SetVectorArray("_planetHighM21", _planetBuffer[12]);
        _rayMarchMat.SetVectorArray("_planetHighM30", _planetBuffer[13]);
        _rayMarchMat.SetVectorArray("_planetHighM31", _planetBuffer[14]);

        RenderTexture.active = destination;
        _rayMarchMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        _rayMarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        //BL
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);

        //BR
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);

        //TR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);

        //TL
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 CamFrustum(Camera cam)
    {
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);


        Vector3 goUp = Vector3.up * fov;
        Vector3 goRight =  fov * cam.aspect * Vector3.right;



        Vector3 TL = -Vector3.forward - goRight + goUp;
        Vector3 TR = -Vector3.forward + goRight + goUp;
        Vector3 BL = -Vector3.forward - goRight - goUp;
        Vector3 BR = -Vector3.forward + goRight - goUp;

        frustum.SetRow(0, TL);
        frustum.SetRow(1, TR);
        frustum.SetRow(2, BR);
        frustum.SetRow(3, BL);
        return frustum;
    }




    private void OnEnable()
    {
        _planetBuffer = new()
        {
            //LOW
            new Vector4[256], // pos.xyz+radius
            new Vector4[256], // Minstr, lightDirection.xyz
            new Vector4[256], // M10
            new Vector4[256], // M11
            new Vector4[256], // M20
            new Vector4[256], // M21
            new Vector4[256], // M30
            new Vector4[256], // M31
            //HIGH
            new Vector4[256], // HMinstr, null.xyz
            new Vector4[256], // HM10
            new Vector4[256], // HM11
            new Vector4[256], // HM20
            new Vector4[256], // HM21
            new Vector4[256], // HM30
            new Vector4[256], // HM31
        };
        _dataHolder.HookInit();
        // CREATE BUFFER
        UpdateShaderBuffer();
    }

    private void OnDisable()
    {
        _dataHolder.HookClean();
        _planetBuffer = null;
    }


    private void UpdateShaderBuffer()
    {
        List<Vector4>[] pData = _dataHolder.GetData();
        Vector4[] pos = _planetBuffer[0];
        Vector4[] modifiers = _planetBuffer[1];
        Vector4[] mArgs10 = _planetBuffer[2];
        Vector4[] mArgs11 = _planetBuffer[3];
        Vector4[] mArgs20 = _planetBuffer[4];
        Vector4[] mArgs21 = _planetBuffer[5];
        Vector4[] mArgs30 = _planetBuffer[6];
        Vector4[] mArgs31 = _planetBuffer[7];
        Vector4[] Hmodifiers = _planetBuffer[8];
        Vector4[] HmArgs10 = _planetBuffer[9];
        Vector4[] HmArgs11 = _planetBuffer[10];
        Vector4[] HmArgs20 = _planetBuffer[11];
        Vector4[] HmArgs21 = _planetBuffer[12];
        Vector4[] HmArgs30 = _planetBuffer[13];
        Vector4[] HmArgs31 = _planetBuffer[14];


        for (int i = 0; i < _dataHolder.NumElem(); i++)
        {
            Vector4 pr = pData[i][0];
            pos[i] = new Vector4(pr.x - _playerPos.x, pr.y - _playerPos.y, pr.z - _playerPos.z, pr.w);
            modifiers[i] = new Vector4(pData[i][1].x, pData[i][1].y - _playerPos.x, pData[i][1].z - _playerPos.y, pData[i][1].w - _playerPos.z);
            mArgs10[i] = pData[i][2];
            mArgs11[i] = pData[i][3];
            mArgs20[i] = pData[i][4];
            mArgs21[i] = pData[i][5];
            mArgs30[i] = pData[i][6];
            mArgs31[i] = pData[i][7];
            Hmodifiers[i] = new Vector4(pData[i][8].x, 0, 0, 0);
            HmArgs10[i] = pData[i][9];
            HmArgs11[i] = pData[i][10];
            HmArgs20[i] = pData[i][11];
            HmArgs21[i] = pData[i][12];
            HmArgs30[i] = pData[i][13];
            HmArgs31[i] = pData[i][14];
        }
        _planetBuffer[0] = pos;
        _planetBuffer[1] = modifiers;
        _planetBuffer[2] = mArgs10;
        _planetBuffer[3] = mArgs11;
        _planetBuffer[4] = mArgs20;
        _planetBuffer[5] = mArgs21;
        _planetBuffer[6] = mArgs30;
        _planetBuffer[7] = mArgs31;
        _planetBuffer[8] = Hmodifiers;
        _planetBuffer[9] = HmArgs10;
        _planetBuffer[10] = HmArgs11;
        _planetBuffer[11] = HmArgs20;
        _planetBuffer[12] = HmArgs21;
        _planetBuffer[13] = HmArgs30;
        _planetBuffer[14] = HmArgs31;
        _dataHolder.UnmarkDirty();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;



[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
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
    public Color _lightCol;
    public Vector3 _StarPosition;
    public float _lightIntensity;
    public float _ShadowPenumbra;
    public float _ShadowIntensity;
    public Vector2 _ShadowDistance;
    public float _AoStepSize;
    public float _AoIntensity;
    public int _AoIterations;
    public int _MaxPlanets = 10;
    public UniverseDataHolder _dataHolder = new();
    
    private List<Vector4[]> _planetBuffer;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_rayMarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }
        UpdateShaderBuffer();


        _rayMarchMaterial.SetMatrix("_CamFrustum", CamFrustum(_camera));
        _rayMarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);
        _rayMarchMaterial.SetFloat("_minDistance", _Distance.x);
        _rayMarchMaterial.SetFloat("_maxDistance",  _Distance.y);
        _rayMarchMaterial.SetInt("_MaxIteration", _MaxIteration);
        _rayMarchMaterial.SetFloat("_Accuracy", _Accuracy);
        _rayMarchMaterial.SetColor("_lightCol", _lightCol);
        _rayMarchMaterial.SetVector("_lightOrigin", _StarPosition);
        _rayMarchMaterial.SetFloat("_lightIntensity", _lightIntensity);
        _rayMarchMaterial.SetFloat("_ShadowPenumbra", _ShadowPenumbra);
        _rayMarchMaterial.SetFloat("_ShadowIntensity", _ShadowIntensity);
        _rayMarchMaterial.SetVector("_ShadowDistance", _ShadowDistance); 
        _rayMarchMaterial.SetFloat("_AoStepSize", _AoStepSize);
        _rayMarchMaterial.SetFloat("_AoIntensity", _AoIntensity);
        _rayMarchMaterial.SetInt("_AoIterations", _AoIterations);
        _rayMarchMaterial.SetInt("_NumPlanets",_dataHolder.NumElem());

        _rayMarchMat.SetVectorArray("_planetPR", _planetBuffer[0]);


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
        Vector3 goRight = Vector3.right * fov * cam.aspect;



        Vector3 TL = (-Vector3.forward - goRight + goUp);
        Vector3 TR = (-Vector3.forward + goRight + goUp);
        Vector3 BL = (-Vector3.forward - goRight - goUp);
        Vector3 BR = (-Vector3.forward + goRight - goUp);

        frustum.SetRow(0, TL);
        frustum.SetRow(1, TR);
        frustum.SetRow(2, BR);
        frustum.SetRow(3, BL);
        return frustum;
    }

    


    private void OnEnable()
    {
        _planetBuffer = new(){new Vector4[256]};
        _dataHolder.HookInit();
        // CREATE BUFFER
        UpdateShaderBuffer();
    }

    private void OnDisable()
    {
        _dataHolder.HookClean();
        _planetBuffer=null;
    }


    private void UpdateShaderBuffer()
    {
        List<Tuple<Vector3, float>> pData = _dataHolder.GetData();
        Vector4[] pos = new Vector4[256];   
        for (int i = 0; i < _dataHolder.NumElem(); i++)
        {
            Vector3 position = pData[i].Item1;
            float radius = pData[i].Item2;
            pos[i] = new Vector4(position.x, position.y, position.z, radius);
        }
        _planetBuffer[0] = pos;
        _dataHolder.UnmarkDirty();
    }
}

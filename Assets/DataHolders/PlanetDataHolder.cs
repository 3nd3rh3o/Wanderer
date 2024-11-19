using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class PlanetDataHolder
{
    public Action OnChange;

    [SerializeField]
    private Vector3 basePosition;
    public Vector3 BasePosition
    {
        get
        {
            return basePosition;
        }
        set
        {
            basePosition = value;
            OnChange?.Invoke();
        }
    }
    [SerializeField]
    [Range(0.5f, 30f)]
    private float radius;
    public float Radius
    {
        get
        {
            return  radius;
        }
        set
        {
            radius = value;
            OnChange?.Invoke();
        }
    }
    [SerializeField]
    private MODIFIERS[] globalHighModifiers = new MODIFIERS[0];
    public MODIFIERS[] GlobalHighModifiers
    {
        get
        {
            return globalHighModifiers;
        }
        set
        {
            globalHighModifiers = value;
        }
    }

    private MODIFIERS[] globalModifiers = new MODIFIERS[0];
    public MODIFIERS[] GlobalModifiers
    {
        get
        {
            return globalModifiers;
        }
        set
        {
            globalModifiers = value;
        }
    }

    [SerializeField]
    public Vector4 mParams10;
    [SerializeField]
    public Vector4 mParams11;
    [SerializeField]
    public Vector4 mParams20;
    [SerializeField]
    public Vector4 mParams21;
    [SerializeField]
    public Vector4 mParams30;
    [SerializeField]
    public Vector4 mParams31;
    [SerializeField]
    public Vector4 mHParams10;
    [SerializeField]
    public Vector4 mHParams11;
    [SerializeField]
    public Vector4 mHParams20;
    [SerializeField]
    public Vector4 mHParams21;
    [SerializeField]
    public Vector4 mHParams30;
    [SerializeField]
    public Vector4 mHParams31;



    public enum MODIFIERS
    {
        NONE,
        SIMPLENOISE
    }
    private static int nModifier = 2; // number of possible values for a modifier


    public static int ToInstr(MODIFIERS[] m)
    {
        int res = 0;
        int i = 0;
        foreach (MODIFIERS mod in m)
        {
            res+= (int)(((int)mod)*Mathf.Pow(nModifier, i));
            i++;
        }
        return res;
    }


    /* 
    A planet => 
    radius? 1 common
    position? 3 common
    upDirection? 3 common
    rotAmount? 1 common
    ringed? 
    gazous? true => emissive?
    telluric? true => atmosphere?
    as moons?
        moons =>
            same as planets but with additional offset
    gazous, telluric?
    one or several biomes? ~ use simplex noise, snoise(x)*nbBiomes = id of biome to draw
        biome => 
            has features?
            color? (lerp w/ neightboor)
            glob feat multiplier? 
            glob feat scale?

    has general features? (applied for all biomes)
     */

     /* 
        Dans OnRenderImage je fais tout les sets sur mon shader.
        Quand une valeur est modifié, je met a jour ma texture
        OnEnable => On crée la texture
        OnDisable => On supprime la texture
        column => one planet
        row i(RGBA):
            -radius, position.XYZ
            -
            -
     
        num row = 32(*4 max args), num col = 256 (max obj)


     
     
      */
}
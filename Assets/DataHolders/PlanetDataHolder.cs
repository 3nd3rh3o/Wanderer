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

    /* public static TAGS[] GetTAGS(this FEATURES feature)
    {
        return feature switch{
            FEATURES.OCEAN => new TAGS[]{},
            FEATURES.MOUNTAINS => new TAGS[]{},
            FEATURES.CRATERS => new TAGS[]{},
            FEATURES.L_MOUNTAINS_VALLEYS => new TAGS[]{}
        };
    } */


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
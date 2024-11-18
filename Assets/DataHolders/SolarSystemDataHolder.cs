using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SolarSystemDataHolder
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
    public List<PlanetDataHolder> planets = new(10); 


    public List<Vector4>[] GetData()
    {
        List<Vector4>[] pData = new List<Vector4>[NumElem()];
        for (int i = 0; i < NumElem(); i++)
        {
            Vector3 pos = planets[i].BasePosition + BasePosition;
            Vector3 lDir = (planets[i].BasePosition - BasePosition).normalized;


            pData[i] = new()
            {
                new Vector4(pos.x, pos.y, pos.z, planets[i].Radius), 
                new Vector4(PlanetDataHolder.ToInstr(planets[i].GlobalModifiers), lDir.x, lDir.y, lDir.z),
                planets[i].mParams10,
                planets[i].mParams11,
                planets[i].mParams20,
                planets[i].mParams21,
                planets[i].mParams30,
                planets[i].mParams31
            };

        }
        return pData;
    }


    public void TOnChange()
    {
        OnChange?.Invoke();
    }
    
    public void HookInit()
    {
        planets.ForEach(p => 
        {
            p.OnChange = TOnChange;
            //TODO hook init for moons 
        });
    }

    public void HookClean()
    {
        planets.ForEach(p => 
        {
            p.OnChange = null;
        });
    }

    public int NumElem()
    {
        return planets.Count;
    }
}
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


    public List<Tuple<Vector3, float>> GetData()
    {
        List<Tuple<Vector3, float>> pData = new (NumElem());
        planets.ForEach(p => 
        {
            pData.Add(new(p.BasePosition+BasePosition, p.Radius));
        });
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
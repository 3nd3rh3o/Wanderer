using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class UniverseDataHolder
{
    private bool dirty;


    [SerializeField]
    public List<SolarSystemDataHolder> solarSystems = new(10);

    public List<Tuple<Vector3, float>> GetData()
    {
        List<Tuple<Vector3, float>> pData = new(NumElem());
        solarSystems.ForEach(s => pData.AddRange(s.GetData()));
        return pData;
    }

    public int NumElem()
    {
        int res = 0;
        foreach (SolarSystemDataHolder s in solarSystems)
        {
            res+=s.NumElem();
        }
        return res;
    } 

    public bool IsDirty()
    {
        return this.dirty;
    }
    public void MarkDirty()
    {
        this.dirty = true;
    }
    public void UnmarkDirty()
    {
        this.dirty = false;
    }

    public void HookInit()
    {
        solarSystems.ForEach(s => 
        {
            s.OnChange = MarkDirty;
            s.HookInit();
        });
    }

    public void HookClean()
    {
        solarSystems.ForEach(s => 
        {
            s.OnChange = null;
            s.HookClean();
        });
    }
}
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

    public List<Vector4>[] GetData()
    {
        List<Vector4>[] pData = new List<Vector4>[NumElem()];
        int l = 0;
        for (int s = 0; s < solarSystems.Count; s++)
        {
            List<Vector4>[] pDatas = solarSystems[s].GetData();
            for (int p = 0; p < pDatas.Length; p++)
            {
                pData[l] = pDatas[p];
                l++;
            }
        }
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
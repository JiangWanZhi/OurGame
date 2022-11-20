using System;
using System.Collections.Generic;

public class ObjectPool<K, V> where V : class
{
    Action<V> dter;
    Dictionary<K, List<V>> objectPoolList = new Dictionary<K, List<V>>();
    public ObjectPool(Action<V> d = null)
    {
        dter = d;
    }

    public void Recycle(K k, V v)
    {
        if (!objectPoolList.ContainsKey(k))
        {
            objectPoolList.Add(k, new List<V>());
        }
        objectPoolList[k].Add(v);
    }

    public V Request(K k)
    {
        V ret = null;
        if (objectPoolList.TryGetValue(k, out var list))
        {
            if (list.Count > 0)
            {
                ret = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
            }
        }
        return ret;
    }

    public void Clear()
    {
        if (dter != null)
        {
            foreach (var item in objectPoolList)
            {
                foreach (var sitem in item.Value)
                {
                    dter(sitem);
                }
            }
        }
        objectPoolList.Clear();
    }
}

public class CoreObject
{
    public int Id { get; set; }
    public bool Active { get; set; }
    public bool Recycled { get; set; }

    protected virtual void OnUpdate() { }
    protected virtual void OnRecycle() { }
    protected virtual void OnStart() { }

    public void Update() { OnUpdate(); }
    public void Start() { OnStart(); }
    public void Recycle() { OnRecycle(); }

    public void Destroy() { CoreObjectManager.Instance.Recycle(this); }

}

public class CoreObjectManager : BaseMonoManager<CoreObjectManager>
{
    int _objectIdOrder = 0;
    Dictionary<int, CoreObject> _objectMap = new Dictionary<int, CoreObject>();
    List<CoreObject> _toRecycle = new List<CoreObject>();
    List<CoreObject> _toAdd = new List<CoreObject>();
    ObjectPool<Type, CoreObject> objectPool = new ObjectPool<Type, CoreObject>();





    public T Request<T>() where T : CoreObject, new()
    {
        if (!(objectPool.Request(typeof(T)) is T ret))
        {
            ret = new T();
        }
        ret.Id = ++_objectIdOrder;
        ret.Active = true;
        ret.Recycled = false;
        AddObject(ret, false);
        return ret;
    }

    public void Recycle(CoreObject o)
    {
        o.Recycled = true;
        o.Active = false;
        o.Recycle();
        _toRecycle.Add(o);
    }

    public Dictionary<int, CoreObject> GetAllObject()
    {
        return _objectMap;
    }

    public void Clear()
    {
        foreach (var item in _objectMap)
        {
            _Recycle(item.Value);
        }

        foreach (var item in _toAdd)
        {
            _Recycle(item);
        }

        _objectIdOrder = 0;
        _objectMap.Clear();
        _toRecycle.Clear();
        _toAdd.Clear();
        objectPool.Clear();
    }

    public void Update()
    {
        foreach (var item in _objectMap)
        {
            if (item.Value.Active)
            {
                item.Value.Update();
            }
        }
        ProcessAdd();
        ProcessDelete();
    }

    public void AddObject(CoreObject o, bool delay = true)
    {
        if (o.Id != 0 && !_objectMap.ContainsKey(o.Id))
        {
            if (delay)
            {
                _toAdd.Add(o);
            }
            else
            {
                _objectMap[o.Id] = o;
                o.Start();
            }
        }
    }

    public T GetObject<T>(int id) where T : CoreObject
    {
        CoreObject o = null;
        if (id != 0)
        {
            _objectMap.TryGetValue(id, out o);
            if (o != null && o.Recycled)
            {
                o = null;
            }
        }
        T ret = o as T;
        return ret;
    }








    void _Recycle(CoreObject o)
    {
        objectPool.Recycle(o.GetType(), o);
    }

    void ProcessDelete()
    {
        for (int i = 0; i < _toRecycle.Count; i++)
        {
            CoreObject o = _toRecycle[i];
            _objectMap.Remove(o.Id);
            _Recycle(o);
        }
        _toRecycle.Clear();
    }

    void ProcessAdd()
    {
        for (int i = 0; i < _toAdd.Count; i++)
        {
            AddObject(_toAdd[i], false);
        }
        _toAdd.Clear();
    }
}
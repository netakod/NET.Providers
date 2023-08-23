using System;
using System.Collections;
using System.Collections.Generic;

public class SynchronizedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private Dictionary<TKey, TValue> innerDict;
    private readonly object syncRoot = new object();

    public SynchronizedDictionary()
    {
        innerDict = new Dictionary<TKey, TValue>();
    }

    public object SyncRoot
    { get { return syncRoot; } }

    #region IDictionary<TKey,TValue> Members

    public void Add(TKey key, TValue value)
    {
        lock (syncRoot)
        {
            innerDict.Add(key, value);
        }
    }

    public bool ContainsKey(TKey key)
    {
        lock (syncRoot)
        {
            return innerDict.ContainsKey(key);
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            lock (syncRoot)
            {
                return innerDict.Keys;
            }
        }
    }

    public bool Remove(TKey key)
    {
        lock (syncRoot)
        {
            return innerDict.Remove(key);
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (syncRoot)
        {
            return innerDict.TryGetValue(key, out value);
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            lock (syncRoot)
            {
                return innerDict.Values;
            }
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            lock (syncRoot)
            {
                return innerDict[key];
            }
        }
        set
        {
            lock (syncRoot)
            {
                innerDict[key] = value;
            }
        }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        lock (syncRoot)
        {
            (innerDict as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        }
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            innerDict.Clear();
        }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        lock (syncRoot)
        {
            return (innerDict as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        lock (syncRoot)
        {
            (innerDict as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }
    }

    public int Count
    {
        get
        {
            lock (syncRoot)
            {
                return innerDict.Count;
            }
        }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        lock (syncRoot)
        {
            return (innerDict as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return innerDict.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
        return innerDict.GetEnumerator();
    }

    #endregion
}

//And a simple example of usage should clear up the questions:

//static GenericSyncedDictionary<string, object> SynchronizedDictionary= new SynchronizedDictionary<string, object>();

//public static string AddObject(object value)
//{
//    string id = new Guid().ToString();
//    //No locking needed, as the wrapper locks for add
//    syncedDict.Add(id, value);
//    return id;
//}

//public static object[] SelectCahcedObjects()
//{

//    object[] returnValue;
//    //Ensures that no one tinkers with the dictionary, while the linq query executes
//    lock (syncedDict.SyncRoot)
//    {
//        var query = from o in syncedDict
//                    where o.Value.GetHashCode() > 42
//                    select o.Value;
//        returnValue = query.ToArray();
//    }
//    return returnValue;
//}
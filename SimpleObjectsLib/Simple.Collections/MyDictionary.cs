using System;

namespace Simple.Collections
{
    public class MyDictionary<TKey, TValue>  where TKey : IComparable<TKey>
    {
        private readonly System.Collections.Generic.Dictionary<TKey, TValue> dictionary = new System.Collections.Generic.Dictionary<TKey, TValue>();
        private readonly System.Collections.Generic.Dictionary<string, IIndex> indices = new System.Collections.Generic.Dictionary<string, IIndex>();

        public virtual void Add(TKey Key, TValue Value )
        {
            lock (this.dictionary)
            {
                this.dictionary.Add(Key, Value);
            }
        }

        public virtual bool Remove(TKey Key)
        {
            lock (this.dictionary)
            {
                foreach (IIndex index in this.indices.Values)
                {
                    index.RemoveKey(Key);
                }

                return (this.dictionary.Remove(Key));
            }
        }

        public virtual int Count
        {
            get
            {
                lock (this.dictionary)
                {
                    return (this.dictionary.Count);
                }
            }
        }

        public virtual TValue this[TKey Key]
        {
            get
            {
                lock (this.dictionary)
                {
                    return (this.dictionary[Key]);
                }
            }

            set
            {
                lock (this.dictionary)
                {
                    this.dictionary[Key] = value;
                }

                return;
            }
        }

        public virtual void AddIndex<TIndex>(string IndexName)
        {
            lock (this.dictionary)
            {
                if (this.indices.ContainsKey(IndexName))
                {
                    throw (new System.InvalidOperationException("An index with that name already exists"));
                }

                this.indices[IndexName] = new MyIndex<TIndex>();
            }

            return;
        }

        public virtual bool RemoveIndex(string Name)
        {
            lock (this.dictionary)
            {
                return (this.indices.Remove(Name));
            }
        }

        public virtual void SetIndexEntry<TIndex>(string IndexName, TIndex IndexValue, TKey Key)
        {
            lock (this.dictionary)
            {
                if (!this.dictionary.ContainsKey(Key))
                {
                    throw (new System.InvalidOperationException("That key doesn't exist"));
                }

                ((IIndex<TIndex>)this.indices[IndexName]).AddIndex(IndexValue, Key);
            }

            return;
        }

        public virtual bool RemoveIndexEntry<TIndex>(string IndexName, TIndex IndexValue)
        {
            lock (this.dictionary)
            {
                return (((IIndex<TIndex>)this.indices[IndexName]).RemoveIndex(IndexValue));
            }
        }

        public virtual TValue GetIndexValue<TIndex>(string IndexName, TIndex IndexValue )
        {
            lock (this.dictionary)
            {
                return (this.dictionary[((IIndex<TIndex>)this.indices[IndexName])[IndexValue]]);
            }
        }

        public virtual bool IndexContains<TIndex>(string IndexName, TIndex IndexValue)
        {
            lock (this.dictionary)
            {
                return (((IIndex<TIndex>)this.indices[IndexName]).ContainsIndex(IndexValue));
            }
        }

        private interface IIndex
        {
            bool RemoveKey(TKey Key);
        }

        private interface IIndex<TIndex> : IIndex
        {
            void AddIndex(TIndex Index, TKey Key);
            bool RemoveIndex(TIndex Index);
            TKey this[TIndex Index] { get; }
            bool ContainsIndex(TIndex Index);
        }

        private class MyIndex<TIndex> : IIndex<TIndex>
        {
            public readonly System.Collections.Generic.Dictionary<TKey, System.Collections.Generic.HashSet<TIndex>> byKey = new System.Collections.Generic.Dictionary<TKey, System.Collections.Generic.HashSet<TIndex>>();
            public readonly System.Collections.Generic.Dictionary<TIndex, TKey> byIndex = new System.Collections.Generic.Dictionary<TIndex, TKey>();

            public void AddIndex(TIndex Index, TKey Key)
            {
                if (!this.byKey.ContainsKey(Key))
                {
                    this.byKey[Key] = new System.Collections.Generic.HashSet<TIndex>();
                }

                this.byKey[Key].Add(Index);
                this.byIndex[Index] = Key;
            }

            public bool RemoveIndex(TIndex Index)
            {
                this.byKey[this.byIndex[Index]].Remove(Index);
                return (this.byIndex.Remove(Index));
            }

            public bool RemoveKey(TKey Key)
            {
                if (this.byKey[Key] != null)
                {
                    foreach (TIndex index in this.byKey[Key])
                    {
                        this.byIndex.Remove(index);
                    }
                }

                return (this.byKey.Remove(Key));
            }

            public TKey this[TIndex Index]
            {
                get { return (this.byIndex[Index]); }
            }

            public bool ContainsIndex(TIndex Index)
            {
                return (this.byIndex.ContainsKey(Index));
            }
        }
    }

    public static class Template
    {
        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
            int result = 0;

            try
            {
                MyDictionary<string, int> dic = new MyDictionary<string, int>();

                dic["One"] = 1;
                dic["Two"] = 2;
                dic["Three"] = 3;

                System.Console.WriteLine(dic["One"]);
                System.Console.WriteLine(dic["Two"]);
                System.Console.WriteLine(dic["Three"]);


                dic.AddIndex<string>("Spanish");

                dic.SetIndexEntry("Spanish", "Uno", "One");
                dic.SetIndexEntry("Spanish", "Dos", "Two");
                dic.SetIndexEntry("Spanish", "Tres", "Three");

                System.Console.WriteLine(dic.GetIndexValue("Spanish", "Uno"));
                System.Console.WriteLine(dic.GetIndexValue("Spanish", "Dos"));
                System.Console.WriteLine(dic.GetIndexValue("Spanish", "Tres"));


                dic.AddIndex<string>("German");

                dic.SetIndexEntry("German", "Ein", "One");
                dic.SetIndexEntry("German", "Zwei", "Two");
                dic.SetIndexEntry("German", "Drei", "Three");

                System.Console.WriteLine(dic.GetIndexValue("German", "Ein"));
                System.Console.WriteLine(dic.GetIndexValue("German", "Zwei"));
                System.Console.WriteLine(dic.GetIndexValue("German", "Drei"));


                dic["One"] = -1;

                System.Console.WriteLine(dic.GetIndexValue("Spanish", "Uno"));
                System.Console.WriteLine(dic.GetIndexValue("Spanish", "Dos"));
                System.Console.WriteLine(dic.GetIndexValue("Spanish", "Tres"));

                System.Console.WriteLine(dic.GetIndexValue("German", "Ein"));
                System.Console.WriteLine(dic.GetIndexValue("German", "Zwei"));
                System.Console.WriteLine(dic.GetIndexValue("German", "Drei"));


                dic.RemoveIndexEntry("Spanish", "Tres");

                System.Console.WriteLine(dic.IndexContains("Spanish", "Tres"));
                System.Console.WriteLine(dic.IndexContains("German", "Drei"));


                dic.Remove("Three");

                System.Console.WriteLine(dic.IndexContains("Spanish", "Tres"));
                System.Console.WriteLine(dic.IndexContains("German", "Drei"));
            }
            catch (System.Exception err)
            {
                while (err != null)
                {
                    System.Console.WriteLine(err);

                    err = err.InnerException;
                }
            }

            return (result);
        }
    }
}
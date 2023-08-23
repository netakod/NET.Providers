using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public class NameIndexDictionary<T> : NameIndexDictionary, IEnumerable<T>, IEnumerable where T : NameIndex
    {
        public NameIndexDictionary()
        {
        }

        public NameIndexDictionary(bool ignoreCase)
            : base(ignoreCase)
        {
        }

        protected NameIndexDictionary(Dictionary<int, NameIndex> indexDictionary, Dictionary<string, NameIndex> nameDictionary, bool ignoreCase, bool isReadOnly)
            : base(indexDictionary, nameDictionary, ignoreCase, isReadOnly)
        {
        }

        public new T this[int indexKey]
        {
            get { return base[indexKey] as T; }
        }

        public new T this[string nameKey]
        {
            get { return base[nameKey] as T; }
        }

        public void Add(T nameIndex)
        {
            base.Add(nameIndex);
        }

        public bool Remove(T nameIndex)
        {
            return base.Remove(nameIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns> A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection. </returns>
        public new IEnumerator<T> GetEnumerator()
        {
            foreach (T item in this as IEnumerable)
                yield return item;
        }
    }
    
    
    public class NameIndexDictionary : IEnumerable<NameIndex>, IEnumerable
    {
        private Dictionary<int, NameIndex> indexDictionary = null;
        private Dictionary<string, NameIndex> nameDictionary = null;
        private bool ignoreCase = false;
        private bool isReadOnly = false;
        private NameIndexDictionary readOnlyNameIndexDictionary = null;

        public NameIndexDictionary()
        {
            this.indexDictionary = new Dictionary<int, NameIndex>();
            this.nameDictionary = new Dictionary<string, NameIndex>();
        }

        public NameIndexDictionary(bool ignoreCase)
            : this()
        {
            this.ignoreCase = ignoreCase;
        }

        protected NameIndexDictionary(Dictionary<int, NameIndex> indexDictionary, Dictionary<string, NameIndex> nameDictionary, bool ignoreCase, bool isReadOnly)
        {
            this.indexDictionary = indexDictionary;
            this.nameDictionary = nameDictionary;
            this.ignoreCase = ignoreCase;
            this.isReadOnly = isReadOnly;
        }
        
        public NameIndex this[int indexKey]
        {
            get { return this.indexDictionary[indexKey]; }
        }

        public NameIndex this[string nameKey]
        {
            get { return this.nameDictionary[nameKey]; }
        }

        public int Count
        {
            get { return this.indexDictionary.Count; }
        }

        public bool IgnoreCase
        {
            get { return this.ignoreCase; }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public void Add(NameIndex nameIndex)
        {
            if (!this.IsReadOnly)
            {
                this.indexDictionary.Add(nameIndex.Index, nameIndex);
                this.NameDictionaryAdd(nameIndex.Name, nameIndex);
            }
            else
            {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        public bool Remove(int indexKey)
        {
            if (!this.IsReadOnly)
            {
                if (this.ContainsKey(indexKey))
                {
                    NameIndex nameIndex = this[indexKey];
                    this.indexDictionary.Remove(nameIndex.Index);
                    this.NameDictionaryRemove(nameIndex.Name);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        public bool Remove(string nameKey)
        {
            if (!this.IsReadOnly)
            {
                if (this.ContainsKey(nameKey))
                {
                    NameIndex nameIndex = this[nameKey];
                    this.indexDictionary.Remove(nameIndex.Index);
                    this.NameDictionaryRemove(nameIndex.Name);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        public bool Remove(NameIndex nameIndex)
        {
            if (!this.IsReadOnly)
            {
                if (this.ContainsKey(nameIndex.Index) && this.ContainsKey(nameIndex.Name))
                {
                    this.indexDictionary.Remove(nameIndex.Index);
                    this.NameDictionaryRemove(nameIndex.Name);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        public bool ContainsKey(int indexKey)
        {
            return this.indexDictionary.ContainsKey(indexKey);
        }

        public bool ContainsKey(string nameKey)
        {
            if (this.ignoreCase)
            {
                foreach (KeyValuePair<string, NameIndex> nameDictionaryItem in this.nameDictionary)
                {
                    if (nameDictionaryItem.Value.Name.ToLower().Trim() == nameKey.ToLower().Trim())
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return this.nameDictionary.ContainsKey(nameKey);
            }
        }

        public NameIndexDictionary AsReadOnly()
        {
            if (this.readOnlyNameIndexDictionary == null)
            {
                this.readOnlyNameIndexDictionary = new NameIndexDictionary(this.indexDictionary, this.nameDictionary, this.ignoreCase, true);
            }

            return this.readOnlyNameIndexDictionary;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns> A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection. </returns>
        public IEnumerator<NameIndex> GetEnumerator()
        {
            return this.indexDictionary.Values.GetEnumerator();
        }

        private void NameDictionaryAdd(string nameKey, NameIndex nameIndex)
        {
            string key = this.GetNameKey(nameKey);
            this.nameDictionary.Add(key, nameIndex);
        }
        
        private void NameDictionaryRemove(string nameKey)
        {
            string key = this.GetNameKey(nameKey);
            this.nameDictionary.Remove(key);
        }

        private string GetNameKey(string nameKey)
        {
            string key = this.ignoreCase ? nameKey.ToLower() : nameKey;
            return key;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.indexDictionary.GetEnumerator();
        }

    }
}

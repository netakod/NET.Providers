using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Simple.Collections
{
    /// <summary>
    /// ImmutableHash is a hybrid Hashtable collection in which:
    /// 
    /// - The internal member Hashtable is never directly accessible to the user,
    ///   to prevent them from modifying its contents outside of the interface.
    /// 
    /// - Items may only be added to the collection via the Add () method, and 
    ///   only until the SetReadOnly () method is called.  Once that method is
    ///   invoked, the Hashtable is immutable.
    /// 
    /// - Immutability is enforced both by exception throwing and by Debug.Assert tests
    ///   (since this is a design issue) when the program is run in the debug mode.
    ///   So if a user attempts to call a method such as Add () after the p_isReadOnlyFlag 
    ///   is true, the ImmutableHash object throws either an assertion error when running
    ///   in Debug mode, or a InvalidOperationException in Release mode.
    /// </summary>
    public class ImmutableHashtable
    {
        #region Objects and Variables:

        private readonly string r_readonlyExceptionText =
          "Attempt to modify the hashtable after it has been set to Immutable";

        /// <summary>
        /// The Hashtable holding all internal entries
        /// </summary>
        private Hashtable m_htable = null;


        /// <summary>
        /// Mutability Flag.  Once it is changed to true, it cannot be reset.
        /// </summary>
        private bool m_isReadOnlyFlag = false;

        /// <summary>
        /// Mutability Flag.  Once it is changed to true, it cannot be reset.
        /// </summary>
        public bool p_isReadOnlyFlag
        {
            get { return m_isReadOnlyFlag; }
        }

        #endregion

        #region - Other Properties

        /// <summary>
        /// Given a key object, return the associated value object.
        /// </summary>
        /// <param name="key">a hashtable key object</param>
        public object this[object key]
        {
            get
            {
                return m_htable[key];
            }

            set
            {
                Debug.Assert(!m_isReadOnlyFlag, r_readonlyExceptionText);

                if (m_isReadOnlyFlag)
                    throw new InvalidOperationException(r_readonlyExceptionText);

                m_htable[key] = value;
            }
        }


        /// <summary>
        /// Count of the table keys:
        /// </summary>
        public int Count
        {
            get
            {
                return m_htable.Count;
            }
        }

        #endregion -

        #region Constructors and Initializers:

        #region - Constructors

        /// <summary>
        /// Default constructor:
        /// </summary>
        public ImmutableHashtable()
        {
            m_htable = new Hashtable();
        }


        /// <summary>
        /// Constructor, specifying initial capacity:
        /// </summary>
        /// <param name="initCapacity">initial capacity</param>
        public ImmutableHashtable(int initCapacity)
        {
            m_htable = new Hashtable(initCapacity);
        }


        /// <summary>
        /// Constructor, specifying a source hash or dictionary 
        /// from which to copy data
        /// </summary>
        /// <param name="srcDictionary">source hash or dictionary from which to copy data</param>
        public ImmutableHashtable(IDictionary srcDictionary)
        {
            m_htable = new Hashtable(srcDictionary);
        }



        /// <summary>
        /// Constructor, in which the user provides a new Hashtable to be managed.
        /// </summary>
        /// <param name="newHashtable">externally created Hashtable object</param>
        public ImmutableHashtable(Hashtable newHT)
        {
            m_htable = newHT;
        }

        #endregion -

        #endregion

        #region Public Methods:

        /// <summary>
        /// Once you call this method, the hashtable becomes immutable.
        /// </summary>
        public void SetReadOnly()
        {
            m_isReadOnlyFlag = true;
        }


        #region - Entry Management Operations:

        /// <summary>
        /// Add a key/value object pair to the list.
        /// </summary>
        /// <param name="key">any legal hashtable key object</param>
        /// <param name="value">any legal hashtable value object</param>
        public void Add(object key, object value)
        {
            Debug.Assert(!m_isReadOnlyFlag, r_readonlyExceptionText);

            if (m_isReadOnlyFlag)
                throw new InvalidOperationException(r_readonlyExceptionText);

            m_htable[key] = value;
        }



        /// <summary>
        /// Add a key/value object pair to the list, if the key is not already present.
        /// Returns false if key was already present.
        /// </summary>
        /// <param name="key">any legal hashtable key object</param>
        /// <param name="value">any legal hashtable value object</param>
        public bool AddIfNew(object key, object value)
        {
            bool added_flag = false;

            if (!m_htable.ContainsKey(key))
            {
                Add(key, value);

                added_flag = true;
            }

            return added_flag;
        }


        /// <summary>
        /// Clear the table:
        /// </summary>
        public void Clear()
        {
            Debug.Assert(!m_isReadOnlyFlag, r_readonlyExceptionText);

            if (m_isReadOnlyFlag)
                throw new InvalidOperationException(r_readonlyExceptionText);


            m_htable.Clear();
        }


        #endregion -

        #region - Lookup Operations:

        /// <summary>
        /// Returns true if obj exists in table:
        /// </summary>
        /// <param name="obj"></param>
        public bool ContainsKey(object key)
        {
            Debug.Assert(key != null);

            return m_htable.Contains(key);
        }



        /// <summary>
        /// Given the key, get the associated value, if any.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue(object key)
        {
            return m_htable[key];
        }



        /// <summary>
        /// Safely provide the key collection to the user.
        /// (meaning -- users can't directly access the internal Hashtable object.)
        /// </summary>
        public ICollection Keys
        {
            get { return m_htable.Keys; }
        }

        #endregion -

        #endregion
    }
}

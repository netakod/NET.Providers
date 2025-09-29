using System;
using System.Collections;

namespace Simple.Collections
{
    /// <summary>
    /// A simple singly-linked list implementation. 
    /// </summary>
    [Serializable]
    public class SinglyLinkedList : ICollection, ICloneable
    {
        // Always references the head node in the list. 
        // The node value is used to point to the tail.
        private Node head = new Node();

        // The internal Enumerator class uses this flag to 
        // determine if the enumerator has been invalidated 
        // by updates to the list.
        [NonSerialized]
        private bool isEnumerating = false;

        // The count of items in the list is updated each time an item 
        // is added or removed so that calls to the Count property will
        // always be constant time.
        private int count = 0;

        /// <summary>
        /// Create a default instance, with an empty list.
        /// </summary>
        public SinglyLinkedList()
        {
        }

        /// <summary>
        /// Create a new list that contains all the items in the 
        /// specified collection.
        /// </summary>
        /// <param name="collection">
        /// A collection from which to copy members.
        /// </param>
        public SinglyLinkedList(ICollection collection)
        {
            foreach (object o in collection)
            {
                this.AddBack(o);
            }
        }

        /***************************************************************
        The following methods are the class public interface  
        ***************************************************************/

        /// <summary>
        /// Add an object to the end of the list.
        /// </summary>
        /// <param name="item">
        /// The object to add.
        /// </param>
        /// <remarks>
        /// This method is a constant-time operation.
        /// </remarks>
        public void AddFront(object item)
        {
            // Invalidate any active enumerators
            isEnumerating = false;

            // Insert a new node after the head
            head.Next = new Node(head.Next);

            // Set the value of the new node.
            head.Next.Value = item;

            ++count;
        }

        /// <summary>
        /// Removes an item from the front of the list.
        /// </summary>
        /// <returns>
        /// The value of the node that was removed.
        /// </returns>
        /// <remarks>
        /// This method is a constant-time operation.
        /// </remarks>
        public object RemoveFront()
        {
            // Invalidate any active enumerators
            isEnumerating = false;

            // Point the head's next reference to the node after the 
            // node being removed, effectively unlinking the node from 
            // the chain.
            object o = head.Next.Value;
            head.Next = head.Next.Next;

            --count;

            return o;
        }

        /// <summary>
        /// Add an object to the end of the list.
        /// </summary>
        /// <param name="item">
        /// The object to add.
        /// </param>
        /// <remarks>
        /// This method is a constant-time operation.
        /// </remarks>
        public void AddBack(object item)
        {
            // Invalidate any active enumerators
            isEnumerating = false;

            // Insert a new node after the head.
            head.Next = new Node(head.Next);

            // Store the value in the head.
            head.Value = item;

            // Move the head reference forward, which makes the 
            // old head the new tail of the chain. 
            head = head.Next;
            head.Value = null;

            ++count;
        }

        /// <summary>
        /// Remove an item from the back of the list.
        /// </summary>
        /// <returns>
        /// The value of the node that was removed.
        /// </returns>
        /// <remarks>
        /// This method must walk the list to remove the last item, 
        /// which means this method becomes less efficient as the list 
        /// grows larger.
        /// </remarks>
        public object RemoveBack()
        {
            // Invalidate any active enumerators
            isEnumerating = false;

            // Move the head pointer to the last node in the list, 
            // then call RemoveFront to remove the node that 
            // used to be the head node.

            Node removeNode = head;

            // Find the tail of the list.
            while (head.Next != removeNode)
            {
                head = head.Next;
            }

            return this.RemoveFront();
        }

        /// <summary>
        /// Add an object to the list.
        /// </summary>
        /// <param name="lhs">The list to hold the object.</param>
        /// <param name="rhs">The object to add.</param>
        /// <returns>A reference to the list.</returns>
        public static SinglyLinkedList operator +(
            SinglyLinkedList lhs,
            object rhs
            )
        {
            lhs.AddBack(rhs);
            return lhs;
        }

        /// <summary>
        /// Test for equality of two SinglyLinkedList objects.
        /// </summary>
        /// <param name="lhs">
        /// An object of type <c>SinglyLinkedList</c>.
        /// </param>
        /// <param name="rhs">
        /// An object of type <c>SinglyLinkedList</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the objects are equal; 
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(
            SinglyLinkedList lhs,
            SinglyLinkedList rhs
            )
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Test for inequality of two SinglyLinkedList objects.
        /// </summary>
        /// <param name="lhs">
        /// An object of type <c>SinglyLinkedList</c>.
        /// </param>
        /// <param name="rhs">
        /// An object of type <c>SinglyLinkedList</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the objects are not equal; 
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(
            SinglyLinkedList lhs,
            SinglyLinkedList rhs
            )
        {
            return !lhs.Equals(rhs);
        }

        /***************************************************************
        The following methods implement the ICollection interface. 
        ***************************************************************/

        /// <summary>
        /// When implemented by a class, copies the elements of the 
        /// <c>Collections.ICollection</c> to an Array, starting at a 
        /// particular Array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <c>Array</c> that is the destination of the 
        /// elements copied from <c>Collections.ICollection</c>. The 
        /// <c>Array</c> must have zero-based indexing.
        /// </param>
        /// <param name="index">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public virtual void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("Null array reference", "array");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Index is out of range", "index");
            }

            if (array.Rank > 1)
            {
                throw new ArgumentException("Array is multi-dimensional", "array");
            }

            foreach (object o in this)
            {
                array.SetValue(o, index);
                index++;
            }
        }

        /// <summary>
        /// When implemented by a class, gets the number of elements 
        /// contained in the <c>Collections.ICollection</c>.
        /// </summary>
        public virtual int Count
        {
            get { return this.count; }
        }

        /// <summary>
        /// When implemented by a class, gets a value indicating whether 
        /// access to the <c>Collections.ICollection</c> is synchronized 
        /// (thread-safe).
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// When implemented by a class, gets an object that can be used to 
        /// synchronize access to the <c>Collections.ICollection</c>.
        /// </summary>
        public virtual object SyncRoot
        {
            get { return this; }
        }


        /***************************************************************
        The following method implements the IEnumerable interface. 
        ***************************************************************/

        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>
        /// An <c>IEnumerator</c> that can be used to iterate through the 
        /// collection.
        /// </returns>
        public virtual IEnumerator GetEnumerator()
        {
            // Flag the collection as being in an enumeration state. As 
            // long as this flag is true, the enumeration is valid. If 
            // any nodes are added or removed, existing enumerators are 
            // no longer valid.
            isEnumerating = true;
            return new Enumerator(this);
        }


        /***************************************************************
        The following method implements the ICloneable interface. 
        ***************************************************************/

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual object Clone()
        {
            return new SinglyLinkedList(this);
        }


        /***************************************************************
        The following methods override System.Object methods.
        ***************************************************************/

        /// <summary>
        /// Determines whether the specified <c>Object</c> is equal to the 
        /// current <c>Object</c>.
        /// </summary>
        /// <param name="compare">
        /// The <c>Object</c> to compare with the current <c>Object</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <c>Object</c> is equal to the 
        /// current <c>Object</c>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object compare)
        {
            // If compare is another reference to this, they're the same.
            if (base.Equals(compare))
            {
                return true;
            }
            else
            {
                SinglyLinkedList compareList = compare as SinglyLinkedList;

                // If compare is of type LinkedList, and it has the same 
                // number of elements as this object, continue test.
                if (compareList != null
                    && this.Count == compareList.Count)
                {
                    IEnumerator thisEnum = this.GetEnumerator();
                    IEnumerator compareEnum = compareList.GetEnumerator();

                    // Compare each element in compare to each element 
                    // in this list. If an element does not match, these 
                    // lists are not equal.
                    while (thisEnum.MoveNext() && compareEnum.MoveNext())
                    {
                        if (!thisEnum.Current.Equals(compareEnum.Current))
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type, suitable for 
        /// use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <c>System.Object</c>.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 0;

            foreach (object o in this)
            {
                hashCode ^= o.GetHashCode();
            }

            return hashCode;
        }


        /***************************************************************
        The following classes are internal classes used to support the 
        implmentation of the SinglyLinkedList class.
        ***************************************************************/

        /// <summary>
        /// A node in a singly-linked list.
        /// </summary>
        [Serializable]
        internal class Node
        {
            private object nodeValue;
            private Node next;

            /// <summary>
            /// A reference to the object stored in this node.
            /// </summary>
            public object Value
            {
                get { return this.nodeValue; }
                set { this.nodeValue = value; }
            }

            /// <summary>
            /// The next node in the list.
            /// </summary>
            public Node Next
            {
                get { return this.next; }
                set { this.next = value; }
            }

            /// <summary>
            /// Construct a node which is self-referential 
            /// by default.
            /// </summary>
            public Node()
            {
                this.next = this;
            }

            /// <summary>
            /// Construct a node which points to the node specified by the 
            /// <paramref>next</paramref> parameter.
            /// </summary>
            /// <param name="next">
            /// The next node in the list.
            /// </param>
            public Node(Node next)
            {
                this.next = next;
            }
        }

        /// <summary>
        /// An implementation of <c>IEnumerator</c> to enumerate items in a 
        /// <c>SinglyLinkedList</c> object.
        /// </summary>
        internal class Enumerator : IEnumerator
        {
            private SinglyLinkedList list;
            private Node current;
            private bool isValid;

            /// <summary>
            /// Construct an enumerator for the <c>SinglyLinkedList</c> 
            /// instance specified by the <paramref>list</paramref> 
            /// parameter.
            /// </summary>
            /// <param name="list">
            /// The collection containing the items to be enumerated.
            /// </param>
            public Enumerator(SinglyLinkedList list)
            {
                this.list = list;
                this.current = list.head;
                this.isValid = true;
            }

            /// <summary>
            /// Check if the enumerator is valid, and throw an 
            /// exception if it isn't. 
            /// </summary>
            /// <remarks>
            /// An enumerator is invalid when all the items in 
            /// the list have been enumerated already, or when the 
            /// underlying collection has been modified.
            /// </remarks>
            private void CheckValid()
            {
                if (!isValid || !list.isEnumerating)
                {
                    throw new InvalidOperationException();
                }
            }


            /***************************************************************
            The following methods are implementations of the IEnumerator 
            interface.
            ***************************************************************/

            /// <summary>
            /// Advances the enumerator to the next element of the 
            /// collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next 
            /// element; false if the enumerator has passed the end of the 
            /// collection.
            /// </returns>
            public virtual bool MoveNext()
            {
                // Throw exception if the enumerator is not valid.
                CheckValid();

                // Advance to the next node in the list.
                current = current.Next;

                // If the current node is the head of the list, then 
                // the entire list has been traversed.
                if (current == list.head)
                {
                    // Consequently, the enumerator is no longer valid.
                    isValid = false;
                }

                // Returns true if the current node reference was 
                // successfully advanced; false if the enumerator is 
                // no longer valid.
                return isValid;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before 
            /// the first element in the collection.
            /// </summary>
            public virtual void Reset()
            {
                // The enumerator may only be reset if the underlying 
                // collection has not been modified.
                if (!list.isEnumerating)
                {
                    throw new InvalidOperationException();
                }

                // Move the current node reference back to the 
                // head of the list.
                current = list.head;

                // Once reset, the enumerator is valid again.
                isValid = true;
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public object Current
            {
                get
                {
                    // Throw exception if the enumerator is not valid.
                    CheckValid();
                    return current.Value;
                }
            }
        }

    }

    class Application
    {
        static void Main()
        {
            SinglyLinkedList list = new SinglyLinkedList();

            list.AddBack("Paul");
            list.AddBack("Jen");
            list.AddBack("Madeline");
            list.AddBack("Amanda");

            foreach (string item in list)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine();
            list.RemoveBack();

            foreach (string item in list)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine();
            list.AddFront("Marvin");
            list.AddFront("Dorothy");
            list.AddFront("Al");
            list.AddFront("Vera");

            foreach (string item in list)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine();
            list.AddBack("Amanda");
            list.AddBack("Rover");

            foreach (string item in list)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine();
            list.RemoveFront();

            foreach (string item in list)
            {
                Console.WriteLine(item);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Modeling;

namespace Simple.Datastore
{

    // TODO: Remove FieldName and introduce PropertyIndex
    public class WhereCriteriaElement
    {
        public WhereCriteriaElement(int propertyIndex, object fieldValue, WhereComparator comparator)
            : this(propertyIndex, fieldValue, comparator, LogicalComparator.AND)
        {
        }

        public WhereCriteriaElement(int propertyIndex, object fieldValue, WhereComparator comparator, LogicalComparator comparatorWithPreviousElement)
        {
            this.PropertyIndex = propertyIndex;
            this.FieldValue = fieldValue;
            this.Comparator = comparator;
            this.ComparatorWithPreviousElement = comparatorWithPreviousElement;
        }

        public int PropertyIndex { get; private set; }
        //public string FieldName { get; private set; }
        public object FieldValue { get; private set; }
        public WhereComparator Comparator { get; private set; }
        public LogicalComparator ComparatorWithPreviousElement { get; private set; }
    }
}

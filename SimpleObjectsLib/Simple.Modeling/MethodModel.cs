using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Modeling
{
    public class MethodModel : ModelElement, IMethodModel, IModelElement
    {
        public MethodModel()
        {
            //this.Owner = this;
            
            this.MethodName = String.Empty;
            this.ReturnType = typeof(void);
            this.Arguments = new ModelCollection<MethodArgumentModel>();
            this.Arguments.Owner = this;

        }

        //public MethodModel(string methodName, Type returnType)
        //    : this(methodName, returnType, new Type[] { })
        //{
        //}

        //public MethodModel(string methodName, Type[] methodArgumentTypes)
        //    : this(methodName, typeof(void), methodArgumentTypes)
        //{
        //}

        //public MethodModel(string methodName, Type returnType, Type[] methodArgumentTypes)
        //{
        //    this.MethodName = methodName;
        //    this.ReturnType = returnType;
        //    this.MethodArgumentTypes = methodArgumentTypes;
        //}

        public string MethodName { get; set; }
        public Type ReturnType { get; set; }
        public ModelCollection<MethodArgumentModel> Arguments { get; private set; }

        IList<IMethodArgumentModel> IMethodModel.Arguments
        {
            get { return this.Arguments.AsCustom<IMethodArgumentModel>().AsReadOnly(); }
        }
    }

    public interface IMethodModel : IModelElement
    {
        string MethodName { get; }
        Type ReturnType { get; }
        IList<IMethodArgumentModel> Arguments { get; }
    }
}

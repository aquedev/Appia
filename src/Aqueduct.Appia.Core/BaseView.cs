using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Aqueduct.Appia.Core
{
    public class ViewBase
    {
        private StringBuilder _contents = new StringBuilder();

        public string Contents
        {
            get { return _contents.ToString(); }
        }

        public string Path { get; set; }

        public HttpStringLiteral RenderPartial(string viewName)
        {
            return RenderPartial(viewName, null);
        }

        public HttpStringLiteral RenderPartial(string viewName, object model)
        {
            return RenderPartialImpl(viewName, ExpandoHelper.AnonymousTypeToExpando(model));
        }

        public HttpStringLiteral RenderPartial(string viewName, string jsonModel)
        {
            return RenderPartialImpl(viewName, JsonHelpers.ParseJsonObject(jsonModel));
        }

        public Func<string, dynamic, HttpStringLiteral> RenderPartialImpl = (str, model) => { return new HttpStringLiteral(String.Empty); };

        public dynamic Global { get; set; }

        public dynamic Model { get; set; }

        public string Layout { get; set; }

        public virtual void Execute() { }

        public virtual void Write(object value)
        {
            if (value is HttpStringLiteral)
                WriteLiteral(value);
            else
                WriteLiteral(HttpUtility.HtmlEncode(value));
        }

        public virtual void WriteLiteral(object value)
        {
            _contents.Append(value);
        }
    }
}
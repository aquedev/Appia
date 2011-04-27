using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace Aqueduct.Appia.Core
{
    public abstract class ViewBaseRenderingBase
    {
        protected StringBuilder _contents = new StringBuilder();
        public string Contents
        {
            get { return _contents.ToString(); }
        }

        public Func<string, dynamic, HtmlStringLiteral> RenderPartialImpl = (str, model) =>
        {
            return new HtmlStringLiteral(String.Empty);
        };
        public HtmlStringLiteral RenderPartial(string viewName)
        {
            return RenderPartial(viewName, null);
        }
        public HtmlStringLiteral RenderPartial(string viewName, string jsonModel)
        {
            return RenderPartialImpl(viewName, JsonHelpers.ParseJsonObject(jsonModel));
        }
        public HtmlStringLiteral RenderPartial(string viewName, object model)
        {
            return RenderPartialImpl(viewName, ExpandoHelper.AnonymousTypeToExpando(model));
        }

        public virtual void Write(HelperResult content)
        {
            if (content != null)
                WriteLiteral(content.ToHtmlString());
        }

        public virtual void Write(object value)
        {
            if (value is IHtmlString)
                WriteLiteral(((IHtmlString)value).ToHtmlString());
            else
                WriteLiteral(HttpUtility.HtmlEncode(value));
        }

        public virtual void WriteLiteral(object value)
        {
            _contents.Append(value);
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteTo(TextWriter writer, HelperResult content)
        {
            if (content != null)
                content.WriteTo(writer);
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteTo(TextWriter writer, object content)
        {
            writer.Write(HttpUtility.HtmlEncode(content));
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteLiteralTo(TextWriter writer, object content)
        {
            writer.Write(content);
        }

        public void DefineSection(string name, Action action)
        {
            throw new NotImplementedException("Sections are not implemented yet");
        }

        public abstract void Execute();
    }

    public abstract class ViewBase : ViewBaseRenderingBase
    {
        public string Path { get; set; }

        public dynamic Global { get; set; }

        public dynamic Model { get; set; }

        public string Layout { get; set; }
    }
}
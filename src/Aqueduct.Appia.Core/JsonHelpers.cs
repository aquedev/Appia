using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Text;

namespace Aqueduct.Appia.Core
{
    public class JsonHelpers
    {
        public static dynamic ParseJsonObject(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            JavaScriptSerializer ser = new JavaScriptSerializer();
            var dictionary = ser.Deserialize<IDictionary<string, object>>(json);
            return ExpandoHelper.DictionaryToExpando(dictionary);
        }
    }
}

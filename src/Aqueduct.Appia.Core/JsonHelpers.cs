using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Aqueduct.Appia.Core
{
    public class JsonHelpers
    {
        public static dynamic ParseJsonObject(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
			          
				
            var dictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(json);
            return ExpandoHelper.DictionaryToExpando(dictionary);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Dynamic;

namespace Aqueduct.Appia.Core
{
    public static class ExpandoHelper
    {
        public static dynamic AnonymousTypeToExpando(object obj)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            
            if (obj != null)
            {
                var allProperties = obj.GetType().GetProperties().Select((p) => new { p.Name, Value = p.GetValue(obj, null) });

                foreach (var property in allProperties.Where(p => !expando.ContainsKey(p.Name)))
                    expando.Add(property.Name, property.Value);
            }
            
            return expando;
        }

        public static ExpandoObject DictionaryToExpando(IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDic = (IDictionary<string, object>)expando;

            foreach (var item in dictionary)
            {
                bool alreadyProcessed = false;

                if (item.Value is IDictionary<string, object>)
                {
                    expandoDic.Add(item.Key, DictionaryToExpando((IDictionary<string, object>)item.Value));
                    alreadyProcessed = true;
                }
                else if (item.Value is ICollection)
                {
                    var itemList = new List<object>();
                    foreach (var item2 in (ICollection)item.Value)
                        if (item2 is IDictionary<string, object>)
                            itemList.Add(DictionaryToExpando((IDictionary<string, object>)item2));
                        else
                            itemList.Add(item2);

                    if (itemList.Count > 0)
                    {
                        expandoDic.Add(item.Key, itemList);
                        alreadyProcessed = true;
                    }
                }

                if (!alreadyProcessed)
                    expandoDic.Add(item);
            }

            return expando;
        }
    }
}
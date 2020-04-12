using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.ClearScript.V8;

namespace ClearScriptBug
{
    public static class Program
    {
        public static void Main()
        {
            var objects = new[]
            {
                DateTime.UtcNow,
                new Uri("https://github.com/"),
                new KeyValuePair<string, string>("A", "B"),
                new object(),
                "String!!!",
                0,
                3.14,
                true,
                'A'
            };

            foreach (var o in objects)
            {
                Execute(new Context { Value = o });
            }
        }

        public class Context
        {
            public object Value { get; set; }
        }

        private static void Execute(Context context)
        {
            try
            {
                using var engine = new V8ScriptEngine
                {
                    AllowReflection = true
                };

                engine.AddHostObject("context", context);
                engine.AddHostType("__helper", typeof(Helper));
                engine.Execute("var $ = function(arg){return __helper._(arg);};");

                Console.WriteLine(engine.Evaluate("$(context.Value).ToString()"));
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{exception.Message} Error testing {context.Value} [{context.Value.GetType().Name}]");
            }
        }

        public static class Helper
        {
            public static object _(object o) => new Wrapped(o);

            private class Wrapped : DynamicObject
            {
                private readonly object _o;

                public Wrapped(object o) => _o = o;

                public override IEnumerable<string> GetDynamicMemberNames() => _o.GetType().GetMethods().Select(method => method.Name);

                public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
                {
                    var method = _o.GetType().GetMethod(binder.Name, args.Select(arg => arg.GetType()).ToArray());

                    if (method == null)
                    {
                        result = null;
                        return false;
                    }

                    result = method.Invoke(_o, args);
                    return true;
                }

                public override string ToString() => _o.ToString();
            }
        }
    }
}

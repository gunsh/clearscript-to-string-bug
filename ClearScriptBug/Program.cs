using System;
using System.Collections.Generic;
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

                Console.WriteLine(engine.Evaluate("context.Value.ToString()"));
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{exception.Message} Error testing {context.Value} [{context.Value.GetType().Name}]");
            }
        }
    }
}

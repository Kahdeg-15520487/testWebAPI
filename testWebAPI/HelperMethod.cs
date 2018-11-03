using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace testWebAPI
{
    static class HelperMethod
    {
        public static Guid InputGuid(string prompt = "input guid")
        {
            do
            {
                Console.Write("{0}: ", prompt);
                var input = Console.ReadLine();
                if (Guid.TryParse(input, out Guid id))
                {
                    return id;
                }
            } while (true);
        }

        public static int InputNumber(string prompt = "input number")
        {
            do
            {
                Console.Write("{0}: ", prompt);
                var quantity = Console.ReadLine();
                if (int.TryParse(quantity, out int q))
                {
                    return q;
                }
            } while (true);
        }

        public static string InputPassword()
        {
            string pass = "";
            Console.Write("Enter your password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();

            return pass;
        }

        public static string InputString(string prompt = "input string")
        {
            Console.Write("{0}: ", prompt);
            return Console.ReadLine();
        }

        public static DateTime InputDate(string prompt = "input date")
        {
            Console.WriteLine("{0}: ", prompt);
            return new DateTime(
                                    InputNumber("year"),
                                    InputNumber("month"),
                                    InputNumber("day")
                                );
        }

        public static bool InputPrompt(string prompt = "Is it", string ok = "y|yes", string no = "n|no", bool defaultTo = false)
        {
            Console.Write("{0}?({1}/{2}): ", prompt, ok, no);
            var input = Console.ReadLine();
            var oks = ok.Split('|');
            var nos = no.Split('|');

            if (oks.Any(s => s.Equals(input)))
            {
                return true;
            }

            if (nos.Any(s => s.Equals(input)))
            {
                return true;
            }

            return defaultTo;
        }

        public static IEnumerable<T> GetAllTypeImplementInterface<T>(params Assembly[] assemblies)
        {
            var interfaceType = typeof(T);
            List<T> result = null;
            try
            {
                result = assemblies
                  .SelectMany(x => x.GetTypes())
                  .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                  .Select(x => Activator.CreateInstance(x))
                  .Cast<T>()
                  .ToList()
                  ;
            }
            catch (Exception e)
            {
                if (e is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = e as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;

                    //Console.WriteLine(typeLoadException.Message);
                    //foreach (var exception in loaderExceptions)
                    //{
                    //    Console.WriteLine(exception.Message);
                    //}

                }
                //throw e;
            }

            return result;
        }

        public static IEnumerable<T> GetAllTypeImplementInterfaceInType<T>(object obj)
        {
            var interfaceType = typeof(T);
            var containerType = obj.GetType();
            List<T> result = null;
            try
            {
                result = containerType.GetNestedTypes(BindingFlags.NonPublic)
                    .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x => Activator.CreateInstance(x))
                    .Cast<T>()
                    .ToList()
                    ;
            }
            catch (Exception e)
            {
                if (e is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = e as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;

                    //Console.WriteLine(typeLoadException.Message);
                    //foreach (var exception in loaderExceptions)
                    //{
                    //    Console.WriteLine(exception.Message);
                    //}

                }
                //throw e;
            }

            return result;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string JwtStringToBase64(string s)
        {
            string result = s;
            result = result.Replace('-', '+');
            result = result.Replace('_', '/');
            switch (result.Length % 4)
            {
                case 2:
                    result += "==";
                    break;
                case 3:
                    result += "=";
                    break;
            }
            return result;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}

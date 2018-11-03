using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using testWebAPI.Core;

namespace testWebAPI
{
    class Program
    {
        public static APIClient Client;

        static Dictionary<string, Dictionary<string, IAPICaller>> Modules;
        public static string BaseAddress { get; set; } = "http://localhost:61428";
        public static string Token { get; set; } = "";

        //public static string BaseAddress { get; private set; } = "https://localhost:44320";
        //public static string Token { get; private set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbkBxbHRiLnRtYS5jb20iLCJqdGkiOiJkZTRmNTRhYi0yMWJkLTRiZWMtYmY5NS1iMzdkZjNjZWY2MzMiLCJuYmYiOjE1MzE4MTIzNTUsImV4cCI6MTUzMTg5ODc1NSwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo0NDMyMC8iLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQ0MzIwLyJ9.hlH8U6hJWIQGa7_kPtIxPM8YN_XEk9EA2XZ47zjanQg";

        class Utility : IAPICallerModule
        {
            public string Name => "utility";

            public string Uri => "";

            class BaseAddressSetter : IAPICaller
            {
                public string Module { get; set; }

                public string Name => "setBaseAddr";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    BaseAddress = args[0];
                    return Result.Success(BaseAddress);
                }
            }

            //class CurrentTokenExpireation : IAPICaller
            //{
            //    public string Module { get; set; }

            //    public string Name => "tokenExpire";

            //    public int ArgumentCount => -1;

            //    public Result Do(params string[] args)
            //    {
            //        if (string.IsNullOrWhiteSpace(Token))
            //        {
            //            return Result.Error(string.Format("invalid token : \"{0}\"", Token));
            //        }

            //        var parts = Token.Split('.').Select(s => HelperMethod.Base64Decode(HelperMethod.JwtStringToBase64(s))).ToArray();

            //        var payload = JsonConvert.DeserializeObject<JwtPayload>(parts[1]);

            //        long.TryParse(payload.exp, out long expireAt);

            //        var expireTime = HelperMethod.FromUnixTime(expireAt);

            //        return Result.Success(expireTime.ToLocalTime().ToString());
            //    }
            //}

            //class Refresh : IAPICaller
            //{
            //    public string Module { get; set; }

            //    public string Name => "refresh";

            //    public int ArgumentCount => -1;

            //    public Result Do(params string[] args)
            //    {
            //        Program.Client = new APIClient(BaseAddress, Token);

            //        return Result.Success();
            //    }
            //}

            //class TokenSetter : IAPICaller
            //{
            //    public string Module { get; set; }

            //    public string Name => "setToken";

            //    public int ArgumentCount => 1;

            //    public Result Do(params string[] args)
            //    {
            //        Token = args[0];
            //        return Result.Success();
            //    }
            //}

            //class TokenGetter : IAPICaller
            //{
            //    public string Module { get; set; }

            //    public string Name => "getToken";

            //    public int ArgumentCount => -1;

            //    public Result Do(params string[] args)
            //    {
            //        return Result.Success(Token);
            //    }
            //}

            class HelpCommand : IAPICaller
            {
                public string Module { get; set; }

                public string Name => "help";

                public int ArgumentCount => -1;

                public Result Do(params string[] args)
                {
                    if (args.Length == 2)
                    {

                    }

                    Console.WriteLine("Modules: ");
                    //Console.WriteLine("help \"command\" for more help");
                    foreach (var module in Modules)
                    {
                        Console.WriteLine("  {0}", module.Key);
                        foreach (var command in module.Value.Keys)
                        {
                            Console.WriteLine("    {0}", command);
                        }
                    }

                    return Result.Success();
                }
            }

            class ClearScreen : IAPICaller
            {
                public string Module { get; set; }

                public string Name => "clear";

                public int ArgumentCount => -1;

                public Result Do(params string[] args)
                {
                    Console.Clear();
                    return Result.Success();
                }
            }

            class RunFile : IAPICaller
            {
                public string Module { get; set; }

                public string Name => "run";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var path = args[0];
                    if (!File.Exists(path))
                    {
                        return Result.Error("file not found");
                    }

                    foreach (var line in File.ReadAllLines(path))
                    {
                        Run(ParseArgument(line.Trim()));
                    }

                    return Result.Success();
                }
            }

            class ViewFile : IAPICaller
            {
                public string Module { get; set; }

                public string Name => "view";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var path = args[0];
                    if (!File.Exists(path))
                    {
                        return Result.Error("file not found");
                    }
                    Console.WriteLine();
                    Console.WriteLine(path);
                    foreach (var line in File.ReadAllLines(path))
                    {
                        Console.WriteLine(line);
                    }
                    Console.WriteLine();
                    return Result.Success();
                }
            }

            class LoadModule : IAPICaller
            {
                public string Module { get; set; }

                public string Name => "loadMDL";

                public int ArgumentCount => 1;

                public Result Do(params string[] args)
                {
                    var path = args[0];

                    if (!File.Exists(path))
                    {
                        return Result.Error($"MDL {path} does not exist");
                    }

                    LoadCommands(Assembly.LoadFile(path));

                    return Result.Success($"loaded {args[0]}");
                }
            }
        }

        #region parse argument
        static string[] ParseArgument(string rawInput)
        {
            var result = new List<string>();

            bool isInString = false;
            int cursor = 0;

            StringBuilder accumulator = new StringBuilder();

            void getNewString()
            {
                result.Add(accumulator.ToString());
                accumulator.Clear();
            }

            while (cursor < rawInput.Length)
            {
                var c = rawInput[cursor];

                switch (c)
                {
                    case '"':
                        if (isInString)
                        {
                            isInString = false;
                            getNewString();
                        }
                        else
                        {
                            isInString = true;
                        }
                        break;
                    case ' ':
                    case '.':
                        if (!isInString)
                        {
                            getNewString();
                        }
                        else
                        {
                            accumulator.Append(c);
                        }
                        break;

                    default:
                        accumulator.Append(c);
                        break;
                }

                cursor++;
            }

            if (accumulator.Length > 0)
            {
                getNewString();
            }

            result.RemoveAll(s => string.IsNullOrEmpty(s));

            return result.ToArray();
        }
        #endregion

        static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            //load config

            Client = new APIClient(BaseAddress, Token);

            LoadCommands(AppDomain.CurrentDomain.GetAssemblies());

            bool running = true;
            while (running)
            {
                Console.Write(">");
                var rawInput = Console.ReadLine();

                var input = ParseArgument(rawInput);

                running = Run(input);
            }
            //Console.Write("Press enter to exit...");
            //Console.ReadLine();
        }

        public static bool Run(string[] input)
        {
            if (input.Length == 0)
            {
                return true;
            }

            if (input[0] == "exit")
            {
                return false;
            }

            if (Modules.ContainsKey(input[0]))
            {
                var module = Modules[input[0]];
                if (input.Length == 1)
                {
                    Console.WriteLine(input[0]);
                    foreach (var cmd in module.Keys)
                    {
                        Console.WriteLine("  {0}", cmd);
                    }
                    return true;
                }

                if (module.ContainsKey(input[1]))
                {
                    var command = module[input[1]];

                    if (ValidateArgumentCount(command, input))
                    {
                        Console.WriteLine(command.Do(input.Skip(2).ToArray()));
                    }
                    else
                    {
                        Console.WriteLine("wrong amount of argument: {0} != {1}", input.Length - 2, command.ArgumentCount);
                    }
                }
            }
            else
            {
                var module = Modules["utility"];

                if (module.ContainsKey(input[0]))
                {
                    var command = module[input[0]];

                    if (ValidateArgumentCount(command, input, true))
                    {
                        Console.WriteLine(command.Do(input.Skip(1).ToArray()));
                    }
                    else
                    {
                        Console.WriteLine("wrong amount of argument: {0} != {1}", input.Length - 1, command.ArgumentCount);
                    }
                }
                else
                {
                    Console.WriteLine("command not found : {0}", input[0]);
                }
            }

            return true;
        }

        private static bool ValidateArgumentCount(IAPICaller cmd, string[] input, bool isStandAloneCmd = false)
        {
            if (!isStandAloneCmd)
            {
                return cmd.ArgumentCount == -1 || cmd.ArgumentCount == input.Length - 2;
            }
            else
            {
                return cmd.ArgumentCount == -1 || cmd.ArgumentCount == input.Length - 1;
            }
        }

        private static int LoadCommands(params Assembly[] assemblies)
        {
            var allModule = HelperMethod.GetAllTypeImplementInterface<IAPICallerModule>(assemblies);
            Modules = new Dictionary<string, Dictionary<string, IAPICaller>>();
            int cmdCount = 0;
            foreach (var module in allModule)
            {
                if (Modules.ContainsKey(module.Name))
                {
                    Console.WriteLine("{0} already loaded", module.Name);
                    continue;
                }
                Console.WriteLine("loaded {0}", module.Name);
                var commands = HelperMethod.GetAllTypeImplementInterfaceInType<IAPICaller>(module);
                var Commands = new Dictionary<string, IAPICaller>();
                foreach (var cmd in commands)
                {
                    Console.WriteLine("  {0}", cmd.Name);
                    Commands.Add(cmd.Name, cmd);
                    cmdCount++;
                }
                Modules.Add(module.Name, Commands);
            }

            return cmdCount;

            //Commands = new Dictionary<string, IAPICaller>();
            //foreach (var obj in allModule)
            //{
            //    Commands.Add(obj.Name, obj);
            //    Console.WriteLine("Loaded {0}", obj.Name);
            //}
        }
    }
}

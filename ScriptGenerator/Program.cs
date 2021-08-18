using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.IO;

namespace ScriptGenerator
{
    public class Program
    {
        public static string _basePath = AppDomain.CurrentDomain.BaseDirectory;

        static void Main()
        {
            Logger.LogHeader("Program Started");
            try
            {
                Run();
                Logger.LogHeader("Program Completed");
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        private static void Run()
        {
            var outFolder = Path.Combine(_basePath, $"GeneratedScripts_{GetTimestamp(DateTime.Now)}");
            Directory.CreateDirectory(outFolder);

            var server = GetConnection(AppContext.HostName, AppContext.UserName, AppContext.Password);

            foreach (var db in AppContext.DatabasesToScript)
            {
                var scripter = new Script(db, server, outFolder, AppContext.ItemsToScript.DBs
                    , AppContext.GlobelObjectsToExclude);
                scripter.ScriptAll(AppContext.GenerateBaseline, AppContext.RerunableObjectsToScript);
            }
        }

        private static String GetTimestamp(DateTime value) => value.ToString("yyyyMMddHHmmss");

        private static Server GetConnection(string hostName, string userName, string password)
        {
            Logger.Log($"Connecting to {hostName} Database..");

            ServerConnection srvConn = new ServerConnection(hostName)
            {
                LoginSecure = false,
                Login = userName,
                Password = password
            };
            var server = new Server(srvConn);
            return server;
        }

    }

}

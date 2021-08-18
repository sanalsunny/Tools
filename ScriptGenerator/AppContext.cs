using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace ScriptGenerator
{
    public static class AppContext
    {
        public static string[] DatabasesToScript { get; }
        public static List<string> GlobelObjectsToExclude { get; }
        public static DatabaseObjectTypes[] RerunableObjectsToScript { get; }
        public static string HostName { get; }
        public static string UserName { get; }
        public static string Password { get; }
        public static bool GenerateBaseline { get; }
        public static ObjectModel ItemsToScript { get; }


        static AppContext()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            IConfiguration appConfiguration = new ConfigurationBuilder()
                                                .SetBasePath(basePath)
                                                .AddJsonFile("AppSettings.json").Build();

            DatabasesToScript = appConfiguration.GetSection("DatabasesToScript").Get<string[]>();
            RerunableObjectsToScript = appConfiguration.GetSection("RerunableObjectsToScript").Get<DatabaseObjectTypes[]>();
            GlobelObjectsToExclude = appConfiguration.GetSection("GlobelObjectsToExclude").Get<List<string>>();
            string connKey = "SQLServerConnection";
            HostName = appConfiguration.GetSection(connKey)["HostName"];
            UserName = appConfiguration.GetSection(connKey)["UserName"];
            Password = appConfiguration.GetSection(connKey)["Password"];
            GenerateBaseline = appConfiguration.GetValue<bool>("GenerateBaseline");
            var objListPath = Path.Combine(basePath, "ObjectFilterList.json"); 
            ItemsToScript = JsonConvert.DeserializeObject<ObjectModel>(File.ReadAllText(objListPath));
        }
    }
}

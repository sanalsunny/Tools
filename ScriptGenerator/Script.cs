using Microsoft.SqlServer.Management.Smo;
using ScriptGenerator.Constants;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptGenerator
{
    public class Script
    {
        private readonly string _scriptingDB;
        private readonly Server _server;
        private readonly string _rootFolder;
        private readonly FilterObjects _filterObjects;
        private readonly Generate _genrate;
        private readonly char _nameDelimeter;

        public Script(
            string scriptingDB,
            Server server,
            string rootFolder,
            List<DBCollection> predefinedFilters,
            List<string> globalObjectFilters
            )
        {
            _scriptingDB = scriptingDB;
            _server = server;
            _rootFolder = rootFolder;
            _filterObjects = new FilterObjects(predefinedFilters, globalObjectFilters, scriptingDB, server);
            _genrate = new Generate();
            _nameDelimeter = StringConstants.NameDelimeter;
        }

        public void ScriptAll(bool generateBaseline, DatabaseObjectTypes[] rerunableObjetsToScript)
        {
            Logger.LogHeader($"{_scriptingDB} Started");

            var dbPath = SetupOutFolder(_rootFolder, _scriptingDB);

            if (generateBaseline)
            {
                ScriptBaseline(_server, _scriptingDB, dbPath);
            }

            foreach (var obj in rerunableObjetsToScript)
            {
                ScriptRerunableObject(_server, _scriptingDB, dbPath, obj);
            }

            Logger.LogHeader($"{_scriptingDB} Completed");
        }

        private void ScriptBaseline(Server server, string scriptingDb, string outFolder)
        {
            var migPath = SetupOutFolder(outFolder, "Migration");
            var objectToScript = DatabaseObjectTypes.ExtendedStoredProcedure |
                                                DatabaseObjectTypes.FullTextCatalog |
                                                DatabaseObjectTypes.PartitionFunction |
                                                DatabaseObjectTypes.PartitionScheme |
                                                DatabaseObjectTypes.Schema |
                                                DatabaseObjectTypes.Synonym |
                                                DatabaseObjectTypes.Table |
                                                DatabaseObjectTypes.UserDefinedAggregate |
                                                DatabaseObjectTypes.UserDefinedDataType |
                                                DatabaseObjectTypes.UserDefinedType |
                                                DatabaseObjectTypes.XmlSchemaCollection |
                                                DatabaseObjectTypes.UserDefinedTableTypes |
                                                DatabaseObjectTypes.FullTextStopList |
                                                DatabaseObjectTypes.Sequence |
                                                DatabaseObjectTypes.QueryStoreOptions;

            Logger.LogHeader($"Baseline object filtering for {scriptingDb} Started");

            var filteredObjects = _filterObjects.GetObjects(objectToScript, true);

            Logger.LogHeader($"Baseline object filtering  for {scriptingDb} Completed");

            Logger.LogHeader($"Baseline scripting for {scriptingDb} Started");

            _genrate.GenerateScript(Path.Combine(migPath, "V1__Baseline.sql"), server, filteredObjects.Values.ToArray(), true);

            Logger.LogHeader($"Baseline scripting for {scriptingDb} Completed");
        }


        private static string SetupOutFolder(string maindir, string subdir)
        {
            var outPath = Path.Combine(maindir, subdir);
            Directory.CreateDirectory(outPath);
            return outPath;
        }

        private void ScriptRerunableObject(Server server, string scriptingDb,
            string outFolder, DatabaseObjectTypes objectToScript)
        {
            var objectPath = SetupOutFolder(outFolder, objectToScript.ToString());

            Logger.LogHeader($"{objectToScript} filtering for {scriptingDb} Started");

            var filteredObjects = _filterObjects.GetObjects(objectToScript, false, _nameDelimeter);

            Logger.LogHeader($"{objectToScript} filtering for {scriptingDb} Completed");

            foreach (var obj in filteredObjects)
            {
                var objectName = obj.Key.Split(_nameDelimeter)[0];

                Logger.Log($"Scripting for {objectName} Started");

                var outFile = Path.Combine(objectPath, $"R__{objectName}.sql");
                _genrate.GenerateScript(outFile, server, new[] { obj.Value });

                Logger.Log($"Scripting for {objectName} Completed");
            }
        }
    }
}

using Microsoft.SqlServer.Management.Smo;
using PoorMansTSqlFormatterLib;
using System.IO;
using System.Text;

namespace ScriptGenerator
{
    public class Generate
    {
        public void GenerateScript(string outFile, Server server, SqlSmoObject[] scriptingObjects, bool forBaseline = false)
        {
            Scripter scripter = new Scripter
            {
                Server = server
            };
            scripter.Options.IncludeHeaders = false;
            scripter.Options.SchemaQualify = true;
            scripter.Options.ToFileOnly = true;
            scripter.Options.FileName = outFile;
            scripter.Options.TargetServerVersion = SqlServerVersion.Version140;
            scripter.Options.IncludeDatabaseContext = false;
            scripter.Options.AllowSystemObjects = false;
            scripter.Options.AnsiFile = true;
            scripter.Options.Encoding = Encoding.UTF8;

            if (forBaseline)
            {
                scripter.Options.DriAllConstraints = true;
                scripter.Options.DriAllKeys = true;
                scripter.Options.DriChecks = true;
                scripter.Options.DriClustered = true;
                scripter.Options.DriDefaults = true;
                scripter.Options.DriForeignKeys = true;
                scripter.Options.DriIncludeSystemNames = true;
                scripter.Options.DriIndexes = true;
                scripter.Options.DriNonClustered = true;
                scripter.Options.DriPrimaryKey = true;
                scripter.Options.DriUniqueKeys = true;
                scripter.Options.ExtendedProperties = true;
                scripter.Options.FullTextCatalogs = true;
                scripter.Options.FullTextIndexes = true;
                scripter.Options.FullTextStopLists = true;
                scripter.Options.IncludeFullTextCatalogRootPath = true;
                scripter.Options.Indexes = true;
                scripter.Options.NonClusteredIndexes = true;
                scripter.Options.SchemaQualifyForeignKeysReferences = true;
                scripter.Options.SpatialIndexes = true;
                scripter.Options.Triggers = true;
                scripter.Options.NoCollation = true;
            }

            scripter.Script(scriptingObjects);

            if (!forBaseline)
            {
                FormatGeneratedFile(outFile);
            }
        }

        private static void FormatGeneratedFile(string outFile)
        {
            var sqlFormattingManager = new SqlFormattingManager();
            var sqltext = File.ReadAllText(outFile);
            File.WriteAllText(outFile, sqlFormattingManager.Format(sqltext)
                .Replace("CREATE P", "CREATE OR ALTER P")
                .Replace("CREATE F", "CREATE OR ALTER F")
                .Replace("CREATE V", "CREATE OR ALTER V"));
        }
    }
}

using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ScriptGenerator
{
    public class FilterObjects
    {
        private readonly List<DBCollection> _predefinedFilters;
        private readonly List<string> _globalObjectFilters;
        private readonly string _scriptingDB;
        private readonly Server _server;

        public FilterObjects(
            List<DBCollection> predefinedFilters,
            List<string> globalObjectFilters,
            string scriptingDB,
            Server server)
        {
            _predefinedFilters = predefinedFilters;
            _globalObjectFilters = globalObjectFilters;
            _scriptingDB = scriptingDB;
            _server = server;
        }

        public SortedList<string, SqlSmoObject> GetObjects(DatabaseObjectTypes objectToScript, bool forBaseline = false
            , char nameDelimeter = 'º')
        {
            var db = _server.Databases[_scriptingDB];
            DataTable dataTable = db.EnumObjects(objectToScript);

            var objlist = new SortedList<string, SqlSmoObject>();
            GetPredefinedFilter(objectToScript, out string[] definedObjs, out string[] excludedObjs, forBaseline);

            foreach (DataRow row in dataTable.Rows)
            {
                string sSchema = (string)row["Schema"];
                string sName = (string)row["Name"];

                if (sSchema == "sys" || sSchema == "INFORMATION_SCHEMA")
                {
                    goto Skip;
                }

                if (_globalObjectFilters.Any(s => sName.Contains(s))
                    || excludedObjs.Contains(sName)
                    || (definedObjs.Length > 0 && !definedObjs.Contains(sName)))
                {
                    goto Skip;
                }

                var objectSMO = _server.GetSmoObject(new Urn((string)row["Urn"]));
                switch (objectToScript)
                {
                    case DatabaseObjectTypes.StoredProcedure:
                        var spobj = (StoredProcedure)objectSMO;
                        if (spobj.IsSystemObject)
                            goto Skip;
                        break;

                    case DatabaseObjectTypes.View:
                        var viewobj = (View)objectSMO;
                        if (viewobj.IsSystemObject)
                            goto Skip;
                        break;

                    case DatabaseObjectTypes.UserDefinedFunction:
                        var funobj = (UserDefinedFunction)objectSMO;
                        if (funobj.IsSystemObject)
                            goto Skip;
                        break;

                }
                objlist.Add($"{row["Name"]}{nameDelimeter}{row[0]}", objectSMO);
                Logger.Log($"{_scriptingDB}-{sName}(Added To List)");
                continue;

            Skip:
                Logger.Log($"{_scriptingDB}-{sName}(Skipped)");

            }

            return objlist;
        }

        private void GetPredefinedFilter(DatabaseObjectTypes objectToScript, out string[] definedObjs, out string[] excludedObjs
            , bool forBaseline = false)
        {
            var processingObject = forBaseline ? "Baseline" : objectToScript.ToString();

            definedObjs = new string[] { };
            excludedObjs = new string[] { };

            var dbItem = _predefinedFilters.Where(x => x.DBName == _scriptingDB)
                        .FirstOrDefault();

            if (dbItem != null)
            {
                var excl = dbItem.ExcludedObjects
                                .Where(x => x.ObjectType == processingObject)
                                .Select(x => x.ObjectNames)
                                .FirstOrDefault();

                var def = dbItem.ScriptSpecificOnes
                                .Where(x => x.ObjectType == processingObject)
                                .Select(x => x.ObjectNames)
                                .FirstOrDefault();

                definedObjs = def ?? definedObjs;
                excludedObjs = excl ?? excludedObjs;
            }
        }
    }
}

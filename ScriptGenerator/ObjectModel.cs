using System.Collections.Generic;

namespace ScriptGenerator
{
    public class ObjectModel 
    {
        public List<DBCollection> DBs { get; set; }
    }
    public class DBCollection
    {
        public string DBName { get; set; }
        public List<DBObject> ExcludedObjects { get; set; }
        public List<DBObject> ScriptSpecificOnes { get; set; }
    }

    public class DBObject
    { 
        public string ObjectType { get; set; }
        public string[] ObjectNames { get; set; }
    }

}

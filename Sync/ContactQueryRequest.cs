using System.Collections.Generic;

namespace Sync
{
    public class ContactQueryRequest
    {
        public ContactQueryRequest()
        {
            Groups = new List<QueryConditions>();
            SortBy = "Id";
            Descending = false;
        }

        public List<QueryConditions> Groups { get; set; }

        public string SortBy { get; set; }

        public bool Descending { get; set; }
    }

    public class Condition
    {
        public string Parameter { get; set; }
        public string Operator { get; set;}
        public string Value { get; set; }
        public string SecondaryValue { get; set; }
        public List<string> Values { get; set; }

    }

    public class QueryConditions
    {
        public QueryConditions()
        {
            Conditions = new List<Condition>();
        }

        public List<Condition> Conditions { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.DataTables
{
    /// <summary>
    /// 
    /// </summary>
    public class DataTableParameters
    {
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<DataTableColumn> Columns { get; set; }
        public int Draw { get; set; }
        public int Length { get; set; }
        public List<DataOrder> Order { get; set; }
        public Search Search { get; set; }
        public int Start { get; set; }
        public string Filter { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    public class DataTableColumn
    {
        public int Data { get; set; }
        public string Name { get; set; }
        public bool Orderable { get; set; }
        public bool Searchable { get; set; }
        public Search Search { get; set; }
    }

    public class Search
    {
        public bool Regex { get; set; }
        public string Value { get; set; }
    }

    public class DataOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }
}
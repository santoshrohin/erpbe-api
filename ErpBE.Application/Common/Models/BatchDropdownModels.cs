using System.Collections.Generic;
namespace ErpBE.Application.Common.Models
{
    public class NamedDropdownBatchRequest
    {
        public string Key { get; set; } // e.g. "customer", "item"
        public DropdownRequest Request { get; set; }
    }

    public class BatchDropdownRequest
    {
        public List<NamedDropdownBatchRequest> Requests { get; set; } = new();
    }

    public class BatchDropdownResponse
    {
        public Dictionary<string, List<DropdownItem>> Data { get; set; } = new();
    }
}




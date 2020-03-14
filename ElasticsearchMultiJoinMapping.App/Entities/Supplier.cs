using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticsearchMultiJoinMapping.App
{
    [ElasticsearchType(RelationName = "supplier")]
    public class Supplier : BaseDocument
    {
        [Keyword]
        public string SupplierDescription { get; set; }
      
    }
}

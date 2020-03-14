using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticsearchMultiJoinMapping.App
{
    [ElasticsearchType(RelationName = "category")]
    public class Category : BaseDocument
    {
        [Keyword]
        public string CategoryDescription { get; set; }
      
    }
}

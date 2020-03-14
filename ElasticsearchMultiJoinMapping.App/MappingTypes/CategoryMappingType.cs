using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticsearchMultiJoinMapping.App
{
    [ElasticsearchType(RelationName = "category")]
    public class CategoryMappingType : BaseDocument
    {
        [Keyword]
        public string CategoryDescription { get; set; }
      
    }
}

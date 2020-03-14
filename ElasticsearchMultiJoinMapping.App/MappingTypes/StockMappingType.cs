﻿using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticsearchMultiJoinMapping.App
{
    [ElasticsearchType(RelationName = "stock")]
    public class StockMappingType : BaseDocument
    {
        [Keyword]
        public string Country { get; set; }

        public int Quantity { get; set; }
      
    }
}

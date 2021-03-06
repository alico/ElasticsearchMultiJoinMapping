﻿using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticsearchMultiJoinMapping.App
{
    [ElasticsearchType(RelationName = "product")]
    public class Product : BaseDocument
    {
        [Keyword]
        public string Name { get; set; }

        public decimal Price { get; set; }
      
    }
}

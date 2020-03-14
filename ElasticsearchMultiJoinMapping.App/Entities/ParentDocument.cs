using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticsearchMultiJoinMapping.App
{
    public abstract class BaseDocument
    {
        public virtual int Id { get; set; }
        public int Parent { get; set; }

        public JoinField JoinField { get; set; }

        public BaseDocument()
        {

        }
    }
}

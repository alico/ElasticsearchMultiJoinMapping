using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElasticsearchMultiJoinMapping.App
{
    class Program
    {
        static ElasticClient _ElasticClient;
        const int _IndexingDocumentCount = 1000;
        static string _IndexName = "multiplejoinindex";
        private static void CreateIndexAndMappings(int numberOfReplicas, int numberOfShards, int refreshInterval)
        {
            if (_ElasticClient.Indices.Exists(_IndexName).Exists)
            {
                throw new Exception("The index is available, unable to create mapping!");
            }

            var createIndexResult = _ElasticClient.Indices.Create(_IndexName, x => x
                             .Settings(s => s
                                             .NumberOfReplicas(numberOfReplicas)
                                             .RefreshInterval(refreshInterval)
                                             .NumberOfShards(numberOfShards))
                            .Index<BaseDocument>()
                            .Map<BaseDocument>(m => m
                                .RoutingField(r => r.Required())
                                .AutoMap<ProductMappingType>()
                                .Properties<ProductMappingType>(props => props
                                //You can describe your properties below for Manual mapping
                                .Text(s => s
                                   .Name(n => n.Name)
                                   .Analyzer("default")
                                   .Fields(pprops => pprops))
                              )
                                                    .AutoMap<CategoryMappingType>()
                                                    .AutoMap<SupplierMappingType>()
                                                    .AutoMap<StockMappingType>()
                                                    //You can add more join types here
                                                    .Properties(props => props
                                                                .Join(j => j
                                                                    .Name(p => p.JoinField)
                                                                        //This is so important here. You can describe a relation that product is parent and the others is join type.
                                                                        .Relations(r => r
                                                                        .Join<ProductMappingType>("category", "supplier", "stock")
                                                                                )
                                                                )
                                                            )
                                                    )
                        );

            if (!createIndexResult.IsValid || !createIndexResult.Acknowledged)
            {
                throw new Exception("Error on mapping!");
            }
        }

        public static void IndexDocuments(List<BaseDocument> documents)
        {
            if (documents != null && documents.Count != 0)
            {
                int lastIndex = (int)Math.Ceiling((decimal)documents.Count / _IndexingDocumentCount);

                for (int i = 1; i <= lastIndex; i++)
                {
                    List<BaseDocument> items = documents.Skip(1000 * (i - 1)).Take(_IndexingDocumentCount).ToList();
                    var descriptor = new BulkDescriptor();

                    if (items != null && items.Count > 0)
                        for (int index = 0; index < items.Count; index++)
                        {
                            var indexingDocument = items[index];
                            if (indexingDocument != null)
                                descriptor.Index<BaseDocument>(o => o.Document(indexingDocument)
                                    .Routing(indexingDocument.Id)
                                    .Id(indexingDocument.Id)
                                    .Index(_IndexName)).Refresh(Elasticsearch.Net.Refresh.True);
                        }

                    //You can use BulkAsync if you need
                    var response = _ElasticClient.Bulk(descriptor);

                    if (!response.IsValid || response.ItemsWithErrors.Any())
                    {
                        throw new Exception("Error on indexing!");
                    }
                }
            }
        }

        public static void IndexChildDocuments(List<BaseDocument> documents)
        {
            if (documents != null && documents.Count != 0)
            {
                int lastIndex = (int)Math.Ceiling((decimal)documents.Count / _IndexingDocumentCount);

                for (int i = 1; i <= lastIndex; i++)
                {
                    List<BaseDocument> items = documents.Skip(1000 * (i - 1)).Take(_IndexingDocumentCount).ToList();
                    var descriptor = new BulkDescriptor();

                    if (items != null && items.Count > 0)
                        for (int index = 0; index < items.Count; index++)
                        {
                            var indexingDocument = items[index];
                            if (indexingDocument != null)
                                descriptor.Index<BaseDocument>(o => o.Document(indexingDocument)
                                //It's so important. Child document must be in the same routing
                                    .Routing(indexingDocument.Parent)
                                    .Id(indexingDocument.Id)
                                    .Index(_IndexName)).Refresh(Elasticsearch.Net.Refresh.True);
                        }

                    //You can use BulkAsync if you need
                    var response = _ElasticClient.Bulk(descriptor);

                    if (!response.IsValid || response.ItemsWithErrors.Any())
                    {
                        throw new Exception("Error on indexing!");
                    }
                }
            }
        }

        private static ConnectionSettings GetConnection()
        {
            Uri node = new Uri("http://localhost:9200/");

            return new ConnectionSettings(node).EnableHttpCompression()
            .DisableDirectStreaming()
            .DefaultMappingFor<BaseDocument>(m => m.IndexName(_IndexName))
            .DefaultMappingFor<CategoryMappingType>(m => m.IndexName(_IndexName))
            .DefaultMappingFor<SupplierMappingType>(m => m.IndexName(_IndexName))
            .DefaultMappingFor<StockMappingType>(m => m.IndexName(_IndexName))
            .DefaultMappingFor<ProductMappingType>(m => m.IndexName(_IndexName));
        }

        static void Main(string[] args)
        {
            _ElasticClient = new ElasticClient(GetConnection());
            CreateIndexAndMappings(0, 5, -1);

            #region Documents
            List<BaseDocument> products = new List<BaseDocument>()
            {
                new ProductMappingType()
                {
                    Id = 1,
                    Name = "IPhone 7",
                    Price = 100,
                    JoinField = "product",
                },

                new ProductMappingType()
                {
                    Id = 2,
                    Name = "IPhone 8",
                    Price = 100,
                    JoinField = "product"
                }
            };
            var suppliers = new List<BaseDocument>()
            {
                new SupplierMappingType()
                {
                    Id = 3,
                    SupplierDescription="Apple",
                    Parent = 1,
                    JoinField = JoinField.Link("supplier", 1),

                },

                new SupplierMappingType()
                {
                    Id = 4,
                    SupplierDescription="A supplier",
                    Parent = 1,
                    JoinField = JoinField.Link("supplier", 1)
                },

                new SupplierMappingType()
                {
                    Id = 5,
                    SupplierDescription="Another supplier",
                    Parent = 2,
                    JoinField = JoinField.Link("supplier", 2)
                }
            };
            var stocks = new List<BaseDocument>()
            {
                new StockMappingType()
                {
                    Id = 6,
                    Country="USA",
                    JoinField = JoinField.Link("stock", 1)
                },

                new StockMappingType()
                {
                    Id = 7,
                    Country="UK",
                    JoinField = JoinField.Link("stock", 2)
                },

                new StockMappingType()
                {
                    Id = 8,
                    Country="Germany",
                    JoinField = JoinField.Link("stock", 2)
                }
            };
            var categoriees = new List<BaseDocument>()
            {
                new CategoryMappingType()
                {
                    Id = 9,
                    CategoryDescription= "Electronic",
                    JoinField = JoinField.Link("category", 1)
                },

                new CategoryMappingType()
                {
                    Id = 10,
                    CategoryDescription = "Smart Phone",
                    JoinField = JoinField.Link("category", 2)
                },

                new CategoryMappingType()
                {
                    Id = 11,
                    CategoryDescription = "Phone",
                    JoinField = JoinField.Link("category", 2)
                }
            };
            #endregion

            IndexDocuments(products);
            IndexChildDocuments(categoriees);
            IndexChildDocuments(stocks);
            IndexChildDocuments(suppliers);
        }
    }
}

<h1>Elasticsearch 7.6  Multiple Join with Nest</h1>
I had to use the multiple Join for a project that I was working on in the past days. I looked for the muti join example on the internet but couldn't find anything even in the official documentation. Finally, I wrote it myself in some way and decided to write this article so that you could use it. Have Fun!
Source Code
You can get the source code form my GitHub. 
Intro
If you have experience with any old version of Elasticsearch before 5.6, you probably know the parent-child relation. After version 5.6 Elasticsearch decided to change this relation for some performance reasons (Ref. 5.6 breaking changes). Child documents have been removed. So a new type has entered our world, "Join" type. 
According to our scenario, we have four mapping types such as Product, Category, Supplier, and Stock. Actually we are using the Elasticsearch like a relational database that is not recommended but don't worry about the usage and project structure. In this article, we just focus on how Join is used.
Project
Basically, the Product is parent and others are its children. So we are going to create a base document class that contains a JoinField. JoinField defines a relationship between parent and child. 
Create a Base Document Class
Create Product Mapping Type derives from BaseDocument.
Create Category Mapping Type derives from BaseDocument.
Create other child mapping types like Category. 
Create Index and Mapping 
We create a mapping manually and describe the relations with the Join extension. The usage is Join<YourParentMappingTypeHere>("YourChildMappingType1", "YourChildMappingType2", "YourChildMappingType3" ….).
If you want to use DSL it should be like this: 
Request: 
PUT multiplejoinindex
{
 "mappings": {
 "properties": {
 "country": {
 "type": "keyword"
 },
 "quantity": {
 "type": "integer"
 },
 "id": {
 "type": "integer"
 },
 "parent": {
 "type": "integer"
 },
 "joinField": {
 "relations": {
 "product": [
 "category",
 "supplier",
 "stock"
 ]
 },
 "type": "join"
 },
 "supplierDescription": {
 "type": "keyword"
 },
 "categoryDescription": {
 "type": "keyword"
 },
 "name": {
 "analyzer": "default",
 "fields": {},
 "type": "text"
 },
 "price": {
 "type": "double"
 }
 },
 "_routing": {
 "required": true
 }
 },
 "settings": {
 "index.refresh_interval": -1,
 "index.number_of_replicas": 0,
 "index.number_of_shards": 5
 }
}
Mapping:
Check mapping with the GET mapping API.
GET multiplejoinindex/_mapping
The response should be like this:
{
 "multiplejoinindex" : {
 "aliases" : { },
 "mappings" : {
 "_routing" : {
 "required" : true
 },
 "properties" : {
 "categoryDescription" : {
 "type" : "keyword"
 },
 "country" : {
 "type" : "keyword"
 },
 "id" : {
 "type" : "integer"
 },
 "joinField" : {
 "type" : "join",
 "eager_global_ordinals" : true,
 "relations" : {
 "product" : [
 "supplier",
 "category",
 "stock"
 ]
 }
 },
 "name" : {
 "type" : "text"
 },
 "parent" : {
 "type" : "integer"
 },
 "price" : {
 "type" : "double"
 },
 "quantity" : {
 "type" : "integer"
 },
 "supplierDescription" : {
 "type" : "keyword"
 }
 }
 },
 "settings" : {
 "index" : {
 "refresh_interval" : "-1",
 "number_of_shards" : "5",
 "provided_name" : "multiplejoinindex",
 "creation_date" : "1584191933199",
 "number_of_replicas" : "0",
 "uuid" : "i_15FbtFS6mncJA2UjwIyw",
 "version" : {
 "created" : "7050199"
 }
 }
 }
 }
Index Documents
In this step, we are going to index multiple documents with the IndexDocument method. This method will be used for only indexing our Parent mapping type. 
Indexing Parent DocumentsWe are using a different method for indexing child documents. Parent and child documents must be in the same routing. So we have to assign the routing Id (parent Id) when indexing the child documents. That is the difference with the previous method.


Search
After sending some dummy data to the Elasticsearch index, we are able to search for our parent and child documents. 
Use the parent_id query to get the child documents of a parent. (Ref. Parent_Id Query)
Request:
GET multiplejoinindex/_search
{
 "query": {
 "parent_id": {
 "type": "supplier",
 "id": "1"
 }
 }
}
Response:
{
 "took" : 0,
 "timed_out" : false,
 "_shards" : {
 "total" : 5,
 "successful" : 5,
 "skipped" : 0,
 "failed" : 0
 },
 "hits" : {
 "total" : {
 "value" : 2,
 "relation" : "eq"
 },
 "max_score" : 0.13353139,
 "hits" : [
 {
 "_index" : "multiplejoinindex",
 "_type" : "_doc",
 "_id" : "3",
 "_score" : 0.13353139,
 "_routing" : "1",
 "_source" : {
 "supplierDescription" : "Apple",
 "id" : 3,
 "parent" : 1,
 "joinField" : {
 "name" : "supplier",
 "parent" : "1"
 }
 }
 },
 {
 "_index" : "multiplejoinindex",
 "_type" : "_doc",
 "_id" : "4",
 "_score" : 0.13353139,
 "_routing" : "1",
 "_source" : {
 "supplierDescription" : "A supplier",
 "id" : 4,
 "parent" : 1,
 "joinField" : {
 "name" : "supplier",
 "parent" : "1"
 }
 }
 }
 ]
 }
}
Use the has_child query to get the parent documents of a child document. (Ref. Has Child Query)
Request: 
GET multiplejoinindex/_search
{
 "from": 0,
 "size": 20, 
 "query": {
 "has_child" : {
 "type" : "supplier",
 "min_children": 1, 
 "query" : {
 "match_all": {}
 }
 }
 }
}
Response:
{
 "took" : 2,
 "timed_out" : false,
 "_shards" : {
 "total" : 5,
 "successful" : 5,
 "skipped" : 0,
 "failed" : 0
 },
 "hits" : {
 "total" : {
 "value" : 2,
 "relation" : "eq"
 },
 "max_score" : 1.0,
 "hits" : [
 {
 "_index" : "multiplejoinindex",
 "_type" : "_doc",
 "_id" : "2",
 "_score" : 1.0,
 "_routing" : "2",
 "_source" : {
 "name" : "IPhone 8",
 "price" : 100.0,
 "id" : 2,
 "parent" : 0,
 "joinField" : "product"
 }
 },
 {
 "_index" : "multiplejoinindex",
 "_type" : "_doc",
 "_id" : "1",
 "_score" : 1.0,
 "_routing" : "1",
 "_source" : {
 "name" : "IPhone 7",
 "price" : 100.0,
 "id" : 1,
 "parent" : 0,
 "joinField" : "product"
 }
 }
 ]
 }
}

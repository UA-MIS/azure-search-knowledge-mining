// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using CognitiveSearch.UI.Models;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using Microsoft.Azure;
using static CognitiveSearch.UI.Models.Annotations;
using static CognitiveSearch.UI.Models.Comments;
using static CognitiveSearch.UI.Models.DeletedAnnotations;
using static CognitiveSearch.UI.Models.DeletedComments;
using static CognitiveSearch.UI.Models.DocClassifications;
using static CognitiveSearch.UI.Models.Documents;
using static CognitiveSearch.UI.Models.EntityClassifications;
using static CognitiveSearch.UI.Models.TextClassifications;
using Microsoft.AspNetCore.Http;

namespace CognitiveSearch.UI.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration { get; set; }
        private DocumentSearchClient _docSearch { get; set; }

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _docSearch = new DocumentSearchClient(configuration);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string q)
        {
            var searchidId = _docSearch.GetSearchId().ToString();

            if (searchidId != string.Empty)
                TempData["searchId"] = searchidId;

            TempData["query"] = q;
            TempData["applicationInstrumentationKey"] = _configuration.GetSection("InstrumentationKey")?.Value;

                // connect to storage account
                CloudStorageAccount storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                "mbdmisstorage", "vM3gjO1z1qp2xj0GubaCiswvwklpb9HvodnH14hTXZAvtyRyKiLG540PO9ahG/X0UfU0MdElepH0p52I2JRdzQ=="), true);

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable Documents = tableClient.GetTableReference("Documents");
                CloudTable DocClassifications = tableClient.GetTableReference("DocClassifications");
                CloudTable TextClassifications = tableClient.GetTableReference("TextClassifications");
                CloudTable EntityClassifications = tableClient.GetTableReference("EntityClassifications");

                async void CreateDocumentTableAsync()
                {
                    // Create the CloudTable if it does not exist
                    await Documents.CreateIfNotExistsAsync();
                }
                CreateDocumentTableAsync();

                async void CreateDocClassificationTableAsync()
                {
                    // Create the CloudTable if it does not exist
                    await DocClassifications.CreateIfNotExistsAsync();
                }
                CreateDocClassificationTableAsync();

                async void CreateTextClassificationTableAsync()
                {
                    // Create the CloudTable if it does not exist
                    await TextClassifications.CreateIfNotExistsAsync();
                }
                CreateTextClassificationTableAsync();

                async void CreateEntityClassificationTableAsync()
                {
                    // Create the CloudTable if it does not exist
                    await EntityClassifications.CreateIfNotExistsAsync();
                }
                CreateEntityClassificationTableAsync();

            //creating document classification list for dropdown list
            List<DocClassification> docClassificationList = new List<DocClassification>();
                TableContinuationToken token = null;
                do
                {
                    var x = new TableQuery<DocClassification>();
                    var queryResult = Task.Run(() => DocClassifications.ExecuteQuerySegmentedAsync(x, token)).GetAwaiter().GetResult();
                    foreach (var item in queryResult.Results)
                    {
                        docClassificationList.Add(item);
                    }
                    token = queryResult.ContinuationToken;
                } while (token != null);

                ViewBag.docClassList = docClassificationList;

            //creating TEXT classification list for dropdown list
            List<TextClassification> textClassificationList = new List<TextClassification>();
            TableContinuationToken token1 = null;
            do
            {
                var qT = new TableQuery<TextClassification>();
                var queryResult1 = Task.Run(() => TextClassifications.ExecuteQuerySegmentedAsync(qT, token1)).GetAwaiter().GetResult();
                foreach (var item in queryResult1.Results)
                {
                    textClassificationList.Add(item);
                }
                token1 = queryResult1.ContinuationToken;
            } while (token1 != null);
            ViewBag.textClassList = textClassificationList;

            //creating ENTITY classification list for dropdown list
            List<EntityClassification> entityClassificationList = new List<EntityClassification>();
            TableContinuationToken token2 = null;
            do
            {
                var qE = new TableQuery<EntityClassification>();
                var queryResult2 = Task.Run(() => EntityClassifications.ExecuteQuerySegmentedAsync(qE, token2)).GetAwaiter().GetResult();
                foreach (var item in queryResult2.Results)
                {
                    entityClassificationList.Add(item);
                }
                token2 = queryResult2.ContinuationToken;
            } while (token2 != null);
            ViewBag.entityClassList = entityClassificationList;

            return View();
        }

        public IActionResult CreateTable(string sText, string id, string commentText, string docClassID, string entityClassID, string textClassID)
        {
            //get highlighted text from user
            string highlightedText = sText;

            //get document ID
            string docID = id;

            //get comment
            string comment = commentText;

            //get doc classification
            string docClassification = docClassID;

            //used for annotation partition key, row key, and ID
            int annotationCounter = 1;

            //used for comment partition key, row key, and ID
            int commentCounter = 1;

            // connect to storage account
            CloudStorageAccount storageAccount = new CloudStorageAccount(
            new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            "mbdmisstorage", "vM3gjO1z1qp2xj0GubaCiswvwklpb9HvodnH14hTXZAvtyRyKiLG540PO9ahG/X0UfU0MdElepH0p52I2JRdzQ=="), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable Documents = tableClient.GetTableReference("Documents");
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            CloudTable Comments = tableClient.GetTableReference("Comments");
            CloudTable DeletedAnnotations = tableClient.GetTableReference("DeletedAnnotations");
            CloudTable DeletedComments = tableClient.GetTableReference("DeletedComments");

            async void CreateAnnotationTableAsync()
            {
                // Create the CloudTable if it does not exist
                await Annotations.CreateIfNotExistsAsync();
            }
            CreateAnnotationTableAsync();

            async void CreateCommentTableAsync()
            {
                // Create the CloudTable if it does not exist
                await Comments.CreateIfNotExistsAsync();
            }
            CreateCommentTableAsync();

            async void CreateDeletedAnnotationTableAsync()
            {
                // Create the CloudTable if it does not exist
                await DeletedAnnotations.CreateIfNotExistsAsync();
            }
            CreateDeletedAnnotationTableAsync();

            async void CreateDeletedCommentTableAsync()
            {
                // Create the CloudTable if it does not exist
                await DeletedComments.CreateIfNotExistsAsync();
            }
            CreateDeletedCommentTableAsync();

            //add entity to existing annotation table
            async void createEntity()
            {
                //retrieves annotation entity where partitionKey = counter in table
                TableOperation retrieveOperation = TableOperation.Retrieve<Annotation>(annotationCounter.ToString(), "A" + annotationCounter.ToString());
                TableResult query = await Annotations.ExecuteAsync(retrieveOperation);

                //if entity annotation exists add to counter
                while (query.Result != null)
                {
                    annotationCounter++;
                    retrieveOperation = TableOperation.Retrieve<Annotation>(annotationCounter.ToString(), "A" + annotationCounter.ToString());

                    query = await Annotations.ExecuteAsync(retrieveOperation);
                }

                // Create an annotation entity and add it to the table.
                Annotation Annotation = new Annotation(annotationCounter.ToString(), annotationCounter.ToString());
                Annotation.AnnotationID = "A" + annotationCounter.ToString();
                Annotation.DocumentID = docID;
                Annotation.StartCharLocation = "253"; 
                Annotation.EndCharLocation = "300";
                Annotation.Accept = 0;
                Annotation.Deny = 0;
                Annotation.HighlightedText = highlightedText;

                TableOperation insertOperation = TableOperation.Insert(Annotation);

                //saves whatever user selected to table
                if (entityClassID != null)
                {
                    Annotation.ClassificationID = entityClassID;
                } else if (textClassID != null)
                {
                    Annotation.ClassificationID = textClassID;
                }
                else
                {
                    Annotation.ClassificationID = null;
                }

                

                async void AddAnnotationEntities()
                {
                    await Annotations.ExecuteAsync(insertOperation);
                }
                AddAnnotationEntities();

                if (comment != null)
                {
                    //retrieves comment entity where partitionKey = counter in table
                    TableOperation retrieveOperation2 = TableOperation.Retrieve<Comment>(commentCounter.ToString(), "C" + commentCounter.ToString());
                    TableResult query2 = await Comments.ExecuteAsync(retrieveOperation2);

                    //if comment entity exists add to counter
                    while (query2.Result != null)
                    {
                        commentCounter++;
                        retrieveOperation2 = TableOperation.Retrieve<Comment>(commentCounter.ToString(), "C" + commentCounter.ToString());

                        query2 = await Comments.ExecuteAsync(retrieveOperation2);
                    }

                    // Create a comment entity and add it to the table.
                    Comment Comment = new Comment(commentCounter.ToString(), commentCounter.ToString());
                    Comment.CommentID = "C" + commentCounter.ToString();
                    Comment.CommentText = comment; //get from text box in view
                    Comment.Date = DateTime.Now;
                    Comment.AnnotationID = Annotation.AnnotationID;

                    TableOperation insertOperation2 = TableOperation.Insert(Comment);

                    async void AddCommentEntities()
                    {
                        await Comments.ExecuteAsync(insertOperation2);
                    }
                    AddCommentEntities();
                }

                TableOperation retrieveOperation3 = TableOperation.Retrieve<Document>(docID, docID + "_Doc");
                TableResult query3 = await Documents.ExecuteAsync(retrieveOperation3);

                // Update the document entity in the table.
                Document document = query3.Result as Document;
                document.DocClassID = docClassification;

                TableOperation insertOperation3 = TableOperation.Replace(document);

                async void UpdateDocumentEntities()
                { 
                    await Documents.ExecuteAsync(insertOperation3);
                }
                UpdateDocumentEntities();
            }
            createEntity();

            return Json("Annotation has been saved.");
        }

        [HttpPost]
        public IActionResult GetDocuments(string q = "", SearchFacet[] searchFacets = null, int currentPage = 1)
        {
            var token = GetContainerSasUri();
            var selectFilter = _docSearch.Model.SelectFilter;

            if (!string.IsNullOrEmpty(q))
            {
                q = q.Replace("-", "").Replace("?", "");
            }

            var response = _docSearch.Search(q, searchFacets, selectFilter, currentPage);
            var searchId = _docSearch.GetSearchId().ToString();
            var facetResults = new List<object>();
            var tagsResults = new List<object>();

            if (response.Facets != null)
            {
                // Return only the selected facets from the Search Model
                foreach (var facetResult in response.Facets.Where(f => _docSearch.Model.Facets.Where(x => x.Name == f.Key).Any()))
                {
                    facetResults.Add(new
                    {
                        key = facetResult.Key,
                        value = facetResult.Value
                    });
                }

                foreach (var tagResult in response.Facets.Where(t => _docSearch.Model.Tags.Where(x => x.Name == t.Key).Any()))
                {
                    tagsResults.Add(new
                    {
                        key = tagResult.Key,
                        value = tagResult.Value
                    });
                }
            }

            return new JsonResult(new DocumentResult { Results = response.Results, Facets = facetResults, Tags = tagsResults, Count = Convert.ToInt32(response.Count), Token = token, SearchId = searchId });
        }

        [HttpPost]
        public IActionResult GetDocumentById(string id = "")
        {
            var token = GetContainerSasUri();

            var response = _docSearch.LookUp(id);
            var facetResults = new List<object>();

            return new JsonResult(new DocumentResult { Result = response, Token = token });
        }

        private string GetContainerSasUri()
        {
            string sasContainerToken;
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            string containerAddress = _configuration.GetSection("StorageContainerAddress")?.Value;
            CloudBlobContainer container = new CloudBlobContainer(new Uri(containerAddress), new StorageCredentials(accountName, accountKey));

            SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read
            };

            sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            return sasContainerToken;
        }

        [HttpPost]
        public JObject GetGraphData(string query)
        {
            if (query == null)
            {
                query = "*";
            }
            FacetGraphGenerator graphGenerator = new FacetGraphGenerator(_docSearch);
            var graphJson = graphGenerator.GetFacetGraphNodes(query, "keyPhrases");

            return graphJson;
        }
    }
}

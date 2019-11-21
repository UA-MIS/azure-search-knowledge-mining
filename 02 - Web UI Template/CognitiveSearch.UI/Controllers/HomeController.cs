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
using Data = System.Collections.Generic.KeyValuePair<string, string>;
using System.Text;

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
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable Documents = tableClient.GetTableReference("Documents");
            CloudTable DocClassifications = tableClient.GetTableReference("DocClassifications");
            CloudTable TextClassifications = tableClient.GetTableReference("TextClassifications");
            CloudTable EntityClassifications = tableClient.GetTableReference("EntityClassifications");
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            CloudTable Comments = tableClient.GetTableReference("Comments");
            CloudTable DeletedAnnotations = tableClient.GetTableReference("DeletedAnnotations");
            CloudTable DeletedComments = tableClient.GetTableReference("DeletedComments");

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

            //Set all doc classifications to null
            var allDocuments = new List<Documents>();
            TableContinuationToken token1 = null;
            do
            {
                var x1 = new TableQuery<Document>();
                var queryResult1 = Task.Run(() => Documents.ExecuteQuerySegmentedAsync(x1, token1)).GetAwaiter().GetResult();
                foreach (var item in queryResult1.Results)
                {
                    string result = item.DocClassID.Substring(0, 2);
                    if (result != "DC")
                    {
                        Document document = item as Document;
                        document.DocClassID = "null";
                        TableOperation insertOperation3 = TableOperation.Replace(document);
                        async void UpdateDocumentEntities()
                        {
                            await Documents.ExecuteAsync(insertOperation3);
                        }
                        UpdateDocumentEntities();
                    }
                }
                token1 = queryResult1.ContinuationToken;
            } while (token1 != null);

            //creating document classification list for dropdown list
            List<DocClassification> docClassificationList = new List<DocClassification>();
            TableContinuationToken token2 = null;
            do
            {
                var x2 = new TableQuery<DocClassification>();
                var queryResult2 = Task.Run(() => DocClassifications.ExecuteQuerySegmentedAsync(x2, token2)).GetAwaiter().GetResult();
                foreach (var item in queryResult2.Results)
                {
                    docClassificationList.Add(item);
                }
                token2 = queryResult2.ContinuationToken;
            } while (token2 != null);

            ViewBag.docClassList = docClassificationList;

            //creating TEXT classification list for dropdown list
            List<TextClassification> textClassificationList = new List<TextClassification>();
            TableContinuationToken token3 = null;
            do
            {
                var x3 = new TableQuery<TextClassification>();
                var queryResult3 = Task.Run(() => TextClassifications.ExecuteQuerySegmentedAsync(x3, token3)).GetAwaiter().GetResult();
                foreach (var item in queryResult3.Results)
                {
                    textClassificationList.Add(item);
                }
                token3 = queryResult3.ContinuationToken;
            } while (token3 != null);
            ViewBag.textClassList = textClassificationList;

            //creating ENTITY classification list for dropdown list
            List<EntityClassification> entityClassificationList = new List<EntityClassification>();
            TableContinuationToken token4 = null;
            do
            {
                var x4 = new TableQuery<EntityClassification>();
                var queryResult4 = Task.Run(() => EntityClassifications.ExecuteQuerySegmentedAsync(x4, token4)).GetAwaiter().GetResult();
                foreach (var item in queryResult4.Results)
                {
                    entityClassificationList.Add(item);
                }
                token4 = queryResult4.ContinuationToken;
            } while (token4 != null);
            ViewBag.entityClassList = entityClassificationList;

            return View();
        }

        public IActionResult SaveAnnotations(string sText, string id, string commentText, string docClassID, string entityClassID, string textClassID, string start, string end)
        {
            //get highlighted text from user
            string highlightedText = sText;

            //get document ID
            string docID = id;

            //get comment
            string comment = commentText;

            //get doc classification
            string docClassification = docClassID;

            //get startChar
            string startChar = start;

            //get endChar
            string endChar = end;

            //used for annotation partition key, row key, and ID
            int annotationCounter = 1;

            //used for comment partition key, row key, and ID
            int commentCounter = 1;

            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable Documents = tableClient.GetTableReference("Documents");
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            CloudTable Comments = tableClient.GetTableReference("Comments");

            //add New Annotation to existing annotation table
            async void addAnnotation()
            {
                //retrieves annotation entity where partitionKey = counter in table
                TableOperation retrieveOperation = TableOperation.Retrieve<Annotation>(annotationCounter.ToString(), "A" + annotationCounter.ToString());
                TableResult query = await Annotations.ExecuteAsync(retrieveOperation);

                //if annotation with PKey and RKey exists add to counter
                while (query.Result != null)
                {
                    annotationCounter++;
                    retrieveOperation = TableOperation.Retrieve<Annotation>(annotationCounter.ToString(), "A" + annotationCounter.ToString());

                    query = await Annotations.ExecuteAsync(retrieveOperation);
                }

                // Create an New annotation and add it to the table.
                Annotation Annotation = new Annotation(annotationCounter.ToString(), annotationCounter.ToString());
                Annotation.AnnotationID = "A" + annotationCounter.ToString();
                Annotation.DocumentID = docID;
                Annotation.StartCharLocation = startChar;
                Annotation.EndCharLocation = endChar;
                Annotation.Accept = 0;
                Annotation.Deny = 0;
                Annotation.HighlightedText = highlightedText;

                TableOperation insertOperation = TableOperation.Insert(Annotation);

                //saves whatever user selected to table
                if (entityClassID != "null")
                {
                    Annotation.ClassificationID = entityClassID;
                }
                else if (textClassID != "null")
                {
                    Annotation.ClassificationID = textClassID;
                }
                else
                {
                    Annotation.ClassificationID = "null";
                }

                async void AddAnnotationEntities()
                {
                    await Annotations.ExecuteAsync(insertOperation);
                }
                AddAnnotationEntities();

                if (comment != null)
                {
                    //retrieves comment entity where partitionKey = counter in table
                    TableOperation retrieveOperation3 = TableOperation.Retrieve<Comment>(commentCounter.ToString(), "C" + commentCounter.ToString());
                    TableResult query3 = await Comments.ExecuteAsync(retrieveOperation3);

                    //if comment entity exists add to counter
                    while (query3.Result != null)
                    {
                        commentCounter++;
                        retrieveOperation3 = TableOperation.Retrieve<Comment>(commentCounter.ToString(), "C" + commentCounter.ToString());

                        query3 = await Comments.ExecuteAsync(retrieveOperation3);
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

                //Gets Document from Document table
                TableOperation retrieveOperation4 = TableOperation.Retrieve<Document>(docID, docID + "_Doc");
                TableResult query4 = await Documents.ExecuteAsync(retrieveOperation4);

                // Update the document entity in the table.
                Document document2 = query4.Result as Document;
                document2.DocClassID = docClassification;

                TableOperation insertOperation3 = TableOperation.Replace(document2);

                async void UpdateDocumentEntities()
                {
                    await Documents.ExecuteAsync(insertOperation3);
                }
                UpdateDocumentEntities();
            }
            addAnnotation();

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

        [HttpPost]
        public IActionResult getArrayOfChars(string id)
        {
            //Redisplay all past annotations for current document
            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Get Annotation Table
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            //Make list of all annotations
            var allAnnotations = new List<Annotation>();
            TableContinuationToken token1 = null;
            do
            {
                var x1 = new TableQuery<Annotation>();
                var queryResult1 = Task.Run(() => Annotations.ExecuteQuerySegmentedAsync(x1, token1)).GetAwaiter().GetResult();
                foreach (var item in queryResult1.Results)
                {
                    allAnnotations.Add(item);
                }
                token1 = queryResult1.ContinuationToken;
            } while (token1 != null);

            //Make list of annotations associated with this document
            var allDocAnnotations = new List<Annotation>();
            foreach (var annotation in allAnnotations)
            {
                if (annotation.DocumentID == id)
                {
                    allDocAnnotations.Add(annotation);
                }
            }

            //Make lists for text and entity classifications
            var textAnnotations = new List<Annotation>();
            var entityAnnotations = new List<Annotation>();

            foreach (var annotation in allDocAnnotations)
            {
                if (annotation.ClassificationID.StartsWith("T"))
                {
                    textAnnotations.Add(annotation);
                }
                else if (annotation.ClassificationID.StartsWith("E"))
                {
                    entityAnnotations.Add(annotation);
                }
            }

            //make lists for the start and end characters of text annotations
            int c = 0;
            string[] textStartChars = new string[textAnnotations.Count()];
            string[] textEndChars = new string[textAnnotations.Count()];
            Annotation[] tAnnotations = new Annotation[textAnnotations.Count()];
            foreach (var item in textAnnotations)
            {
                textStartChars[c] = item.StartCharLocation;
                textEndChars[c] = item.EndCharLocation;
                tAnnotations[c] = item;
                c++;
            }

            //make lists for the start and end characters of entity annotations
            int ch = 0;
            string[] entityStartChars = new string[entityAnnotations.Count()];
            string[] entityEndChars = new string[entityAnnotations.Count()];
            Annotation[] eAnnotations = new Annotation[entityAnnotations.Count()];
            foreach (var item in entityAnnotations)
            {
                entityStartChars[ch] = item.StartCharLocation;
                entityEndChars[ch] = item.EndCharLocation;
                eAnnotations[ch] = item;
                ch++;
            }
            return new JsonResult(new DocumentResult { textStartChars = textStartChars, textEndChars = textEndChars, entityStartChars = entityStartChars, entityEndChars = entityEndChars, textAnnotations = tAnnotations, entityAnnotations = eAnnotations });
        }

        public async Task<IActionResult> AnnotationView(string id)
        {
            string pKey = id.Trim('A');
            string rKey = id;
            string classification = "";

            //get annotation
            Annotation newAnnotation = await GetAnnotation(pKey, rKey);
            //get all comments on that annotation
            List<string> comments = await GetComments(pKey, rKey);

            //check if annotation is classified as text or entity
            if (newAnnotation.ClassificationID.StartsWith("T"))
            {
                //get classification
                TextClassification textClassification = await GetTextClassifications(pKey, rKey);
                classification = textClassification.Classification;               
            }
            else if (newAnnotation.ClassificationID.StartsWith("E"))
            {
                //get classification
                EntityClassification entityClassification = await GetEntityClassifications(pKey, rKey);
                classification = entityClassification.Classification;             
            }
            return new JsonResult(new DocumentResult { annotation = newAnnotation, comments = comments, classification = classification });
        }

        async Task<Annotation> GetAnnotation(string pKey, string rKey)
        {
            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
            TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
            Annotation annotation = query4.Result as Annotation;

            return annotation;
        }

        //text redisplay
        async Task<TextClassification> GetTextClassifications(string pKey, string rKey)
        {
            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Get annotation
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
            TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
            Annotation annotation = query4.Result as Annotation;

            //Get list of text classifications
            CloudTable TextClassifications = tableClient.GetTableReference("TextClassifications");
            var allTextClassification = new List<TextClassification>();
            TableContinuationToken token2 = null;
            do
            {
                var x2 = new TableQuery<TextClassification>();
                var queryResult2 = Task.Run(() => TextClassifications.ExecuteQuerySegmentedAsync(x2, token2)).GetAwaiter().GetResult();
                foreach (var item2 in queryResult2.Results)
                {
                    allTextClassification.Add(item2);
                }
                token2 = queryResult2.ContinuationToken;
            } while (token2 != null);

            TextClassification textClassification = new TextClassification();

            foreach (var item in allTextClassification)
            {
                if (item.TextClassID == annotation.ClassificationID)
                {
                    textClassification = item;
                }
            }

            //return annotations classification
            return textClassification;
        }

        //entity redisplay
        async Task<EntityClassification> GetEntityClassifications(string pKey, string rKey)
        {
            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Get annotation
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
            TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
            Annotation annotation = query4.Result as Annotation;

            //Get list of text classifications
            CloudTable EntityClassifications = tableClient.GetTableReference("EntityClassifications");
            var allEntityClassification = new List<EntityClassification>();
            TableContinuationToken token3 = null;
            do
            {
                var x3 = new TableQuery<EntityClassification>();
                var queryResult3 = Task.Run(() => EntityClassifications.ExecuteQuerySegmentedAsync(x3, token3)).GetAwaiter().GetResult();
                foreach (var item3 in queryResult3.Results)
                {
                    allEntityClassification.Add(item3);
                }
                token3 = queryResult3.ContinuationToken;
            } while (token3 != null);

            EntityClassification entityClassification = new EntityClassification();

            foreach (var item in allEntityClassification)
            {
                if (item.EntityClassID == annotation.ClassificationID)
                {
                    entityClassification = item;
                }
            }

            //return annotations classification
            return entityClassification;
        }


        async Task<List<string>> GetComments(string pKey, string rKey)
        {
            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Get annotation
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
            TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
            Annotation annotation = query4.Result as Annotation;

            //Get list opf all comments
            var allComments = new List<Comment>();
            CloudTable Comments = tableClient.GetTableReference("Comments");
            TableOperation retrieveOperation5 = TableOperation.Retrieve<Comment>(pKey, rKey);
            TableContinuationToken token1 = null;
            do
            {
                var x1 = new TableQuery<Comment>();
                var queryResult1 = Task.Run(() => Comments.ExecuteQuerySegmentedAsync(x1, token1)).GetAwaiter().GetResult();
                foreach (var item in queryResult1.Results)
                {
                    allComments.Add(item);
                }
                token1 = queryResult1.ContinuationToken;
            } while (token1 != null);

            //Get list of all coments associated with this annotation
            var allAnnComments = new List<string>();
            foreach (var comment in allComments)
            {
                if (comment.AnnotationID == annotation.AnnotationID)
                {
                    allAnnComments.Add(comment.CommentText);
                }
            }

            return allAnnComments;
        }

        public async Task<IActionResult> getDocClass(string id)
        {
            string docClassification = "";
            string classID = "";

            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Get document
            CloudTable Documents = tableClient.GetTableReference("Documents");
            TableOperation retrieveOperation4 = TableOperation.Retrieve<Document>(id, id + "_Doc");
            TableResult query4 = await Documents.ExecuteAsync(retrieveOperation4);
            Document document = query4.Result as Document;

            if (document.DocClassID != "null")
            {
                //Get list of all classifications
                CloudTable DocClassifications = tableClient.GetTableReference("DocClassifications");
                var allDocClassifications = new List<DocClassification>();
                TableContinuationToken token3 = null;
                do
                {
                    var x3 = new TableQuery<DocClassification>();
                    var queryResult3 = Task.Run(() => DocClassifications.ExecuteQuerySegmentedAsync(x3, token3)).GetAwaiter().GetResult();
                    foreach (var item3 in queryResult3.Results)
                    {
                        allDocClassifications.Add(item3);
                    }
                    token3 = queryResult3.ContinuationToken;
                } while (token3 != null);

                //Get this documents classification
                foreach (var item in allDocClassifications)
                {
                    if (item.DocClassID == document.DocClassID)
                    {
                        classID = item.DocClassID;
                        docClassification = item.Classification;
                    }
                }
            }
            else
            {
                classID = "null";
                docClassification = "Document Classification";
            }
            return new JsonResult(new DocumentResult { classID = classID, docClassification = docClassification });

        }

        public IActionResult addClass()
        {
            return View();
        }

        public IActionResult SaveAcceptValue(string id, int likes)
        {
            string pKey = id.Trim('A');
            string rKey = id;

            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            async void updateAnnotation()
            {
                //Get annotation
                CloudTable Annotations = tableClient.GetTableReference("Annotations");
                TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
                TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
                Annotation annotation = query4.Result as Annotation;
                //Update accept value
                annotation.Accept = likes;
                //Update annotation in table
                TableOperation insertOperation3 = TableOperation.Replace(annotation);
                await Annotations.ExecuteAsync(insertOperation3);
            }
            updateAnnotation();
            return Json("Accept has been saved.");


        }

        public IActionResult SaveDenyValue(string id, int dislikes)
        {
            string pKey = id.Trim('A');
            string rKey = id;

            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            async void updateAnnotation()
            {
                //Get annotation
                CloudTable Annotations = tableClient.GetTableReference("Annotations");
                TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
                TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
                Annotation annotation = query4.Result as Annotation;
                string apkey = annotation.PartitionKey;
                string arkey = annotation.RowKey;
                //Update deny value
                annotation.Deny = dislikes;

                if (annotation.Deny < 5)
                {
                    //update annotation in table
                    TableOperation insertOperation3 = TableOperation.Replace(annotation);
                    await Annotations.ExecuteAsync(insertOperation3);
                  
                }
                else
                {
                    //insert into soft delete tables
                    //Get Soft delete Tables
                    CloudTable DeletedAnnotations = tableClient.GetTableReference("DeletedAnnotations");
                    CloudTable DeletedComments = tableClient.GetTableReference("DeletedComments");
                    Comment c = new Comment();

                    //retrieves annotation entity where partitionKey = counter in table
                    int annotationCounter = 1;
                    TableOperation retrieveOperation = TableOperation.Retrieve<DeletedAnnotation>(annotationCounter.ToString(), "DA" + annotationCounter.ToString());
                    TableResult query = await DeletedAnnotations.ExecuteAsync(retrieveOperation);

                    //if deleted annotation with PKey and RKey exists add to counter
                    while (query.Result != null)
                    {
                        annotationCounter++;
                        retrieveOperation = TableOperation.Retrieve<DeletedAnnotation>(annotationCounter.ToString(), "DA" + annotationCounter.ToString());

                        query = await DeletedAnnotations.ExecuteAsync(retrieveOperation);
                    }
                    annotation.PartitionKey = annotationCounter.ToString();
                    annotation.RowKey = "DA" + annotationCounter.ToString();

                    //insert annotation into deleted annotation
                    TableOperation insertOperation = TableOperation.Insert(annotation);
                    await DeletedAnnotations.ExecuteAsync(insertOperation);

                    //Comments (linked to annotation from above)
                    var allComments = new List<Comment>();
                    var allAnnotationComments = new List<Comment>();
                    var allCommentPKeys = new List<string>();
                    var allCommentRKeys = new List<string>();
                    CloudTable Comments = tableClient.GetTableReference("Comments");
                    TableContinuationToken token1 = null;

                    //get all comments
                    do
                    {
                        var x1 = new TableQuery<Comment>();
                        var queryResult1 = Task.Run(() => Comments.ExecuteQuerySegmentedAsync(x1, token1)).GetAwaiter().GetResult();
                        foreach (var item in queryResult1.Results)
                        {
                            allComments.Add(item);
                        }
                        token1 = queryResult1.ContinuationToken;
                    } while (token1 != null);

                    //get all comments for specific annotation
                    foreach (var comment in allComments)
                    {
                        allAnnotationComments.Add(comment);
                        allCommentPKeys.Add(comment.PartitionKey);
                        allCommentRKeys.Add(comment.RowKey);

                        if (comment.AnnotationID == annotation.AnnotationID)
                        {
                            //retrieves Comment where partitionKey = counter in table
                            int commentCounter = 1;
                            TableOperation retrieveOperation2 = TableOperation.Retrieve<DeletedComment>(commentCounter.ToString(), "DC" + commentCounter.ToString());
                            TableResult query2 = await DeletedComments.ExecuteAsync(retrieveOperation2);

                            //if comment exists add to counter
                            while (query2.Result != null)
                            {
                                commentCounter++;
                                retrieveOperation2 = TableOperation.Retrieve<DeletedComment>(commentCounter.ToString(), "DC" + commentCounter.ToString());

                                query2 = await DeletedComments.ExecuteAsync(retrieveOperation2);
                            }
                            comment.PartitionKey = commentCounter.ToString();
                            comment.RowKey = "DC" + commentCounter.ToString();

                            //insert comments into deleted comments table
                            TableOperation insertOperation2 = TableOperation.Insert(comment);
                            await DeletedComments.ExecuteAsync(insertOperation2);

                        }
                    }
                    //Delete annotation from annotation table and all comments related
                    DeletedAnnotationAndComments(annotation, apkey, arkey, allAnnotationComments, allCommentPKeys, allCommentRKeys);
                }
            }
            updateAnnotation();

            return Json("Deny has been saved.");
        }

        async void DeletedAnnotationAndComments(Annotation annotation, string pkey, string rkey, List<Comment> allAnnotationComments, List<string> commentPKeys, List<string> commentRKeys)
        {
            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Delete annotation
            CloudTable Annotations = tableClient.GetTableReference("Annotations");
            annotation.ETag = "*";
            annotation.PartitionKey = pkey;
            annotation.RowKey = rkey;
            TableOperation deleteOperation = TableOperation.Delete(annotation);
            await Annotations.ExecuteAsync(deleteOperation);

            //delete comments
            CloudTable Comments = tableClient.GetTableReference("Comments");

            int count = 0;

            //get all comments for specific annotation
            foreach (var comment in allAnnotationComments)
            {
                
                comment.ETag = "*";
                comment.PartitionKey = commentPKeys[count];
                comment.RowKey = commentRKeys[count];

                if (comment.AnnotationID == annotation.AnnotationID)
                {
                    //Delete comment from comment table
                    TableOperation deleteOperation2 = TableOperation.Delete(comment);
                    await Comments.ExecuteAsync(deleteOperation2);

                }
                count++;
            }
        }


		public IActionResult SaveEntityClass(string text) //saves NEW Entity Class to Entity Classifications Table
        {
            // connect to storage account
            string classification = text;
			string accountName = _configuration.GetSection("StorageAccountName")?.Value;
			string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
			CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
			accountName, accountKey), true);

            //Get entity classification table
			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
			CloudTable Entityclassification = tableClient.GetTableReference("EntityClassifications");
			
			int classificationCounter = 1;

			async void createEntity()
			{
				//retrieves annotation where partitionKey = counter in table
				TableOperation retrieveOperation = TableOperation.Retrieve<EntityClassification>(classificationCounter.ToString(), "EC" + classificationCounter.ToString());
				TableResult query = await Entityclassification.ExecuteAsync(retrieveOperation);

				//if annotation with PKey and RKey exists add to counter
				while (query.Result != null)
				{
					classificationCounter++;
					retrieveOperation = TableOperation.Retrieve<EntityClassification>(classificationCounter.ToString(), "EC" + classificationCounter.ToString());

					query = await Entityclassification.ExecuteAsync(retrieveOperation);
				}

				// Create an annotation and add it to the table.
				EntityClassification EntityClassification = new EntityClassification(classificationCounter.ToString(), classificationCounter.ToString());
				EntityClassification.EntityClassID = "EC" + classificationCounter.ToString();
				EntityClassification.Classification = classification;

				TableOperation insertOperation = TableOperation.Insert(EntityClassification);
				await Entityclassification.ExecuteAsync(insertOperation);

			}
			createEntity();
			return RedirectToAction("AddClass");
		}

        public IActionResult SaveDocClass(string text) //saves NEW Document Class to Document Classifications Table
        {
            // connect to storage account
            string classification = text;
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            //Get document classification table
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable Docclassification = tableClient.GetTableReference("DocClassifications");

            int classificationCounter = 1;

            async void createDocClass()
            {
                //retrieves doc class where partitionKey = counter in table
                TableOperation retrieveOperation = TableOperation.Retrieve<DocClassification>(classificationCounter.ToString(), "DC" + classificationCounter.ToString());
                TableResult query = await Docclassification.ExecuteAsync(retrieveOperation);

                //if doc class with PKey and RKey exists add to counter
                while (query.Result != null)
                {
                    classificationCounter++;
                    retrieveOperation = TableOperation.Retrieve<DocClassification>(classificationCounter.ToString(), "DC" + classificationCounter.ToString());

                    query = await Docclassification.ExecuteAsync(retrieveOperation);
                }

                // Create a DOC CLASS and add it to the table.
                DocClassification DocClassification = new DocClassification(classificationCounter.ToString(), classificationCounter.ToString());
                DocClassification.DocClassID = "DC" + classificationCounter.ToString();
                DocClassification.Classification = classification;

                TableOperation insertOperation = TableOperation.Insert(DocClassification);
                await Docclassification.ExecuteAsync(insertOperation);

            }
            createDocClass();
            return RedirectToAction("AddClass");
        }

        public IActionResult SaveTextClass(string text) //saves NEW Text Class to Text Classifications Table
        {
            // connect to storage account
            string classification = text;
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            //Get text classification table
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable Textclassification = tableClient.GetTableReference("TextClassifications");

            int classificationCounter = 1;

            async void createTextClass()
            {
                //retrieves text class where partitionKey = counter in table
                TableOperation retrieveOperation = TableOperation.Retrieve<TextClassification>(classificationCounter.ToString(), "TC" + classificationCounter.ToString());
                TableResult query = await Textclassification.ExecuteAsync(retrieveOperation);

                //if text class with PKey and RKey exists add to counter
                while (query.Result != null)
                {
                    classificationCounter++;
                    retrieveOperation = TableOperation.Retrieve<TextClassification>(classificationCounter.ToString(), "TC" + classificationCounter.ToString());

                    query = await Textclassification.ExecuteAsync(retrieveOperation);
                }

                // Create an TEXT CLASS and add it to the table.
                TextClassification TextClassification = new TextClassification(classificationCounter.ToString(), classificationCounter.ToString());
                TextClassification.TextClassID = "TC" + classificationCounter.ToString();
                TextClassification.Classification = classification;

                TableOperation insertOperation = TableOperation.Insert(TextClassification);
                await Textclassification.ExecuteAsync(insertOperation);

            }
            createTextClass();
            return RedirectToAction("AddClass");
        }

        public IActionResult SoftDelete(string id)
        {
            // connect to storage account
            string accountName = _configuration.GetSection("StorageAccountName")?.Value;
            string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
            accountName, accountKey), true);

            //Get Deleted annotations table
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable DeletedAnnotations = tableClient.GetTableReference("DeletedAnnotations");

            //Make list of all deleted annotations
            var allAnnotations = new List<DeletedAnnotation>();

            TableContinuationToken token1 = null;
            do
            {
                var x1 = new TableQuery<DeletedAnnotation>();
                var queryResult1 = Task.Run(() => DeletedAnnotations.ExecuteQuerySegmentedAsync(x1, token1)).GetAwaiter().GetResult();
                foreach (var item in queryResult1.Results)
                {
                    allAnnotations.Add(item);
                }
                token1 = queryResult1.ContinuationToken;
            } while (token1 != null);

            ViewBag.text = "";
            ViewBag.AnnotationList = allAnnotations.OrderBy(ann => ann.DocumentID);
            if (allAnnotations.Count <= 0)
            {
                ViewBag.text = "There are no deleted annotations";
            }
            return View();
        }
    }
}
    


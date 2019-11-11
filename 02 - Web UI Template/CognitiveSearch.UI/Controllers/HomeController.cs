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
			CloudTable DeletedAnnotations = tableClient.GetTableReference("DeletedAnnotations");
			CloudTable DeletedComments = tableClient.GetTableReference("DeletedComments");

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

		[HttpPost]
		public IActionResult getArrayOfChars(string id)
		{
			//Redisplay all past annotations
			// connect to storage account
			string accountName = _configuration.GetSection("StorageAccountName")?.Value;
			string accountKey = _configuration.GetSection("StorageAccountKey")?.Value;
			CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
			accountName, accountKey), true);

			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
			CloudTable Annotations = tableClient.GetTableReference("Annotations");
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

			var allDocAnnotations = new List<Annotation>();
			foreach (var annotation in allAnnotations)
			{
				if (annotation.DocumentID == id)
				{
					allDocAnnotations.Add(annotation);
				}
			}


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

		public async Task<IActionResult> PartialView(string id)
		{
			string pKey = id.Trim('A');
			string rKey = id;
			string classification = "";

			Annotation newAnnotation = await GetAnnotation(pKey, rKey);
			List<string> comments = await GetComments(pKey, rKey);

			if (newAnnotation.ClassificationID.StartsWith("T"))
			{
				TextClassification textClassification = await GetTextClassifications(pKey, rKey);
				classification = textClassification.Classification;
				//return new JsonResult(new DocumentResult { annotation = newAnnotation, comments = comments, textClassification = textClassification });
			}
			else if (newAnnotation.ClassificationID.StartsWith("E"))
			{
				EntityClassification entityClassification = await GetEntityClassifications(pKey, rKey);
				classification = entityClassification.Classification;
				//return new JsonResult(new DocumentResult { annotation = newAnnotation, comments = comments, entityClassification = entityClassification });
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

		////text redisplay
		async Task<TextClassification> GetTextClassifications(string pKey, string rKey)
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


			return textClassification;
		}

		////entity redisplay
		async Task<EntityClassification> GetEntityClassifications(string pKey, string rKey)
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

			CloudTable Annotations = tableClient.GetTableReference("Annotations");
			TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
			TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
			Annotation annotation = query4.Result as Annotation;

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

			CloudTable Documents = tableClient.GetTableReference("Documents");
			TableOperation retrieveOperation4 = TableOperation.Retrieve<Document>(id, id + "_Doc");
			TableResult query4 = await Documents.ExecuteAsync(retrieveOperation4);

			// Update the document entity in the table.
			Document document = query4.Result as Document;

			if (document.DocClassID != "null")
			{
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

				CloudTable Annotations = tableClient.GetTableReference("Annotations");
				TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
				TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
				Annotation annotation = query4.Result as Annotation;
				annotation.Accept = likes;

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
				CloudTable Annotations = tableClient.GetTableReference("Annotations");
				TableOperation retrieveOperation4 = TableOperation.Retrieve<Annotation>(pKey, rKey);
				TableResult query4 = await Annotations.ExecuteAsync(retrieveOperation4);
				Annotation annotation = query4.Result as Annotation;
				annotation.Deny = dislikes;

				if (annotation.Deny <= 5)
				{
					TableOperation insertOperation3 = TableOperation.Replace(annotation);
					await Annotations.ExecuteAsync(insertOperation3);
				}
			}
			return Json("Accept has been saved.");
		}
	}
}

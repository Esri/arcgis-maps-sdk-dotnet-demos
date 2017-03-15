using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceRequestsSample
{
	public class ServiceRequestDataAccess
	{
		private long _lastRequestId = 0;

		#region Constructor and unique instance management

		// Private constructor
		private ServiceRequestDataAccess()
		{
			Table = new ServiceFeatureTable(
				new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/ServiceRequest/FeatureServer/0"));
		}

		// Static initialization of the unique instance 
		private static readonly ServiceRequestDataAccess SingleInstance = new ServiceRequestDataAccess();

		/// <summary>
		/// Gets the single <see cref="IdentityManager"/> instance.
		/// This is the only way to get an IdentifyManager instance.
		/// </summary>
		public static ServiceRequestDataAccess Current
		{
			get { return SingleInstance; }
		}

		/// <summary>
		/// Checks if service request table is already initialized, if not, initializes it.
		/// </summary>
		public async Task Initialize()
		{
			if (!Table.IsInitialized)
			{
				Table.OutFields = OutFields.All;
				await Table.InitializeAsync();
			}
		}

		#endregion

		public ServiceFeatureTable Table { get; protected set; }

		public async Task<List<Feature>> GetServiceRequestsAsync()
		{
			// Initialzie if Table is not initialized yet
			await Initialize();

			// Query all service requests that has requestid, status and requesteddate set
			var results = await Table.QueryAsync(new QueryFilter() { WhereClause = "requestid <> '' AND status <> '' AND requestdate IS NOT NULL" });
			var serviceRequests = results.ToList();

			// Update latest request id
			// In this sample, we expect this to be the latest but this could be queried from the server when needed
			// to make sure that we get the latest.
			UpdateLatestRequestId(serviceRequests);

			return serviceRequests;
		}

		/// <summary>
		/// Gets all comments for given service request. (Single)
		/// </summary>
		public async Task<IReadOnlyList<GeodatabaseFeature>> GetCommentsForServiceRequestAsync(Feature serviceRequest)
		{
			// Initialzie if Table is not initialized yet
			await Initialize();

			if (Table.ServiceInfo.Relationships.Count == 0)
				return null;

			// Get first relationship that is defined in ServiceRequest service, in this case we know that it
			// defines relation between ServiceRequest and Comment
			var serviceRequestToCommentRelation = Table.ServiceInfo.Relationships.First();

			// Create query to get all comment that is created for same request identifier
			var requestId = Convert.ToInt64(serviceRequest.Attributes[Table.ServiceInfo.ObjectIdField]);
			var requestIds = new List<long> { requestId };

			// Query related comments
			var results = await Table.QueryRelatedAsync(requestIds, serviceRequestToCommentRelation.ID);
			var commentsForServiceRequst = results.Where(result => result.Key == requestId).Select(item => item.Value).ToList();
			return commentsForServiceRequst.FirstOrDefault();
		}

		/// <summary>
		/// Gets all comments for given service requests. (Multiple)
		/// </summary>
		public async Task<IReadOnlyDictionary<long, IReadOnlyList<GeodatabaseFeature>>> GetCommentsForServiceRequestsAsync(List<Feature> serviceRequests)
		{
			// Initialzie if Table is not initialized yet
			await Initialize();

			if (Table.ServiceInfo.Relationships.Count == 0 || serviceRequests == null || serviceRequests.Count == 0)
				return null;

			// Get first relationship that is defined in ServiceRequest service, in this case we know that it
			// defines relation between ServiceRequest and Comment
			var serviceRequestToCommentRelation = Table.ServiceInfo.Relationships.First();

			// Create query to get all comment that is created for same request identifier
			var requestIds = new List<long>();
			foreach (var serviceRequest in serviceRequests)
			{
				var requestId = Convert.ToInt64(
					serviceRequest.Attributes[Table.ServiceInfo.ObjectIdField]);
				requestIds.Add(requestId);
			}
			// Query related comments
			var results = await Table.QueryRelatedAsync(requestIds, serviceRequestToCommentRelation.ID);
			return results;
		}

		/// <summary>
		/// Adds new ServiceRequest to the local cache and pushes that to the server. Returns updated feature.
		/// </summary>
		public async Task<Feature> AddServiceRequestAsync(Feature newServiceRequest)
		{
			// Before commit, get latest requestId and increment it
			_lastRequestId++;
			var newRequestId = _lastRequestId;
			newServiceRequest.Attributes["requestid"] = newRequestId.ToString();

			// Since used FeatureService doesn't default values, that is needed to do in the code
			newServiceRequest.Attributes["status"] = "Unassigned"; 

			// Commit changes to the local cache
			var id = await Table.AddAsync(newServiceRequest);

			// To commit changed to the service use ApplyEdits
			var results = await Table.ApplyEditsAsync();

			// Update feature that we created and return it.
			var updatedFeature = await Table.QueryAsync(id);
			return updatedFeature;
		}

		/// <summary>
		/// Updated ServiceRequest and pushes that to the server.
		/// </summary>
		public async Task UpdateServiceRequestAsync(Feature serviceRequest)
		{
			// Update changes to local cache
			await Table.UpdateAsync(serviceRequest);

			// To commit changed to the service use ApplyEdits
			var results = await Table.ApplyEditsAsync();
		}

		public async Task DeleteServiceRequestAsync(Feature serviceRequest, bool deleteComments = false)
		{
			// Check if you can delete the feature
			// Checks if there's a service level contraint like no delete supported or ownership settings
			// doesn't give you a permission
			if (!Table.CanDeleteFeature(serviceRequest as GeodatabaseFeature))
				return;

			if (deleteComments)
			{
				// Delete all found related comments and push changes to the FeatureService
				await CommentDataAccess.Current.DeleteCommentsFromServiceRequest(serviceRequest);
			}

			// Delete ServiceRequest from local cache
			await Table.DeleteAsync(serviceRequest);

			// Push changes to the FeatureService
			await Table.ApplyEditsAsync();
		}

		/// <summary>
		/// Updates internal value that keeps track about latest requestid. 
		/// </summary>
		private void UpdateLatestRequestId(List<Feature> serviceRequests)
		{
			long requestId = 0;
			foreach (var serviceRequest in serviceRequests)
			{
				long value = 0;
				var success = long.TryParse(serviceRequest.Attributes["requestid"].ToString(), out value);
				if (success)
					requestId = requestId < value ? value : requestId;
			}

			_lastRequestId = requestId;
		}
	}
}

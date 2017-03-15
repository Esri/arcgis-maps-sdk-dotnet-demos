using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceRequestsSample
{
	public class CommentDataAccess
	{
		#region Constructor and unique instance management

		// Private constructor
		private CommentDataAccess()
		{
			Table = new ServiceFeatureTable(
				new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/ServiceRequest/FeatureServer/1"));
		}

		// Static initialization of the unique instance 
		private static readonly CommentDataAccess SingleInstance = new CommentDataAccess();

		/// <summary>
		/// Gets the single <see cref="IdentityManager"/> instance.
		/// This is the only way to get an IdentifyManager instance.
		/// </summary>
		public static CommentDataAccess Current
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

		/// <summary>
		/// Adds new ServiceRequest to the local cache and pushes that to the server. Returns updated feature.
		/// </summary>
		public async Task<Feature> AddCommentAsync(Feature comment)
		{
			// Initialzie if Table is not initialized yet
			await Initialize();

			// Commit changes to the local cache
			var id = await Table.AddAsync(comment);

			// To commit changed to the service use ApplyEdits
			var results = await Table.ApplyEditsAsync();

			// Update feature that we created and return it.
			var updatedFeature = await Table.QueryAsync(id);
			return updatedFeature;
		}

		/// <summary>
		/// Deletes all comments that are related to ServiceRequest
		/// </summary>
		public async Task DeleteCommentsFromServiceRequest(Feature serviceRequest)
		{
			// Get all comments that are being deleted
			var relatedComments = await GetAllCommentForServiceRequest(serviceRequest);
			
			foreach (var comment in relatedComments)
			{
				// Delete comment from local cache
				await Table.DeleteAsync(comment);
			}

			// Push changes to the FeatureService
			await Table.ApplyEditsAsync();
		}

		/// <summary>
		/// Gets all comments for ServiceRequest
		/// </summary>
		public async Task<List<Feature>> GetAllCommentForServiceRequest(Feature serviceRequest)
		{
			// requestid is stored as a string in the FeatureService
			var filter = new QueryFilter()
				{
					WhereClause =
						string.Format("requestid = '{0}'", serviceRequest.Attributes["requestid"].ToString()),
				};
			
			// Get all comments that has same requestid
			var results = await Table.QueryAsync(filter);
			return results.ToList();
		}
	}
}

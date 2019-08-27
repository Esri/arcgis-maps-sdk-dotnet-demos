using System.Threading.Tasks;

namespace RoutingSample.Forms
{
    public enum LocationAccessStatus
    {
        /// <summary>
        /// Location access has been denied by the user.
        /// </summary>
        Denied,

        /// <summary>
        /// Location access is disabled on the device.
        /// </summary>
        Disabled,

        /// <summary>
        /// Location access has been granted by the user.
        /// </summary>
        Granted,

        /// <summary>
        /// Location access is restricted on the device (iOS only).
        /// </summary>
        Restricted,
        
        /// <summary>
        /// Unable to determine the status of location access.
        /// </summary>
        Unknown,
    }

    /// <summary>
    /// Provides methods for checking and requesting location access.
    /// </summary>
    public interface ILocationAccessService
    {
        /// <summary>
        /// Requests location access from the user.
        /// </summary>
        /// <returns></returns>
        Task<LocationAccessStatus> RequestAsync();
    }
}

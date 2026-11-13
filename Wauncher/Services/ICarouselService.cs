using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wauncher.Services
{
    public interface ICarouselService
    {
        Task SetupCarouselAsync();
        Task<List<string>?> LoadCarouselUrlsFromGitHubAsync();
        /// <summary>
        /// Loads image paths from a local folder chosen by the user.
        /// Returns null if the folder is empty or invalid.
        /// </summary>
        Task<List<string>?> LoadCarouselUrlsFromLocalFolderAsync(string folderPath);
        Task TeardownCarouselAsync();
        bool IsOfflineMode { get; }
    }
}

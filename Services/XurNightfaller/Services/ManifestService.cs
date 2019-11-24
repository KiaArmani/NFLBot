using System;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using BungieNet.Destiny;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XurClassLibrary.Models;
using XurClassLibrary.Models.Destiny.NDestinyActivityDefinition;
using XurClassLibrary.Models.NDestinyInventoryItemDefinition;
using static System.String;

namespace XurNightfaller.Services
{
    public class ManifestService
    {
        private readonly BungieService _bungieService;
        private readonly ILogger<ManifestService> _logger;
        private SQLiteConnection sqlConnection;

        public ManifestService(ILogger<ManifestService> logger, IServiceProvider services)
        {
            _logger = logger;
            _bungieService = services.GetRequiredService<BungieService>();
            DownloadAndExtractManifest();
            InitializeSqliteDatabase();
        }

        /// <summary>
        ///     Loads Manifest URLs using GetDestinyManifest, downloads the english MobileWorldContent Database and extracts it
        ///     into \manifest.
        /// </summary>
        private void DownloadAndExtractManifest()
        {
            var manifestUrl = _bungieService.GetManifestUrl();
            using var client = new WebClient();
            var runningDirectoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            _logger.LogInformation("Downloading Manifest..");
            client.DownloadFile(manifestUrl, Path.Combine(runningDirectoryName, "manifest.zip"));
            client.Dispose();
            _logger.LogInformation("Downloading Manifest done!");

            _logger.LogInformation("Extracting Manifest..");
            ZipFile.ExtractToDirectory(Path.Combine(runningDirectoryName, "manifest.zip"),
                Path.Combine(runningDirectoryName), true);
            _logger.LogInformation("Extracting Manifest done!");
        }

        /// <summary>
        ///     Searches for the manifest file and opens it
        /// </summary>
        private void InitializeSqliteDatabase()
        {
            var runningDirectoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            try
            {
                _logger.LogInformation("Loading Manifest Database..");
                var files = Directory.GetFiles(runningDirectoryName, "*.content");
                var fileName = Path.GetFileName(files[0]);
                _logger.LogInformation($"Loading {Path.Combine(runningDirectoryName, fileName)}");
                sqlConnection =
                    new SQLiteConnection($"Data Source={Path.Combine(runningDirectoryName, fileName)};Version=3;");
                sqlConnection.Open();

                _logger.LogInformation(" Manifest Database loaded!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Converts the given unsigned int hash into a signed int hash and looks up the Activity Name and Description in the
        ///     manifest Database.
        /// </summary>
        /// <param name="hash">Activity Hash</param>
        /// <returns></returns>
        public ActivityDisplayProperties GetDisplayPropertiesForActivity(string hash)
        {
            // Convert unsigned Hash into signed Hash
            var signedHash = (int) uint.Parse(hash);

            // Prepare SQL Query
            var query = new SQLiteCommand(sqlConnection)
            {
                CommandText = $"SELECT json FROM DestinyActivityDefinition WHERE ID = {signedHash}"
            };

            // Read result object into string variable
            var jsonString = Empty;
            var sqliteDataReader = query.ExecuteReader();

            while (sqliteDataReader.Read())
            {
                var jsonData = (byte[]) sqliteDataReader["json"];
                jsonString = Encoding.Default.GetString(jsonData);
            }

            // Dispose Command
            query.Dispose();

            // If no JSON was found, the activity ID doesn't exist in Manifest. Return "Unknown"
            if (IsNullOrEmpty(jsonString))
                return new ActivityDisplayProperties();

            // Bungie's naming scheme got inconsistent with Ordeals, so we have to do some string manipulation here to get proper results
            var dad = NDestinyActivityDefinition.FromJson(jsonString);

            var returnData = new ActivityDisplayProperties
            {
                Name = dad.DisplayProperties.Name, Description = dad.DisplayProperties.Description
            };

            return returnData;
        }

        public TierType GetWeaponQuality(uint hash)
        {
            // Convert unsigned Hash into signed Hash
            var signedHash = (int) hash;

            // Prepare SQL Query
            var query = new SQLiteCommand(sqlConnection)
            {
                CommandText = $"SELECT json FROM DestinyInventoryItemDefinition WHERE ID = {signedHash}"
            };

            // Read result object into string variable
            var jsonString = Empty;
            var sqliteDataReader = query.ExecuteReader();
            while (sqliteDataReader.Read())
            {
                var jsonData = (byte[]) sqliteDataReader["json"];
                jsonString = Encoding.Default.GetString(jsonData);
            }

            // Dispose Command
            query.Dispose();

            // If no JSON was found, the activity ID doesn't exist in Manifest. Return "Unknown"
            if (IsNullOrEmpty(jsonString))
                return TierType.Unknown;

            var itemDefinition = NDestinyInventoryItemDefinition.FromJson(jsonString);
            if (itemDefinition.Inventory.TierType != null) return (TierType) itemDefinition.Inventory.TierType.Value;
            return TierType.Unknown;
        }

        public string GetWeaponType(uint hash)
        {
            // Convert unsigned Hash into signed Hash
            var signedHash = (int) hash;

            // Prepare SQL Query
            var query = new SQLiteCommand(sqlConnection)
            {
                CommandText = $"SELECT json FROM DestinyInventoryItemDefinition WHERE ID = {signedHash}"
            };

            // Read result object into string variable
            var jsonString = Empty;
            var sqliteDataReader = query.ExecuteReader();
            while (sqliteDataReader.Read())
            {
                var jsonData = (byte[]) sqliteDataReader["json"];
                jsonString = Encoding.Default.GetString(jsonData);
            }

            // Dispose Command
            query.Dispose();

            // If no JSON was found, the activity ID doesn't exist in Manifest. Return "Unknown"
            if (IsNullOrEmpty(jsonString))
                return null;

            var itemDefinition = NDestinyInventoryItemDefinition.FromJson(jsonString);
            return itemDefinition.ItemTypeDisplayName;
        }
    }
}
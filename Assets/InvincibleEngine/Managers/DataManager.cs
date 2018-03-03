using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using InvincibleEngine.DataTypes;
using Newtonsoft.Json;
using UnityEngine;
using _3rdParty.Steamworks.Plugins.Steamworks.NET.autogen;

namespace InvincibleEngine.Managers {
	/// <summary>
	/// Manages generic data such as player settings, config files, profile information, save files, and so on.
	/// </summary>
	[DisallowMultipleComponent]
	public class DataManager : MonoBehaviour {
		
		// True: Will use the working directory of the application
		// False: Will use the directory specified by DEFAULT_DIRECTORY
		public const bool USE_LOCAL_DIRECTORY = false;
		public const string DEFAULT_DIRECTORY = "%APPDATA%/TankShooter/";
		
		// Reserved data directories
		public static readonly string[] DefaultDirectories = {
			"Profile",
			"Config",
			"Save",
			"Log"
		};
		
		// Singeton Instance & Accessor
		private static DataManager _singleton;
		public static DataManager Instance => _singleton ?? new GameObject("DataManager").AddComponent<DataManager>();
		
		// Public Properties (Read-Only)
		public static UserProfile MainProfile => _singleton._mainProfile;
		
		// Loaded user profiles
		private UserProfile _mainProfile;
		private readonly List<UserProfile> _userProfiles = new List<UserProfile>();
		
		// Working directory for the session
		private string _directory;

		// Initialization
		private void Awake () {
			// Enforce Singleton Instance
			if (_singleton == null) { _singleton = this; }
			else if (_singleton != this) { Destroy(gameObject); }
			
			// Ensure this manager is not destroyed on scene load
			DontDestroyOnLoad(gameObject);
			
			// Check for reserved directories and create them if needed
			_directory = USE_LOCAL_DIRECTORY ? Application.dataPath + "/" : Environment.ExpandEnvironmentVariables(DEFAULT_DIRECTORY);
			Debug.Log($"<b><color=Brown>DataManager:</color></b> Working directory set to: {_directory}");
			foreach (var dir in DefaultDirectories) {
				// Continue if the directory exists
				if (Directory.Exists(_directory + dir)) { continue; }
				
				// Try to create the directory and log an exception if needed
				try {
					Debug.Log($"<b><color=Brown>DataManager:</color></b> Attempting to create default directory '{dir}'...");
					Directory.CreateDirectory(_directory + dir);
				}
				catch (Exception ex) { Debug.LogWarning("<b><color=Brown>DataManager:</color></b> An exception has occured while attempting to create default directories!\n" + ex.Message); }
			}
			
			// Set Json.Net default settings
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
				Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			};
			
			// Attempt to load any profiles stored in the Profiles directory
			var profiles = Directory.GetFiles(_directory + "Profile", "*.profile").Select(Path.GetFileName).ToList();
			foreach (var profile in profiles) {
				try {
					UserProfile userProfile;
					LoadObjectFromJson("Profile", profile, out userProfile);
					_userProfiles.Add(userProfile);
					Debug.Log($"<b><color=Brown>DataManager:</color></b> Successfully loaded profile data for {userProfile.DisplayName}:{userProfile.UniqueId} from {profile}!");
				}
				catch (Exception ex) {
					Debug.LogWarning($"<b><color=Brown>DataManager:</color></b> An exception occured while trying to load profile data from {profile}!\n" +
					                 ex.Message);
				}
			}
			
			// Attempt to find and set the main user profile (the active Steam user)
			var steamId = SteamUser.GetSteamID().m_SteamID;
			var displayName = SteamFriends.GetPersonaName();
			_mainProfile = GetProfileById(steamId);
			
			// Create and set a new main profile if the previous block fails
			if (_mainProfile == null) {
				Debug.Log($"<b><color=Brown>DataManager:</color></b> Failed to load main profile data for active Steam user {displayName}:{steamId}!\n" +
				          $"A new profile will be created and saved to disk...");
				_mainProfile = new UserProfile(false, steamId, displayName);
				_userProfiles.Add(MainProfile);
			}
			
			// Attempt to save the new main profile to disk
			try {
				SaveObjectAsJson("Profile", steamId.ToString() + ".profile", MainProfile);
			}
			catch (Exception ex) {
				Debug.LogWarning($"<b><color=Brown>DataManager:</color></b> An exception occured while trying to save main profile data for {displayName}:{steamId}!\n" +
				                 ex.Message);
			}
		}
		
		/// <summary>
		/// Forces all loaded profiles to be saved to disk.
		/// This operation may cause a small delay and therefore should not be called during gameplay.
		/// </summary>
		public static void SaveProfileData() {
			foreach (var profile in Instance._userProfiles) {
				try {
					SaveObjectAsJson("Profile", profile.UniqueId.ToString() + ".profile", profile);
				}
				catch (Exception ex) {
					Debug.LogWarning($"<b><color=Brown>DataManager:</color></b> An exception occured while trying to save profile data for {profile.DisplayName}:{profile.UniqueId}!\n" +
					                 ex.Message);
				}
			}
		}
		
		/// <summary>
		/// Attempts to retrieve a profile from the list of loaded profiles by Display Name. 
		/// If multiple records exist, this function will always return the first one found.
		/// It is recommended to retrieve a profile by UniqueId instead.
		/// </summary>
		/// <param name="displayName">The display name to use for the search.</param>
		public static UserProfile GetProfileByName(string displayName) {
			// Iterate through the loaded profiles and look for a matching name
			return Instance._userProfiles.FirstOrDefault(profile => profile.DisplayName == displayName);
		}
		
		/// <summary>
		/// Attempts to retrieve a profile from the list of loaded profiles by Unique ID. 
		/// If multiple records exist, this function will always return the first one found though this should never be the case.
		/// </summary>
		/// <param name="uniqueId">The unique ID to use for the search.</param>
		public static UserProfile GetProfileById(ulong uniqueId) {
			// Iterate through the loaded profiles and look for a matching ID
			return Instance._userProfiles.FirstOrDefault(profile => profile.UniqueId == uniqueId);
		}
		
		/// <summary>
		/// Save a given object as a Json file to disk.
		/// Should be wrapped in a try-catch block as this function may fail and throw an exception.
		/// </summary>
		/// <param name="folder">The folder in which the specified file will be stored.</param>
		/// <param name="fileName">The desired name of the file being written.</param>
		/// <param name="obj">The object to be serialized to a Json file.</param>
		public static void SaveObjectAsJson(string folder, string fileName, object obj) {
			// Check if the specified directory exists and try to create it if needed
			if (!Directory.Exists(Instance._directory + folder)) Directory.CreateDirectory(Instance._directory + folder);
			
			// Try to serialize the object to a file
			var filePath = $"{Instance._directory}/{folder}/{fileName}";
			var jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented);
			File.WriteAllText(filePath, jsonString);
		}

		/// <summary>
		/// Tries to load a given Json file from disc and return the data as a generic object.
		/// Should be wrapped in a try-catch block as this function may fail and throw an exception.
		/// </summary>
		/// <param name="folder">The folder from which to load the file.</param>
		/// <param name="fileName">The name of the file to be loaded.</param>
		/// <param name="obj">The object to write the loaded data to.</param>
		public static void LoadObjectFromJson<T>(string folder, string fileName, out T obj) {
			// Try to load the specified file
			var filePath = $"{Instance._directory}/{folder}/{fileName}";
			var jsonString = File.ReadAllText(filePath);
			obj = JsonConvert.DeserializeObject<T>(jsonString);
		}
		
		/// <summary>
		/// Save a given object as a binary file to disk.
		/// Folder and File names should be simple names as the directory string and extension are generated automatically.
		/// Should be wrapped in a try-catch block as this function can fail and throw an exception.
		/// </summary>
		/// <param name="folder">The folder in which the specified file will be stored.</param>
		/// <param name="fileName">The desired name of the file being written.</param>
		/// <param name="obj">The object to be serialized to a binary file.</param>
		public static void SaveObjectAsBinary(string folder, string fileName, object obj) {
			// Check if the specified directory exists and try to create it if needed
			if (!Directory.Exists(Instance._directory + folder)) Directory.CreateDirectory(Instance._directory + folder);
			
			// Try to serialize the object to a file
			var filePath = $"{Instance._directory}/{folder}/{fileName}.bin";
			using (var stream = new MemoryStream()) {
				var formatter = new BinaryFormatter();
            
				// Serialize the object
				formatter.Serialize(stream, obj);
				File.WriteAllBytes(filePath, stream.ToArray());
			}
		}
		
		/// <summary>
		/// Tries to load a given binary file from disc and return the data as a generic object.
		/// Should be wrapped in a try-catch block as this function can fail and throw an exception.
		/// </summary>
		/// <param name="folder">The folder from which to load the file.</param>
		/// <param name="fileName">The name of the file to be loaded.</param>
		/// <param name="obj">The object to write the loaded data to.</param>
		public static void LoadObjectFromBinary<T>(string folder, string fileName, out T obj) {
			// Try to load the specified file
			var filePath = $"{Instance._directory}/{folder}/{fileName}.bin";
			using (var stream = new MemoryStream(File.ReadAllBytes(filePath))) {
				var formatter = new BinaryFormatter();
				obj = (T) formatter.Deserialize(stream);
			}
		}
	}
}

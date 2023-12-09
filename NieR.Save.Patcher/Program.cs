using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;

namespace NieR.Save.Patcher
{
	internal class Program
	{
		public const int SaveSize = 0x399CC;

		private static UInt64 SteamID;
		private static string FilePath;
		private static byte[] FileData;

		static void Main(string[] args)
		{
			if (args.Length > 1)
			{
				Batch(args[0], args[1]);
				return;
			}

            SteamID = GetSteamID();

			if (args.Length < 2 && args.Length > 0)
			{
				Batch(args[0], SteamID);
				return;
			} 

			FilePath = PromptFilePath();

			FileData = LoadFileData(FilePath);
			CreateOldFile(FilePath);

			FileData = PatchFileData(FileData, SteamID);

			File.WriteAllBytes(FilePath, FileData);
		}

		private static byte[] PatchFileData(byte[] fileData, UInt64 steamID)
		{
			const int startAddress = 0x00000004;

			byte[] steamBytes = BitConverter.GetBytes(steamID);

			for (long i = startAddress; i < startAddress + steamBytes.Length; i++)
			{
				// Access and process each byte
				byte currentByte = steamBytes[i - startAddress];
				fileData[i] = currentByte;
			}

			return fileData;

		}

		private static void CreateOldFile(string filePath)
		{
			string folder = Path.GetDirectoryName(filePath);
			string filename = Path.GetFileNameWithoutExtension(filePath);
			string extention = Path.GetExtension(filePath) + ".old";
			string newpath = Path.Combine(folder, filename + extention);
			File.Move(filePath, newpath);
		}

		private static byte[] LoadFileData(string filePath)
		{
			byte[] data = File.ReadAllBytes(filePath);
			if(data.Length != SaveSize)
			{
				ExitWithError("Save file size is wrong.");
			}

			return data;
		}

		private static string PromptFilePath()
		{
			Console.Write($"Please enter a path to a SlotData file: ");
			string path = Console.ReadLine();

			if (!File.Exists(path))
			{
				Console.WriteLine($"File not found: {path}");
				return PromptFilePath();
			}

			return path;
		}

		private static UInt64 PromptSteamID()
		{
			Console.Write($"Please enter your SteamID: ");
			string id_s = Console.ReadLine();
			UInt64 id_i;
			try
			{
				UInt64.TryParse(id_s, out id_i);
				return id_i;
			}
			catch
			{
				Console.WriteLine("Unable to parse SteamID64");
				return PromptSteamID();
			}
		}

		private static UInt64 GetSteamID()
		{
			const uint appID = 480; // Spacewar https://steamdb.info/app/480/info/
			UInt64 id;
			try
			{
				SteamClient.Init(appID, true);
				id = SteamClient.SteamId;
				SteamClient.Shutdown();
				Console.WriteLine($"Sucessfully obtained SteamID_64: {id}");
				return id;
			}
			catch
			{
				Console.WriteLine("Steamworks has encountered an error and could not obtain your steam ID automatically.");
				return PromptSteamID();
			}
		}
		private static void Batch(string path, string steamID)
		{
			UInt64 id;
			try
			{
				UInt64.TryParse(steamID, out id);
				Batch(path, id);
			}
			catch (Exception e)
			{
				ExitWithError(e.Message);
			}

		}

		private static void Batch(string path, UInt64 steamID)
		{
			FileData = LoadFileData(path);
			CreateOldFile(path);
			FileData = PatchFileData(FileData, steamID);
			File.WriteAllBytes(path, FileData);
		}

		private static void ExitWithError(string e = "An error has occured.")
		{
			Console.WriteLine(e);
			Console.WriteLine($"Press any key to exit.");
			Console.ReadKey();
			Environment.Exit(0);

		}

	}
}

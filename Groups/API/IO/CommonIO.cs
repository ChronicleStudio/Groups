
using System;
using System.IO;
using System.Text.Json;
using Vintagestory.API.Server;

namespace Groups.API.IO
{
	class CommonIO
	{

		public static void WriteData(ICoreServerAPI sapi, string FolderName, byte[] data, IServerPlayer player = null, string key = "", Object jData = null)
		{

			string path = $"{sapi.DataBasePath}\\Groups\\{FolderName}\\";
			Directory.CreateDirectory(path);
			path += $"{sapi.World.SavegameIdentifier}{player?.PlayerUID ?? ""}{key}";
			File.WriteAllBytes(path, Encrypt(data, 2));
#if DEBUG
			if (jData != null)
			{
				File.WriteAllText(path + ".json", JsonSerializer.Serialize(jData, new JsonSerializerOptions { IncludeFields = true }));
			}
#endif
		}
		public static byte[] ReadData(ICoreServerAPI sapi, string FolderName, IServerPlayer player = null, string key = "")
		{
			string path = $"{sapi.DataBasePath}\\Groups\\{FolderName}\\";
			Directory.CreateDirectory(path);
			path += $"{sapi.World.SavegameIdentifier}{player?.PlayerUID ?? ""}{key}";
			if (!File.Exists(path)) return null;
			return Decrypt(File.ReadAllBytes(path), 2);
		}

		public static void WriteModData(ICoreServerAPI sapi, string FolderName, byte[] data, string key = "", Object jData = null)
		{
			WriteData(sapi, FolderName, data, key: key, jData: jData);
		}
		public static byte[] ReadModData(ICoreServerAPI sapi, string FolderName, string key = "")
		{
			return ReadData(sapi, FolderName, key: key);
		}
		public static void WritePlayerData(ICoreServerAPI sapi, IServerPlayer player, string FolderName, byte[] data, string key = "", Object jData = null)
		{
			WriteData(sapi, FolderName, data, player: player, key: key, jData: jData);
		}
		public static byte[] ReadPlayerData(ICoreServerAPI sapi, IServerPlayer player, string FolderName, string key = "")
		{
			return ReadData(sapi, FolderName, player: player, key: key);
		}
		private static byte[] Encrypt(byte[] value, int bitcount)
		{
			byte[] temp = new byte[value.Length];
			if (bitcount >= 8)
			{
				Array.Copy(value, bitcount / 8, temp, 0, temp.Length - (bitcount / 8));
			}
			else
			{
				Array.Copy(value, temp, temp.Length);
			}
			if (bitcount % 8 != 0)
			{
				for (int i = 0; i < temp.Length; i++)
				{
					temp[i] <<= bitcount % 8;
					if (i < temp.Length - 1)
					{
						temp[i] |= (byte)(temp[i + 1] >> 8 - bitcount % 8);
					}
				}
			}
			return temp;
		}
		public static byte[] Decrypt(byte[] value, int bitcount)
		{
			byte[] temp = new byte[value.Length];
			if (bitcount >= 8)
			{
				Array.Copy(value, 0, temp, bitcount / 8, temp.Length - (bitcount / 8));
			}
			else
			{
				Array.Copy(value, temp, temp.Length);
			}
			if (bitcount % 8 != 0)
			{
				for (int i = temp.Length - 1; i >= 0; i--)
				{
					temp[i] >>= bitcount % 8;
					if (i > 0)
					{
						temp[i] |= (byte)(temp[i - 1] << 8 - bitcount % 8);
					}
				}
			}
			return temp;
		}
	}
}

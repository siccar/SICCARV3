﻿// Transaction V2 Payload Class File - The Welder/Skid Row
// 10.04.2021
// Wallet Services (Siccar)

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;
using Sodium;
#nullable enable

namespace Siccar.Platform.Cryptography.TransactionV2
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051: Mostly reflection invoked constructors and methods")]
	[Serializable]
	public class Payload : IPayload
	{
		private class PayloadBox(byte[] wallet, byte[] box)
		{
			private readonly byte[] box_wallet = wallet;
			private readonly byte[] box_data = box;

			public byte[] GetCryptoBox() { return box_data; }
			public byte[] GetWallet() { return box_wallet; }
			public byte[] GetSigningData()
			{
				List<byte> data = [.. box_wallet, .. WalletUtils.VLEncode(box_data.Length), .. box_data];
				return [.. data];
			}
		}

		private byte[]? payload_data;
		private readonly List<PayloadBox> payload_boxes = [];
		private byte[]? payload_iv;
		private byte[]? payload_hash;
		private bool payload_encrypted;
		private readonly HashSet<string> payload_wallets = [];
		private Status LastError;

		private Payload(byte[] data, string[] wallets)
		{
			if (data == null || data.Length < 1)
				throw new ArgumentException(string.Empty);
			if (wallets == null || wallets.Length < 1)
			{
				payload_encrypted = false;
				payload_data = data;
			}
			else
			{
				Aes aes = CreateCrypto();
				try
				{
					for (int i = 0; i < wallets.Length; i++)
					{
						(byte Network, byte[] PubKey)? k = WalletUtils.WalletToPubKey(wallets[i]);
						if (k != null && k.Value.PubKey != null && k.Value.PubKey.Length == WalletNetworks.ED25519.GetPubKeySizeAttribute())
						{
							if (payload_wallets.Add(wallets[i]))
								payload_boxes.Add(new PayloadBox(k.Value.PubKey, SealedPublicKeyBox.Create(aes.Key, ConvertPubKey(k.Value.PubKey))));
						}
					}
					if (payload_wallets.Count > 0)
					{
						payload_data = WalletUtils.CryptoData(CryptoMode.ENCRYPT, data, ref aes);
						payload_iv = aes.IV;
						payload_encrypted = true;
					}
					else
					{
						payload_encrypted = false;
						payload_data = data;
					}
				}
				catch (Exception)
				{
					LastError = Status.TX_BAD_CRYPTOBOX;
					return;
				}
				finally { aes.Dispose(); }
			}
			payload_hash = WalletUtils.ComputeSHA256Hash(payload_data);
			LastError = Status.STATUS_OK;
		}
		private Payload(BinaryReader reader)
		{
			try
			{
				payload_encrypted = reader.ReadUInt16() == (UInt16)PayloadFlags.PAYLOAD_ENCRYPTION;
				UInt32 count = (UInt32)WalletUtils.ReadVLSize(reader);
				if (count > 0)
				{
					for (uint i = 0; i < count; i++)
					{
						byte[] wdata = reader.ReadBytes(WalletNetworks.ED25519.GetPubKeySizeAttribute());
						string? wallet = WalletUtils.PubKeyToWallet(wdata, (byte)WalletNetworks.ED25519);
						if (wallet != null)
						{
							byte[] box = reader.ReadBytes((int)WalletUtils.ReadVLSize(reader));
							if (payload_wallets.Add(wallet))
								payload_boxes.Add(new PayloadBox(wdata, box));
						}
					}
					payload_iv = reader.ReadBytes((int)WalletUtils.ReadVLSize(reader));
				}
				if (payload_wallets.Count == count && payload_boxes.Count == count)
				{
					UInt64 payload_size = (UInt64)WalletUtils.ReadVLSize(reader);
					payload_hash = reader.ReadBytes((int)TxDefs.SHA256HashSize);
					payload_data = reader.ReadBytes((int)payload_size);
					if (VerifyHash())
					{
						LastError = Status.STATUS_OK;
						return;
					}
				}
			}
			catch (Exception) {}
			LastError = Status.TX_CORRUPT_PAYLOAD;
		}
		private byte[]? GetData(string? key = null)
		{
			byte[]? data;
			if (!payload_encrypted)
			{
				data = payload_data;
				LastError = key != null ? Status.TX_NOT_ENCRYPTED : Status.STATUS_OK;
			}
			else
			{
				PayloadBox? box = WIFAccessible(key);
				if (GetLastError() != Status.STATUS_OK)
					return null;
				Aes aes = CreateCrypto(payload_iv, false);
				try
				{
					aes.Key = SealedPublicKeyBox.Open(box!.GetCryptoBox(), GetKeyPair(key));
					data = WalletUtils.CryptoData(CryptoMode.DECRYPT, payload_data, ref aes);
				}
				catch (Exception)
				{
					LastError = Status.TX_ACCESS_DENIED;
					return null;
				}
				finally { aes.Dispose(); }
				LastError = Status.STATUS_OK;
			}
			return data;
		}
		private PayloadBox? WalletAccessible(string? wallet)
		{
			(byte Network, byte[] PubKey)? key_data = WalletUtils.WalletToPubKey(wallet);
			if (key_data == null && wallet != null && wallet.Length > 0)
				LastError = Status.TX_INVALID_WALLET;
			else
			{
				if (!payload_encrypted)
					LastError = Status.TX_NOT_ENCRYPTED;
				else if (key_data == null)
					LastError = Status.TX_ACCESS_DENIED;
				else
				{
					ReadOnlySpan<byte> kd = key_data.Value.PubKey;
					for (int i = 0; i < payload_boxes.Count; i++)
					{
						if (kd.SequenceEqual(payload_boxes[i].GetWallet()))
						{
							LastError = Status.STATUS_OK;
							return payload_boxes[i];
						}
					}
					LastError = Status.TX_ACCESS_DENIED;
				}
			}
			return null;
		}
		private PayloadBox? WIFAccessible(string? key)
		{
			if (!payload_encrypted)
				LastError = Status.TX_NOT_ENCRYPTED;
			else
			{
				(byte Network, byte[] PrivateKey)? key_data = WalletUtils.WIFToPrivKey(key);
				if (key_data == null && key != null && key.Length > 0)
					LastError = Status.TX_INVALID_KEY;
				else if (key_data == null)
					LastError = Status.TX_ACCESS_DENIED;
				else
				{
					PayloadBox? box = WalletAccessible(WalletUtils.PubKeyToWallet(PublicKeyAuth.ExtractEd25519PublicKeyFromEd25519SecretKey(key_data.Value.PrivateKey), key_data.Value.Network));
					if (GetLastError() == Status.STATUS_OK)
						return box;
				}
			}
			return null;
		}
		private bool[] AddWallets(string? key, string[]? wallets)
		{
			bool[] added = new bool[wallets!.Length];
			if (payload_encrypted)
			{
				PayloadBox? pb = WIFAccessible(key);
				if (pb != null && LastError == Status.STATUS_OK)
				{
					try
					{
						byte[] k = SealedPublicKeyBox.Open(pb!.GetCryptoBox(), GetKeyPair(key));
						for (int i = 0; i < wallets.Length; i++)
						{
							(byte Network, byte[] PubKey)? w = WalletUtils.WalletToPubKey(wallets[i]);
							if (w != null && w.Value.PubKey.Length == WalletNetworks.ED25519.GetPubKeySizeAttribute() && payload_wallets.Add(wallets[i]))
							{
								payload_boxes.Add(new PayloadBox(w.Value.PubKey, SealedPublicKeyBox.Create(k, ConvertPubKey(w.Value.PubKey))));
								added[i] = true;
							}
						}
						LastError = Status.STATUS_OK;
					}
					catch (Exception) { LastError = Status.TX_BAD_CRYPTOBOX; }
				}
			}
			else
			{
				Aes aes = CreateCrypto();
				try
				{
					for (int i = 0; i < wallets.Length; i++)
					{
						(byte Network, byte[] PubKey)? w = WalletUtils.WalletToPubKey(wallets[i]);
						if (w != null && payload_wallets.Add(wallets[i]))
						{
							payload_boxes.Add(new PayloadBox(w.Value.PubKey, SealedPublicKeyBox.Create(aes.Key, ConvertPubKey(w.Value.PubKey))));
							added[i] = true;
						}
					}
					payload_iv = aes.IV;
					payload_data = WalletUtils.CryptoData(CryptoMode.ENCRYPT, payload_data, ref aes);
					payload_hash = WalletUtils.ComputeSHA256Hash(payload_data);
					payload_encrypted = true;
					LastError = Status.STATUS_OK;
				}
				catch (Exception) { LastError = Status.TX_BAD_CRYPTOBOX; }
				finally { aes.Dispose(); }
			}
			return added;
		}
		private bool[] RemoveWallets(string[] wallets)
		{
			bool[] removed = new bool[wallets.Length];
			if (payload_encrypted)
			{
				for (int i = 0; i < wallets.Length; i++)
				{
					(byte Network, byte[] PubKey)? w = WalletUtils.WalletToPubKey(wallets[i]);
					if (w != null && w.Value.PubKey.Length == WalletNetworks.ED25519.GetPubKeySizeAttribute() && payload_wallets.Remove(wallets[i]))
					{
						int index = payload_boxes.FindIndex(b => new ReadOnlySpan<byte>(w.Value.PubKey).SequenceEqual(b.GetWallet()));
						if (index >= 0)
						{
							payload_boxes.RemoveAt(index);
							removed[i] = true;
						}
					}
				}
				if (payload_wallets.Count < 1)
					payload_boxes.Clear();
			}
			LastError = Status.STATUS_OK;
			return removed;
		}
		private bool DecryptData(string? key)
		{
			byte[]? data = GetData(key);
			if (data == null)
				return false;
			payload_data = data;
			payload_encrypted = false;
			payload_hash = WalletUtils.ComputeSHA256Hash(payload_data);
			payload_boxes.Clear();
			payload_wallets.Clear();
			LastError = Status.STATUS_OK;
			return true;
		}
		private bool ReplaceData(byte[] data)
		{
			if (payload_wallets.Count < 1)
			{
				payload_data = data;
				payload_encrypted = false;
			}
			else
			{
				payload_boxes.Clear();
				Aes aes = Aes.Create();
				try
				{
					if (payload_wallets.Count > 0)
					{
						foreach (string wallet in payload_wallets)
						{
							byte[] w = WalletUtils.WalletToPubKey(wallet)!.Value.PubKey;
							payload_boxes.Add(new PayloadBox(w, SealedPublicKeyBox.Create(aes.Key, PublicKeyAuth.ConvertEd25519PublicKeyToCurve25519PublicKey(w))));
						}
						payload_iv = aes.IV;
					}
					payload_data = WalletUtils.CryptoData(CryptoMode.ENCRYPT, data, ref aes);
					payload_encrypted = true;
				}
				catch (Exception)
				{
					LastError = Status.TX_BAD_CRYPTOBOX;
					return false;
				}
				finally { aes.Dispose(); }
			}
			payload_hash = WalletUtils.ComputeSHA256Hash(payload_data);
			LastError = Status.STATUS_OK;
			return true;
		}
		private List<byte> SigningData()
		{
			List<byte> data =
			[
				.. BitConverter.GetBytes((UInt16)(payload_encrypted ? PayloadFlags.PAYLOAD_ENCRYPTION : 0)),
				.. WalletUtils.VLEncode(payload_wallets.Count),
			];
			if (payload_wallets.Count > 0)
			{
				payload_boxes.ForEach(b => { data.AddRange(b.GetSigningData()); });
				data.AddRange(WalletUtils.VLEncode(payload_iv!.Length));
				data.AddRange(payload_iv);
			}
			data.AddRange(WalletUtils.VLEncode(payload_data!.Length));
			data.AddRange(payload_hash!);
			return data;
		}
		private List<byte> HashingData()
		{
			List<byte> data = SigningData();
			data.AddRange(payload_data!);
			return data;
		}
		private string[] PayloadWallets()
		{
			List<string> wallets = [];
			if (!payload_encrypted)
				LastError = Status.TX_NOT_ENCRYPTED;
			else
			{
				wallets.AddRange(payload_wallets);
				LastError = Status.STATUS_OK;
			}
			return [.. wallets];
		}
		private bool WIFAccess(string? key) { return WIFAccessible(key) != null; }
		private bool WalletAccess(string? wallet) { return WalletAccessible(wallet) != null || GetLastError() == Status.TX_NOT_ENCRYPTED; }
		private static KeyPair GetKeyPair(string? key)
		{
			(byte Network, byte[] PrivateKey)? key_data = WalletUtils.WIFToPrivKey(key);
			using KeyPair pair = new(ConvertPubKey(PublicKeyAuth.ExtractEd25519PublicKeyFromEd25519SecretKey(key_data!.Value.PrivateKey)),
				PublicKeyAuth.ConvertEd25519SecretKeyToCurve25519SecretKey(key_data.Value.PrivateKey));
			return pair;
		}
		private static byte[] ConvertPubKey(byte[]? key) { return PublicKeyAuth.ConvertEd25519PublicKeyToCurve25519PublicKey(key!); }
		private static Aes CreateCrypto(byte[]? iv = null, bool generate = true)
		{
			Aes aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.KeySize = 256;
			if (generate)
				aes.GenerateKey();
			if (iv != null)
				aes.IV = iv;
			else
				aes.GenerateIV();
			return aes;
		}
		private Status GetLastError() { return LastError; }

/** Publically Accessible Functions **/
		public IPayloadInfo GetInfo()
		{
			ConstructorInfo? ci = typeof(PayloadInfo).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
				[typeof(bool), typeof(UInt64), typeof(byte[]), typeof(string[])], null);
			return (PayloadInfo)ci!.Invoke([payload_encrypted, DataSize(), DataHash()!, PayloadWallets()]);
		}
		public byte[]? DataHash()
		{
			LastError = Status.STATUS_OK;
			return payload_hash;
		}
		public UInt64 DataSize()
		{
			LastError = Status.STATUS_OK;
			return (UInt64)payload_data!.Length;
		}
		public bool VerifyHash() { return ((ReadOnlySpan<byte>)WalletUtils.ComputeSHA256Hash(payload_data)).SequenceEqual(DataHash()); }
		public bool DataSparse() { return false; }
		public bool DataProtected() { return false; }
		public bool DataCompressed() { return false; }
		public bool DataEncrypted() { return payload_encrypted; }
	}
}
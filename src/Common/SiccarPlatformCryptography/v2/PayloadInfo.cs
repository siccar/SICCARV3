// Transaction V2 PayloadInfo Class File - The Welder/Skid Row
// 10.04.2021
// Wallet Services (Siccar)

using System;
#nullable enable

namespace Siccar.Platform.Cryptography.TransactionV2
{
	[Serializable]
	public class PayloadInfo : IPayloadInfo
	{
		private readonly bool PayloadEncrypted;
		private readonly UInt64 PayloadSize;
		private readonly byte[]? PayloadHash;
		private readonly string[] PayloadWallets;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051: Reflection invoked constructor")]
		private PayloadInfo(bool encrypted, UInt64 size, byte[]? hash, string[]? wallets)
		{
			PayloadEncrypted = encrypted;
			PayloadSize = size;
			PayloadHash = hash;
			PayloadWallets = wallets ?? [];
		}
		public bool GetPayloadEncrypted() { return PayloadEncrypted; }
		public bool GetPayloadCompressed() { return false; }
		public bool GetPayloadProtected() { return false; }
		public bool GetPayloadSparse() { return false; }
		public UInt64 GetPayloadSize() { return PayloadSize; }
		public byte[]? GetPayloadHash() { return PayloadHash; }
		public string[] GetPayloadWallets() { return PayloadWallets; }
		public UInt32 GetPayloadWalletsCount() { return (UInt32)PayloadWallets.Length; }
		public HashType GetPayloadHashType() { return HashType.SHA256; }
		public EncryptionType GetPayloadEncryptionType() { return PayloadEncrypted ? EncryptionType.AES_256 : EncryptionType.None; }
		public CompressionType GetPayloadCompressionType() { return CompressionType.None; }
		public PayloadTypeField GetPayloadTypeField() { return PayloadTypeField.Transaction; }
		public UInt16 GetPayloadUserField() { return 0; }
		public PayloadFlags GetPayloadFlags() { return 0; }
	}
}
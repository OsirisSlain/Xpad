class Encode {
	public static byte[] Xor(string fileName, byte[] padBytes) {
		using(var file = File.OpenRead(fileName))
		{
			var fileBytes = new byte[file.Length];
			file.Read(fileBytes, 0, fileBytes.Length);
			for (int i = 0; i < padBytes.Length; i++)
				fileBytes[i] ^= padBytes[i];
			return fileBytes;
		}
	}
}
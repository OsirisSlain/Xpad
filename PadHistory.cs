using System.Text.Json;
using System.Text.Json.Serialization;

//A class to represent the history of a one-time pad file use
class PadHistory {
	const string FolderName = "Xpad-Encryption";

	[JsonRequired]
	public int PadFingerPrint { get; set; }

	[JsonRequired]
	public long StartByteInclusive { get; set; }

	[JsonRequired]
	public long EndByteInclusive { get; set; }

	[JsonRequired]
	public DateTime DateUsed { get; set; }

	public static int GenerateFingerPrint(string padfileName) {
		const int fingerprintSize = 1024;

		var fileLength = new FileInfo(padfileName).Length;
		var firstByteSize = fileLength > fingerprintSize
			? fingerprintSize
			: fileLength;
		var bytesLeft = fileLength - firstByteSize > 0
			? fileLength - firstByteSize
			: 0;
		var lastByteSize = bytesLeft > fingerprintSize
			? fingerprintSize
			: bytesLeft;

		using(var padFile = File.OpenRead(padfileName)) {
			var firstBytes = new byte[firstByteSize];
			padFile.Read(firstBytes, 0, firstBytes.Length);
			padFile.Seek(-lastByteSize, SeekOrigin.End);

			var lastBytes = new byte[lastByteSize];
			padFile.Read(lastBytes, 0, lastBytes.Length);

			var finalBytes = firstBytes.Concat(lastBytes).ToArray();
			return Checksum.Fletcher16(finalBytes);
		}
	}

	public static void ToAppData(List<PadHistory> padHistory) {
		var appData = Environment.GetFolderPath(
			Environment.SpecialFolder.ApplicationData,
			Environment.SpecialFolderOption.Create);
		var folder = Path.Combine(appData, FolderName);
		if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
		var historyFilename = Path.Combine(folder, ".Xpad.history");
		ToFile(historyFilename, padHistory);
	}

	public static List<PadHistory> FromAppdata() {
		var appData = Environment.GetFolderPath(
			Environment.SpecialFolder.ApplicationData,
			Environment.SpecialFolderOption.None);
		if (String.IsNullOrEmpty(appData)) return new List<PadHistory>();
		var historyFilename = Path.Combine(appData, FolderName, ".Xpad.history");
		if(!File.Exists(historyFilename)) return new List<PadHistory>();
		return FromFile(historyFilename);
	}

	public static void ToFile(string filename, List<PadHistory> padHistory) {
		JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
		var json = JsonSerializer.Serialize(padHistory, options);
		File.WriteAllText(filename, json);
		Console.WriteLine($"Wrote PadHistory to file: {filename}");
	}

	public static List<PadHistory> FromFile(string filename) {
		if(File.Exists(filename))
			return FromJson(File.ReadAllText(filename));
		return new List<PadHistory>();
	}

	public static List<PadHistory> FromJson(string json) {
		var value = JsonSerializer.Deserialize<List<PadHistory>>(json);
		if(value == null) {
			throw new Exception("Unable to deserialize PadHistory");
		}
		return value;
	}
}
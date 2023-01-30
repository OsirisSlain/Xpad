using System.Text.Json;
using System.Text.Json.Serialization;

class MetaData
{
	[JsonRequired]
	public long StartByteInclusive { get; set; }

	[JsonRequired]
	public long EndByteInclusive { get; set; }

	[JsonRequired]
	public int CheckSum { get; set; }

	[JsonIgnore]
	public long PadLength => EndByteInclusive - StartByteInclusive + 1;

	public static MetaData FromFile(string filename) {
		return FromJson(File.ReadAllText(filename));
	}

	public void ToFile(string filename) {
		JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
		var json = JsonSerializer.Serialize(this, options);
		File.WriteAllText(filename, json);
		Console.WriteLine();
		Console.WriteLine($"Wrote Metadate to file: {filename}");
	}

	public bool Matches(MetaData other) {
		return StartByteInclusive == other.StartByteInclusive
			&& EndByteInclusive == other.EndByteInclusive
			&& CheckSum == other.CheckSum;
	}

	public bool MatchesFile(string filename) => Matches(FromFile(filename));

	public static MetaData FromJson(string json) {
		var value = JsonSerializer.Deserialize<MetaData>(json);
		if(value == null) {
			throw new Exception("Unable to deserialize MetaData");
		}
		if(value.StartByteInclusive < 1) {
			throw new Exception(
				"MetaData deserialization error:" + Environment.NewLine +
				"StartByteInclusive must be greater than 0"
			);
		}
		if(value.EndByteInclusive < value.StartByteInclusive) {
			throw new Exception(
				"MetaData deserialization error:" + Environment.NewLine +
				"EndByteInclusive must be greater than or equal to StartByteInclusive"
			);
		}
		return value;
	}
}
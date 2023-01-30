//Xpad
//Description: Command line programe to use a one-time pad to encrypt or decrypt files
//Author: Blake Shaw
//License: MIT

var usage = "Usage: Xpad {encode|decode} <input file> <pad file> [one-based pad index]";

# region Parse Arguments
if (args.Length < 3 || args.Length > 4) {
	Console.WriteLine(usage);
	return;
}

var mode = args[0].Trim().ToLower();
if(mode != "encode" && mode != "decode") {
	Console.WriteLine(usage);
	return;
}

var inFilename = args[1].Trim();
if(!File.Exists(inFilename)) {
	Console.WriteLine($"{inFilename} does not exist");
	return;
}
var inFileSize = new FileInfo(inFilename).Length;

var padFilename = args[2].Trim();
if(!File.Exists(padFilename)) {
	Console.WriteLine($"{padFilename} does not exist");
	return;
}
var padFileSize = new FileInfo(padFilename).Length;

long startByte = -1;
bool startByteSpecified = false;
if(args.Length == 4) {
	if(!long.TryParse(args[3], out startByte)) {
		Console.WriteLine(usage);
		return;
	}
	if(startByte < 1) {
		Console.WriteLine("Start byte must be 1 or greater");
		return;
	}
	startByteSpecified = true;
}
# endregion Parse Arguments

# region Handle Metadata
var metaFile = mode == "encode"
	? $"{inFilename}.xpad.meta"
	: $"{inFilename}.meta";
var metaFileExists = File.Exists(metaFile);
if(!startByteSpecified && !metaFileExists) {
	if(mode == "decode") Console.Write($"{metaFile} does not exist, ");
	if(mode == "encode") Console.Write("When encoding, ");
	Console.WriteLine("a one-based byte index into the pad file is required");
	Console.WriteLine(usage);
	return;
}

var padLength = inFileSize;
var recordedChecksum = -1;
if(!startByteSpecified && metaFileExists) {
	var metaData = MetaData.FromFile(metaFile);
	startByte = metaData.StartByteInclusive;
	padLength = metaData.PadLength;
	recordedChecksum = metaData.CheckSum;
	if(padLength != inFileSize) {
		Console.WriteLine($"{inFilename} is not the same length as the metadata specified pad");
		return;
	}
}
# endregion Handle Metadata

var endByte = startByte + (padLength - 1);
if(endByte > padFileSize) {
	Console.WriteLine($"{padFilename} starting at {startByte} is not long enough for {inFilename}");
	return;
}

# region Prompt user to verify
bool ContinuePrompt() {
	Console.Write("Continue? (y/n) ");
	var key = Console.ReadKey();
	if(Char.ToLower(key.KeyChar) != 'y') return false;
	Console.WriteLine();
	return true;
}

string RemoveXpadExtension(string filename) {
	if(filename.EndsWith(".xpad")) return filename.Substring(0, filename.Length - 5);
	return filename;
}

if(mode == "decode" && metaFileExists && startByteSpecified) {
	Console.WriteLine($"Warning: A metadata file exists. Are you sure you want to override it with starting byte {startByte}?");
	if(!ContinuePrompt()) return;
}
if(mode == "decode" &! metaFileExists) {
	Console.WriteLine("Warning: No metadata file found. Checksum will not be verified");
	if(!ContinuePrompt()) return;
}

var outFile = mode == "encode"
	? $"{inFilename}.xpad"
	: RemoveXpadExtension(inFilename);
if(File.Exists(outFile)) {
	Console.WriteLine($"{outFile} already exists, if you continue it will be overwritten");
	if(!ContinuePrompt()) return;
}

var padBytes = new byte[padLength];
using(var padFile = File.OpenRead(padFilename))
	padFile.Read(padBytes, 0, padBytes.Length);
var calculatedChecksum = Checksum.Fletcher16(padBytes);
if(recordedChecksum >= 0 && mode == "decode") {
	if(calculatedChecksum != recordedChecksum) {
		Console.WriteLine("Checksum mismatch. The pad file may have been tampered with or corrupted");
		Console.WriteLine("You may continue, but the output file will not be the same as the original");
		if(!ContinuePrompt()) return;
	}
}
# endregion Prompt user to verify

# region Check history
var padFingerPrint = PadHistory.GenerateFingerPrint(padFilename);
var padHistory = PadHistory.FromAppdata();
var padUsed = false;
foreach(var history in padHistory.Where(h => h.PadFingerPrint == padFingerPrint)) {
	if( (history.StartByteInclusive >= startByte && history.StartByteInclusive <= endByte) ||
		(history.EndByteInclusive >= startByte && history.EndByteInclusive <= endByte))
	{
		padUsed = true;
		Console.WriteLine($"This pad has already been used on {history.DateUsed}" + Environment.NewLine +
		$"\tbetween bytes {history.StartByteInclusive} and {history.EndByteInclusive}");
	}
}
if(padUsed) {
	Console.WriteLine("You may continue, but risk leaking date by reusing the pad");
	if(!ContinuePrompt()) return;
}
padHistory.Add(new PadHistory {
	PadFingerPrint = padFingerPrint,
	StartByteInclusive = startByte,
	EndByteInclusive = endByte,
	DateUsed = DateTime.Now
});
PadHistory.ToAppData(padHistory);
# endregion Check history

var xorBytes = Encode.Xor(inFilename, padBytes);
File.WriteAllBytes(outFile, xorBytes);
Console.WriteLine();
Console.WriteLine($"Produced {outFile} from {inFilename} using pad {padFilename}");
Console.WriteLine($"Starting at byte {startByte}");
Console.WriteLine($"Ending at byte {startByte + padLength - 1}");
Console.WriteLine($"Checksum: {calculatedChecksum}");

# region Write Metadata
var meta = new MetaData {
	StartByteInclusive = startByte,
	EndByteInclusive = endByte,
	CheckSum = calculatedChecksum
};
if(metaFileExists && !meta.MatchesFile(metaFile)) {
	Console.WriteLine($"{metaFile} doesn't match new data. Update the file?");
	if(!ContinuePrompt()) return;
}
meta.ToFile(metaFile);
# endregion Write Metadata

Console.WriteLine();
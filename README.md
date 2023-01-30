## Xpad
A tool to encode or decode a file using a one-time pad.

A pad file is required; it should be completely random and not used anywhere else.\
Memory of at least 2X the size of the file to be encoded or decoded is required.\
Write permission is required in the directory of the file to be encoded.

### Usage
Xpad {encode|decode} \<FileName> \<PadFile> [StartByte]\
STARTBYTE IS ONE-BASED INCLUSIVE and required for encoding. It is required for decoding if no .meta file exists.

### Encoding
Xpad will return the range of the pad used with the end byte being inclusive as well.
Files named '{filename}.xpad', and '{filename}.xpad.meta' will be generated in the same directory as the file being encoded.
The .xpad file is the encoded output.
The .xpad.meta file contains information for decoding, and should be kept/sent with the .xpad file.

### Decoding
If StartByte is not set, '{filename}.xpad.meta' is required to be in the same directory as the file being decoded.
It is highly recommended to use a .xpad.meta file rather than StartByte to ensure proper decoding.
If you don't receive a .xpad.meta file with the .xpad file, complain to whoever sent it.
The output file will be the name of the input file with '.xpad' extension removed.

When both encoding and decoding, a file named '.Xpad.history' will be generated or updated in your users AppData folder.\
This location will be included in the output from the command.\
It will keep a record of all the pad files and sections that have been used, and warn you of unintentional re-use.
Make sure to keep an up to date copy of '.Xpad.history' on any system used with the same pads.
Failure to keep the '.Xpad.history' file up to date could result in leaked information.

== Changelog ==
= 1.0 =
- Initial release

== Future Features ==
These are planned for a future release which may or may not happen.

- Ability to specify the output file and location
- Stream the pad to reduce the memory usage/requirements
- Calculate the checksum while reading the file
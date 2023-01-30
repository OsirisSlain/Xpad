class Checksum
{
	// Implements the Fletcher-16 checksum
	public static int Fletcher16(byte[] bytes)
	{
		var c1 = 0;
		var c2 = 0;
		foreach(byte b in bytes)
		{
			c1 = (c1 + b) % 255;
			c2 = (c2 + c1) % 255;
		}
		return (c2 << 8) | c1;
	}
}
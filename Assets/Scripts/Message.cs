public class Message
{
	public sbyte command;

	private myReader dis;

	private myWriter dos;

	public Message(int command)
	{
		this.command = (sbyte)command;
		dos = new myWriter();
	}

	public Message()
	{
		dos = new myWriter();
	}

	public Message(sbyte command)
	{
		this.command = command;
		dos = new myWriter();
	}

	public Message(sbyte command, sbyte[] data)
	{
		this.command = command;
		dis = new myReader(data);
	}
	public Message(sbyte command, sbyte[] data, int length)
	{
		this.command = command;
		// Tạo một mảng mới với kích thước đúng bằng length (nếu cần)
		if (data != null && length > 0 && length <= data.Length)
		{
			if (length < data.Length)
			{
				sbyte[] newData = new sbyte[length];
				System.Array.Copy(data, 0, newData, 0, length);
				dis = new myReader(newData);
			}
			else
			{
				dis = new myReader(data);
			}
		}
		else
		{
			dis = new myReader(data);
		}
	}
	public sbyte[] getData()
	{
		return dos.getSByteData();
	}

	public myReader reader()
	{
		return dis;
	}

	public myWriter writer()
	{
		return dos;
	}

	public int readInt3Byte()
	{
		return dis.readInt();
	}

	public void cleanup()
	{
	}
}

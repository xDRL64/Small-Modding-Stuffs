public static void GetBuffersFromDKModelFile(
	ref ModelFile file,
	ref byte[] bytesInFile, // unused in this fuction so useless
	ref uint vboVertexHandle, ref float[] vertexData,
	ref uint vboColorHandle, ref uint vboTexCoordHandle,
	ref List<uint> iboHandles, ref List<ushort[]> iboData,
	ref List<int> texturesGL,
	bool exportModel
){
	texturesGL.Clear();
	texturesGL.Add(-1);

	string str1 = Program.EXPORT + file.fileAddress + "//";

	if (exportModel)
		Directory.CreateDirectory(str1);

	List<ushort> ushortList = new List<ushort>();
	bool newTexture = false;
	float sScale = 0.0f;
	float tScale = 0.0f;
	ushort[] numArray = new ushort[32];
	F3DEX2 f3DeX2 = new F3DEX2();
	Texture texture = new Texture(0U, 0U, 0, 0);
	List<Texture> textures = new List<Texture>();
	byte[] textureTable = File.ReadAllBytes(".\\resources\\texturetable.bin");

	try
	{
		Hashtable hashtable = new Hashtable();   // works only with itself so useless
		List<uint> uintList1 = new List<uint>(); // used to catch last byte of every DL cmd 0x00 (only if 4 last bytes < 0xFF)
		List<uint> uintList2 = new List<uint>(); // unused in this fuction so useless

		// geo file props (structured by ReadModel) // props relative to the entier geo file
		int f3Dstart = file.F3DStart; // unused in this fuction so useless
		int f3Dend = file.F3DEnd; // unused in this fuction so useless
		int f3Dcommands = file.F3DCommands; // DL cmd count
		int vertStart = file.vertStart; // unused in this fuction so useless
		int vtCount = file.VTCount; // vertex count
		int textureCount = file.textureCount;
		F3DEX_VERT[] f3dVerts = file.f3d_verts; // vertices (object array [file.VTCount]) // object {x,y,z, u,v, r,g,b,a}
		List<byte[]> commands = file.commands; // list of all raw cmds as byte array [8] // list size == file.F3DCommands

		vertexData    = new float[file.VTCount * 3]; // probably gl buffer "positions"
		float[] data1 = new float[file.VTCount * 4]; // probably gl buffer "colors"
		float[] data2 = new float[file.VTCount * 2]; // probably gl buffer "uvs"
		int index1 = 0;
		int index2 = 0;
		int index3 = 0;
		foreach (F3DEX_VERT f3DexVert in f3dVerts)
		{
			vertexData[index1    ] = (float) f3DexVert.x;
			vertexData[index1 + 1] = (float) f3DexVert.y;
			vertexData[index1 + 2] = (float) f3DexVert.z;

			data1[index2    ] = f3DexVert.r;
			data1[index2 + 1] = f3DexVert.g;
			data1[index2 + 2] = f3DexVert.b;
			data1[index2 + 3] = f3DexVert.a;

			data2[index3    ] = (float) f3DexVert.u;
			data2[index3 + 1] = (float) f3DexVert.v;

			index1 += 3;
			index2 += 4;
			index3 += 2;
		}
		GL.GenBuffers(1, out vboVertexHandle);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vboVertexHandle);
		GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr) (vertexData.Length * 4), vertexData, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		GL.GenBuffers(1, out vboColorHandle);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vboColorHandle);
		GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr) (data1.Length * 4), data1, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		int num1 = 0;
		int num2 = 0;
		int num3 = 0;
		uint key = 0;
		for (int index4 = 0; index4 < file.F3DCommands; ++index4)
		{
			// current (loop step) DL command
			byte[] command1 = file.commands[index4]; 

			// current (loop step) DL command [Low Bytes Part] as 32bits value
			// -- -- -- -- xx xx xx xx
			uint num4 = (uint) ((int) command1[4] * 16777216 + (int) command1[5] * 65536 + (int) command1[6] * 256) + (uint) command1[7];

			// current (loop step) DL command [High Bytes Part without cmd num] as 24bits value
			// -- xx xx xx -- -- -- --
			uint w0 = (uint) ((int) command1[1] * 65536 + (int) command1[2] * 256) + (uint) command1[3];

			// IF (cmd is 0x00) AND (cmd [Low Bytes Part] <= 0xFF)
			if (command1[0] == (byte) 0 && num4 <= (uint) byte.MaxValue)
			{
				key = num4;
				int num5;
				if (uintList1.Contains(num4))
				{
					int useNumber = 0;
					foreach (int num6 in uintList1)
					{
						if ((long) num6 == (long) num4)
						++useNumber;
					}
					num5 = Core.CalcSectionVtxOffset(file.groups, file.sections, (int) num4, useNumber);
					hashtable[(object) num4] = (object) 0;
					uintList1.Add(num4);
				}
				else
				{
					hashtable.Add((object) num4, (object) 0);
					uintList1.Add(num4);
					num5 = Core.CalcSectionVtxOffset(file.groups, file.sections, (int) num4, 0);
				}
				num1 = num5 + num3;
				num2 = num1;
			}

			if (command1[0] == (byte) 223) // 0xDF G_ENDDL
				key = 0U;

			if (command1[0] == (byte) 253) // 0xFD G_SETTIMG
				F3DEX2.GL_G_SETTIMG(ref texture, w0, num4, file.commands[index4 + 2], ref newTexture, sScale, tScale);

			if (command1[0] == (byte) 243) // 0xF3 G_LOADBLOCK
				F3DEX2.GL_G_LOADBLOCK(ref texture, num4);

			if (command1[0] == (byte) 242) // 0xF2 G_SETTILESIZE
				F3DEX2.GL_G_SETTILESIZE(ref texture, num4);

			int num7 = (int) command1[0]; // unused in this fuction so useless
			int num8 = (int) command1[0]; // unused in this fuction so useless
			int num9 = (int) command1[0]; // unused in this fuction so useless

			if (command1[0] == (byte) 245) // 0xF5 G_SETTILE
				F3DEX2.GL_G_SETTILE(command1, ref texture);

			if (command1[0] == (byte) 240) // 0xF0 G_LOADTLUT
			{
				int palSize = (int) ((num4 << 8 >> 8 & 16773120U) >> 14) * 2 + 2;
				int index5 = (int) texture.id << 2;
				int pntr = ((int) textureTable[index5] << 24) + ((int) textureTable[index5 + 1] << 16) + ((int) textureTable[index5 + 2] << 8) + (int) textureTable[index5 + 3] + 1055824;
				RomHandler.DecompressFileToHDD(pntr);
				texture.pointer = (uint) pntr;
				texture.file = texture.pointer.ToString("x");
				byte[] bytesInFile1 = File.ReadAllBytes(Program.TMP + texture.file);
				texture.loadPalette(bytesInFile1, palSize);
				if (file.commands[index4 + 4][0] == (byte) 186)
				newTexture = true;
			}

			if (command1[0] == (byte) 215) // 0xD7 G_TEXTURE
			{
				sScale = (float) (num4 >> 16) / 65536f;
				tScale = (float) (num4 & (uint) ushort.MaxValue) / 65536f;
			}

			if ((int) command1[0] == F3DEX2.F3DEX2_VTX)
			{
				byte[] command2 = file.commands[index4]; // unused in this fuction so useless

				// current (loop step) DL command [Low Bytes Part] as 32bits value
				// -- -- -- -- xx xx xx xx
				int num10 = (int) command1[4] * 16777216 + (int) command1[5] * 65536 + (int) command1[6] * 256 + (int) command1[7];

				// current (loop step) DL command [High Bytes Part without cmd num] as 24bits value
				// -- xx xx xx -- -- -- --
				int num11 = (int) command1[1] * 65536 + (int) command1[2] * 256 + (int) command1[3];

				// memo :  01 0[N N]0 [II] [SS SS SS SS]

				// N
				byte num12 = (byte) ((uint) num11 >> 12 & (uint) byte.MaxValue);

				// I
				byte num13 = (byte) (((uint) num11 >> 1 & (uint) sbyte.MaxValue) - (uint) num12);

				if (num13 > (byte) 63)
					num13 = (byte) 63;

				// S / 16 == first vertex index
				uint num14 = ((uint) (num10 << 8) >> 8) / 16U;

				if (key != 0U)
				{
					int num15 = (int) num14 + (int) num12;
					if ((int) hashtable[(object) key] < num15)
						hashtable[(object) key] = (object) num15;
				}
				else
				{
					int num16 = (int) num14 + (int) num12;
					if (num3 < num16)
						num3 = num16;
				}

				uint num17;
				if (key != 0U)
				{
					num17 = num14 + (uint) num1;
					if ((long) num2 < (long) (num17 + (uint) num12))
						num2 = (int) num17 + (int) num12;
				}
				else
					num17 = num14 + (uint) num2;
				uint num18 = num17;

				try
				{
					for (int index6 = (int) num13; index6 < (int) num12 + (int) num13; ++index6)
					{
						if ((long) num18 < (long) file.f3d_verts.Length)
							numArray[index6] = (ushort) num18;
						++num18;
					}
				}
				catch (Exception ex){ }
			}
			if ((int) command1[0] == F3DEX2.F3DEX2_TRI1)
			{
				if (newTexture)
				{
				if (texturesGL.Count > 0)
				{
					ushort[] array = ushortList.ToArray();
					uint buffers = 0;
					GL.GenBuffers(1, out buffers);
					GL.BindBuffer(BufferTarget.ArrayBuffer, buffers);
					GL.BufferData<ushort>(BufferTarget.ArrayBuffer, (IntPtr) (array.Length * 2), array, BufferUsageHint.StaticDraw);
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
					iboHandles.Add(buffers);
					iboData.Add(array);
					ushortList.Clear();
				}
				newTexture = false;
				int glTextureName = 0;
				Core.LoadNewTexture(ref textureTable, textures, ref texture, ref glTextureName, sScale, tScale, exportModel, str1);
				//texturesGL[texturesGL.Count - 1] = glTextureName;
				texturesGL.Add(glTextureName);
							}
				short index7 = (short) ((int) command1[1] / 2);
				short index8 = (short) ((int) command1[2] / 2);
				short index9 = (short) ((int) command1[3] / 2);
				ushortList.Add(numArray[(int) index7]);
				ushortList.Add(numArray[(int) index8]);
				ushortList.Add(numArray[(int) index9]);
				data2[(int) numArray[(int) index7] * 2] = (float) file.f3d_verts[(int) numArray[(int) index7]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index7] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index7]].v * texture.textureHRatio;
				data2[(int) numArray[(int) index8] * 2] = (float) file.f3d_verts[(int) numArray[(int) index8]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index8] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index8]].v * texture.textureHRatio;
				data2[(int) numArray[(int) index9] * 2] = (float) file.f3d_verts[(int) numArray[(int) index9]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index9] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index9]].v * texture.textureHRatio;
			}
			if ((int) command1[0] == F3DEX2.F3DEX2_TRI2)
			{
				if (newTexture)
				{
				if (texturesGL.Count > 0)
				{
					ushort[] array = ushortList.ToArray();
					uint buffers = 0;
					GL.GenBuffers(1, out buffers);
					GL.BindBuffer(BufferTarget.ArrayBuffer, buffers);
					GL.BufferData<ushort>(BufferTarget.ArrayBuffer, (IntPtr) (array.Length * 2), array, BufferUsageHint.StaticDraw);
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
					iboHandles.Add(buffers);
					iboData.Add(array);
					ushortList.Clear();
				}
				newTexture = false;
				int glTextureName = 0;
				Core.LoadNewTexture(ref textureTable, textures, ref texture, ref glTextureName, sScale, tScale, exportModel, str1);
				texturesGL.Add(glTextureName);
				}
				short index10 = (short) ((int) command1[1] / 2);
				short index11 = (short) ((int) command1[2] / 2);
				short index12 = (short) ((int) command1[3] / 2);
				ushortList.Add(numArray[(int) index10]);
				ushortList.Add(numArray[(int) index11]);
				ushortList.Add(numArray[(int) index12]);
				data2[(int) numArray[(int) index10] * 2] = (float) file.f3d_verts[(int) numArray[(int) index10]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index10] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index10]].v * texture.textureHRatio;
				data2[(int) numArray[(int) index11] * 2] = (float) file.f3d_verts[(int) numArray[(int) index11]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index11] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index11]].v * texture.textureHRatio;
				data2[(int) numArray[(int) index12] * 2] = (float) file.f3d_verts[(int) numArray[(int) index12]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index12] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index12]].v * texture.textureHRatio;
				short index13 = (short) ((int) command1[5] / 2);
				short index14 = (short) ((int) command1[6] / 2);
				short index15 = (short) ((int) command1[7] / 2);
				ushortList.Add(numArray[(int) index13]);
				ushortList.Add(numArray[(int) index14]);
				ushortList.Add(numArray[(int) index15]);
				data2[(int) numArray[(int) index13] * 2] = (float) file.f3d_verts[(int) numArray[(int) index13]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index13] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index13]].v * texture.textureHRatio;
				data2[(int) numArray[(int) index14] * 2] = (float) file.f3d_verts[(int) numArray[(int) index14]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index14] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index14]].v * texture.textureHRatio;
				data2[(int) numArray[(int) index15] * 2] = (float) file.f3d_verts[(int) numArray[(int) index15]].u * texture.textureWRatio;
				data2[(int) numArray[(int) index15] * 2 + 1] = (float) file.f3d_verts[(int) numArray[(int) index15]].v * texture.textureHRatio;
			}
		}
		ushort[] array1 = ushortList.ToArray();
		uint buffers1 = 0;
		GL.GenBuffers(1, out buffers1);
		GL.BindBuffer(BufferTarget.ArrayBuffer, buffers1);
		GL.BufferData<ushort>(BufferTarget.ArrayBuffer, (IntPtr) (array1.Length * 2), array1, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		iboHandles.Add(buffers1);
		iboData.Add(array1);
		ushortList.Clear();
		GL.GenBuffers(1, out vboTexCoordHandle);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoordHandle);
		GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr) (data2.Length * 4), data2, BufferUsageHint.StaticDraw);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


		// model exporting part
		//

		if (!exportModel)
			return;


		string str2 = "# Exported with DK64Viewer" + Environment.NewLine + "mtllib dklevel.mtl" + Environment.NewLine;
				var in2 = 0;
				for (int index16 = 0; index16 < vertexData.Length; index16 += 3)
				{
					str2 += string.Format("v {0} {1} {2} {3} {4} {5} {6}" + Environment.NewLine,
										(object)vertexData[index16],
										(object)vertexData[index16 + 1],
										(object)vertexData[index16 + 2],
										Math.Max(0, Math.Min(255, (int)Math.Floor(data1[in2 + 0] * 256.0))),
										Math.Max(0, Math.Min(255, (int)Math.Floor(data1[in2 + 1] * 256.0))),
										Math.Max(0, Math.Min(255, (int)Math.Floor(data1[in2 + 2] * 256.0))),
										Math.Max(0, Math.Min(255, (int)Math.Floor(data1[in2 + 3] * 256.0))));
					in2 += 4;
				}
				for (int index17 = 0; index17 < data2.Length; index17 += 2)
		str2 = str2 + "vt " + (object) data2[index17] + " " + (object) (float) (((double) data2[index17 + 1] * -1.0)+ 1.0) + Environment.NewLine;
		for (int index18 = 0; index18 < iboHandles.Count; ++index18)
		{
		try
		{
			//if (texturesGL.Count > index18)
			{
			if (texturesGL[index18] == -1)
				str2 = str2 + "usemtl none" + Environment.NewLine;
			else
				str2 = str2 + "usemtl " + (object) texturesGL[index18] + Environment.NewLine;
			}
			for (int index19 = 0; index19 < iboData[index18].Length; index19 += 3)
			str2 = str2 + string.Format("f {0}/{0} {1}/{1} {2}/{2}", (object) ((int) iboData[index18][index19] + 1), (object) ((int) iboData[index18][index19 + 1] + 1), (object) ((int) iboData[index18][index19 + 2] + 1)) + Environment.NewLine;
		}
		catch (Exception ex)
		{
		}
		}
		string str3 = "newmtl none" + Environment.NewLine;
		List<int> intList = new List<int>();
		for (int index20 = 0; index20 < texturesGL.Count; index20++)
		{
		//if (!intList.Contains(texturesGL[index20]))
		{
			str3 = str3 + "newmtl " + (object) texturesGL[index20] + Environment.NewLine + "map_Kd " + (object) texturesGL[index20] + ".png" + Environment.NewLine;
			intList.Add(texturesGL[index20]);
		}
		}
		StreamWriter streamWriter1 = new StreamWriter(str1 + "dklevel.obj");
		streamWriter1.WriteLine(str2);
		streamWriter1.Close();
		StreamWriter streamWriter2 = new StreamWriter(str1 + "dklevel.mtl");
		streamWriter2.WriteLine(str3);
		streamWriter2.Close();
		int num19 = (int) MessageBox.Show("Export Complete");
	}
	catch (Exception ex)
	{
	}
}
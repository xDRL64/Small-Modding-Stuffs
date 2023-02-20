    public static ModelFile ReadModel(ref byte[] bytesInFile)
    {
		ModelFile modelFile = new ModelFile();
		if (((int) bytesInFile[8] << 8) + (int) bytesInFile[9] == (int) ushort.MaxValue)
		{
			modelFile.FileType = ModelFileType.Level;
			int sourceIndex = 12;
			byte[] numArray = new byte[6];
			Array.Copy((Array) bytesInFile, sourceIndex, (Array) numArray, 0, 6);
			modelFile.fileName = Core.convertHexString(numArray);
			int index1 = 64;
			modelFile.F3DStart = ((int) bytesInFile[index1] << 24) + ((int) bytesInFile[index1 + 1] << 16) + ((int) bytesInFile[index1 + 2] << 8) + (int) bytesInFile[index1 + 3];
			int index2 = index1 + 4 + 4;
			modelFile.vertStart = ((int) bytesInFile[index2] << 24) + ((int) bytesInFile[index2 + 1] << 16) + ((int) bytesInFile[index2 + 2] << 8) + (int) bytesInFile[index2 + 3];
			int index3 = index2 + 4;
			int num = ((int) bytesInFile[index3] << 24) + ((int) bytesInFile[index3 + 1] << 16) + ((int) bytesInFile[index3 + 2] << 8) + (int) bytesInFile[index3 + 3];
			modelFile.F3DCommands = (modelFile.vertStart - modelFile.F3DStart) / 8;
			modelFile.F3DEnd = modelFile.vertStart;
			modelFile.VTCount = (num - modelFile.vertStart) / 16;
			modelFile.textureCount = 0;
		}
		else if (((int) bytesInFile[128] << 8) + (int) bytesInFile[129] == (int) ushort.MaxValue && ((int) bytesInFile[130] << 8) + (int) bytesInFile[131] == (int) ushort.MaxValue)
		{
			modelFile.FileType = ModelFileType.Level;

			// DL Start offset
			int index4 = 52;
			modelFile.F3DStart = ((int) bytesInFile[index4] << 24) + ((int) bytesInFile[index4 + 1] << 16) + ((int) bytesInFile[index4 + 2] << 8) + (int) bytesInFile[index4 + 3];
			
			// vertex Start offset
			int index5 = index4 + 4;
			modelFile.vertStart = ((int) bytesInFile[index5] << 24) + ((int) bytesInFile[index5 + 1] << 16) + ((int) bytesInFile[index5 + 2] << 8) + (int) bytesInFile[index5 + 3];
			
			// vertex End offset
			int index6 = index5 + 4 + 4;
			int num1 = ((int) bytesInFile[index6] << 24) + ((int) bytesInFile[index6 + 1] << 16) + ((int) bytesInFile[index6 + 2] << 8) + (int) bytesInFile[index6 + 3];
			
			modelFile.F3DCommands = (modelFile.vertStart - modelFile.F3DStart) / 8;
			modelFile.F3DEnd = modelFile.vertStart;
			modelFile.VTCount = (num1 - modelFile.vertStart) / 16;
			modelFile.textureCount = 0;

			// section Start offset
			int num2 = ((int) bytesInFile[88] << 24) + ((int) bytesInFile[89] << 16) + ((int) bytesInFile[90] << 8) + (int) bytesInFile[91];
			
			// section End offset
			int num3 = ((int) bytesInFile[92] << 24) + ((int) bytesInFile[93] << 16) + ((int) bytesInFile[94] << 8) + (int) bytesInFile[95];
			
			// getting sections
			//

			int num4 = num2 + 4; // +4 to jump section count // 4 first bytes [xx xx xx xx] of all section block
			for (int index7 = 0; num4 + index7 < num3; index7 += 28)
				modelFile.sections.Add(new ModelSection()
				{
					sectionID        = ((int) bytesInFile[num4 + index7     ] << 8) + (int) bytesInFile[num4 + index7 + 1],
					meshID           = ((int) bytesInFile[num4 + index7 + 2 ] << 8) + (int) bytesInFile[num4 + index7 + 3],
					vertStart        = ((int) bytesInFile[num4 + index7 + 8 ] << 8) + (int) bytesInFile[num4 + index7 + 9],
					unknownVertStart = ((int) bytesInFile[num4 + index7 + 10] << 8) + (int) bytesInFile[num4 + index7 + 11],
					vertStart2       = ((int) bytesInFile[num4 + index7 + 12] << 8) + (int) bytesInFile[num4 + index7 + 13],
					vertStart3       = ((int) bytesInFile[num4 + index7 + 14] << 8) + (int) bytesInFile[num4 + index7 + 15],
					numVerts1        = ((int) bytesInFile[num4 + index7 + 16] << 8) + (int) bytesInFile[num4 + index7 + 17],
					numVertsUnknown  = ((int) bytesInFile[num4 + index7 + 18] << 8) + (int) bytesInFile[num4 + index7 + 19],
					numVerts2        = ((int) bytesInFile[num4 + index7 + 20] << 8) + (int) bytesInFile[num4 + index7 + 21],
					numVerts3        = ((int) bytesInFile[num4 + index7 + 22] << 8) + (int) bytesInFile[num4 + index7 + 23]
				});

			List<int> intList = new List<int>();
			foreach (ModelSection section in modelFile.sections)
			{
				if (!intList.Contains(section.sectionID))
				{
					intList.Add(section.sectionID);
					modelFile.groups.Add(new ModelSectionGroup()
					{
						sectionID = section.sectionID
					});
				}
			}

			modelFile.groups = modelFile.groups.OrderBy<ModelSectionGroup, int>((Func<ModelSectionGroup, int>) (o => o.sectionID)).ToList<ModelSectionGroup>();
			
			foreach (ModelSectionGroup group in modelFile.groups)
			{
				foreach (ModelSection section in modelFile.sections)
				{
					if (section.sectionID == group.sectionID)
						group.endOffset += section.numVerts1 + section.numVerts2 + section.numVerts3 + section.numVertsUnknown;
				}
			}
		}
		else
		{
			modelFile.FileType = ModelFileType.Character;
			modelFile.fileName = "Character Format?";
			modelFile.vertStart = 40;
			modelFile.F3DStart = modelFile.vertStart;
			while (modelFile.F3DStart + 16 < bytesInFile.Length && (int) bytesInFile[modelFile.F3DStart] + (int) bytesInFile[modelFile.F3DStart + 1] + (int) bytesInFile[modelFile.F3DStart + 2] + (int) bytesInFile[modelFile.F3DStart + 3] != 231)
			modelFile.F3DStart += 16;
			int f3Dstart = modelFile.F3DStart;
			modelFile.F3DEnd = f3Dstart;
			while (modelFile.F3DEnd + 8 < bytesInFile.Length && bytesInFile[modelFile.F3DEnd] != (byte) 16)
			modelFile.F3DEnd += 8;
			modelFile.F3DCommands = (modelFile.F3DEnd - modelFile.F3DStart) / 8;
			modelFile.VTCount = (f3Dstart - modelFile.vertStart) / 16;
			modelFile.textureCount = 0;
		}

		// getting vertex data (x,y,z ,u,v, r,g,b,a)
		//

		modelFile.f3d_verts = new F3DEX_VERT[modelFile.VTCount];
		Core.RipVerts(ref bytesInFile, ref modelFile.f3d_verts, modelFile.VTCount, modelFile.vertStart);

		// getting DL commands
		//

		int num5 = 0;
		int f3Dstart1 = modelFile.F3DStart;
		for (; num5 < modelFile.F3DCommands; ++num5)
		{
			byte[] numArray = new byte[8];
			for (int index = 0; index < 8; ++index)
			numArray[index] = bytesInFile[f3Dstart1 + index];
			f3Dstart1 += 8;
			modelFile.commands.Add(numArray);
		}
		return modelFile;
    }
    public static int CalcSectionVtxOffset(
		List<ModelSectionGroup> groups,
		List<ModelSection> sections,
		int meshIndex,
		int useNumber
	)
    {
		int num1 = 0;
		int num2 = 0;

		foreach (ModelSection section in sections)
		{
			if (section.meshID == meshIndex)
			{
				num1 = section.sectionID;
				if (useNumber == 0)
					num2 = section.vertStart;
				if (useNumber == 1 && section.unknownVertStart != 0)
					num2 = section.unknownVertStart;
				if (useNumber == 1 && section.unknownVertStart == 0)
					num2 = section.vertStart2;
				if (useNumber == 2)
				{
					num2 = section.vertStart3;
					break;
				}
				break;
			}
		}

		int num3 = 0;

		if (num1 > 0)
		{
			foreach (ModelSectionGroup group in groups)
			{
				if (num1 > group.sectionID)
					num3 += group.endOffset;
				else
					break;
			}
		}

		return num2 + num3;
    }
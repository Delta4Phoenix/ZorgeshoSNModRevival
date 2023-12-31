﻿using Common.Configuration;

namespace SeamothStorageSlots
{
	class ModConfig: Config
	{
		[Field.Range(max: 8)]
		readonly int extraStorageSlotsOffset = 8;

		public int SlotsOffset => extraStorageSlotsOffset < 3? 0: extraStorageSlotsOffset;
	}
}
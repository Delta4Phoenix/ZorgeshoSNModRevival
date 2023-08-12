using System.Collections.Generic;
using Common;

namespace GravTrapImproved
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		static readonly string configPath = Paths.FormatFileName("types_config", "json");
		static List<TypesConfig.TechTypeList> TechTypeLists => Main.typesConfig.techTypeLists;

		static void UpdateLists()
		{
			Main.typesConfig.Reinit();
			Main.typesConfig.save(configPath); // typesConfig is read-only, so we need to use full path

			foreach (var g in FindObjectsOfType<Gravsphere>())
			{
				g.OnPickedUp(g.pickupable);
				g.OnDropped(g.pickupable);
			}
		}

		static TypesConfig.TechTypeList GetList(string name)
		{
			var list = TechTypeLists.Find(list => list.name.ToLower().StartsWith(name));
			list ??= TechTypeLists.Find(list => list.name.ToLower().Contains(name));

			if (list == null)
				$"Tech type list '{name}' not found!".OnScreen();

			return list;
		}

		public void Gti_addtech(string listName, TechType techType)
		{
			if (GetList(listName) is not TypesConfig.TechTypeList list)
				return;

			if (list.Add(techType))
			{
				UpdateLists();
				$"Tech type '{techType}' added to '{list.name}' list".OnScreen();
			}
		}

		public void Gti_removetech(string listName, TechType techType)
		{
			if (GetList(listName) is not TypesConfig.TechTypeList list)
				return;

			if (list.Remove(techType))
			{
				UpdateLists();
				$"Tech type '{techType}' removed from '{list.name}' list".OnScreen();
			}
		}

		[Command(caseSensitive = true, combineArgs = true)]
		public void Gti_addlist(string listName)
		{
			TechTypeLists.Add(new (listName));
			UpdateLists();
			$"Tech type list '{listName}' added".OnScreen();
		}

		public void Gti_removelist(string listName)
		{
			if (GetList(listName) is not TypesConfig.TechTypeList list)
				return;

			TechTypeLists.Remove(list);
			UpdateLists();
			$"Tech type list '{list.name}' removed".OnScreen();
		}
	}
}
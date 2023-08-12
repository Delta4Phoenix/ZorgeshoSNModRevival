using Common.Configuration;

namespace Common
{
	static partial class Mod
	{
		public static C Init<C>() where C: Config
		{
			Init();
			return LoadConfig<C>();
		}


		public static C LoadConfig<C>(string name = Config.defaultName, Config.LoadOptions loadOptions = Config.LoadOptions.Default) where C: Config
		{
			C config = Config.tryLoad<C>(name, loadOptions);

			return config;
		}
	}
}
﻿using System;
using System.Linq;
using System.Reflection;

namespace Common.Configuration.Actions
{
	using Harmony;
	using Reflection;

	// action for updating optional patches
	// updates all patches if used without params or only selected patches
	class UpdateOptionalPatches: Config.Field.IAction, Config.Field.IActionArgs
	{
		public object[] args { get; set; }

		public void action()
		{
			Debug.assert(args.IsNullOrEmpty() || args.All(arg => arg is Type));

			if (args.IsNullOrEmpty())
				OptionalPatches.Update();
			else
				args.ForEach(arg => OptionalPatches.Update(arg as Type));
		}
	}

	// action for calling config method (not for use with nested classes)
	// first arg is the name of the method, other args if for calling method
	class CallMethod: Config.Field.IAction, Config.Field.IActionArgs, Config.IRootConfigInfo
	{
		MethodInfo targetMethod;

		public object[] args
		{
			get => _args;

			set
			{
				Debug.assert(!value.IsNullOrEmpty());

				_args = value;
				argsMethod = _args.Length == 1? null: _args.SubArray(1);
			}
		}
		object[] _args, argsMethod;

		Config rootConfig;
		public void SetRootConfig(Config config) => rootConfig = config;

		public void action()
		{
			Debug.assert(!args.IsNullOrEmpty() && args[0] is string);
			Debug.assert(rootConfig != null);

			targetMethod ??= rootConfig.GetType().method(args[0] as string);
			Debug.assert(targetMethod != null);

			targetMethod.Invoke(rootConfig, argsMethod);
		}
	}
}
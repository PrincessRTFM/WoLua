using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

using PrincessRTFM.WoLua.Constants;
using PrincessRTFM.WoLua.Lua.Api;

namespace PrincessRTFM.WoLua.Lua.Docs;

internal static class LuadocGenerator {
	private static readonly Assembly ownAssembly = typeof(LuadocGenerator).Assembly;
	public const string ApiDefinitionFileName = "api.lua";
	public static string ApiDefinitionFilePath => Path.Combine(Service.Configuration.BasePath, ApiDefinitionFileName);

	private readonly struct NamedApiType {
		internal readonly Type type;
		internal readonly string? name;
		internal NamedApiType(Type type, string? name) {
			this.type = type;
			this.name = name;
		}
	}

	public static Task<string> GenerateLuadocAsync() => Task.Run(GenerateLuadoc);
	public static string GenerateLuadoc() {
		static bool typeIsApi(Type t) => !t.IsAbstract && t.GetCustomAttribute<MoonSharpUserDataAttribute>(true) is not null;

		Type apiBase = typeof(ApiBase);
		Queue<NamedApiType> apis = new(typeof(ScriptContainer)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(p => typeIsApi(p.PropertyType))
			.Select(p => new NamedApiType(p.PropertyType, p.GetCustomAttribute<LuaGlobalAttribute>()?.Name))
		);
		HashSet<Type> documented = new();

		StringBuilder docs = new(1024 * 64); // the first run of the dumper produced ~20k, so 64k should be good for a bit
		docs.AppendLine("---@meta");
		docs.AppendLine();
		docs.AppendLine();
		docs.AppendLine();
		docs.AppendLine();

		addType(docs, apiBase);
		docs.AppendLine();

		while (apis.TryDequeue(out NamedApiType api)) {
			if (api.type is null) // unpossible!
				continue;

			if (documented.Contains(api.type))
				continue;
			documented.Add(api.type);

			addType(docs, api.type, api.name);

			IEnumerable<NamedApiType> childApis = api.type
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => typeIsApi(p.PropertyType))
				.Select(p => new NamedApiType(p.PropertyType, p.GetCustomAttribute<LuaGlobalAttribute>()?.Name))
				.Concat(api.type
					.GetMethods(BindingFlags.Public | BindingFlags.Instance)
					.Where(m => typeIsApi(m.ReturnType))
					.Select(m => new NamedApiType(m.ReturnType, null))
				);
			foreach (NamedApiType subApi in childApis)
				apis.Enqueue(subApi);
		}

		return docs.ToString();
	}

	private static void addType(StringBuilder docs, Type type, string? name = null) {
		MoonSharpHideMemberAttribute[] hideMembers = type.GetCustomAttributes<MoonSharpHideMemberAttribute>().ToArray();
		bool includeMemberInDocs(MemberInfo m) {
			return m.GetCustomAttribute<MoonSharpHiddenAttribute>() is null
				&& m.GetCustomAttribute<MoonSharpVisibleAttribute>() is not { Visible: false }
				&& m.GetCustomAttribute<SkipDocAttribute>() is null
				&& m is not MethodBase { IsSpecialName: true }
				&& m.DeclaringType?.Assembly == ownAssembly
				&& !hideMembers.Any(a => a.MemberName == m.Name);
		}
		static bool methodIsToString(MethodInfo m) => m.Name is "ToString" && m.GetParameters().Length is 0 && m.ReturnType == typeof(string);
		static bool methodIsMeta(MethodInfo m) => methodIsToString(m) || m.GetCustomAttributes<MoonSharpUserDataMetamethodAttribute>().Any();
		IEnumerable<PropertyInfo> properties = type
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(includeMemberInDocs);
		IEnumerable<MethodInfo> methods = type
			.GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(includeMemberInDocs)
			.Where(m => !methodIsMeta(m));
		IEnumerable<MethodInfo> metamethods = type
			.GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(methodIsMeta);

		Service.Log.Info($"[{LogTag.GenerateDocs}] Documenting API type {type.Name}");

		docs.Append($"---@class {type.Name}");
		if (type.BaseType?.IsAssignableTo(typeof(ApiBase)) is true)
			docs.Append($": {type.BaseType.Name}");
		docs.AppendLine();

		if (type.GetCustomAttribute<LuaDocAttribute>() is LuaDocAttribute typeDetails) {
			foreach (string line in typeDetails.Lines)
				docs.AppendLine($"---{line}");
		}

		addProps(docs, properties);

		addMetamethods(docs, metamethods);

		if (!string.IsNullOrWhiteSpace(name))
			docs.AppendLine($"{name} = {{}}");
		else
			docs.AppendLine($"local {type.Name} = {{}}");
		docs.AppendLine();

		addMethods(docs, !string.IsNullOrWhiteSpace(name) ? name : type.Name, methods);

		docs.AppendLine();
	}

	private static void addMetamethods(StringBuilder docs, IEnumerable<MethodInfo> metamethods) {
		foreach (MethodInfo m in metamethods) {
			IEnumerable<string> overloads = m.GetCustomAttributes<MoonSharpUserDataMetamethodAttribute>().Select(a => a.Name);

			if (m.Name is "ToString") { // special handling for stringification because it's not recognised as an operation, so it has to be left out
				Service.Log.Info($"[{LogTag.GenerateDocs}] Skipping stringification metamethod {m.DeclaringType!.Name}.{m.Name}");
				//docs.AppendLine("---@operator tostring:string");
			}
			else {
				foreach (string overload in overloads) {
					Service.Log.Info($"[{LogTag.GenerateDocs}] Documenting metamethod {m.DeclaringType!.Name}.{m.Name} for {overload}");
					if (overload == Metamethod.FunctionCall) { // apparently, you should be using `@overload` to indicate call signatures for classes, rather than `@operator call`
						docs.Append("---@overload fun(");
						ParameterInfo[] parameters = m.GetParameters().Skip(1).ToArray();
						if (parameters.Length > 0) {
							StringBuilder args = new(10 * parameters.Length);
							foreach (ParameterInfo p in parameters) {
								if (args.Length > 0)
									args.Append(", ");
								args.Append($"{p.Name}: ");
								args.Append(p.GetCustomAttribute<AsLuaTypeAttribute>()?.LuaName ?? getLuaType(p.ParameterType));
							}
							docs.Append(args);
						}
						docs.Append("): ");
						docs.AppendLine(m.ReturnParameter.GetCustomAttribute<AsLuaTypeAttribute>()?.LuaName ?? getLuaType(m.ReturnType));
					}
					else { // all other metamethods are handled basically the same way: `OPERATION(OTHER_TYPE):RETURN_TYPE` or just `OPERATION:RETURN_TYPE` if it's a single-operand method
						ParameterInfo[] args = m.GetParameters();
						if (args.Length == 0 || args[0].ParameterType != m.DeclaringType)
							continue;
						docs.Append($"---@operator {overload[2..]}");
						if (args.Length == 1) {
							// nop
						}
						else if (args.Length == 2) {
							docs.Append($"({args[1].GetCustomAttribute<AsLuaTypeAttribute>()?.LuaName ?? getLuaType(args[1].ParameterType)})");
						}
						docs.AppendLine($":{m.ReturnParameter.GetCustomAttribute<AsLuaTypeAttribute>()?.LuaName ?? getLuaType(m.ReturnType)}");
					}
				}
			}
		}
	}

	private static void addProps(StringBuilder docs, IEnumerable<PropertyInfo> properties) {
		static bool canWrite(PropertyInfo p) => p.SetMethod?.IsPublic is true;
		foreach (PropertyInfo p in properties) {
			Service.Log.Info($"[{LogTag.GenerateDocs}] Documenting property {p.DeclaringType!.Name}.{p.Name}");
			docs.Append("---@field ");
			// there's no way to mark a field as read-only in the annotation, at least not yet
			//if (!canWrite(p))
			//	docs.Append("readonly ");
			docs.Append($"public {p.Name} ");
			docs.Append(p.GetCustomAttribute<AsLuaTypeAttribute>()?.LuaName ?? getLuaType(p.PropertyType));

			// XXX temporary workaround for marking fields as read-only (will NOT be enforced by linting!) since there's no official way to tag them as such
			if (!canWrite(p))
				docs.Append(" [READONLY]");

			if (p.GetCustomAttribute<LuaDocAttribute>() is LuaDocAttribute detail)
				docs.Append($" {detail.Description.Replace("\n", "<br/>")}");
			else if (p.PropertyType.IsAssignableTo(typeof(ApiBase)))
				docs.Append($" Provides access to the {p.Name.Replace("Api", string.Empty)} API");

			docs.AppendLine();
		}
	}

	private static void addMethods(StringBuilder docs, string table, IEnumerable<MethodInfo> methods) {
		foreach (MethodInfo m in methods) {
			Service.Log.Info($"[{LogTag.GenerateDocs}] Documenting method {m.DeclaringType!.Name}.{m.Name}");
			if (m.GetCustomAttribute<LuaDocAttribute>() is LuaDocAttribute usage) {
				foreach (string line in usage.Lines)
					docs.AppendLine($"---{line}");
			}

			ParameterInfo[] parameters = m.GetParameters();
			ParameterInfo returns = m.ReturnParameter;
			foreach (ParameterInfo p in parameters) {
				docs.Append($"---@param {p.Name}");
				if (p.HasDefaultValue || p.IsOptional)
					docs.Append('?');
				docs.Append(' ');
				docs.Append(p.GetCustomAttribute<AsLuaTypeAttribute>()?.LuaName ?? getLuaType(p.ParameterType));
				if (p.GetCustomAttribute<LuaDocAttribute>() is LuaDocAttribute detail)
					docs.Append($" {detail.Description}");
				docs.AppendLine();
			}

			docs.Append("---@return ");
			docs.Append(returns.GetCustomAttribute<AsLuaTypeAttribute>()?.LuaName ?? getLuaType(returns.ParameterType));
			if (returns.GetCustomAttribute<LuaDocAttribute>() is LuaDocAttribute retDetail)
				docs.Append($" # {retDetail.Description}");
			docs.AppendLine();

			if (m.GetCustomAttribute<ObsoleteAttribute>() is not null)
				docs.AppendLine("---@deprecated");

			docs.AppendLine($"function {table}.{m.Name}({string.Join(", ", parameters.Select(p => p.Name))}) end");
			docs.AppendLine();
		}
	}

	private static string getLuaType(Type originalType) {
		Type realType = originalType;
		if (originalType.IsGenericType && originalType.GetGenericTypeDefinition() == typeof(Nullable<>))
			realType = originalType.GenericTypeArguments[0];
		LuaType result = LuaTypeExtensions.FromType(originalType);
		string generatedName;
		if (realType == typeof(DynValue)) {
#if DEBUG
			Service.Log.Warning($"[{LogTag.GenerateDocs}] Automatically generating return type descriptor for DynValue! This should probably use [AsLuaType()] to override!");
#endif
			generatedName = result.LuaName();
		}
		else {
			generatedName = result.LuaName(realType.Name);
		}

		Service.Log.Info($"[{LogTag.GenerateDocs}] Translated C# type " + (realType == originalType ? realType.Name : $"Nullable<{realType.Name}>") + $" to lua type {(ushort)result} \"{generatedName}\"");
		return generatedName;
	}
}

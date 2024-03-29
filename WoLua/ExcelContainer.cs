using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace PrincessRTFM.WoLua;

public static class ExcelContainer {
	private static Lazy<ExcelSheet<T>> init<T>() where T: ExcelRow {
		return new(() => {
			if (Service.DataManager is null)
				throw new InvalidOperationException("Cannot load excel sheet without DataManager instance", new NullReferenceException("Service.DataManager does not exist"));
			ExcelSheet<T> sheet = Service.DataManager.GetExcelSheet<T>()
				?? throw new InvalidOperationException($"Requested sheet type {typeof(T).FullName} does not exist");
			return sheet;
		}, LazyThreadSafetyMode.ExecutionAndPublication);
	}

	private static readonly Lazy<ExcelSheet<Title>> titles = init<Title>();
	public static ExcelSheet<Title> Titles => titles.Value;
}

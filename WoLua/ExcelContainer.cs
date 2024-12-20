using System;
using System.Threading;

using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace PrincessRTFM.WoLua;

public static class ExcelContainer {
	private static Lazy<ExcelSheet<T>> init<T>() where T: struct, IExcelRow<T> {
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

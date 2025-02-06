using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FermenterUtilities;

[BepInPlugin(_modUid, _modDescription, _modVersion)]
[HarmonyPatch]
public class FermenterUtilitiesPlugin : BaseUnityPlugin
{
	internal const string _modVersion = "1.1.2";
	internal const string _modDescription = "Fermenter Utilities";
	internal const string _modUid = "kompjoefriek.FermenterUtilities";

	internal static new ManualLogSource Logger;

	private Harmony _harmony;

	private static ConfigEntry<bool> _configEnableMod;
	private static ConfigEntry<bool> _configEnableLogging;
	private static ConfigEntry<bool> _configShowPercentage;
	private static ConfigEntry<bool> _configShowColorPercentage;
	private static ConfigEntry<bool> _configShowTime;
	private static ConfigEntry<bool> _configCustomFermentTime;
	private static ConfigEntry<bool> _configNoCover;
	private static ConfigEntry<int> _configAmountOfDecimals;
	private static ConfigEntry<int> _configNewFermentTime;

	private static object _logObject;
	private static DateTime _lastLogTime;

	private void Awake()
	{
		Logger = base.Logger;

		// Normal configs
		_configEnableMod = Config.Bind("1 - Global", "Enable Mod", true, "Enable or disable this mod");
		_configEnableLogging = Config.Bind("1 - Global", "Enable Mod Logging", false, "Enable or disable logging for this mod");

		_configShowPercentage = Config.Bind("2 - Progress", "Show Percentage", true, "Shows the fermentation progress as a percentage when you hover over the fermenter");
		_configShowColorPercentage = Config.Bind("2 - Progress", "Show Percentage Color", true, "Makes it so the percentage changes color depending on the progress");
		_configAmountOfDecimals = Config.Bind("2 - Progress", "Show Percentage Decimal Places", 2, "The amount of decimal places to show for the percentage");
		_configShowTime = Config.Bind("2 - Progress", "Show Time", false, "Show the time when done");

		_configCustomFermentTime = Config.Bind("3 - Time", "Custom Time", false, "Enables the custom time for fermentation");
		_configNewFermentTime = Config.Bind("3 - Time", "Fermentation Time", 5, "The amount of minutes fermentation takes (Default 40)");

		_configNoCover = Config.Bind("4 - Cover", "Work Without Cover", false, "Allow the Fermenter to work without any cover");

		// Deprecated config
		Dictionary<ConfigDefinition, string> orphanedEntries = Traverse.Create(Config).Property("OrphanedEntries").GetValue<Dictionary<ConfigDefinition, string>>();
		if (orphanedEntries != null)
		{
			ConfigDefinition deprecatedConfigShowColorPercentageDefinition = new ConfigDefinition("2 - Progress", "Show Color Percentage");
			ConfigDefinition deprecatedConfigAmountOfDecimalsDefinition = new ConfigDefinition("2 - Progress", "Amount of Decimal Places");
			bool hasDeprecatedConfigShowColorPercentage = orphanedEntries.TryGetValue(deprecatedConfigShowColorPercentageDefinition, out string deprecatedConfigShowColorPercentageValue);
			bool hasDeprecatedConfigAmountOfDecimals = orphanedEntries.TryGetValue(deprecatedConfigAmountOfDecimalsDefinition, out string deprecatedConfigAmountOfDecimalsValue);

			bool didConfigChange = false;
			if (hasDeprecatedConfigShowColorPercentage)
			{
				_configShowColorPercentage.SetSerializedValue(deprecatedConfigShowColorPercentageValue);
				didConfigChange |= RemoveDeprecatedConfigDefinition(ref orphanedEntries, deprecatedConfigShowColorPercentageDefinition);
			}
			if (hasDeprecatedConfigAmountOfDecimals)
			{
				_configAmountOfDecimals.SetSerializedValue(deprecatedConfigAmountOfDecimalsValue);
				didConfigChange |= RemoveDeprecatedConfigDefinition(ref orphanedEntries, deprecatedConfigAmountOfDecimalsDefinition);
			}

			if (_configEnableLogging.Value)
			{
				foreach (KeyValuePair<ConfigDefinition, string> entry in orphanedEntries)
				{
					Logger.LogWarning("Orphaned config: " + entry.Key.ToString());
				}
			}

			if (didConfigChange)
			{
				Config.Save();
			}
		}

		_lastLogTime = DateTime.Now;

		if (!_configEnableMod.Value) { return; }

		_harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
	}

	private void OnDestroy()
	{
		_harmony?.UnpatchSelf();
	}

	private static bool RemoveDeprecatedConfigDefinition(ref Dictionary<ConfigDefinition, string> entries, ConfigDefinition definition)
	{
		if (!entries.Remove(definition))
		{
			Logger.LogWarning("Failed to remove deprecated config: " + definition.ToString());
			return false;
		}
		return true;
	}

	private static string GetColorStringFromPercentage(double percentage)
	{
		if (!_configShowColorPercentage.Value) return "white";
		if (percentage >= 75.0) return "green";
		if (percentage >= 50.0) return "yellow";
		if (percentage >= 25.0) return "orange";
		return "red";
	}

	private static string GetValueAsColoredString(string color, double value)
	{
		return $"<color={color}>{value}%</color>";
	}

	private static string FormatSecondsAsString(double seconds)
	{
		int hours = (int)(seconds / 3600.0);
		double remainingSeconds = seconds - (hours * 3600.0);
		int mins = (int)(remainingSeconds / 60.0);
		remainingSeconds -= mins * 60.0;
		int secs = (int)(remainingSeconds);
		if (hours > 1) return $"{hours:D2}:{mins:D2}:{secs:D2}";
		return $"{mins:D2}:{secs:D2}";
	}

	// Log message at Info log level, but only once per second for the same object
	private static void LogInfoThrottled(object obj, string message)
	{
		if (_configEnableLogging.Value)
		{
			if (object.Equals(_logObject, obj))
			{
				double secondsSinceLastLog = (DateTime.Now - _lastLogTime).TotalSeconds;
				if (secondsSinceLastLog >= 1)
				{
					_logObject = obj;
					_lastLogTime = DateTime.Now;
					Logger.LogInfo(message);
				}
			}
			else
			{
				_logObject = obj;
				_lastLogTime = DateTime.Now;
				Logger.LogInfo(message);
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Fermenter), "Awake")]
	public static void FermenterAwake_Patch(Fermenter __instance)
	{
		if (__instance == null) return;

		if (_configCustomFermentTime.Value) __instance.m_fermentationDuration = _configNewFermentTime.Value * 60;
		if (_configNoCover.Value) Traverse.Create(__instance).Field("m_updateCoverTimer").SetValue(-100f);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Fermenter), nameof(Fermenter.GetHoverText))]
	public static string FermenterGetHoverText_Patch(string __result, Fermenter __instance)
	{
		if (__instance == null) return __result;

		object STATUS_FERMENTING = __instance.GetType().GetNestedType("Status", BindingFlags.NonPublic).GetField("Fermenting").GetValue(__instance);
		object fermenterStatus = Traverse.Create(__instance).Method("GetStatus").GetValue<object>();
		if (!fermenterStatus.Equals(STATUS_FERMENTING)) return __result;

		if (_configShowPercentage.Value)
		{
			double fermentationTime = Traverse.Create(__instance).Method("GetFermentationTime").GetValue<double>();

			// Don't change hover text when item is done
			if (fermentationTime >= __instance.m_fermentationDuration) return __result;

			double percentage = (fermentationTime / __instance.m_fermentationDuration) * 100.0;
			string color = GetColorStringFromPercentage(percentage);
			string newString = GetValueAsColoredString(color, Math.Round(percentage, _configAmountOfDecimals.Value, MidpointRounding.AwayFromZero));
			string logMessage = "Fermenter percentage: " + percentage + ", time since start: " + fermentationTime + ", fermentation duration: " + __instance.m_fermentationDuration;

			if (_configShowTime.Value)
			{
				double timeRemaining = __instance.m_fermentationDuration - fermentationTime;
				string formattedTime = FormatSecondsAsString(timeRemaining);
				newString += ", " + formattedTime;
				logMessage += "\nFermenter timeRemaining: " + timeRemaining + " (seconds), formatted: " + formattedTime;
			}
			LogInfoThrottled(__instance, logMessage);

			string replaceString = Localization.instance.Localize("$piece_fermenter_fermenting");
			return __result.Replace(replaceString, newString);
		}
		else if (_configShowTime.Value)
		{
			double fermentationTime = Traverse.Create(__instance).Method("GetFermentationTime").GetValue<double>();

			// Don't change hover text when item is done
			if (fermentationTime >= __instance.m_fermentationDuration) return __result;

			double timeRemaining = __instance.m_fermentationDuration - fermentationTime;
			string formattedTime = FormatSecondsAsString(timeRemaining);

			LogInfoThrottled(__instance, "Fermenter timeRemaining: " + timeRemaining + " (seconds), formatted: " + formattedTime);

			string replaceString = Localization.instance.Localize("$piece_fermenter_fermenting");
			return __result.Replace(replaceString, formattedTime);
		}

		return __result;
	}
}

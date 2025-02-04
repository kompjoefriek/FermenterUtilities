using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FermenterUtilities;

[BepInPlugin(_modUid, _modDescription, _modVersion)]
[HarmonyPatch]
public class FermenterUtilitiesPlugin : BaseUnityPlugin
{
	internal const string _modVersion = "1.1.0";
	internal const string _modDescription = "Fermenter Utilities";
	internal const string _modUid = "kompjoefriek.FermenterUtilities";

	internal static new ManualLogSource Logger;

	private static ConfigEntry<bool> _configEnableMod;
	private static ConfigEntry<bool> _configShowPercentage;
	private static ConfigEntry<bool> _configShowColorPercentage;
	private static ConfigEntry<bool> _configShowTime;
	private static ConfigEntry<bool> _configCustomFermentTime;
	private static ConfigEntry<bool> _configNoCover;
	private static ConfigEntry<int> _configAmountOfDecimals;
	private static ConfigEntry<int> _configNewFermentTime;

	private void Awake()
	{
		Logger = base.Logger;
		_configEnableMod = Config.Bind("1 - Global", "Enable Mod", true, "Enable or disable this mod");
		if (!_configEnableMod.Value) { return; }

		_configShowPercentage = Config.Bind("2 - Progress", "Show Percentage", true, "Shows the fermentation progress as a percentage when you hover over the fermenter");
		_configShowColorPercentage = Config.Bind("2 - Progress", "Show Color Percentage", true, "Makes it so the percentage changes color depending on the progress");
		_configAmountOfDecimals = Config.Bind("2 - Progress", "Amount of Decimal Places", 2, "The amount of decimal places to show");
		_configShowTime = Config.Bind("2 - Progress", "Show Time", false, "Show the time when done");

		_configCustomFermentTime = Config.Bind("3 - Time", "Custom Time", false, "Enables the custom time for fermentation");
		_configNewFermentTime = Config.Bind("3 - Time", "Fermentation Time", 5, "The amount of minutes fermentation takes (Default 40)");

		_configNoCover = Config.Bind("4 - Cover", "Work Without Cover", false, "Allow the Fermenter to work without any cover");

		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
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

	private static string FormatMinutesAsString(double minutes)
	{
		int hours = (int)(minutes / 60.0);
		int mins = (int)(minutes % 60.0);
		int secs = (int)((minutes - Math.Floor(minutes)) * 60.0);
		if (hours > 1) return $"{hours:D2}:{mins:D2}:{secs:D2}";
		return $"{mins:D2}:{secs:D2}";
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
		if (_configShowPercentage.Value && fermenterStatus.Equals(STATUS_FERMENTING))
		{
			double fermentationTime = Traverse.Create(__instance).Method("GetFermentationTime").GetValue<double>();
			double percentage = fermentationTime / __instance.m_fermentationDuration * 100.0;
			string color = GetColorStringFromPercentage(percentage);
			string newString = GetValueAsColoredString(color, Math.Round(percentage, _configAmountOfDecimals.Value, MidpointRounding.AwayFromZero));

			if (_configShowTime.Value)
			{
				double timeRemaining = (__instance.m_fermentationDuration - fermentationTime) / 60.0;
				newString += $", {FormatMinutesAsString(timeRemaining)}";
			}

			string replaceString = Localization.instance.Localize("$piece_fermenter_fermenting");
			return __result.Replace(replaceString, newString);
		}

		return __result;
	}
}

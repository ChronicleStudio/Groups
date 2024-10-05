using Groups.API.Group;
using Groups.GUI.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Groups.GUI
{
	internal class PlayersGUI : HudElement
	{
		private const EnumDialogArea GUI_Position = EnumDialogArea.LeftTop;
		private const int DisplayQTY = 25;


		public override string ToggleKeyCombinationCode => "playersgui";
		public const string DisplayKey = "playerList";
		public string DisplayText = "This is a piece of text at the center of your screen - Enjoy!";
		public int page = 0;
		public Sort sort = Sort.Default;
		public SortModifier sortModifier = SortModifier.Ascending;



		public PlayersGUI(ICoreClientAPI capi) : base(capi)
		{
			SetupDialog();
			RegisterHotKeys();

			capi.Event.PlayerJoin += UpdateList;
			capi.Event.PlayerLeave += UpdateList;
			capi.Event.RegisterGameTickListener((args) => { UpdateList(); }, 500);
		}
		private void SetupDialog()
		{
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(GUI_Position);

			// Just a simple 300x100 pixel box with 40 pixels top spacing for the title bar
			ElementBounds textBounds = ElementBounds.Fixed(0, 40, 300, 100);

			// Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
			bgBounds.BothSizing = ElementSizing.FitToChildren;
			bgBounds.WithChildren(textBounds);
			SingleComposer = capi.Gui.CreateCompo("playersGUI", dialogBounds)
				.AddShadedDialogBG(bgBounds)
				.AddDialogTitleBar("Player Standings", OnTitleBarCloseClicked)
				.AddRichtext(DisplayText, CairoFont.WhiteDetailText(), textBounds, DisplayKey)
				.Compose();
		}
		private void RegisterHotKeys()
		{
			capi.Input.RegisterHotKey(ToggleKeyCombinationCode + "None", "Opens Player Standing List", GlKeys.Tab, HotkeyType.GUIOrOtherControls);
			capi.Input.RegisterHotKey(ToggleKeyCombinationCode + "Shift", "Opens Player Standing List", GlKeys.Tab, HotkeyType.GUIOrOtherControls, false, false, true);
			capi.Input.RegisterHotKey(ToggleKeyCombinationCode + "CTRL", "Opens Player Standing List", GlKeys.Tab, HotkeyType.GUIOrOtherControls, false, true, false);
			capi.Input.RegisterHotKey(ToggleKeyCombinationCode + "CTRLShift", "Opens Player Standing List", GlKeys.Tab, HotkeyType.GUIOrOtherControls, false, true, true);
			capi.Input.SetHotKeyHandler(ToggleKeyCombinationCode + "None", ToggleGUI);
			capi.Input.SetHotKeyHandler(ToggleKeyCombinationCode + "Shift", ToggleGUI);
			capi.Input.SetHotKeyHandler(ToggleKeyCombinationCode + "CTRL", ToggleGUI);
			capi.Input.SetHotKeyHandler(ToggleKeyCombinationCode + "CTRLShift", ToggleGUI);
		}

		private bool ToggleGUI(KeyCombination comb)
		{

			page = comb.Ctrl ? 0 : page;
			sort = comb.Ctrl && !comb.Shift ? Utilities.IncrementSort(sort) : sort;
			sortModifier = comb.Ctrl && comb.Shift ? Utilities.IncrementSortModifier(sortModifier) : sortModifier;
			_ = !comb.Ctrl && comb.Shift ?
				IsOpened() ? TryClose() : TryOpen() :
				IsOpened() ? TryOpen() : TryOpen();
			return true;
		}

#nullable enable
		private void UpdateList(IPlayer? notUsed = null)
		{
			_ = this.IsOpened() ? TryOpen() : TryClose();
		}

		public override bool TryOpen()
		{
			SingleComposer.GetRichtext(DisplayKey).SetNewText(GeneratateDisplayText(), CairoFont.WhiteDetailText());
			SingleComposer.ReCompose();
			return base.TryOpen();
		}
		public override bool TryClose()
		{
			page = 0;
			return base.TryClose();
		}

		private void OnTitleBarCloseClicked()
		{
			TryClose();
			String.Format("$0 $1", DisplayQTY, page);
		}


		public string GeneratateDisplayText()
		{
			List<KeyValuePair<IPlayer, sbyte?>> standings = SortStandings(GetAllPlayersStandings(capi));
			String result = $"Players ({standings.Count})"
				+ (standings.Count > DisplayQTY ? $"<font align=\"center\">Page {(page + 1)} of {Math.Ceiling((double)standings.Count / DisplayQTY)}</font>" : "")
				+ "<font align=\"right\">Standing</font><br><br>";

			foreach (KeyValuePair<IPlayer, sbyte?> standing in GetPage(standings))
			{
				result += standing.Key.PlayerName;
				string color = GroupSettings.GetRank(standing.Value) switch
				{
					GroupRank.RankW => GUIColors.RED,
					GroupRank.RankE => GUIColors.ORANGE,
					GroupRank.RankN => GUIColors.YELLOW,
					GroupRank.RankA => GUIColors.BLUE,
					_ => GUIColors.GREEN,
				};
				result += $"<font align=\"right\" color=\"{color}\">{String.Format("{0:0.00}", (standing.Value / 100d))}</font><br>";
			}

			return result;
		}
		public List<KeyValuePair<IPlayer, sbyte?>> GetPage(List<KeyValuePair<IPlayer, sbyte?>> standings)
		{
			List<KeyValuePair<IPlayer, sbyte?>> displayStandings;
			if (standings.Count > DisplayQTY)
			{
				try { displayStandings = standings.GetRange(page * DisplayQTY, DisplayQTY); }
				catch (ArgumentException)
				{
					displayStandings = standings.GetRange(page * DisplayQTY, standings.Count % DisplayQTY);
				}
				page++;
				if (page >= Math.Ceiling((double)standings.Count / DisplayQTY))
				{
					page = 0;
				}
			}
			else
			{
				displayStandings = standings;
				page = 0;
			}
			return displayStandings;
		}
		public List<KeyValuePair<IPlayer, sbyte?>> SortStandings(List<KeyValuePair<IPlayer, sbyte?>> standings) => sort switch
		{
			Sort.Player => sortModifier == SortModifier.Ascending ? standings.OrderBy(standing => standing.Key.PlayerName).ToList() : Reverse(standings.OrderBy(standing => standing.Key.PlayerName).ToList()),
			Sort.Standing => sortModifier == SortModifier.Ascending ? Reverse(standings.OrderBy(standing => standing.Value).ToList()) : standings.OrderBy(standing => standing.Value).ToList(),
			_ => sortModifier == SortModifier.Ascending ? standings : Reverse(standings),
		};
		private static List<T> Reverse<T>(List<T> standings) { standings.Reverse(); return standings; }
		public static List<KeyValuePair<IPlayer, sbyte?>> GetAllPlayersStandings(ICoreClientAPI capi)
		{
			List<KeyValuePair<IPlayer, sbyte?>> standings = new();

			List<IPlayer> players = capi.World.AllOnlinePlayers.ToList();
			Dictionary<string, sbyte?> standingsDictionary = capi.ModLoader.GetModSystem<PlayerStandingsNetwork>().GetStandings();

			foreach (IPlayer player in players)
			{


				sbyte? playerStandings = standingsDictionary.Get(player.PlayerUID);
				if (playerStandings != null)
				{
					standings.Add(new KeyValuePair<IPlayer, sbyte?>(player, playerStandings));
				}
			}

			return standings;
		}

	}
}

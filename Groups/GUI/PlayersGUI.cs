using Groups.Standings;
using Groups.Standings.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

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

            capi.Event.RegisterGameTickListener((args) => { UpdateList(); }, 30000);
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
        }


        public string GeneratateDisplayText()
        {
            List<KeyValuePair<IPlayer, IStandings>> standings = SortStandings(PlayersStandings.GetAllPlayersStandings(capi));
            String result = "Players (" + standings.Count + ")"
                + (standings.Count > DisplayQTY ? "<font align=\"center\">Page " + (page + 1) + " of " + Math.Ceiling((double)standings.Count / DisplayQTY) + "</font>" : "")
                + "<font align=\"right\">Standing</font><br><br>";

            foreach (KeyValuePair<IPlayer, IStandings> standing in GetPage(standings))
            {
                result += standing.Key.PlayerName;
                string color;
                if (standing.Value.Standings < -0.30)
                {
                    color = GUIColors.RED;
                }
                else if (standing.Value.Standings > 0.30)
                {
                    color = GUIColors.GREEN;
                }
                else
                {
                    color = GUIColors.YELLOW;
                }
                result += "<font align=\"right\" color=\"" + color + "\">" + String.Format("{0:0.00}", standing.Value.Standings) + "</font>";
                result += "<br>";
            }

            return result;
        }
        public List<KeyValuePair<IPlayer, IStandings>> GetPage(List<KeyValuePair<IPlayer, IStandings>> standings)
        {
            List<KeyValuePair<IPlayer, IStandings>> displayStandings;
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
        public List<KeyValuePair<IPlayer, IStandings>> SortStandings(List<KeyValuePair<IPlayer, IStandings>> standings)
        {
            List<KeyValuePair<IPlayer, IStandings>> _standings;
            switch (sort)
            {
                case Sort.Player:
                    _standings = standings.OrderBy(standing => standing.Key.PlayerName).ToList();
                    _standings = sortModifier == SortModifier.Ascending ? _standings : Reverse(_standings);
                    break;
                case Sort.Standing:
                    _standings = standings.OrderBy(standing => standing.Value.Standings).ToList();
                    _standings = sortModifier == SortModifier.Ascending ? Reverse(_standings) : _standings;
                    break;
                default:
                    _standings = sortModifier == SortModifier.Ascending ? standings : Reverse(standings);
                    break;
            }
            return _standings;
        }

        private List<KeyValuePair<IPlayer, IStandings>> Reverse(List<KeyValuePair<IPlayer, IStandings>> standings) { standings.Reverse(); return standings; }

    }
}

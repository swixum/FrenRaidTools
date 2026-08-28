using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private static readonly string[] SourceNames = ["Auto", "IINACT", "ACT"];

    private void DrawParser()
    {
        var link = _plugin.Parser;

        PageHeader("Parser", link.Source.Length > 0 ? $"on {link.Source}" : "no feed");

        DrawParserCard(link);

        Widgets.SectionHeader("Parser");
        Widgets.ListBegin();

        var on = C.ParserOn;
        if (Widgets.RowCheckClick("Use a parser", "Read off IINACT or ACT", ref on))
        {
            C.ParserOn = on;
            Touch();
        }

        if (!on)
        {
            Widgets.RowNote("Off. Calls run off the game client.");
            Widgets.ListEnd();
            return;
        }

        var source = C.ParserSource;
        if (Widgets.RowCombo("Source", "IINACT if running, else ACT",
                ref source, SourceNames, 160f))
        {
            C.ParserSource = source;
            link.Retry();
            Touch();
        }

        Widgets.RowBegin("Address", "Where OverlayPlugin listens", Theme.S(280f));
        var address = C.ParserAddress;
        if (ImGui.InputText("##addr", ref address, 200))
        {
            C.ParserAddress = address;
            Touch();
        }
        Widgets.Tip("ws://127.0.0.1:10501/ws is the usual one.\n"
                    + "Point it at another PC with its address instead of 127.0.0.1.");
        Widgets.RowEnd();

        Widgets.ListEnd();

        ImGui.Spacing();
        if (Widgets.AccentButton("Reconnect"))
        {
            link.Retry();
            _plugin.Runtime.RetryFeed();
        }
        Widgets.Tip("Drops both links and starts over");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Reset address"))
        {
            C.ParserAddress = "ws://127.0.0.1:10501/ws";
            link.Retry();
            _plugin.Runtime.RetryFeed();
            Touch();
        }

        Widgets.SectionHeader("In IINACT");
        Faint("Links straight to it. On its Parser tab:");
        StepToggle(1, "Disable Damage Shield Estimates", false, "or shields read zero.");
        StepToggle(2, "End encounter automatically after leaving combat", true);
        StepWord(3, "Player name: leave it as", "YOU", Theme.Accent, ".");
        Faint("The network log file is for uploads, not this.");

        Widgets.SectionHeader("In ACT");
        Step(1, "Run ACT, with its FFXIV plugin.");
        Step(2, "Plugins > OverlayPlugin.dll > WSServer > Start.");
        Step(3, "Options > Main Table/Encounters > Idle Limit: 180.");
        Faint("Lower splits a fight at downtime.");
    }

    private static void Faint(string text) =>
        ImGui.TextColored(Theme.V(Theme.Muted), text);

    private static void StepNumber(int n)
    {
        ImGui.TextColored(Theme.V(Theme.Accent), $"{n}");
        ImGui.SameLine(0, Theme.S(10f));
    }

    private static void Step(int n, string text)
    {
        StepNumber(n);
        ImGui.TextUnformatted(text);
    }

    private static void StepToggle(int n, string setting, bool wanted, string why = "")
    {
        StepNumber(n);
        ImGui.TextUnformatted(setting + ":");
        ImGui.SameLine(0, Theme.S(5f));
        ImGui.TextColored(Theme.V(wanted ? Theme.Good : Theme.Danger), wanted ? "ON" : "OFF");

        if (why.Length == 0) return;
        ImGui.SameLine(0, Theme.S(5f));
        Faint(why);
    }

    private static void StepWord(int n, string before, string word, uint color, string after)
    {
        StepNumber(n);
        ImGui.TextUnformatted(before);
        ImGui.SameLine(0, Theme.S(5f));
        ImGui.TextColored(Theme.V(color), word);

        if (after.Length == 0) return;
        ImGui.SameLine(0, 0);
        ImGui.TextUnformatted(after);
    }

    private void DrawParserCard(ParserLink link)
    {
        Widgets.CardBegin();

        Dot(link.Dot);
        ImGui.SameLine(0, Theme.S(9f));
        ImGui.AlignTextToFramePadding();

        var headline = link.State switch
        {
            ParserState.Live => $"Connected to {link.Source}",
            ParserState.Looking => "Waiting on lines",
            ParserState.Broken => "Not connected",
            _ => "Turned off",
        };
        ImGui.TextColored(Theme.V(link.Dot), headline);

        ImGui.Indent(Theme.S(21f));
        ImGui.TextColored(Theme.V(Theme.Muted), link.Detail);

        var feed = _plugin.Runtime;
        ImGui.TextColored(
            Theme.V(feed.FeedUp ? Theme.Muted : Theme.Warn),
            feed.FeedUp
                ? $"Calls feed: {feed.FeedDetail}"
                : C.ParserOn
                    ? $"Calls feed: {feed.FeedDetail} Retrying, calls run off the game client meanwhile."
                    : "Calls feed: off, calls run off the game client.");
        ImGui.Unindent(Theme.S(21f));

        ImGui.Dummy(new Vector2(0, Theme.S(3f)));

        Feed("IINACT", link.IinactFound ? "running" : "not found", link.IinactFound);
        ImGui.SameLine(0, Theme.S(6f));
        Feed("ACT", link.SocketOpen ? "connected" : "closed", link.SocketOpen);
        ImGui.SameLine(0, Theme.S(6f));

        var silence = link.Silence;
        Widgets.Chip("Lines", link.Lines > 0 ? $"{link.Lines}" : "none",
            link.Lines > 0 ? Theme.Good : Theme.Muted);

        if (silence >= 0)
        {
            ImGui.SameLine(0, Theme.S(6f));
            Widgets.Chip("Last", $"{silence:0}s ago", silence > 15 ? Theme.Warn : Theme.Good);
        }

        Widgets.CardEnd();
    }

    private static void Feed(string name, string state, bool good) =>
        Widgets.Chip(name, state, good ? Theme.Good : Theme.Muted);

    private static void Dot(uint color)
    {
        var radius = Theme.S(5f);
        var size = new Vector2(radius * 2f, ImGui.GetFrameHeight());
        var p = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();
        var center = p + new Vector2(radius, size.Y * 0.5f);

        dl.AddCircleFilled(center, radius * 1.9f, Theme.Wash(color, 0x33));
        dl.AddCircleFilled(center, radius, color);
        ImGui.Dummy(size);
    }
}

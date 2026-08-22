using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace FrenRaidTools.Ui;

public partial class MainWindow
{
    private void DrawParser()
    {
        var link = _plugin.Parser;

        PageHeader("Parser", link.Source.Length > 0 ? $"on {link.Source}" : "no feed");

        DrawParserCard(link);

        Widgets.SectionHeader("Feed");
        Widgets.ListBegin();

        var on = C.ParserOn;
        if (Widgets.RowCheckClick("Use a parser", "Read the fight off IINACT or ACT", ref on))
        {
            C.ParserOn = on;
            Touch();
        }

        if (!on)
        {
            Widgets.RowNote("Off. Calls run off the game client only.");
            Widgets.ListEnd();
            return;
        }

        var prefer = C.ParserPreferIinact;
        if (Widgets.RowCheckClick("Take IINACT first", "Use IINACT when it is running, ACT otherwise", ref prefer))
        {
            C.ParserPreferIinact = prefer;
            Touch();
        }

        Widgets.RowBegin("ACT address", "Where OverlayPlugin listens", Theme.S(280f));
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
        if (Widgets.AccentButton("Try again")) link.Retry();

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Reset address"))
        {
            C.ParserAddress = "ws://127.0.0.1:10501/ws";
            link.Retry();
            Touch();
        }

        DrawParserHelp(link);
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

    private void DrawParserHelp(ParserLink link)
    {
        if (link.State == ParserState.Live || !C.ParserOn) return;

        Widgets.SectionHeader("Getting it green");
        Widgets.ListBegin();
        Widgets.RowNote("On IINACT: install it and let it start. Nothing else to do.");
        Widgets.RowNote("On ACT: Plugins, OverlayPlugin.dll, WSServer, Start.");
        Widgets.RowNote("Check the port matches the one WSServer prints.");
        Widgets.ListEnd();
    }
}

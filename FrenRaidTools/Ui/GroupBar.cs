using Dalamud.Bindings.ImGui;
using FrenRaidTools.Engine;

namespace FrenRaidTools.Ui;

internal sealed class GroupBar
{
    private readonly string _id;
    private string _typed = "";
    private bool _naming;
    private bool _focus;
    private Roster? _doomed;
    private string _doomedName = "";

    public GroupBar(string id) => _id = id;

    private string DeletePopup => "##groupdelete" + _id;

    public void Forget()
    {
        _naming = false;
        _focus = false;
        _typed = "";
        _doomed = null;
        _doomedName = "";
    }

    public bool Draw(Configuration config, double now)
    {
        var changed = false;
        var asked = false;

        Widgets.ListBegin();
        changed |= OfferRow(config);
        changed |= _naming ? NameRow(config) : PickRow(config);
        changed |= ActionRow(config, ref asked);
        Widgets.ListEnd();

        if (asked)
        {
            _doomed = config.Roles;
            _doomedName = Label(config, config.ActiveSetup);
            ImGui.OpenPopup(DeletePopup);
        }

        if (Widgets.ConfirmPopup(DeletePopup, $"Delete {_doomedName}?", "Delete", "Keep it"))
            changed |= Delete(config);

        if (changed) config.Save(now);
        return changed;
    }

    private bool Delete(Configuration config)
    {
        var doomed = _doomed;
        Forget();

        var before = config.Setups.Count;
        config.ActiveSetup = Roster.Drop(config.Setups, doomed, config.ActiveSetup);
        if (config.Setups.Count == before) return false;

        Picked();
        return true;
    }

    private bool OfferRow(Configuration config)
    {
        var name = Offer.Name(config);
        if (name.Length == 0) return false;

        var taken = false;

        Widgets.RowBegin($"Use {name}?", "It matches the party you are in",
            Widgets.ButtonWidth("Use", "Not now") + Theme.S(6f), id: "offer" + _id,
            edgeColor: Theme.Accent);

        if (Widgets.AccentButton("Use")) taken = Offer.Take(config);

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Not now")) Offer.Dismiss();

        Widgets.RowEnd();
        return taken;
    }

    private bool PickRow(Configuration config)
    {
        var names = Labels(config);
        var index = Math.Clamp(config.ActiveSetup, 0, names.Length - 1);
        var picker = Theme.S(168f);

        Widgets.RowBegin("Group", Fullness(config.Roles),
            picker + Theme.S(6f) + Widgets.ButtonWidth("Rename"), id: "pick" + _id);

        ImGui.SetNextItemWidth(picker);
        var hit = ImGui.Combo("##group" + _id, ref index, names, names.Length);
        if (hit)
        {
            config.ActiveSetup = Math.Clamp(index, 0, config.Setups.Count - 1);
            Picked();
        }

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Rename")) StartNaming(config);
        Widgets.Tip("Give this group a name");

        Widgets.RowEnd();
        return hit;
    }

    private bool NameRow(Configuration config)
    {
        var box = Theme.S(168f);

        Widgets.RowBegin("Name", "Enter to keep it",
            box + Theme.S(12f) + Widgets.ButtonWidth("Save", "Cancel"), id: "name" + _id);

        ImGui.SetNextItemWidth(box);
        if (_focus)
        {
            ImGui.SetKeyboardFocusHere();
            _focus = false;
        }

        var buffer = _typed;
        var typed = ImGui.InputText("##groupname" + _id, ref buffer, 40,
            ImGuiInputTextFlags.EnterReturnsTrue);
        _typed = buffer;

        ImGui.SameLine(0, Theme.S(6f));
        var keep = Widgets.AccentButton("Save") || typed;

        ImGui.SameLine(0, Theme.S(6f));
        var drop = Widgets.GhostButton("Cancel");

        Widgets.RowEnd();

        if (drop)
        {
            Forget();
            return false;
        }

        if (!keep) return false;

        var name = _typed.Trim();
        Forget();
        if (name.Length == 0) return false;

        config.Roles.Name = name;
        return true;
    }

    private bool ActionRow(Configuration config, ref bool asked)
    {
        var changed = false;
        var many = config.Setups.Count > 1;

        Widgets.RowBegin("Saved groups", $"{config.Setups.Count} on file",
            Widgets.ButtonWidth("New", "Copy", "Delete") + Theme.S(12f), id: "acts" + _id);

        if (Widgets.GhostButton("New"))
        {
            config.Setups.Add(new Roster { Name = Fresh(config) });
            config.ActiveSetup = config.Setups.Count - 1;
            StartNaming(config);
            Picked();
            changed = true;
        }
        Widgets.Tip("Start an empty group");

        ImGui.SameLine(0, Theme.S(6f));
        if (Widgets.GhostButton("Copy"))
        {
            var copy = config.Roles.Copy();
            copy.Name = Roster.FreeName(config.Setups, (config.Roles.Name ?? "Group") + " copy");
            config.Setups.Add(copy);
            config.ActiveSetup = config.Setups.Count - 1;
            StartNaming(config);
            Picked();
            changed = true;
        }
        Widgets.Tip("Copy this group with its names");

        ImGui.SameLine(0, Theme.S(6f));
        if (!many) ImGui.BeginDisabled();
        if (Widgets.DangerButton("Delete")) asked = true;
        if (!many) ImGui.EndDisabled();
        if (!many) Widgets.Tip("The last group stays");

        Widgets.RowEnd();
        return changed;
    }

    private static void Picked() => Offer.Dismiss();

    private void StartNaming(Configuration config)
    {
        _typed = config.Roles.Name ?? "";
        _naming = true;
        _focus = true;
    }

    private static string[] Labels(Configuration config)
    {
        var names = new string[config.Setups.Count];
        for (var i = 0; i < names.Length; i++) names[i] = Label(config, i);
        return names;
    }

    private static string Label(Configuration config, int index)
    {
        var name = GroupMatch.Label(config.Setups, index);
        return name.Length > 0 ? name : "this group";
    }

    private static string Fresh(Configuration config)
    {
        for (var n = config.Setups.Count + 1; n <= config.Setups.Count + 100; n++)
        {
            var tried = $"Group {n}";
            if (!Roster.NameTaken(config.Setups, tried)) return tried;
        }

        return Roster.FreeName(config.Setups, "Group");
    }

    private static string Fullness(Roster roster) =>
        roster.Filled == 0 ? "no names yet"
        : roster.Complete ? "full party"
        : $"{roster.Filled} of {Slots.Count} set";
}

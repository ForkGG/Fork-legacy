using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Fork.Logic.Manager;
using Fork.Logic.Model;
using Fork.Logic.Persistence;

namespace Fork.Logic.RoleManagement;

public class RoleUpdater
{
    private static readonly object locker = new();
    private readonly bool checkForFake;
    private readonly Regex regexBanlistAddNew = new(": Banned ([A-Za-z0-9_]+):(.*)$"); //This needs an additional method
    private readonly Regex regexBanlistAddOld = new(": Banned player ([A-Za-z0-9_]+)$");
    private readonly Regex regexBanlistRemoveNew = new(": Unbanned ([A-Za-z0-9_]+)$");
    private readonly Regex regexBanlistRemoveOld = new(": Unbanned player ([A-Za-z0-9_]+)$");
    private readonly Regex regexOPsAddNew = new(": Made ([A-Za-z0-9_]+) a server operator$");
    private readonly Regex regexOPsAddOld = new(": Opped ([A-Za-z0-9_]+)$");
    private readonly Regex regexOPsRemoveNew = new(": Made ([A-Za-z0-9_]+) no longer a server operator$");
    private readonly Regex regexOPsRemoveOld = new(": De-opped ([A-Za-z0-9_]+)$");

    private readonly Regex regexWhitelistAdd = new(": Added ([A-Za-z0-9_]+) to the whitelist$");
    private readonly Regex regexWhitelistRemove = new(": Removed ([A-Za-z0-9_]+) from the whitelist$");
    private readonly ObservableCollection<Player> playerList;

    private readonly Regex regexAdd;
    private readonly Regex regexRemove;

    public RoleUpdater(RoleType roleType, ObservableCollection<Player> playerList, ServerVersion version)
    {
        this.playerList = playerList;
        switch (roleType)
        {
            case RoleType.WHITELIST:
                regexAdd = regexWhitelistAdd;
                regexRemove = regexWhitelistRemove;
                break;

            case RoleType.BAN_LIST:
                if (version.CompareTo(new ServerVersion { Version = "1.13" }) < 0)
                {
                    regexAdd = regexBanlistAddOld;
                    regexRemove = regexBanlistRemoveOld;
                }
                else
                {
                    regexAdd = regexBanlistAddNew;
                    checkForFake = true;
                    regexRemove = regexBanlistRemoveNew;
                }

                break;

            case RoleType.OP_LIST:
                if (version.CompareTo(new ServerVersion { Version = "1.13" }) < 0)
                {
                    regexAdd = regexOPsAddOld;
                    regexRemove = regexOPsRemoveOld;
                }
                else
                {
                    regexAdd = regexOPsAddNew;
                    regexRemove = regexOPsRemoveNew;
                }

                break;

            default: return;
        }
    }

    public async void HandleOutputLine(string line)
    {
        Match addMatch = regexAdd.Match(line);
        if (addMatch.Success)
        {
            if (checkForFake)
            {
                if (line.EndsWith("<--[HERE]"))
                {
                    return;
                }
            }

            string name = addMatch.Groups[1].Value;
            Console.WriteLine("Player " + name + " added to a list");
            Player p = await PlayerManager.Instance.GetPlayer(name);
            Application.Current.Dispatcher.Invoke(() => playerList.Add(p), DispatcherPriority.Background);
            return;
        }

        Match removeMatch = regexRemove.Match(line);
        if (removeMatch.Success)
        {
            string name = removeMatch.Groups[1].Value;
            Console.WriteLine("Player " + name + " removed from a list");
            Player p = playerList.FirstOrDefault(player => player.Name.Equals(name));

            if (p == null)
            {
                Console.WriteLine("Player " + name + " should be removed from a list, where he isn't on");
                return;
            }

            Application.Current.Dispatcher.Invoke(() => playerList.Remove(p), DispatcherPriority.Background);
        }
    }

    public static async void InitializeList(RoleType roleType, ObservableCollection<Player> playerList, Server server)
    {
        List<string> names;
        string path;
        if (server.Version.CompareTo(new ServerVersion { Version = "1.7.5" }) <= 0)
        {
            path = Path.Combine(new DirectoryInfo(Path.Combine(App.ServerPath, server.Name)).FullName,
                roleType.TxtName());
            switch (roleType)
            {
                case RoleType.WHITELIST:
                    names = FileReader.ReadWhiteListTxT(path);
                    break;
                case RoleType.OP_LIST:
                    names = FileReader.ReadOPListTxT(path);
                    break;
                case RoleType.BAN_LIST:
                    names = FileReader.ReadBanListTxT(path);
                    break;
                default:
                    throw new ArgumentException("RoleUpdater.InitializeList() is ignoring an enum value!");
            }
        }
        else
        {
            path = Path.Combine(new DirectoryInfo(Path.Combine(App.ServerPath, server.Name)).FullName,
                roleType.JsonName());
            switch (roleType)
            {
                case RoleType.WHITELIST:
                    names = FileReader.ReadWhiteListJson(path);
                    break;
                case RoleType.OP_LIST:
                    names = FileReader.ReadOPListJson(path);
                    break;
                case RoleType.BAN_LIST:
                    names = FileReader.ReadBanListJson(path);
                    break;
                default:
                    throw new ArgumentException("RoleUpdater.InitializeList() is ignoring an enum value!");
            }
        }

        lock (locker)
        {
            //Add names to PlayerList
            Dispatcher viewDisp = Application.Current.Dispatcher;
            foreach (string name in names)
            {
                Player p = PlayerManager.Instance.GetPlayer(name).Result;
                if (p != null)
                {
                    viewDisp.Invoke(() => playerList.Add(p), DispatcherPriority.Background);
                }
            }
        }
    }
}
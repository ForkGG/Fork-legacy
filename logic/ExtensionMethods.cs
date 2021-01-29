using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Windows;
using System.Windows.Media;
using Fork.Logic.Logging;
using Fork.Logic.Model;
using Fork.Logic.Model.PluginModels;
using Fork.Logic.Model.ServerConsole;
using Fork.Logic.RoleManagement;
using Fork.Logic.Utils;

namespace Fork.Logic
{
    public static class ExtensionMethods
    {
        public static string JsonName(this RoleType roleType)
        {
            return roleType switch
            {
                RoleType.WHITELIST => "whitelist.json",
                RoleType.BAN_LIST => "banned-players.json",
                RoleType.OP_LIST => "ops.json",
                _ => throw new ArgumentException("Undefined enum entry in RoleType.JsonName()")
            };
        }

        public static string TxtName(this RoleType roleType)
        {
            return roleType switch
            {
                RoleType.WHITELIST => "white-list.txt",
                RoleType.OP_LIST => "ops.txt",
                RoleType.BAN_LIST => "banned-players.txt",
                _ => throw new ArgumentException("Undefined enum entry in RoleType.TxtName()")
            };
        }

        public static string FriendlyName(this PluginEnums.Sorting sorting)
        {
            return sorting switch
            {
                PluginEnums.Sorting.RATING => "Most Ratings",
                PluginEnums.Sorting.DOWNLOADS => "Downloads",
                PluginEnums.Sorting.LAST_UPDATE => "Last Updated",
                PluginEnums.Sorting.SUBMISSION_DATE => "Release Date",
                _ => throw new ArgumentException("Undefined enum entry in PluginEnums.Sorting.FriendlyName()")
            };
        }

        public static string APIName(this PluginEnums.Sorting sorting)
        {
            return sorting switch
            {
                PluginEnums.Sorting.RATING => "-rating",
                PluginEnums.Sorting.DOWNLOADS => "-downloads",
                PluginEnums.Sorting.LAST_UPDATE => "-updateDate",
                PluginEnums.Sorting.SUBMISSION_DATE => "-releaseDate",
                _ => throw new ArgumentException("Undefined enum entry in PluginEnums.Sorting.APIName()")
            };
        }

        public static SolidColorBrush Color(this ConsoleMessage.MessageLevel messageLevel)
        {
            return messageLevel switch
            {
                ConsoleMessage.MessageLevel.INFO => (SolidColorBrush) new BrushConverter().ConvertFromString("#D1C2EF"),
                ConsoleMessage.MessageLevel.WARN => (SolidColorBrush) new BrushConverter().ConvertFromString("#E3BD72"),
                ConsoleMessage.MessageLevel.ERROR => (SolidColorBrush) new BrushConverter().ConvertFromString("#E37272"),
                ConsoleMessage.MessageLevel.SUCCESS => (SolidColorBrush) new BrushConverter().ConvertFromString("#72E388"),
                _ => throw new ArgumentException("Undefined enum entry in ConsoleMessage.MessageLevel.Color()")
            };
        }
        
        public static void Sort<T>(this ObservableCollection<T> collection)
            where T : IComparable<T>, IEquatable<T>
        {
            List<T> sorted = collection.OrderBy(x => x).ToList();

            int ptr = 0;
            while (ptr < sorted.Count - 1)
            {
                if (!collection[ptr].Equals(sorted[ptr]))
                {
                    try
                    {
                        int idx = CollectionUtils.Search(collection, ptr + 1, sorted[ptr]);
                        var ptr1 = ptr;
                        Application.Current.Dispatcher?.Invoke(() => collection.Move(idx, ptr1));
                    }
                    catch (Exception e)
                    {
                        ErrorLogger.Append(new AggregateException("Error while sorting ObservableCollection. See inner Exception",e));
                    }
                }
            
                ptr++;
            }
        }

        
    }
}
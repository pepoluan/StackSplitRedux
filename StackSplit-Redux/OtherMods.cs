using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackSplitRedux.MenuHandlers;

namespace StackSplitRedux
    {
    public partial class StackSplit {
        //                         mod_ID    menu_class_name  type_of_handler
        internal static Dictionary<string, List<Tuple<string, Type>>> OtherMods = new() {
                { "CJBok.ItemSpawner" , new() {
                    new("CJBItemSpawner.Framework.ItemMenu", typeof(ItemGrabMenuHandler)) 
                    } 
                }
            };
        }
    }

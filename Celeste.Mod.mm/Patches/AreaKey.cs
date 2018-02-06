﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste.Mod;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Celeste {
    // AreaKey is sealed.
    class patch_AreaKey {

        [XmlAttribute]
        public int ID;

        [XmlAttribute]
        public AreaMode Mode;

        [XmlAttribute]
        public string SID;

        public string LevelSet {
            get {
                string sid = SID;
                if (string.IsNullOrEmpty(sid))
                    return "";
                int lastIndexOfSlash = sid.LastIndexOf('/');
                if (lastIndexOfSlash == -1)
                    return "";
                return sid.Substring(0, lastIndexOfSlash);
            }
        }

        public int ChapterIndex {
            [MonoModReplace]
            get {
                if (AreaDataExt.Get(SID).Interlude)
                    return -1;

                string levelSet = LevelSet;
                int index = 0;
                for (int i = 0; i <= ID; i++) {
                    if (AreaData.Areas[i].GetLevelSet() != levelSet)
                        continue;
                    if (AreaData.Areas[i].Interlude)
                        continue;
                    index++;
                }
                return index;
            }
        }

    }
    public static class AreaKeyExt {

        // Mods can't access patch_ classes directly.
        // We thus expose any new members through extensions.

        public static string GetLevelSet(this AreaKey self)
            => ((patch_AreaKey) (object) self).LevelSet;

        public static string GetSID(this AreaKey self)
            => ((patch_AreaKey) (object) self).SID;
        public static void SetSID(this AreaKey self, string value)
            => ((patch_AreaKey) (object) self).SID = value;

    }
}

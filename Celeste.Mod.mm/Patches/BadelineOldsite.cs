﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste.Mod;
using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Celeste {
    class patch_BadelineOldsite : BadelineOldsite {

        private bool following;
        private bool ignorePlayerAnim;

        public patch_BadelineOldsite(Vector2 position, int index)
            : base(position, index) {
            // no-op. MonoMod ignores this - we only need this to make the compiler shut up.
        }

        // We're hooking the original Added, thus can't call base (Monocle.Entity::Added) without a small workaround.
        // Note that this proxy call must be static, otherwise a callvirt gets emitted.
        [MonoModLinkTo("Monocle.Entity", "System.Void Added(Monocle.Scene)")]
        public static void base_Added(Entity self, Scene scene) { }
        public extern void orig_Added(Scene scene);
        public override void Added(Scene scene) {
            Level level = scene as Level;
            if (level?.Session.Area.GetLevelSet() == "Celeste") {
                orig_Added(scene);
                return;
            }

            base_Added(this, scene);
            Add(new Coroutine(StartChasingRoutine(level)));
        }

        [MonoModIgnore] // We don't want to change anything about the method...
        [PatchBadelineChaseRoutine] // ... except for manually manipulating the method via MonoModRules
        public new extern IEnumerator StartChasingRoutine(Level level);

        private extern IEnumerator orig_StopChasing();
        private IEnumerator StopChasing() {
            Level level = Scene as Level;
            if (level.Session.Area.GetLevelSet() == "Celeste")
                return orig_StopChasing();

            return custom_StopChasing();
        }
        private IEnumerator custom_StopChasing() {
            Level level = Scene as Level;

            while (!CollideCheck<BadelineOldsiteEnd>())
                yield return null;

            following = false;
            ignorePlayerAnim = true;
            Sprite.Play("laugh");
            Sprite.Scale.X = 1f;
            yield return 1f;

            Audio.Play("event:/char/badeline/disappear", Position);
            level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
            level.Particles.Emit(P_Vanish, 12, Center, Vector2.One * 6f);
            RemoveSelf();
        }

        private static bool _IsChaseEnd(bool value, BadelineOldsite self)
            => (self as patch_BadelineOldsite).IsChaseEnd(value);
        public bool IsChaseEnd(bool value) {
            Level level = Scene as Level;
            if (level.Session.Area.GetLevelSet() == "Celeste")
                return value;

            if (level.Tracker.CountEntities<BadelineOldsiteEnd>() != 0)
                return true;

            return false;
        }

    }
}

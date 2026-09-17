using System;
using System.IO;
using System.Collections.Generic;
using game.audio;
using game.faction.player;
using init;
using init.constant;
using init.paths;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.spritecomposer;

namespace menu
{
    public class RESOURCES
    {
        private RSprites s;
        private RSound sound;

        public RESOURCES()
        {
            new GlJob(() =>
            {
                new Initer
                {
                    CreateAssets = () =>
                    {
                        new INIT();
                        s = new RSprites();
                    }
                }.Get("menu", PATHS.TextureSize(), 0);
                sound = new RSound();
            }).Perform();
        }

        public RSprites S()
        {
            return s;
        }

        public RSound Sound()
        {
            return sound;
        }

        public class RSound
        {
            public bool Playing { get; private set; }
            public readonly SoundStream Music;
            public readonly SoundStream S;
            public readonly SoundStream Logo;

            public RSound()
            {
                Json json = new Json(PATHS.AUDIO().Config.Get("Menu"));

                AudioFactory<SoundStream> fm = new AudioFactory<SoundStream>("MUSIC", PATHS.AUDIO().Music, new SoundStream.Dummy())
                {
                    Create = (all, p, key) => CORE.GetSoundCore().GetStream(p, true)
                };

                Music = fm.Read("MENU", json)[0];
                Logo = fm.Read("LOGO", json)[0];
                S = fm.Read("TORCH", json)[0];

                AudioFactory<SoundEffect> sm = new AudioFactory<SoundEffect>("SOUND", PATHS.AUDIO().Mono, new SoundEffect.Dummy())
                {
                    Create = (all, p, key) => CORE.GetSoundCore().GetEffect(p)
                };

                ClickableAbs.DefaultClickSound = sm.Read("CLICK", json)[0];
                ClickableAbs.DefaultHoverSound = sm.Read("HOVER", json)[0];

                CORE.GetSoundCore().Set(C.WIDTH() / 2, C.HEIGHT() / 2);
            }

            public void Play()
            {
                if (Playing) return;
                Logo.Stop();
                //music.setLooping(true);
                Music.Play();
                S.SetLooping(true);
                S.Play();
            }
        }

        public sealed class RSprites
        {
            private readonly PATH g = PATHS.SPRITE().GetFolder("menu");

            public readonly TILE_SHEET Background;
            public readonly TILE_SHEET BackgroundCr;
            public readonly int BackgroundTilesX;

            static RSprites()
            {
                COORDINATE dd = SnakeImage.Dim(g.Get("Background"));
                BackgroundTilesX = (dd.X() - 6 * 4) / (32 * 2);
                int ty = (dd.Y() - 6 * 2) / 32;
                Background = new ITileSheet(g.Get("Background"), dd.X(), dd.Y())
                {
                    Init = (c, s, d) =>
                    {
                        s.Full.Init(0, 0, 1, 1, BackgroundTilesX, ty, d.S32);
                        s.Full.Paste(true);
                        return d.S32.Save(2);
                    }
                }.Get();
                BackgroundCr = new ITileSheet(g.Get("BackgroundCr"), 3096, 396)
                {
                    Init = (c, s, d) =>
                    {
                        s.Full.Init(0, 0, 1, 1, 48, 12, d.S32);
                        s.Full.Paste(true);
                        return d.S32.Save(2);
                    }
                }.Get();
            }

            private static readonly int lHeight = 67;
            public readonly SPRITE[] LogoGlyps = new SPRITE[]
            {
                new ITileSprite(45, lHeight, 1, g.Get("GamatronLogo"), 1552, 92)
                {
                    Init = (c, s, d) =>
                    {
                        s.Full.Init(0, 0, 1, 1, 3, 5, d.S16);
                        s.Full.Paste(true);
                        return d.S16.Save(1);
                    }
                },
                Glyph(56),
                Glyph(64),
                Glyph(55),
                Glyph(46),
                Glyph(57),
                Glyph(48),
                Glyph(57),
            };
            public readonly SPRITE LogoFlash = Glyph(55);
            public readonly SPRITE LogoPresents = new ITileSprite(8 * 16, 19, 1)
            {
                Init = (c, s, d) =>
                {
                    s.Full.Init(s.Full.Body().X2(), 0, 1, 1, 8, 2, d.S16);
                    s.Full.Paste(true);
                    return d.S16.Save(1);
                }
            };
            public readonly SPRITE Logo = new ITileSprite(352, 32 * 7, 1, g.Get("Logo"), 728, 224)
            {
                Init = (c, s, d) =>
                {
                    s.Full.Init(0, 0, 1, 1, 11, 7, d.S32);
                    s.Full.Paste(true);
                    return d.S32.Save(1);
                }
            };
            public readonly COLOR[] LogoColors = new COLOR[]
            {
                new ColorImp(61, 5, 15).SaturateSelf(0.75),
                new ColorImp(75, 26, 5).SaturateSelf(0.75),
                new ColorImp(84, 60, 10).SaturateSelf(0.75),
                new ColorImp(15, 75, 4).SaturateSelf(0.75),
                new ColorImp(15, 75, 10).SaturateSelf(0.75),
                new ColorImp(2, 10, 75).SaturateSelf(0.75),
                new ColorImp(61, 5, 15).SaturateSelf(0.75),
                new ColorImp(75, 30, 5).SaturateSelf(0.75),
            };

            public readonly SPRITE CreditsSmallFrame;
            public readonly SPRITE[] CreditsSmall;
            public readonly SPRITE CreditsBigFrame;
            public readonly SPRITE[] CreditsBig;
            public readonly SPRITE[] ModeIcons;

            private SPRITE Glyph(int width) => new ITileSprite(width, lHeight, 1)
            {
                Init = (c, s, d) =>
                {
                    s.Full.Init(s.Full.Body().X2(), 0, 1, 1, (int)Math.Ceiling((double)width / 16), 5, d.S16);
                    s.Full.Paste(true);
                    return d.S16.Save(1);
                }
            };

            public RSprites()
            {
                CreditsSmall = new SPRITE[13];
                CreditsSmallFrame = new ITileSprite(64, 64, 3, g.Get("CreditSmall"), 2128, 76)
                {
                    Init = (c, s, d) =>
                    {
                        s.Full.Init(0, 0, CreditsSmall.Length + 1, 1, 3, 4, d.S32);
                        s.Full.SetVar(0).Paste(true);
                        return d.S32.Save(3);
                    }
                };

                for (int i = 0; i < CreditsSmall.Length; i++)
                {
                    final int k = i;
                    CreditsSmall[i] = new ITileSprite(64, 64, 3)
                    {
                        Init = (c, s, d) =>
                        {
                            s.Full.SetVar(k + 1).Paste(true);
                            return d.S32.Save(3);
                        }
                    };
                }
                CreditsBig = new SPRITE[11];
                CreditsBigFrame = new ITileSprite(96, 128, 3, g.Get("CreditLarge"), 2596, 140)
                {
                    Init = (c, s, d) =>
                    {
                        s.Full.Init(0, 0, CreditsBig.Length + 1, 1, 3, 4, d.S32);
                        s.Full.SetVar(0).Paste(true);
                        return d.S32.Save(3);
                    }
                };

                for (int i = 0; i < CreditsBig.Length; i++)
                {
                    final int k = i;
                    CreditsBig[i] = new ITileSprite(96, 128, 3)
                    {
                        Init = (c, s, d) =>
                        {
                            s.Full.SetVar(k + 1).Paste(true);
                            return d.S32.Save(3);
                        }
                    };
                }

                ModeIcons = new PTitles.IconMaker().All();
            }
        }
    }
}
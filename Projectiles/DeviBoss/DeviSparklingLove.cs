﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Projectiles.DeviBoss
{
    public class DeviSparklingLove : ModProjectile
    {
        public int scaleCounter;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sparkling Love");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 70;
            projectile.height = 70;
            projectile.hostile = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.timeLeft = 180;
            projectile.alpha = 250;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
            projectile.GetGlobalProjectile<FargoGlobalProjectile>().ImmuneToMutantBomb = true;
        }

        public override bool CanDamage()
        {
            return false;
        }

        public override void AI()
        {
            //the important part
            int ai0 = (int)projectile.ai[0];
            if (ai0 > -1 && ai0 < Main.maxNPCs && Main.npc[ai0].active && Main.npc[ai0].type == ModContent.NPCType<NPCs.DeviBoss.DeviBoss>())
            {
                if (projectile.localAI[0] == 0)
                {
                    projectile.localAI[0] = 1;
                    projectile.localAI[1] = projectile.DirectionFrom(Main.npc[ai0].Center).ToRotation();
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, -1, -17);
                }

                Vector2 offset = new Vector2(projectile.ai[1], 0).RotatedBy(Main.npc[ai0].ai[3] + projectile.localAI[1]);
                projectile.Center = Main.npc[ai0].Center + offset;
            }
            else
            {
                projectile.Kill();
                return;
            }

            //not important part
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 4;
                if (projectile.alpha < 0)
                    projectile.alpha = 0;
            }
            
            //important again
            if (++projectile.localAI[0] > 31)
            {
                projectile.localAI[0] = 1;
                if (++scaleCounter < 3)
                {
                    projectile.position = projectile.Center;

                    projectile.width *= 2;
                    projectile.height *= 2;
                    projectile.scale *= 2;

                    projectile.Center = projectile.position;

                    MakeDust();

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, -1, -16 + scaleCounter);

                    Main.PlaySound(SoundID.Item92, projectile.Center);
                }
            }

            if (projectile.timeLeft == 8)
            {
                Main.PlaySound(SoundID.NPCKilled, projectile.Center, 6);
                Main.PlaySound(SoundID.Item92, projectile.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, -1, -14);

                if (!Main.dedServ && Main.LocalPlayer.active)
                    Main.LocalPlayer.GetModPlayer<FargoPlayer>().Screenshake = 30;

                /*for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.ToRadians(360 / 90))
                {
                    Vector2 dustPos = new Vector2(
                        16f * (float)Math.Pow(Math.Sin(i), 3),
                        13 * (float)Math.Cos(i) - 5 * (float)Math.Cos(2 * i) - 2 * (float)Math.Cos(3 * i) - (float)Math.Cos(4 * i));
                    dustPos.Y *= -1;

                    int d = Dust.NewDust(projectile.Center, 0, 0, 86, 0f, 0f, 0, default, 4f);
                    Main.dust[d].velocity = Vector2.Zero;
                    Main.dust[d].scale = 4f;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity = dustPos * 1.25f;
                }*/

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    /*for (int i = 0; i < 8; i++)
                    {
                        Vector2 target = 400 * Vector2.UnitX.RotatedBy(Math.PI / 4 * i + Math.PI / 8);
                        Vector2 speed = 2 * target / 90;
                        float acceleration = -speed.Length() / 90;
                        float rotation = speed.ToRotation() + (float)Math.PI / 2;
                        Projectile.NewProjectile(projectile.Center, speed, mod.ProjectileType("DeviEnergyHeart"), projectile.damage, 0f, Main.myPlayer, rotation, acceleration);
                    }*/
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 target = 600 * Vector2.UnitX.RotatedBy(Math.PI / 4 * i);
                        Vector2 speed = 2 * target / 90;
                        float acceleration = -speed.Length() / 90;
                        float rotation = speed.ToRotation();
                        Projectile.NewProjectile(projectile.Center, speed, ModContent.ProjectileType<DeviEnergyHeart>(),
                            (int)(projectile.damage * 0.75), 0f, Main.myPlayer, rotation + MathHelper.PiOver2, acceleration);

                        Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<GlowLine>(), projectile.damage, 0f, Main.myPlayer, 2, rotation);
                        Projectile.NewProjectile(projectile.Center, speed, ModContent.ProjectileType<GlowLine>(), projectile.damage, 0f, Main.myPlayer, 2, rotation + MathHelper.PiOver2);
                        Projectile.NewProjectile(projectile.Center, speed, ModContent.ProjectileType<GlowLine>(), projectile.damage, 0f, Main.myPlayer, 2, rotation - MathHelper.PiOver2);
                    }
                }
            }

            projectile.direction = projectile.spriteDirection = Main.npc[ai0].direction;
            projectile.rotation = Main.npc[ai0].ai[3] + projectile.localAI[1] + (float)Math.PI / 2 + (float)Math.PI / 4;
            if (projectile.spriteDirection >= 0)
                projectile.rotation -= (float)Math.PI / 2;
        }

        public override void Kill(int timeLeft)
        {
            MakeDust();
        }

        private void MakeDust()
        {
            Vector2 start = projectile.width * Vector2.UnitX.RotatedBy(projectile.rotation - (float)Math.PI / 4);
            if (Math.Abs(start.X) > projectile.width / 2) //bound it so its always inside projectile's hitbox
                start.X = projectile.width / 2 * Math.Sign(start.X);
            if (Math.Abs(start.Y) > projectile.height / 2)
                start.Y = projectile.height / 2 * Math.Sign(start.Y);
            int length = (int)start.Length();
            start = Vector2.Normalize(start);
            float scaleModifier = scaleCounter / 3f + 0.5f;
            for (int j = -length; j <= length; j += 80)
            {
                Vector2 dustPoint = projectile.Center + start * j;
                dustPoint.X -= 23;
                dustPoint.Y -= 23;

                /*for (int i = 0; i < 5; i++)
                {
                    int dust = Dust.NewDust(dustPoint, 46, 46, 86, 0f, 0f, 0, new Color(), scaleModifier * 2f);
                    Main.dust[dust].velocity *= 1.4f * scaleModifier;
                }*/

                for (int index1 = 0; index1 < 15; ++index1)
                {
                    int index2 = Dust.NewDust(dustPoint, 46, 46, 86, 0f, 0f, 0, new Color(), scaleModifier * 2.5f);
                    Main.dust[index2].noGravity = true;
                    Main.dust[index2].velocity *= 16f * scaleModifier;
                    int index3 = Dust.NewDust(dustPoint, 46, 46, 86, 0f, 0f, 0, new Color(), scaleModifier);
                    Main.dust[index3].velocity *= 8f * scaleModifier;
                    Main.dust[index3].noGravity = true;
                }

                for (int i = 0; i < 5; i++)
                {
                    int d = Dust.NewDust(dustPoint, 46, 46, 86, 0f, 0f, 0, new Color(), Main.rand.NextFloat(1f, 2f) * scaleModifier);
                    Main.dust[d].velocity *= Main.rand.NextFloat(1f, 4f) * scaleModifier;
                }
            }
        }

        /*public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.velocity.X = target.Center.X < Main.npc[(int)projectile.ai[0]].Center.X ? -15f : 15f;
            target.velocity.Y = -10f;
            target.AddBuff(mod.BuffType("Lovestruck"), 240);
        }*/

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * projectile.Opacity;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture2D13 = Main.projectileTexture[projectile.type];
            int num156 = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects effects = projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color color26 = lightColor;
            color26 = projectile.GetAlpha(color26);

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[projectile.type]; i++)
            {
                Color color27 = color26 * 0.5f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[projectile.type];
                Vector2 value4 = projectile.oldPos[i];
                float num165 = projectile.oldRot[i];
                Main.spriteBatch.Draw(texture2D13, value4 + projectile.Size / 2f - Main.screenPosition + new Vector2(0, projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, projectile.scale, effects, 0f);
            }

            Main.spriteBatch.Draw(texture2D13, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), projectile.GetAlpha(lightColor), projectile.rotation, origin2, projectile.scale, effects, 0f);
            return false;
        }
    }
}
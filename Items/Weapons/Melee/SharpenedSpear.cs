using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ThroughTheAges.Items.Weapons.Melee
{
    public class SharpenedSpear : ModItem
    {
        public override void SetStaticDefaults()
        {   
            Tooltip.SetDefault("An ancient spear of the [REDACTED] era");
            ItemID.Sets.Spears[Item.type] = true;
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true; // This skips use animation-tied sound playback, so that we're able to make it be tied to use time instead in the UseItem() hook.
        }

        public override void SetDefaults()
        {
            Item.damage = 6;
            Item.DamageType = DamageClass.Melee;

            // SPRITE
            Item.width = 40;
            Item.height = 40;
            // SPRITE

            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;

            // Spear's are projectiles
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.knockBack = 6;
            Item.value = 10000;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.shootSpeed = 3.7f;
            Item.shoot = ModContent.ProjectileType<SharpenedSpearProj>();
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DirtBlock, 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }

        public override bool? UseItem(Player player)
        {
            // Because we're skipping sound playback on use animation start, we have to play it ourselves whenever the item is actually used.
            if (!Main.dedServ && Item.UseSound.HasValue)
            {
                SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
            }

            return null;
        }
    }

    public class SharpenedSpearProj : ModProjectile
    {
        protected virtual float HoldoutRangeMin => 24f;
        protected virtual float HoldoutRangeMax => 96f;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sharpened Spear");
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Spear);

            // SPRITE
            Projectile.width = 12;
            Projectile.height = 12;
            // SPRITE

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;

        }

        // It appears that for this AI, only the ai0 field is used!
        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner]; // Since we access the owner player instance so much, it's useful to create a helper local variable for this

            int duration = player.itemAnimationMax;

            player.heldProj = Projectile.whoAmI;

            if (Projectile.timeLeft > duration)
            {
                Projectile.timeLeft = duration;
            }

            Projectile.velocity = Vector2.Normalize(Projectile.velocity);

            float halfway = duration * 0.5f;
            float progress;

            // Progress is percent done with the animation
            if (Projectile.timeLeft < halfway)
            {
                progress = Projectile.timeLeft / halfway;
            }
            else
            {
                progress = (duration - Projectile.timeLeft) / halfway;
            }

            Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin, Projectile.velocity * HoldoutRangeMax, progress);


            if (Projectile.spriteDirection == -1)
            {
                // If sprite is facing left, rotate 45 degrees
                Projectile.rotation += MathHelper.ToRadians(45f);
            }
            else
            {
                // If sprite is facing right, rotate 135 degrees
                Projectile.rotation += MathHelper.ToRadians(135f);
            }

            return false;

        }

    }

}
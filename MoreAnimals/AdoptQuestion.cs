using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;

namespace Entoarox.MorePetsAndAnimals
{
    internal class AdoptQuestion
    {
        /*********
        ** Fields
        *********/
        private readonly string Type;
        private readonly int Skin;
        private readonly AnimatedSprite Sprite;
        private Farmer Who;


        /*********
        ** Public methods
        *********/
        public AdoptQuestion(string type, int skin)
        {
            this.Type = type;
            this.Skin = skin;

            this.Sprite = new AnimatedSprite(ModEntry.SHelper.Content.GetActualAssetKey($"assets/skins/{type}_{skin}.png"), 28, 32, 32)
            {
                loop = true
            };
        }

        public static void Show()
        {
            Random random = ModEntry.Random;
            string type = "";
            int id = 0;
            if (ModEntry.Config.BalancedPetTypes)
            {
                Dictionary<string, double> types = ModEntry.Pets.Keys.ToDictionary(k => k, v => 1.0);
                foreach (Pet pet in ModEntry.GetAllPets())
                {
                    string petType = ModEntry.Sanitize(pet.GetType().Name);
                    types[petType] *= 0.5;
                }
                double typeChance = random.NextDouble();
                foreach (KeyValuePair<string, double> pair in types.OrderBy(a => a.Value))
                {
                    if (pair.Value >= typeChance)
                    {
                        type = pair.Key;
                        break;
                    }
                }
            }
            else
                type = ModEntry.Pets.Keys.ToArray()[random.Next(ModEntry.Pets.Count)];
            if (ModEntry.Config.BalancedPetSkins)
            {
                Dictionary<int, double> skins = ModEntry.Pets[type].ToDictionary(k => k.ID, v => 1.0);
                foreach (Pet pet in ModEntry.GetAllPets().Where(pet => ModEntry.Sanitize(pet.GetType().Name) == type))
                {
                    skins[pet.Manners] *= 0.5;
                }
                double skinChance = random.NextDouble();
                foreach (KeyValuePair<int, double> pair in skins.OrderBy(a => a.Value))
                {
                    if (pair.Value >= skinChance)
                    {
                        id = pair.Key;
                        break;
                    }
                }
            }
            else
                id = ModEntry.Pets[type][random.Next(ModEntry.Pets[type].Count)].ID;
            AdoptQuestion q = new AdoptQuestion(type, id);
            ModEntry.SHelper.Events.Display.RenderedHud += q.Display;
            Game1.currentLocation.lastQuestionKey = "AdoptPetQuestion";
            Game1.currentLocation.createQuestionDialogue(
                ModEntry.SHelper.Translation.Get("AdoptMessage", new { petType = type, adoptionPrice = ModEntry.Config.AdoptionPrice }),
                Game1.player.money < ModEntry.Config.AdoptionPrice
                    ? new[]
                    {
                        new Response("n", ModEntry.SHelper.Translation.Get("AdoptNoGold", new { adoptionPrice = ModEntry.Config.AdoptionPrice }))
                    }
                    : new[]
                    {
                        new Response("y", ModEntry.SHelper.Translation.Get("AdoptYes")),
                        new Response("n", ModEntry.SHelper.Translation.Get("AdoptNo"))
                    },
                q.Resolver);
        }

        public void Display(object o, EventArgs e)
        {
            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is DialogueBox)
            {
                DialogueBox box = (DialogueBox)Game1.activeClickableMenu;
                Vector2 c = new Vector2(Game1.viewport.Width / 2 - 128 * Game1.pixelZoom, Game1.viewport.Height - box.height - 56 * Game1.pixelZoom);
                Vector2 p = new Vector2(36 * Game1.pixelZoom + c.X, 32 * Game1.pixelZoom + c.Y);
                IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)c.X, (int)c.Y, 40 * Game1.pixelZoom, 40 * Game1.pixelZoom, Color.White, 1, true);
                Game1.spriteBatch.Draw(this.Sprite.Texture, p, this.Sprite.SourceRect, Color.White, 0, new Vector2(this.Sprite.SpriteWidth, this.Sprite.SpriteHeight), Game1.pixelZoom, SpriteEffects.None, 0.991f);
                this.Sprite.Animate(Game1.currentGameTime, 28, 2, 500);
            }
            else
                ModEntry.SHelper.Events.Display.RenderedHud -= this.Display;
        }

        public void Resolver(Farmer who, string answer)
        {
            ModEntry.SHelper.Events.Display.RenderedHud -= this.Display;
            if (answer == "n")
                return;
            this.Who = who;
            Game1.activeClickableMenu = new NamingMenu(this.Namer, ModEntry.SHelper.Translation.Get("ChooseName"));
        }

        public void Namer(string petName)
        {
            Pet pet;
            this.Who.Money -= ModEntry.Config.AdoptionPrice;
            Type type = ModEntry.PetTypes[this.Type];
            pet = (Pet)Activator.CreateInstance(type, (int)Game1.player.position.X, (int)Game1.player.position.Y);
            pet.Sprite = new AnimatedSprite(ModEntry.SHelper.Content.GetActualAssetKey($"assets/skins/{this.Type}_{this.Skin}.png"), 0, 32, 32);
            pet.Name = petName;
            pet.displayName = petName;
            pet.Manners = this.Skin;
            pet.Age = Game1.year * 1000 + Array.IndexOf(ModEntry.Seasons, Game1.currentSeason) * 100 + Game1.dayOfMonth;
            pet.Position = Game1.player.position;
            Game1.currentLocation.addCharacter(pet);
            pet.warpToFarmHouse(this.Who);
            Game1.drawObjectDialogue(ModEntry.SHelper.Translation.Get("Adopted", new { petName }));
        }
    }
}

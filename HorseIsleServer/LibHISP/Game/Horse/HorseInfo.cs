﻿using HISP.Server;
using HISP.Game.Items;

using System.Collections.Generic;
using System.Globalization;
using HISP.Player;

namespace HISP.Game.Horse
{
    public class HorseInfo
    {
        public enum StatType
        {
            AGILITY,
            CONFORMATION,
            ENDURANCE,
            PERSONALITY,
            SPEED,
            STRENGTH,
            INTELIGENCE
        }
        public class StatCalculator
        {
            public StatCalculator(HorseInstance horse, StatType type, User user=null)
            {
                baseHorse = horse;
                horseStat = type;
                baseUser = user;
            }
            private StatType horseStat;
            private HorseInstance baseHorse;
            private User baseUser;
            public int BaseValue
            {
                get
                {
                    switch (horseStat)
                    {
                        case StatType.AGILITY:
                            return baseHorse.Breed.BaseStats.Agility;
                        case StatType.CONFORMATION:
                            return baseHorse.Breed.BaseStats.Conformation;
                        case StatType.ENDURANCE:
                            return baseHorse.Breed.BaseStats.Endurance;
                        case StatType.PERSONALITY:
                            return baseHorse.Breed.BaseStats.Personality;
                        case StatType.SPEED:
                            return baseHorse.Breed.BaseStats.Speed;
                        case StatType.STRENGTH:
                            return baseHorse.Breed.BaseStats.Strength;
                        case StatType.INTELIGENCE:
                            return baseHorse.Breed.BaseStats.Inteligence;
                        default:
                            return 0;
                    }
                }
            }
            public int MaxValue
            { 
                get
                {
                    return BaseValue * 2;
                }
            }
            public int BreedValue
            {
                get
                {
                    return BaseValue + BreedOffset;
                }
            }
            public int BreedOffset
            {
                get
                {
                    switch (horseStat)
                    {
                        case StatType.AGILITY:
                            return baseHorse.AdvancedStats.Agility;
                        case StatType.CONFORMATION:
                            return baseHorse.AdvancedStats.Conformation;
                        case StatType.ENDURANCE:
                            return baseHorse.AdvancedStats.Endurance;
                        case StatType.PERSONALITY:
                            return baseHorse.AdvancedStats.Personality;
                        case StatType.SPEED:
                            return baseHorse.AdvancedStats.Speed;
                        case StatType.STRENGTH:
                            return baseHorse.AdvancedStats.Strength;
                        case StatType.INTELIGENCE:
                            return baseHorse.AdvancedStats.Inteligence;
                        default:
                            return 0;
                    }
                }
                set
                {
                    switch (horseStat)
                    {
                        case StatType.AGILITY:
                            baseHorse.AdvancedStats.Agility = value;
                            break;
                        case StatType.CONFORMATION:
                            baseHorse.AdvancedStats.Conformation = value;
                            break;
                        case StatType.ENDURANCE:
                            baseHorse.AdvancedStats.Endurance = value;
                            break;
                        case StatType.PERSONALITY:
                            baseHorse.AdvancedStats.Personality = value;
                            break;
                        case StatType.SPEED:
                            baseHorse.AdvancedStats.Speed = value;
                            break;
                        case StatType.STRENGTH:
                            baseHorse.AdvancedStats.Strength = value;
                            break;
                        case StatType.INTELIGENCE:
                            baseHorse.AdvancedStats.Inteligence = value;
                            break;
                    }
                }
            }
            public int CompetitionGearOffset
            { 
                get
                {
                    if(baseUser != null)
                    {
                        int offsetBy = 0;
                        if(baseUser.EquipedCompetitionGear.Head != null)
                            offsetBy += getOffsetFrom(baseUser.EquipedCompetitionGear.Head);
                        if (baseUser.EquipedCompetitionGear.Body != null)
                            offsetBy += getOffsetFrom(baseUser.EquipedCompetitionGear.Body);
                        if (baseUser.EquipedCompetitionGear.Legs != null)
                            offsetBy += getOffsetFrom(baseUser.EquipedCompetitionGear.Legs);
                        if (baseUser.EquipedCompetitionGear.Feet != null)
                            offsetBy += getOffsetFrom(baseUser.EquipedCompetitionGear.Feet);
                        return offsetBy;
                    }
                    else
                    {
                        return 0;
                    }

                }
            }

            public int CompanionOffset
            {
                get
                {
                    if(baseHorse.Equipment.Companion != null)
                        return baseHorse.Equipment.Companion.GetMiscFlag(0);
                    else
                        return 0;
                }
            }


            public int TackOffset
            {
                get
                {
                    int offsetBy = 0;
                    if (baseHorse.Equipment.Saddle != null)
                        offsetBy += getOffsetFrom(baseHorse.Equipment.Saddle);
                    if (baseHorse.Equipment.SaddlePad != null)
                        offsetBy += getOffsetFrom(baseHorse.Equipment.SaddlePad);
                    if (baseHorse.Equipment.Bridle != null)
                        offsetBy += getOffsetFrom(baseHorse.Equipment.Bridle);
                    return offsetBy;
                }
            }
            public int Total
            {
                get
                {
                    return BreedValue + CompanionOffset + TackOffset + CompetitionGearOffset;
                }
            }

            private int getOffsetFrom(Item.ItemInformation tackPeice)
            {
                int offsetBy = 0;
                foreach (Item.Effects effect in tackPeice.Effects)
                {
                    string effects = effect.EffectsWhat;
                    switch (effects)
                    {
                        case "AGILITY":
                            if (horseStat == StatType.AGILITY)
                                offsetBy += effect.EffectAmount;
                            break;
                        case "CONFORMATION":
                            if (horseStat == StatType.CONFORMATION)
                                offsetBy += effect.EffectAmount;
                            break;
                        case "ENDURANCE":
                            if (horseStat == StatType.ENDURANCE)
                                offsetBy += effect.EffectAmount;
                            break;
                        case "PERSONALITY":
                            if (horseStat == StatType.PERSONALITY)
                                offsetBy += effect.EffectAmount;
                            break;
                        case "SPEED":
                            if (horseStat == StatType.SPEED)
                                offsetBy += effect.EffectAmount;
                            break;
                        case "STRENGTH":
                            if (horseStat == StatType.STRENGTH)
                                offsetBy += effect.EffectAmount;
                            break;
                        case "INTELLIGENCE":
                            if (horseStat == StatType.INTELIGENCE)
                                offsetBy += effect.EffectAmount;
                            break;

                    }

                }
                return offsetBy;
            }

        }

        public class AdvancedStats
        {
            public AdvancedStats(HorseInstance horse, int newSpeed,int newStrength, int newConformation, int newAgility, int newInteligence, int newEndurance, int newPersonality, int newHeight)
            {
                if(horse != null)
                    baseHorse = horse;
                speed = newSpeed;
                strength = newStrength;
                conformation = newConformation;
                agility = newAgility;
                endurance = newEndurance;
                inteligence = newInteligence;
                personality = newPersonality;
                height = newHeight;
            }


            public int Speed
            {
                get
                {
                    return speed;
                }
                set
                {
                    if (value > ((baseHorse.Breed.BaseStats.Speed * 2) - baseHorse.Breed.BaseStats.Speed))
                        value = ((baseHorse.Breed.BaseStats.Speed * 2) - baseHorse.Breed.BaseStats.Speed);
                    Database.SetHorseSpeed(baseHorse.RandomId, value);
                    speed = value;
                }
            }

            public int Strength
            {
                get
                {
                    return strength;
                }
                set
                {
                    if (value > ((baseHorse.Breed.BaseStats.Strength * 2) - baseHorse.Breed.BaseStats.Strength))
                        value = ((baseHorse.Breed.BaseStats.Strength * 2) - baseHorse.Breed.BaseStats.Strength);
                    Database.SetHorseStrength(baseHorse.RandomId, value);
                    strength = value;
                }
            }

            public int Conformation
            {
                get
                {
                    return conformation;
                }
                set
                {
                    if (value > ((baseHorse.Breed.BaseStats.Conformation * 2) - baseHorse.Breed.BaseStats.Conformation))
                        value = ((baseHorse.Breed.BaseStats.Conformation * 2) - baseHorse.Breed.BaseStats.Conformation);
                    Database.SetHorseConformation(baseHorse.RandomId, value);
                    conformation = value;
                }
            }
            public int Agility
            {
                get
                {
                    return agility;
                }
                set
                {
                    if (value > ((baseHorse.Breed.BaseStats.Agility * 2) - baseHorse.Breed.BaseStats.Agility))
                        value = ((baseHorse.Breed.BaseStats.Agility * 2) - baseHorse.Breed.BaseStats.Agility);
                    Database.SetHorseAgility(baseHorse.RandomId, value);
                    agility = value;
                }
            }
            public int Endurance
            {
                get
                {
                    return endurance;
                }
                set
                {
                    if (value > ((baseHorse.Breed.BaseStats.Endurance * 2) - baseHorse.Breed.BaseStats.Endurance))
                        value = ((baseHorse.Breed.BaseStats.Endurance * 2) - baseHorse.Breed.BaseStats.Endurance);
                    Database.SetHorseEndurance(baseHorse.RandomId, value);
                    endurance = value;
                }
            }
            public int Inteligence
            {
                get
                {
                    return inteligence;
                }
                set
                {
                    if (value > ((baseHorse.Breed.BaseStats.Inteligence* 2) - baseHorse.Breed.BaseStats.Inteligence))
                        value = ((baseHorse.Breed.BaseStats.Inteligence * 2) - baseHorse.Breed.BaseStats.Inteligence);
                    Database.SetHorseInteligence(baseHorse.RandomId, value);
                    inteligence = value;
                }
            }
            public int Personality
            {
                get
                {
                    return personality;
                }
                set
                {
                    if (value > ((baseHorse.Breed.BaseStats.Personality * 2) - baseHorse.Breed.BaseStats.Personality))
                        value = ((baseHorse.Breed.BaseStats.Personality * 2) - baseHorse.Breed.BaseStats.Personality);
                    Database.SetHorsePersonality(baseHorse.RandomId, value);
                    personality = value;
                }
            }
            public int Height
            {
                get
                {
                    return height;
                }
                set
                {
                    height = value;
                    Database.SetHorseHeight(baseHorse.RandomId, value);
                }
            }
            public int MinHeight;
            public int MaxHeight;

            private HorseInstance baseHorse;
            private int speed;
            private int strength;
            private int conformation;
            private int agility;
            private int endurance;
            private int inteligence;
            private int personality;
            private int height;
        }
        public class BasicStats
        {
            public BasicStats(HorseInstance horse, int newHealth, int newShoes, int newHunger, int newThirst, int newMood, int newGroom, int newTiredness, int newExperience)
            {
                baseHorse = horse;
                health = newHealth;
                shoes = newShoes;
                hunger = newHunger;
                thirst = newThirst;
                mood = newMood;
                groom = newGroom;
                tiredness = newTiredness;
                experience = newExperience;

            }
            public int Health
            {
                get
                {
                    return health;
                }
                set
                {
                    if (value > 1000)
                        value = 1000;
                    if (value < 0)
                        value = 0;
                    health = value;
                    Database.SetHorseHealth(baseHorse.RandomId, value);
                }
            }
            public int Shoes
            {
                get
                {
                    return shoes;
                }
                set
                {
                    if (value > 1000)
                        value = 1000;
                    if (value < 0)
                        value = 0;
                    shoes = value;
                    Database.SetHorseShoes(baseHorse.RandomId, value);
                }
            }
            public int Hunger {
                get
                {
                    return hunger;
                }
                set
                {
                    if (value > 1000)
                        value = 1000;
                    if (value < 0)
                        value = 0;
                    hunger = value;
                    Database.SetHorseHunger(baseHorse.RandomId, value);
                }
            }
            public int Thirst
            {
                get
                {
                    return thirst;
                }
                set
                {
                    if (value > 1000)
                        value = 1000;
                    if (value < 0)
                        value = 0;
                    thirst = value;
                    Database.SetHorseThirst(baseHorse.RandomId, value);
                }
            }
            public int Mood
            {
                get
                {
                    return mood;
                }
                set
                {
                    // Lol turns out pinto forgot to do this and u can have negative mood :D

                    if (value > 1000)
                        value = 1000;
                    /*if (value < 0)
                        value = 0;
                    */

                    mood = value;
                    Database.SetHorseMood(baseHorse.RandomId, value);
                }
            }
            public int Groom
            {
                get
                {
                    return groom;
                }
                set
                {
                    if (value > 1000)
                        value = 1000;
                    if (value < 0)
                        value = 0;
                    groom = value;
                    Database.SetHorseGroom(baseHorse.RandomId, value);
                }
            }
            public int Tiredness
            {
                get
                {
                    return tiredness;
                }
                set
                {
                    if (value > 1000)
                        value = 1000;
                    if (value < 0)
                        value = 0;
                    tiredness = value;
                    Database.SetHorseTiredness(baseHorse.RandomId, value);
                }
            }
            public int Experience
            {
                get
                {
                    return experience;
                }
                set
                {
                    if (value < 0)
                        value = 0;
                    experience = value;
                    Database.SetHorseExperience(baseHorse.RandomId, value);
                }
            }

            private HorseInstance baseHorse;
            private int health;
            private int shoes;
            private int hunger;
            private int thirst;
            private int mood;
            private int groom;
            private int tiredness;
            private int experience;
        }

        public class Breed
        {
            public int Id;
            public string Name;
            public string Description;
            public AdvancedStats BaseStats;
            public string[] Colors;
            public string SpawnOn;
            public string SpawnInArea;
            public string Swf;
            public string Type;

            public string[] GenderTypes()
            {
                string[] options = new string[2];
                if (Type == "camel")
                {
                    options[0] = "cow";
                    options[1] = "bull";
                }
                else if (Type == "llama")
                {
                    options[0] = "male";
                    options[1] = "female";
                }
                else
                {
                    options[0] = "stallion";
                    options[1] = "mare";
                }
                return options;
            }

        }
        public struct HorseEquips
        {
            public Item.ItemInformation Saddle;
            public Item.ItemInformation SaddlePad;
            public Item.ItemInformation Bridle;
            public Item.ItemInformation Companion;
        }

        public struct Category
        {
            public string Name;
            public string MetaOthers;
            public string Meta;
        }

        public static string[] HorseNames;
        private static List<Category> horseCategories = new List<Category>();
        private static List<Breed> breeds = new List<Breed>();

        public static void AddBreed(Breed breed)
        {
            breeds.Add(breed);
        }

        public static void AddHorseCategory(Category category)
        {
            horseCategories.Add(category);
        }
        public static Category[] HorseCategories
        {
            get 
            {
                return horseCategories.ToArray();
            }
        }

        public static Breed[] Breeds
        {
            get
            {
                return breeds.ToArray();
            }
        }

        public static string GenerateHorseName()
        {
            int indx = 0;
            int max = HorseNames.Length;
            int i = GameServer.RandomNumberGenerator.Next(indx, max);
            return HorseNames[i];
        }
        public static double CalculateHands(int height, bool isBreedViewer)
        {
            if(isBreedViewer)
                return ((double)height / 4.00);
            else
            {
                int h1 = 0;
                int h2 = 0;
                for(int i = 0; i < height; i++)
                {
                    h2++;
                    if (h2 == 4)
                    {
                        h2 = 0;
                        h1++;
                    }
                }
                double hands = double.Parse(h1 + "." + h2, CultureInfo.InvariantCulture); // This is terrible. dont do this.
                return hands;
            }
        }
        public static string BreedViewerSwf(HorseInstance horse, string terrainTileType)
        {
            double hands = CalculateHands(horse.AdvancedStats.Height, true);

            string swf = "breedviewer.swf?terrain=" + terrainTileType + "&breed=" + horse.Breed.Swf + "&color=" + horse.Color + "&hands=" + hands.ToString(CultureInfo.InvariantCulture);
            if (horse.Equipment.Saddle != null)
                swf += "&saddle=" + horse.Equipment.Saddle.EmbedSwf;
            if (horse.Equipment.SaddlePad != null)
                swf += "&saddlepad=" + horse.Equipment.SaddlePad.EmbedSwf;
            if (horse.Equipment.Bridle != null)
                swf += "&bridle=" + horse.Equipment.Bridle.EmbedSwf;
            if (horse.Equipment.Companion != null)
                swf += "&companion=" + horse.Equipment.Companion.EmbedSwf;
            swf += "&junk=";

            return swf;
        }
        public static Breed GetBreedById(int id)
        {
            foreach(Breed breed in Breeds)
            {
                if (breed.Id == id)
                    return breed;
            }
            throw new KeyNotFoundException("No horse breed with id " + id);
        }
    }
}

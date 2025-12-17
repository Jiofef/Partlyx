using Avalonia.Controls.Shapes;
using Avalonia.Styling;
using Avalonia.Xaml.Interactions.Custom;
using Material.Icons;
using Partlyx.Services.Helpers;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.UIObjectViewModels;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Joins;
using static Material.Icons.MaterialIconKind;

namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class MaterialAvaloniaVectorIconCatalog : IIconVectorCatalog
    {
        public string LibraryName => "MaterialIcons";
        private readonly List<string> _allTheIconKeys;
        private readonly ReadOnlyDictionary<int, string[]> _allTheIconKeyValueGroupsDic;
        private readonly IReadOnlyList<int> _allTheIconKeysList;

        private readonly List<string> _baseIconKeys;
        private readonly Dictionary<int, string[]> _baseIconKeyValueGroupsDic;
        private readonly IReadOnlyList<int> _baseIconKeysList;

        private readonly List<MaterialIconKind> _baseIconKinds = new()
        {
            Circle, Square, Triangle, Hexagon, Octagon, Star, Heart, Diamond, Decagram, Pentagon, Rhombus, Shield, Starburst, MaterialIconKind.Shape,
            WeatherSunny, WeatherNight, Cloud, Fire, Water, Tree, Flower, Leaf, PineTree, Sprout, Cactus, Mushroom, Earth, Mountain, Waves, LightningBolt, Snowflake, Volcano, Grass, Agriculture,
            Paw, Dog, Cat, Bird, Fish, Duck, Owl, Snake, Bee, Butterfly, Ladybug, Dolphin, Elephant, Panda, Penguin, Kangaroo, Pig, Horse, Cow, Sheep, Chicken, Rabbit, Bat,
            FoodApple, Carrot, Corn, Pizza, Hamburger, IceCream, Coffee, GlassMug, BottleWine, GlassCocktail, BreadSlice, Cheese, Cake, Candy, Cookie, Egg, FoodCroissant, FoodDrumstick, FoodSteak,
            Hammer, Wrench, Screwdriver, Axe, SawBlade, Key, Lightbulb, Magnet, Camera, Flashlight, Microscope, Telescope, Flask, Atom, Dna, Rocket, Satellite, Robot, Battery, Computer, Laptop, Cellphone,
            Watch, Radio, Calculator, Car, Bus, Truck, Bicycle, Motorbike, Airplane, Ship, Submarine, Helicopter, Train, Map, Compass, Anchor, Luggage, Tent, Bridge, TrafficLight, GasStation,
            Gift, Bell, Book, Briefcase, Paperclip, Pencil, Pen, Umbrella, Sword, Crown, Trophy, Medal, Flag, Bone, Skull, Ghost, Balloon, Bomb, Candle, Dice5, Cards, ChessKnight, PokerChip, Controller,
            Music, GuitarAcoustic, GuitarElectric, Piano, Violin, Microphone, Headphones, Speaker, Palette, Brush, Movie, Theater, Saxophone,
            Eye, HandPointingUp, HandOkay, ThumbUp, HumanGreeting, Walk, Run, Pill, Stethoscope, Syringe, HeartPulse, Brain, Tooth,
            Numeric0, Numeric1, Numeric2, Numeric3, Numeric4, Numeric5, Numeric6, Numeric7, Numeric8, Numeric9, Plus, Minus, Equal, Infinity, CurrencyUsd, CurrencyEur, CurrencyBtc, Gold,
            Home, OfficeBuilding, Hospital, School, Church, Castle, Warehouse, Factory, Store, Stadium, Fountain, Bank, LocalPostOffice,
            Beach, Sunglasses, Hanger, Cloth, Tie, ShoeHeel, Mustache, HatFedora, Glasses,
            Ammunition, Barrel, Basket, Bed, Beer, Binoculars, Broom, Bucket, Bug, FileCabinet, Cart, Coffin, Couch, Desk, Door, Fan, FireExtinguisher, Fridge, HammerWrench, Handcuffs, Kettle,
            Ladder, JackOLantern, Mirror, Pipe, Printer, Safe, Scale, Scissors, Shovel, Toilet, Tools, ToyBrick, VacuumCleaner, Wheelchair, ZipBox
        };

        public MaterialAvaloniaVectorIconCatalog()
        {
            _allTheIconKeys = Enum.GetNames(typeof(MaterialIconKind)).ToList();
            _allTheIconKeysList = EnumMapper<MaterialIconKind>.GetOrderedKeys();
            _allTheIconKeyValueGroupsDic = EnumMapper<MaterialIconKind>.GetKeyValueGroups();

            _baseIconKeys = _baseIconKinds.Select(k => k.ToString()).ToList();
            _baseIconKeysList = _baseIconKinds.Select(k => (int)k).Distinct().ToList();
            _baseIconKeyValueGroupsDic = EnumMapper<MaterialIconKind>.GetKeyValueGroupsForValues(_baseIconKeysList);
        }

        public IReadOnlyList<string> GetAllIconKeys()
            => _allTheIconKeys;
        public IReadOnlyList<string> GetBaseIconKeys()
            => _baseIconKeys;

        private List<StoreVectorIconContentViewModel>? _allIconsContentForStoreCache;
        public IReadOnlyList<StoreVectorIconContentViewModel> GetAllIconsContentForStore(bool returnCachedIfPossible = true)
        {
            if (returnCachedIfPossible && _allIconsContentForStoreCache != null)
                return _allIconsContentForStoreCache;

            var list = new List<StoreVectorIconContentViewModel>(_allTheIconKeysList.Count);
            foreach (var key in _allTheIconKeysList)
            {
                var valueNames = _allTheIconKeyValueGroupsDic[key];
                string firstValueName = valueNames[0];
                var iconContent = new StoreVectorIconContentViewModel(firstValueName)
                {
                    SearchKeys = valueNames.ToList()
                };
                list.Add(iconContent);
            }

            _allIconsContentForStoreCache = list;
            return list;
        }

        private List<StoreVectorIconContentViewModel>? _baseIconsContentForStoreCache;
        public IReadOnlyList<StoreVectorIconContentViewModel> GetBaseIconsContentForStore(bool returnCachedIfPossible = true)
        {
            if (returnCachedIfPossible && _baseIconsContentForStoreCache != null)
                return _baseIconsContentForStoreCache;

            var list = new List<StoreVectorIconContentViewModel>(_baseIconKeysList.Count);
            foreach (var key in _baseIconKeysList)
            {
                var valueNames = _baseIconKeyValueGroupsDic[key];
                string firstValueName = valueNames[0];
                var iconContent = new StoreVectorIconContentViewModel(firstValueName)
                {
                    SearchKeys = valueNames.ToList()
                };
                list.Add(iconContent);
            }

            _baseIconsContentForStoreCache = list;
            return list;
        }
    }
}

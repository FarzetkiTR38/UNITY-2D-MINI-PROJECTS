namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    /// <summary>Fifty connected, colored authoring silhouettes in normalized coordinates.</summary>
    public static partial class DenseShapeLibrary
    {
        public const int Count = 50;
        private static readonly string[] Titles =
        {
            "Ruby Heart", "Polar Penguin", "Moon Rabbit", "Prism Butterfly", "Starlight Rocket",
            "Marmalade Cat", "Golden Puppy", "Honey Bear", "Autumn Fox", "Twilight Owl",
            "Coral Fish", "Emerald Turtle", "Blue Whale", "Sunset Crab", "Golden Chick",
            "Pond Duck", "Pearl Swan", "Coral Seahorse", "Jade Dinosaur", "Lavender Elephant",
            "Violet Octopus", "Moon Jellyfish", "Honey Bee", "Ruby Ladybug", "Azure Dragonfly",
            "Tropical Parrot", "Rose Unicorn", "Emerald Dragon", "Coral Flamingo", "Copper Squirrel",
            "Chestnut Hedgehog", "Lily Frog", "Bamboo Panda", "Moonlit Raccoon", "Rainbow Toucan",
            "Ruby Strawberry", "Golden Pineapple", "Woodland Mushroom", "Summer Sunflower", "Spring Tulip",
            "Royal Crown", "Rainbow Castle", "Ocean Anchor", "Sunburst Guitar", "Festival Balloon",
            "Sunset Sailboat", "Triple Scoop", "Porcelain Teapot", "Meadow Windmill", "Royal Peacock"
        };

        /// <summary>Returns the readable campaign title for a zero-based silhouette index.</summary>
        public static string Name(int index) => Titles[Mathf.Clamp(index, 0, Count - 1)];

        /// <summary>Samples the filled silhouette and its painted region at normalized coordinates.</summary>
        public static Color Sample(int index, float x, float y)
        {
            if (Mathf.Abs(x) > 1f || Mathf.Abs(y) > 1f) return Color.clear;
            if (index >= 0 && index < 5)
            {
                Color original=ShapeSilhouettes.Sample(index, x, y);
                if (index==1 && original.a>.5f && E(Mathf.Abs(x),y,.23f,.55f,.1f,.115f)) return Ink;
                if (index==1 && original.r>.7f && original.g>.8f && original.b>.9f) return White;
                if (index==3 && original.a<.5f && E(Mathf.Abs(x),y,.16f,-.13f,.18f,.26f)) return H(0xBD73BF);
                return original;
            }
            switch (index)
            {
                case 5: return Cat(x,y);
                case 6: return Puppy(x,y);
                case 7: return Bear(x,y);
                case 8: return Fox(x,y);
                case 9: return Owl(x,y);
                case 10: return Fish(x,y);
                case 11: return Turtle(x,y);
                case 12: return Whale(x,y);
                case 13: return Crab(x,y);
                case 14: return Chick(x,y);
                case 15: return Duck(x,y);
                case 16: return Swan(x,y);
                case 17: return Seahorse(x,y);
                case 18: return Dinosaur(x,y);
                case 19: return Elephant(x,y);
                case 20: return Octopus(x,y);
                case 21: return Jellyfish(x,y);
                case 22: return Bee(x,y);
                case 23: return Ladybug(x,y);
                case 24: return Dragonfly(x,y);
                case 25: return Parrot(x,y);
                case 26: return Unicorn(x,y);
                case 27: return Dragon(x,y);
                case 28: return Flamingo(x,y);
                case 29: return Squirrel(x,y);
                case 30: return Hedgehog(x,y);
                case 31: return Frog(x,y);
                case 32: return Panda(x,y);
                case 33: return Raccoon(x,y);
                case 34: return Toucan(x,y);
                case 35: return Strawberry(x,y);
                case 36: return Pineapple(x,y);
                case 37: return Mushroom(x,y);
                case 38: return Sunflower(x,y);
                case 39: return Tulip(x,y);
                case 40: return Crown(x,y);
                case 41: return Castle(x,y);
                case 42: return Anchor(x,y);
                case 43: return Guitar(x,y);
                case 44: return Balloon(x,y);
                case 45: return Sailboat(x,y);
                case 46: return IceCream(x,y);
                case 47: return Teapot(x,y);
                case 48: return Windmill(x,y);
                case 49: return Peacock(x,y);
                default: return Color.clear;
            }
        }
    }
}

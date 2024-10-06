using UnityEngine;

public static class Constants {


    //types of Creatures Animations
    public enum AnimationsTypes {
        TakeDamage,
        Attack,
        LookAround,
        Feed,
        Roar,
        CastSpell,
        Dodge,
        Spin,
        SpinToIdel,
        SpawnFromMouth
    }

    public enum GroupsTypes //Stinky Ball and Block Vision particles and creatures colors
    {
        circle,
        rectangle,
        triangle
    }

    //Enums
    public enum ObjectsColors //Stinky Ball and Block Vision particles and creatures colors
    {
        Brown,
        Blue,
        Red,
        Purple,
        Green
    }



    //Tags
    public const string SuppliesCallerTag = "SuppliesCaller"; //tag of the balloon which calls the supplies
    public const string PlayerAttackPoint = "PlayerAttackPoint"; //tag of the point where the air creatures are gonna attack the player
    public const string PlayerLookAtPoint = "PlayerLookAtPoint"; //tag of the point where the air creatures are gonna lookat the player
    public const string PlayerShootAtPoint = "PlayerShootAtPoint"; //tag of the point where the creatures who has guns shoot at
    public const string OnStartWaves = "OnStartWaves"; //tag of the cinimatec creatures(start creatures)
    public const string TeleportObject = "Teleport Object";
    public const string SecurityWeapon = "SecurityWeapon"; //tag of the Security Weapon
    public const string SecuritySensor = "SecuritySensor"; //tag of the Security Sensor of the Security weapon
    public const string Arrow = "Arrow"; //tag of the Arrow
    public const string GateDoor = "GateDoor"; //tag of the GateDoor

    //Animation clips names
    public const string StinkyBallGrowingAnimation = "Growing"; //name of anmation clip of the stinky ball growing
    public const string DamageTextMotionAnimation = "DamageTextMotion"; //name of anmation clip of the creature Damade taken text
    public const string FlyForwardAnimation = "Fly Forward"; //name of anmation clip of the air creature fly forward

    private const string creatureTakeDamageAnimation = "Take Damage"; //name of anmation clip of the creature take damage
    private const string creatureRoarAnimation = "Roar"; //name of anmation clip of the creature Roar
    private const string creatureCastSpellAnimations = "Cast Spell"; //name of anmation clip of the creature Cast Spell
    private const string creatureFeedAnimation = "Feeding"; //name of anmation clip of the creature Feeding
    private const string creatureLookAroundAnimation = "Look Around"; //name of anmation clip of the creature Look Around


    private const string garooAttackAnimation = "Bite Attack";
    private const string longtailTakeDamageAnimation = "Fly Take Damage";
    private const string longtailCastSpellAnimations = "Fly Cast Spell";
    private const string longtailDodgeAnimations = "Fly Dodge";

    private const string maganteeAttackAnimation = "Bite Attack";
    private const string maganteeSpinAnimations = "Rolled Up Spin Fast";
    private const string maganteeSpinToIdelAnimations = "Rolled Up To Idle";
    private const string maganteeSpawnFromMouthAnimation = "Spawn From Mouth";
    private const string magantisSpawnFromMouthAnimation = "Spawn Bug From Mouth";
    private const string sicklusDodgeAnimations = "Dodge";



    //Layers number
    public const int ENEMY_LAYER_ID = 6;
    public const int PROJECTILE_LAYER_ID = 8;
    public const int IGNORE_RAYCAST_LAYER_ID = 2;
    public const int Arrow_LAYER_ID = 7;
    public const int Terrain_LAYER_ID = 3;
    public const int Base_LAYER_ID = 9;

    private static readonly string[] scorpionAttackAnimations = new string[5] { "Front Legs Attack", "Tail Flick Attack", "Tail Slice Attack", "Tail Stab Attack", "Tail Swing Attack" };

    private static readonly string[] longtailAttackAnimations = new string[2] { "Fly Spin Attack", "Fly Stab Attack" };

    private static readonly string[] magantisAttackAnimations = new string[4] { "Claw Attack", "Sting Attack", "Swing Left Attack", "Swing Right Attack" };

    private static readonly string[] serpentAttackAnimations = new string[4] { "Bite Attack", "Claw Attack Left", "Claw Attack Right", "Spit Attack" };

    private static readonly string[] sicklusAttackAnimations = new string[2] { "Bite Attack", "Tail Attack" };

    private static readonly string[] telekinisAttackAnimations = new string[2] { "Projectile Attack 01", "Projectile Attack 02" };

    private static readonly string[] ulifoAttackAnimations = new string[4] { "Left Slice Attack", "Leg Attack", "Right Slice Attack", "Stomp Attack" };


    #region Fmod

    public const string FmodMusicBus = "bus:/Music";
    public const string FmodSfxBus = "bus:/Sfx";

    #endregion



    /// <summary>
    /// </summary>
    /// <param name="creatureName">Name of the creature game object that you want to get it's animation</param>
    /// <param name="animationType">Type of the animation that you want</param>
    /// <returns></returns>
    public static string GetAnimationName(string creatureName, AnimationsTypes animationType) {
        switch (animationType) {
            case AnimationsTypes.TakeDamage: {
                if (creatureName.Contains("Longtail")) return longtailTakeDamageAnimation;
                return creatureTakeDamageAnimation;
            }

            case AnimationsTypes.Attack: {
                if (creatureName.Contains("Garoo")) return garooAttackAnimation;
                if (creatureName.Contains("Scorpion")) return scorpionAttackAnimations[Random.Range(0, scorpionAttackAnimations.Length)];
                if (creatureName.Contains("Longtail")) return longtailAttackAnimations[Random.Range(0, longtailAttackAnimations.Length)];
                if (creatureName.Contains("Magantee")) return maganteeAttackAnimation;
                if (creatureName.Contains("Magantis")) return magantisAttackAnimations[Random.Range(0, magantisAttackAnimations.Length)];
                if (creatureName.Contains("Serpent")) return serpentAttackAnimations[Random.Range(0, serpentAttackAnimations.Length)];
                if (creatureName.Contains("Sicklus")) return sicklusAttackAnimations[Random.Range(0, sicklusAttackAnimations.Length)];
                if (creatureName.Contains("Telekinis")) return telekinisAttackAnimations[Random.Range(0, telekinisAttackAnimations.Length)];
                if (creatureName.Contains("Ulifo")) return ulifoAttackAnimations[Random.Range(0, ulifoAttackAnimations.Length)];
            }
                break;

            case AnimationsTypes.LookAround: return creatureLookAroundAnimation;
            case AnimationsTypes.Feed: return creatureFeedAnimation;
            case AnimationsTypes.Roar: return creatureRoarAnimation;
            case AnimationsTypes.CastSpell: {
                if (creatureName.Contains("Longtail")) return longtailCastSpellAnimations;
                return creatureCastSpellAnimations;
            }

            case AnimationsTypes.Dodge: {
                if (creatureName.Contains("Longtail")) return longtailDodgeAnimations;
                return sicklusDodgeAnimations;
            }

            case AnimationsTypes.Spin: return maganteeSpinAnimations;
            case AnimationsTypes.SpinToIdel: return maganteeSpinToIdelAnimations;
            case AnimationsTypes.SpawnFromMouth: {
                if (creatureName.Contains("Magantis")) return magantisSpawnFromMouthAnimation;
                return maganteeSpawnFromMouthAnimation;
            }
        }

        Debug.LogError(animationType + " Animation has not been Found For " + creatureName + " Creature, function return null");
        return null;
    }
}
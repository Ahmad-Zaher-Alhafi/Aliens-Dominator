using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Constants
{
    //Tags
    public const string SuppliesCallerTag = "SuppliesCaller";                   //tag of the balloon which calls the supplies
    public const string PlayerAttackPoint = "PlayerAttackPoint";                //tag of the point where the air creatures are gonna attack the player
    public const string PlayerLookAtPoint = "PlayerLookAtPoint";                //tag of the point where the air creatures are gonna lookat the player
    public const string PlayerShootAtPoint = "PlayerShootAtPoint";              //tag of the point where the creatures who has guns shoot at
    public const string OnStartWaves = "OnStartWaves";                          //tag of the cinimatec creatures(start creatures)
    public const string SecurityWeapon = "SecurityWeapon";                      //tag of the Security Weapon
    public const string SecuritySensor = "SecuritySensor";                      //tag of the Security Sensor of the Security weapon
    public const string Arrow = "Arrow";                                        //tag of the Arrow
    public const string GateDoor = "GateDoor";                                  //tag of the GateDoor

    //Tags for the body parts in a dictionary, so the 
    public static Dictionary<string, BodyParts> EnemyBodyParts { get; } = new Dictionary<string, BodyParts>()
    {
        { "EnemyHead", BodyParts.Head },
        { "EnemyBody", BodyParts.Body },
        { "EnemyLegs", BodyParts.Legs },
        { "EnemyArms", BodyParts.Arms },
        { "EnemyTail", BodyParts.Tail }
    };

    //Animation clips names
    public const string StinkyBallGrowingAnimation = "Growing";                  //name of anmation clip of the stinky ball growing
    public const string DamageTextMotionAnimation = "DamageTextMotion";          //name of anmation clip of the creature Damade taken text
    public const string FlyForwardAnimation = "Fly Forward";                     //name of anmation clip of the air creature fly forward

    private const string creatureTakeDamageAnimation = "Take Damage";            //name of anmation clip of the creature take damage
    private const string creatureRoarAnimation = "Roar";                         //name of anmation clip of the creature Roar
    private const string creatureCastSpellAnimations = "Cast Spell";             //name of anmation clip of the creature Cast Spell
    private const string creatureFeedAnimation = "Feeding";                      //name of anmation clip of the creature Feeding
    private const string creatureLookAroundAnimation = "Look Around";            //name of anmation clip of the creature Look Around


    private const string garooAttackAnimation = "Bite Attack";

    private static string[] scorpionAttackAnimations = new string[5] { "Front Legs Attack", "Tail Flick Attack", "Tail Slice Attack", "Tail Stab Attack", "Tail Swing Attack" };

    private static string[] longtailAttackAnimations = new string[2] { "Fly Spin Attack", "Fly Stab Attack" };
    private const string longtailTakeDamageAnimation = "Fly Take Damage";
    private const string longtailCastSpellAnimations = "Fly Cast Spell";
    private const string longtailDodgeAnimations = "Fly Dodge";

    private const string maganteeAttackAnimation = "Bite Attack";
    private const string maganteeSpinAnimations = "Rolled Up Spin Fast";
    private const string maganteeSpinToIdelAnimations = "Rolled Up To Idle";
    private const string maganteeSpawnFromMouthAnimation = "Spawn From Mouth";

    private static string[] magantisAttackAnimations = new string[4] { "Claw Attack", "Sting Attack", "Swing Left Attack", "Swing Right Attack" };
    private const string magantisSpawnFromMouthAnimation = "Spawn Bug From Mouth";

    private static string[] serpentAttackAnimations = new string[4] { "Bite Attack", "Claw Attack Left", "Claw Attack Right", "Spit Attack" };

    private static string[] sicklusAttackAnimations = new string[2] { "Bite Attack", "Tail Attack" };
    private const string sicklusDodgeAnimations = "Dodge";

    private static string[] telekinisAttackAnimations = new string[2] { "Projectile Attack 01", "Projectile Attack 02" };

    private static string[] ulifoAttackAnimations = new string[4] { "Left Slice Attack", "Leg Attack", "Right Slice Attack", "Stomp Attack" };



    //Layers number
    public const int enemyLayerNumber = 6;                                      //number of Enemy layer
    public const int projectileLayerNumber = 8;                                 //number of projectile layer
    public const int ignorRaycastLayerNumber = 2;                               //number of ignorRaycast layer


    //types of Creatures Animations
    public enum AnimationsTypes
    {
        TakeDamage,
        Attack,
        LookAround,
        Feed,
        Roar,
        CastSpell,
        Dodge,
        Spin,
        SpinToIdel,
        SpawnFromMouth,
    }

    //Enums
    public enum ObjectsColors//Stinky Ball and Block Vision particles and creatures colors
    {
        Brown,
        Blue,
        Red,
        Purple,
        Green
    }

    public enum GroupsTypes//Stinky Ball and Block Vision particles and creatures colors
    {
        circle,
        rectangle,
        triangle,
    }

    public enum SecurityWeaponsTypes//type of security weapons
    {
        ground,
        air
    }

    public enum SuppliesTypes//types of supplies boxes
    {
        ArrowUpgrade,
        RocketsAmmo,
        BulletsAmmo
    }

    
    public enum BodyParts//Stores the body tags in enum, so its easier to setup in inspector
    {
        Head,
        Body,
        Legs,
        Arms,
        Tail
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="creatureName">Name of the creature game object that you want to get it's animatoin</param>
    /// <param name="animationType">Type of the animation that you want</param>
    /// <returns></returns>
    public static string GetAnimationName(string creatureName, AnimationsTypes animationType)
    {
        switch (animationType)
        {
            case AnimationsTypes.TakeDamage:
                {
                    if (creatureName.Contains("Longtail"))
                    {
                        return longtailTakeDamageAnimation;
                    }
                    else
                    {
                        return creatureTakeDamageAnimation;
                    }
                }

            case AnimationsTypes.Attack:
                {
                    if (creatureName.Contains("Garoo"))
                    {
                        return garooAttackAnimation;
                    }
                    else if (creatureName.Contains("Scorpion"))
                    {
                        return scorpionAttackAnimations[Random.Range(0, scorpionAttackAnimations.Length)];
                    }
                    else if (creatureName.Contains("Longtail"))
                    {
                        return longtailAttackAnimations[Random.Range(0, longtailAttackAnimations.Length)];
                    }
                    else if (creatureName.Contains("Magantee"))
                    {
                        return maganteeAttackAnimation;
                    }
                    else if (creatureName.Contains("Magantis"))
                    {
                        return magantisAttackAnimations[Random.Range(0, magantisAttackAnimations.Length)];
                    }
                    else if (creatureName.Contains("Serpent"))
                    {
                        return serpentAttackAnimations[Random.Range(0, serpentAttackAnimations.Length)];
                    }
                    else if (creatureName.Contains("Sicklus"))
                    {
                        return sicklusAttackAnimations[Random.Range(0, sicklusAttackAnimations.Length)];
                    }
                    else if (creatureName.Contains("Telekinis"))
                    {
                        return telekinisAttackAnimations[Random.Range(0, telekinisAttackAnimations.Length)];
                    }
                    else if (creatureName.Contains("Ulifo"))
                    {
                        return ulifoAttackAnimations[Random.Range(0, ulifoAttackAnimations.Length)];
                    }
                }
                break;

            case AnimationsTypes.LookAround: return creatureLookAroundAnimation;
            case AnimationsTypes.Feed: return creatureFeedAnimation;
            case AnimationsTypes.Roar: return creatureRoarAnimation;
            case AnimationsTypes.CastSpell:
                {
                    if (creatureName.Contains("Longtail"))
                    {
                        return longtailCastSpellAnimations;
                    }
                    else
                    {
                        return creatureCastSpellAnimations;
                    }
                }

            case AnimationsTypes.Dodge:
                {
                    if (creatureName.Contains("Longtail"))
                    {
                        return longtailDodgeAnimations;
                    }
                    else
                    {
                        return sicklusDodgeAnimations;
                    }
                }

            case AnimationsTypes.Spin: return maganteeSpinAnimations;
            case AnimationsTypes.SpinToIdel: return maganteeSpinToIdelAnimations;
            case AnimationsTypes.SpawnFromMouth:
                {
                    if (creatureName.Contains("Magantis"))
                    {
                        return magantisSpawnFromMouthAnimation;
                    }
                    else
                    {
                        return maganteeSpawnFromMouthAnimation;
                    }
                }


            default: break;
        }

        Debug.LogError(animationType + " Animation has not been Found For " + creatureName + " Creature, function return null");
        return null;
    }
}

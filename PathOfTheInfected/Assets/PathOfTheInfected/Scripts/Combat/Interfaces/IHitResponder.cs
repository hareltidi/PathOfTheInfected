using System;

namespace PathOfTheInfected.Combat
{
    ///<summary>
    /// Defines an interface for objects that can respond to hit events in the combat system.
    /// Implementing this interface allows objects to define specific behaviors or responses
    /// when they are hit during gameplay interactions.
    ///</summary>
    public interface IHitResponder
    {
        bool HitRecurseUpwards => true;
        int HitPriority => 0;


        ///<summary>
        /// Represents a hit event in the combat system.
        /// This method handles the interaction logic when an object is struck,
        /// including damage application, state changes, or other custom behaviors
        /// defined by the implementing object.
        ///</summary>
        /// <param name="damageData">
        /// An instance of <see cref="HitData" /> containing information about the hit,
        /// such as the source, attack type, and state of the attacker.
        /// </param>
        /// <returns>A <see cref="HitResponse" /> object encapsulating the outcome or reaction to the hit event.</returns>
        HitResponse OnHit(HitData damageData);
    }

    ///<summary>
    /// Represents the type of response triggered by a hit event in the combat system.
    /// This enumeration is used to categorize and define specific reactions to being hit,
    /// such as causing damage, becoming invincible, or performing no action.
    ///</summary>
    public enum Response
    {
        None,
        GenericHit,
        DamageEnemy,
        DamagePlayer,
        Invincible,
        Blocked
    }


    ///<summary>
    /// Represents a response to a hit event in the combat system.
    /// This struct is used to define the behavior and outcome when an object responds to being hit.
    ///</summary>
    public struct HitResponse : IEquatable<HitResponse>
    {
        ///<summary>
        /// Represents the type of response triggered by a hit event in the combat system.
        /// This enumeration is used to categorize and define specific reactions to being hit,
        /// such as causing damage, becoming invincible, or performing no action.
        ///</summary>
        public Response Response;

        ///<summary>
        /// Indicates whether the hit response should consume charges upon activation.
        /// This property is used to determine if the associated action or effect triggered by the hit
        /// needs to expend a finite resource, such as durability, charges, or energy,
        /// as part of its behavior in the combat system.
        ///</summary>
        public bool ConsumeCharges;



        /// <summary>
        /// Represents the total amount of damage dealt after all calculations, including modifiers, resistances,
        /// and other adjustments, have been applied to the base damage value.
        /// </summary>
        public int FinalDamage;

        ///<summary>
        /// Represents a response to a hit event in the combat system.
        /// This struct encapsulates the specific reaction or outcome of an object
        /// when it is hit during gameplay interactions, such as applying damage,
        /// triggering invincibility, or performing no action.
        ///</summary>
        public HitResponse(Response response, bool consumeCharges, int finalDamage)
        {
            Response = response;
            ConsumeCharges = consumeCharges;
            FinalDamage = finalDamage;
        }

        public static HitResponse Default = new()
        {
            Response = Response.None,
            ConsumeCharges = true
        };

        public static implicit operator HitResponse(Response response)
        {
            return new HitResponse
            {
                Response = response,
                ConsumeCharges = response == Response.DamageEnemy
            };
        }

        public static implicit operator Response(HitResponse hitResponse)
        {
            return hitResponse.Response;
        }

        public bool Equals(HitResponse other)
        {
            return Response == other.Response && ConsumeCharges == other.ConsumeCharges;
        }

        public override bool Equals(object obj)
        {
            return obj is HitResponse other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Response, ConsumeCharges);
        }
    }
}
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
        /// <summary>
        /// Represents the absence of any response to a hit event.
        /// When assigned, no action is taken within the combat system in reaction to the hit.
        /// This response is typically used as a default or neutral state.
        /// </summary>
        None,
        /// <summary>
        /// Represents a generic hit response, typically used for non-specific effects.
        /// </summary>
        GenericHit,
        /// <summary>
        /// Represents a damage response triggered by a hit event.
        /// </summary>
        DamageEnemy,
        /// <summary>
        /// Represents a damage response triggered by a hit event.
        /// </summary>
        DamagePlayer,
        /// <summary>
        /// Represents an invincibility response triggered by a hit event.
        /// </summary>
        Invincible,
        /// <summary>
        /// Represents a block response triggered by a hit event.
        /// </summary>
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
        public float FinalDamage;

        /// <summary>
        ///  Represents a response to a hit event in the combat system.
        ///  This struct encapsulates the specific reaction or outcome of an object
        ///  when it is hit during gameplay interactions, such as applying damage,
        ///  triggering invincibility, or performing no action.
        /// </summary>
        ///  <param name="response">The type of <see cref="Response"/> triggered by the hit event.</param>
        /// <param name="consumeCharges">Should the attack "hurt" our resources (health, poise, etc.)?</param>
        /// <param name="finalDamage">The final damage we need to inflict to the target
        /// (Should be calculated using <see cref="DamageCalculator"/>)</param>
        public HitResponse(Response response, bool consumeCharges, float finalDamage)
        {
            Response = response;
            ConsumeCharges = consumeCharges;
            FinalDamage = finalDamage;
        }

        /// <summary>
        /// Represents the default hit response configuration used in the combat system.
        /// This static member provides a pre-defined response with default behavior,
        /// such as no specified reaction and charges being consumed.
        /// </summary>
        public static HitResponse Default = new()
        {
            Response = Response.None,
            ConsumeCharges = true
        };

        /// <summary>
        /// Defines an implicit conversion operator that allows a <see cref="Response"/> enumeration
        /// value to be converted into a <see cref="HitResponse"/> instance.
        /// When converted, it initializes a new <see cref="HitResponse"/> object with the input
        /// response value and applies specific rules to set additional properties, such as
        /// <c>ConsumeCharges</c>.
        /// </summary>
        /// <param name="response">
        /// The <see cref="Response"/> enumeration value to convert into a <see cref="HitResponse"/>
        /// instance.
        /// </param>
        /// <returns>
        /// A new <see cref="HitResponse"/> instance constructed with the provided
        /// <see cref="Response"/> value.
        /// </returns>
        public static implicit operator HitResponse(Response response)
        {
            return new HitResponse
            {
                Response = response,
                ConsumeCharges = response == Response.DamageEnemy
            };
        }

        /// <summary>
        /// Defines an implicit conversion operation from <see cref="HitResponse" /> to <see cref="Response" />.
        /// This conversion allows a <see cref="HitResponse" /> instance to be directly treated as its encapsulated
        /// <see cref="Response" /> value, simplifying usage in scenarios where only the response type is needed.
        /// </summary>
        /// <param name="hitResponse">
        /// The <see cref="HitResponse" /> instance to convert to a <see cref="Response" />.
        /// Contains details of the hit response, including its response type, charge consumption status, and final damage.
        /// </param>
        /// <returns>
        /// The <see cref="Response" /> value encapsulated within the given <see cref="HitResponse" /> instance.
        /// </returns>
        public static implicit operator Response(HitResponse hitResponse)
        {
            return hitResponse.Response;
        }

        /// <summary>
        /// Determines whether the current <see cref="HitResponse"/> instance is equal to another specified <see cref="HitResponse"/> instance.
        /// This method compares the properties of the two instances, including the response type,
        /// whether charges are consumed, and other relevant response details.
        /// </summary>
        /// <param name="other">
        /// A <see cref="HitResponse"/> instance to compare with the current instance.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the current instance is equal to the specified instance.
        /// </returns>
        public bool Equals(HitResponse other)
        {
            return Response == other.Response && ConsumeCharges == other.ConsumeCharges;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="HitResponse" /> instance.
        /// This method is used to compare the equality of the current instance with another object based on their properties.
        /// </summary>
        /// <param name="obj">
        /// The object to compare with the current <see cref="HitResponse" />. This can be another instance of
        /// <see cref="HitResponse" /> or an object to check for equivalence.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the specified object is equal to the current <see cref="HitResponse" />.
        /// Returns <c>true</c> if the two objects are equal, otherwise <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is HitResponse other && Equals(other);
        }

        /// <summary>
        /// Generates a hash code for the current <see cref="HitResponse"/> instance.
        /// The hash code is computed using the <see cref="Response"/> value and
        /// the <see cref="ConsumeCharges"/> field, ensuring unique identification
        /// for distinct state configurations.
        /// </summary>
        /// <returns>An integer hash code representing the current instance of <see cref="HitResponse"/>.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine((int)Response, ConsumeCharges);
        }
    }
}
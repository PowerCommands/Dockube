/*
 * DockubeTemplate
 *
 * This is a service contract for the Dockube API.
 *
 * The version of the OpenAPI document: 2.0
 * 
 * Generated by: https://openapi-generator.tech
 */

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using DockCubeApi.Server.Converters;

namespace DockCubeApi.Server.Models
{ 
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Pod : IEquatable<Pod>
    {
        /// <summary>
        /// The metadata name of the pod
        /// </summary>
        /// <value>The metadata name of the pod</value>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// The current status of the pod
        /// </summary>
        /// <value>The current status of the pod</value>
        [DataMember(Name="phase", EmitDefaultValue=false)]
        public string Phase { get; set; }

        /// <summary>
        /// The kubernetes kind
        /// </summary>
        /// <value>The kubernetes kind</value>
        [DataMember(Name="kind", EmitDefaultValue=false)]
        public string Kind { get; set; }

        /// <summary>
        /// The uniquee identifier.
        /// </summary>
        /// <value>The uniquee identifier.</value>
        [DataMember(Name="uid", EmitDefaultValue=false)]
        public string Uid { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Pod {\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Phase: ").Append(Phase).Append("\n");
            sb.Append("  Kind: ").Append(Kind).Append("\n");
            sb.Append("  Uid: ").Append(Uid).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Pod)obj);
        }

        /// <summary>
        /// Returns true if Pod instances are equal
        /// </summary>
        /// <param name="other">Instance of Pod to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Pod other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return 
                (
                    Name == other.Name ||
                    Name != null &&
                    Name.Equals(other.Name)
                ) && 
                (
                    Phase == other.Phase ||
                    Phase != null &&
                    Phase.Equals(other.Phase)
                ) && 
                (
                    Kind == other.Kind ||
                    Kind != null &&
                    Kind.Equals(other.Kind)
                ) && 
                (
                    Uid == other.Uid ||
                    Uid != null &&
                    Uid.Equals(other.Uid)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                    if (Name != null)
                    hashCode = hashCode * 59 + Name.GetHashCode();
                    if (Phase != null)
                    hashCode = hashCode * 59 + Phase.GetHashCode();
                    if (Kind != null)
                    hashCode = hashCode * 59 + Kind.GetHashCode();
                    if (Uid != null)
                    hashCode = hashCode * 59 + Uid.GetHashCode();
                return hashCode;
            }
        }

        #region Operators
        #pragma warning disable 1591

        public static bool operator ==(Pod left, Pod right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Pod left, Pod right)
        {
            return !Equals(left, right);
        }

        #pragma warning restore 1591
        #endregion Operators
    }
}

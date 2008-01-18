using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Halligang.CsXmpToolkit
{
	[Flags]
	public enum PropertyFlags : uint
	{
		/// <summary>
		/// None specified.
		/// </summary>
		None				= 0x00000000,

		/// <summary>
		/// The value is a URI, use rdf:resource attribute. DISCOURAGED
		/// </summary>
		ValueIsUri			= 0x00000002,

		/// <summary>
		/// The property has qualifiers, includes rdf:type and xml:lang.
		/// </summary>
		HasQualifiers		= 0x00000010,

		/// <summary>
		/// This is a qualifier, includes rdf:type and xml:lang.
		/// </summary>
		IsQualifier			= 0x00000020,

		/// <summary>
		/// Implies kXMP_PropHasQualifiers, property has xml:lang.
		/// </summary>
		HasLang				= 0x00000040,

		/// <summary>
		/// Implies kXMP_PropHasQualifiers, property has rdf:type.
		/// </summary>
		HasType				= 0x00000080,

		/// <summary>
		/// The value is a structure with nested fields.
		/// </summary>
		ValueIsStruct		= 0x00000100,

		/// <summary>
		/// The value is an array (RDF alt/bag/seq).
		/// </summary>
		ValueIsArray		= 0x00000200,

		/// <summary>
		/// The item order does not matter.
		/// </summary>
		ArrayIsUnordered	= ValueIsArray,

		/// <summary>
		/// Implies ValueIsArray, item order matters.
		/// </summary>
		ArrayIsOrdered		= 0x00000400,

		/// <summary>
		/// Implies ArrayIsOrdered, items are alternates.
		/// </summary>
		ArrayIsAlternate	= 0x00000800,

		/// <summary>
		/// Implies ArrayIsAlternate, items are localized text.
		/// </summary>
		ArrayIsAltText		= 0x00001000,

		/// <summary>
		/// This property is an alias name for another property.
		/// </summary>
		IsAlias				= 0x00010000,

		/// <summary>
		/// This property is the base value for a set of aliases.
		/// </summary>
		HasAliases			= 0x00020000,

		/// <summary>
		/// This property is an "internal" property, owned by applications.
		/// </summary>
		IsInternal			= 0x00040000,

		/// <summary>
		/// This property is not derived from the document content.
		/// </summary>
		IsStable			= 0x0010000,

		/// <summary>
		/// This property is derived from the document content.
		/// </summary>
		IsDerived			= 0x00200000,

		/// <summary>
		/// 
		/// </summary>
		ArrayForm			= ValueIsArray | ArrayIsOrdered | ArrayIsAlternate | ArrayIsAltText,

		/// <summary>
		/// Is it simple or composite (array or struct)?
		/// </summary>
		Composite			= ValueIsStruct | ArrayForm,

		/// <summary>
		/// 
		/// </summary>
		SchemaNode			= 0x80000000,

		/// <summary>
		/// NOTE: Valid for Set... functions. Delete any pre-existing property.
		/// </summary>
		DeleteExisting		= 0x20000000,

		/// <summary>
		/// NOTE: Valid for SetArrayItem. Insert a new item before the given index.
		/// </summary>
		InsertBeforeItem	= 0x00004000,

		/// <summary>
		/// NOTE: Valid for SetArrayItem. Insert a new item after the given index.
		/// </summary>
		InsertAfterItem		= 0x00008000,
	}
}

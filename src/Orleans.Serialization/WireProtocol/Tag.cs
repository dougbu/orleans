using System.Runtime.CompilerServices;

namespace Orleans.Serialization.WireProtocol
{
    /// <summary>
    /// <see langword="abstract" />Tag is the
    /// </summary>
    /// <remarks>
    /// The typical form for a tag byte is <c>[W W W] [S S] [F F F]</c>, where each is a bit.
    /// W is a <see cref="WireType"/>, S is a <see cref="SchemaType"/> bit, and F is a field identifier bit.
    /// </remarks>
    public struct Tag
    {
        /// <summary>
        /// The wire type mask.
        /// </summary>
        public const byte WireTypeMask = 0b1110_0000; // The first 3 bits are dedicated to the wire type.

        /// <summary>
        /// The schema type mask.
        /// </summary>
        public const byte SchemaTypeMask = 0b0001_1000; // The next 2 bits are dedicated to the schema type specifier, if the schema type is expected.

        /// <summary>
        /// The field identifier mask.
        /// </summary>
        public const byte FieldIdMask = 0b000_0111; // The final 3 bits are used for the field id delta, if the delta is expected.

        /// <summary>
        /// The field identifier complete mask.
        /// </summary>
        public const byte FieldIdCompleteMask = 0b0000_0111;

        /// <summary>
        /// The extended wire type mask.
        /// </summary>
        public const byte ExtendedWireTypeMask = 0b0001_1000;

        /// <summary>
        /// The maximum embedded field identifier delta.
        /// </summary>
        public const int MaxEmbeddedFieldIdDelta = 6;

        private byte _tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> struct.
        /// </summary>
        /// <param name="tag">The tag byte.</param>
        public Tag(byte tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Byte"/> to <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Tag(byte tag) => new Tag(tag);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Tag"/> to <see cref="System.Byte"/>.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator byte(Tag tag) => tag._tag;

        /// <summary>
        /// Gets or sets the wire type of the data following this tag.
        /// </summary>
        public WireType WireType
        {
            get => (WireType)(_tag & WireTypeMask);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _tag = (byte)((_tag & ~WireTypeMask) | ((byte)value & WireTypeMask));
        }

        /// <summary>
        /// Gets a value indicating whether this instance has an extended wire type.
        /// </summary>
        /// <value><see langword="true" /> if this instance has an extended wire type; otherwise, <see langword="false" />.</value>
        public bool HasExtendedWireType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _tag >= (byte)WireType.Extended; //(this.tag & (byte) WireType.Extended) == (byte) WireType.Extended;
        }

        /// <summary>
        /// Gets or sets the extended wire type of the data following this tag.
        /// </summary>
        public ExtendedWireType ExtendedWireType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ExtendedWireType)(_tag & ExtendedWireTypeMask);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _tag = (byte)((_tag & ~ExtendedWireTypeMask) | ((byte)value & ExtendedWireTypeMask));
        }

        internal bool IsEndBaseFields => _tag == ((byte)WireType.Extended | (byte)ExtendedWireType.EndBaseFields);

        internal bool IsEndObject => _tag == ((byte)WireType.Extended | (byte)ExtendedWireType.EndTagDelimited);

        /// <summary>
        /// Gets or sets the schema type.
        /// </summary>
        public SchemaType SchemaType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (SchemaType)(_tag & SchemaTypeMask);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _tag = (byte)((_tag & ~SchemaTypeMask) | ((byte)value & SchemaTypeMask));
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SchemaType" /> property is valid.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="SchemaType"/> is valid, <see langword="false"/> otherwise.</value>
        public bool IsSchemaTypeValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !HasExtendedWireType; //(this.tag & (byte) WireType.Extended) != (byte) WireType.Extended;
        }

        /// <summary>
        /// Returns the <see cref="FieldIdDelta"/> of the field represented by this tag.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsFieldIdValid"/> is <see langword="false"/>, this value is not a complete field id delta.
        /// </remarks>
        public uint FieldIdDelta
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)(_tag & FieldIdMask);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _tag = (byte)((_tag & ~FieldIdMask) | ((byte)value & FieldIdMask));
        }

        /// <summary>
        /// Invalidates <see cref="FieldIdDelta"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFieldIdInvalid() => _tag |= FieldIdCompleteMask;

        /// <summary>
        /// Gets a value indicating whether the <see cref="FieldIdDelta"/> property is valid.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="FieldIdDelta"/> represents a complete id, <see langword="false"/> if more data is required.
        /// </value>
        /// <remarks>
        /// If all bits are set in the field id portion of the tag, this field id is not valid and this tag must be followed by a field id.
        /// Therefore, field ids 0-7 can be represented without additional bytes.
        /// </remarks>
        public bool IsFieldIdValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_tag & FieldIdCompleteMask) != FieldIdCompleteMask && !HasExtendedWireType;
        }

        /// <summary>
        /// Gets a value indicating whether the tag is followed by an extended field id.
        /// </summary>
        public bool HasExtendedFieldId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_tag & FieldIdCompleteMask) == FieldIdCompleteMask && !HasExtendedWireType;
        }
    }
}
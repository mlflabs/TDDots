namespace BovineLabs.Event.Containers
{
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe partial struct UnsafeEventStream
    {
        /// <summary>
        /// </summary>
        [BurstCompatible]
        public unsafe struct Reader
        {
            [NativeDisableUnsafePtrRestriction]
            internal UnsafeEventStreamBlockData* MBlockStream;

            [NativeDisableUnsafePtrRestriction]
            internal UnsafeEventStreamBlock* MCurrentBlock;

            [NativeDisableUnsafePtrRestriction]
            internal byte* MCurrentPtr;

            [NativeDisableUnsafePtrRestriction]
            internal byte* MCurrentBlockEnd;

            internal int MRemainingItemCount;
            internal int MLastBlockSize;

            internal Reader(ref UnsafeEventStream stream)
            {
                this.MBlockStream = stream._blockData;
                this.MCurrentBlock = null;
                this.MCurrentPtr = null;
                this.MCurrentBlockEnd = null;
                this.MRemainingItemCount = 0;
                this.MLastBlockSize = 0;
            }

            /// <summary>
            /// Begin reading data at the iteration index.
            /// </summary>
            /// <param name="foreachIndex"></param>
            /// <remarks>BeginForEachIndex must always be called balanced by a EndForEachIndex.</remarks>
            /// <returns>The number of elements at this index.</returns>
            public int BeginForEachIndex(int foreachIndex)
            {
                this.MRemainingItemCount = MBlockStream->Ranges[foreachIndex].ElementCount;
                this.MLastBlockSize = MBlockStream->Ranges[foreachIndex].LastOffset;

                this.MCurrentBlock = MBlockStream->Ranges[foreachIndex].Block;
                this.MCurrentPtr = (byte*)this.MCurrentBlock + MBlockStream->Ranges[foreachIndex].OffsetInFirstBlock;
                this.MCurrentBlockEnd = (byte*)this.MCurrentBlock + UnsafeEventStreamBlockData.AllocationSize;

                return this.MRemainingItemCount;
            }

            /// <summary>
            /// Ensures that all data has been read for the active iteration index.
            /// </summary>
            /// <remarks>EndForEachIndex must always be called balanced by a BeginForEachIndex.</remarks>
            public void EndForEachIndex()
            {
            }

            /// <summary>
            /// Returns for each count.
            /// </summary>
            public int ForEachCount => MBlockStream->RangeCount;

            /// <summary>
            /// Returns remaining item count.
            /// </summary>
            public int RemainingItemCount => MRemainingItemCount;

            /// <summary>
            /// Returns pointer to data.
            /// </summary>
            /// <param name="size">Size in bytes.</param>
            /// <returns>Pointer to data.</returns>
            public byte* ReadUnsafePtr(int size)
            {
                this.MRemainingItemCount--;

                byte* ptr = this.MCurrentPtr;
                this.MCurrentPtr += size;

                if (this.MCurrentPtr > this.MCurrentBlockEnd)
                {
                    this.MCurrentBlock = MCurrentBlock->Next;
                    this.MCurrentPtr = MCurrentBlock->Data;

                    this.MCurrentBlockEnd = (byte*)this.MCurrentBlock + UnsafeEventStreamBlockData.AllocationSize;

                    ptr = this.MCurrentPtr;
                    this.MCurrentPtr += size;
                }

                return ptr;
            }

            /// <summary>
            /// Read data.
            /// </summary>
            /// <typeparam name="T">The type of value.</typeparam>
            /// <returns>Reference to data.</returns>
            [BurstCompatible(GenericTypeArguments = new[] { typeof(int) })]
            public ref T Read<T>()
                where T : struct
            {
                int size = UnsafeUtility.SizeOf<T>();
#if UNITY_COLLECTIONS_0_14_OR_NEWER
                return ref UnsafeUtility.AsRef<T>(this.ReadUnsafePtr(size));
#else
                return ref UnsafeUtilityEx.AsRef<T>(this.ReadUnsafePtr(size));
#endif
            }

            /// <summary>
            /// Peek into data.
            /// </summary>
            /// <typeparam name="T">The type of value.</typeparam>
            /// <returns>Reference to data.</returns>
            [BurstCompatible(GenericTypeArguments = new[] { typeof(int) })]
            public ref T Peek<T>()
                where T : struct
            {
                int size = UnsafeUtility.SizeOf<T>();

                byte* ptr = this.MCurrentPtr;
                if (ptr + size > this.MCurrentBlockEnd)
                {
                    ptr = MCurrentBlock->Next->Data;
                }

#if UNITY_COLLECTIONS_0_14_OR_NEWER
                return ref UnsafeUtility.AsRef<T>(ptr);
#else
                return ref UnsafeUtilityEx.AsRef<T>(ptr);
#endif
            }

            /// <summary>
            /// The current number of items in the container.
            /// </summary>
            /// <returns>The item count.</returns>
            public int Count()
            {
                int itemCount = 0;
                for (int i = 0; i != MBlockStream->RangeCount; i++)
                {
                    itemCount += MBlockStream->Ranges[i].ElementCount;
                }

                return itemCount;
            }
        }
    }
}
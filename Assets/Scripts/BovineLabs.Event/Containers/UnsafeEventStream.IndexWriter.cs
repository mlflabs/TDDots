namespace BovineLabs.Event.Containers
{
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe partial struct UnsafeEventStream
    {
        /// <summary> The writer instance. </summary>
        public struct IndexWriter
        {
            [NativeDisableUnsafePtrRestriction]
            internal UnsafeEventStreamBlockData* MBlockStream;

            [NativeDisableUnsafePtrRestriction]
            private UnsafeEventStreamBlock* _mCurrentBlock;

            [NativeDisableUnsafePtrRestriction]
            private byte* _mCurrentPtr;

            [NativeDisableUnsafePtrRestriction]
            private byte* _mCurrentBlockEnd;

            internal int MForeachIndex;
            private int _mElementCount;

            [NativeDisableUnsafePtrRestriction]
            private UnsafeEventStreamBlock* _mFirstBlock;

            private int _mFirstOffset;
            private int _mNumberOfBlocks;

            [NativeSetThreadIndex]
            private int _mThreadIndex;

            internal IndexWriter(ref UnsafeEventStream stream)
            {
                this.MBlockStream = stream._blockData;
                this.MForeachIndex = int.MinValue;
                this._mElementCount = -1;
                this._mCurrentBlock = null;
                this._mCurrentBlockEnd = null;
                this._mCurrentPtr = null;
                this._mFirstBlock = null;
                this._mNumberOfBlocks = 0;
                this._mFirstOffset = 0;
                this._mThreadIndex = 0;
            }

            /// <summary>
            /// </summary>
            public int ForEachCount => MBlockStream->RangeCount;

            /// <summary>
            /// Begin reading data at the iteration index.
            /// </summary>
            /// <param name="foreachIndex"></param>
            /// <remarks>BeginForEachIndex must always be called balanced by a EndForEachIndex.</remarks>
            public void BeginForEachIndex(int foreachIndex)
            {
                this.MForeachIndex = foreachIndex;
                this._mElementCount = 0;
                this._mNumberOfBlocks = 0;
                this._mFirstBlock = this._mCurrentBlock;
                this._mFirstOffset = (int)(this._mCurrentPtr - (byte*)this._mCurrentBlock);
            }

            /// <summary>
            /// Ensures that all data has been read for the active iteration index.
            /// </summary>
            /// <remarks>EndForEachIndex must always be called balanced by a BeginForEachIndex.</remarks>
            public void EndForEachIndex()
            {
                MBlockStream->Ranges[this.MForeachIndex].ElementCount = this._mElementCount;
                MBlockStream->Ranges[this.MForeachIndex].OffsetInFirstBlock = this._mFirstOffset;
                MBlockStream->Ranges[this.MForeachIndex].Block = this._mFirstBlock;

                MBlockStream->Ranges[this.MForeachIndex].LastOffset = (int)(this._mCurrentPtr - (byte*)this._mCurrentBlock);
                MBlockStream->Ranges[this.MForeachIndex].NumberOfBlocks = this._mNumberOfBlocks;
            }

            /// <summary>
            /// Write data.
            /// </summary>
            /// <typeparam name="T">The type of value.</typeparam>
            /// <param name="value">Value to write.</param>
            [BurstCompatible(GenericTypeArguments = new[] { typeof(int) })]
            public void Write<T>(T value)
                where T : struct
            {
                ref T dst = ref this.Allocate<T>();
                dst = value;
            }

            /// <summary>
            /// Allocate space for data.
            /// </summary>
            /// <typeparam name="T">The type of value.</typeparam>
            /// <returns>Reference to allocated space for data.</returns>
            [BurstCompatible(GenericTypeArguments = new[] { typeof(int) })]
            public ref T Allocate<T>()
                where T : struct
            {
                int size = UnsafeUtility.SizeOf<T>();
#if UNITY_COLLECTIONS_0_14_OR_NEWER
                return ref UnsafeUtility.AsRef<T>(this.Allocate(size));
#else
                return ref UnsafeUtilityEx.AsRef<T>(this.Allocate(size));
#endif
            }

            /// <summary>
            /// Allocate space for data.
            /// </summary>
            /// <param name="size">Size in bytes.</param>
            /// <returns>Pointer to allocated space for data.</returns>
            public byte* Allocate(int size)
            {
                byte* ptr = this._mCurrentPtr;
                this._mCurrentPtr += size;

                if (this._mCurrentPtr > this._mCurrentBlockEnd)
                {
                    var oldBlock = this._mCurrentBlock;

                    this._mCurrentBlock = MBlockStream->Allocate(oldBlock, this._mThreadIndex);
                    this._mCurrentPtr = _mCurrentBlock->Data;

                    if (this._mFirstBlock == null)
                    {
                        this._mFirstOffset = (int)(this._mCurrentPtr - (byte*)this._mCurrentBlock);
                        this._mFirstBlock = this._mCurrentBlock;
                    }
                    else
                    {
                        this._mNumberOfBlocks++;
                    }

                    this._mCurrentBlockEnd = (byte*)this._mCurrentBlock + UnsafeEventStreamBlockData.AllocationSize;
                    ptr = this._mCurrentPtr;
                    this._mCurrentPtr += size;
                }

                this._mElementCount++;

                return ptr;
            }
        }
    }
}
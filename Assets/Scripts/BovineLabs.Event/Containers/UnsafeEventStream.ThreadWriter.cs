namespace BovineLabs.Event.Containers
{
    using Unity.Assertions;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Jobs.LowLevel.Unsafe;

    public unsafe partial struct UnsafeEventStream
    {
        /// <summary> The writer instance. </summary>
        public struct ThreadWriter
        {
            public const int ForEachCount = JobsUtility.MaxJobThreadCount;

            [NativeDisableUnsafePtrRestriction]
            internal UnsafeEventStreamBlockData* MBlockStream;

            [NativeSetThreadIndex]
            private int _mThreadIndex;

            internal ThreadWriter(ref UnsafeEventStream stream)
            {
                this.MBlockStream = stream._blockData;
                this._mThreadIndex = 0; // 0 so main thread works

                Assert.AreEqual(MBlockStream->RangeCount, JobsUtility.MaxJobThreadCount);

                for (var i = 0; i < JobsUtility.MaxJobThreadCount; i++)
                {
                    this.MBlockStream->Ranges[i].ElementCount = 0;
                    this.MBlockStream->Ranges[i].NumberOfBlocks = 0;
                    this.MBlockStream->Ranges[i].OffsetInFirstBlock = 0;
                    this.MBlockStream->Ranges[i].Block = null;
                    this.MBlockStream->Ranges[i].LastOffset = 0;

                    this.MBlockStream->ThreadRanges[i].CurrentBlock = null;
                    this.MBlockStream->ThreadRanges[i].CurrentBlockEnd = null;
                    this.MBlockStream->ThreadRanges[i].CurrentPtr = null;
                }
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
                var ptr = this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentPtr;
                var allocationEnd = ptr + size;
                this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentPtr = allocationEnd;

                if (allocationEnd > this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentBlockEnd)
                {
                    var oldBlock = this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentBlock;
                    var newBlock = MBlockStream->Allocate(oldBlock, this._mThreadIndex);

                    this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentBlock = newBlock;
                    this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentPtr = newBlock->Data;

                    if (this.MBlockStream->Ranges[this._mThreadIndex].Block == null)
                    {
                        this.MBlockStream->Ranges[this._mThreadIndex].OffsetInFirstBlock = (int)(newBlock->Data - (byte*)newBlock);
                        this.MBlockStream->Ranges[this._mThreadIndex].Block = newBlock;
                    }
                    else
                    {
                        this.MBlockStream->Ranges[this._mThreadIndex].NumberOfBlocks++;
                    }

                    this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentBlockEnd = (byte*)newBlock + UnsafeEventStreamBlockData.AllocationSize;

                    ptr = newBlock->Data;
                    this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentPtr = newBlock->Data + size;
                }

                this.MBlockStream->Ranges[this._mThreadIndex].ElementCount++;
                this.MBlockStream->Ranges[this._mThreadIndex].LastOffset = (int)(this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentPtr - (byte*)this.MBlockStream->ThreadRanges[this._mThreadIndex].CurrentBlock);

                return ptr;
            }
        }
    }
}
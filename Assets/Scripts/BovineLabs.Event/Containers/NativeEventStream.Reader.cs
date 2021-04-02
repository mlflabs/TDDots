namespace BovineLabs.Event.Containers
{
    using System;
    using System.Diagnostics;
    using Unity.Assertions;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe partial struct NativeEventStream
    {
        /// <summary> The reader instance. </summary>
        [NativeContainer]
        [NativeContainerIsReadOnly]
        public struct Reader
        {
            private UnsafeEventStream.Reader _reader;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            private int _remainingBlocks;
            private AtomicSafetyHandle _mSafety;
#endif

            internal Reader(ref NativeEventStream stream)
            {
                this._reader = stream._stream.AsReader();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                this._remainingBlocks = 0;
                this._mSafety = stream.m_Safety;
#endif
            }

            /// <summary> Gets the for each count. </summary>
            public int ForEachCount => this._reader.ForEachCount;

            /// <summary> Gets the remaining item count. </summary>
            public int RemainingItemCount => this._reader.RemainingItemCount;

            /// <summary> Begin reading data at the iteration index. </summary>
            /// <param name="foreachIndex"> The index to start reading. </param>
            /// <returns> The number of elements at this index. </returns>
            public int BeginForEachIndex(int foreachIndex)
            {
                this.CheckBeginForEachIndex(foreachIndex);

                var remainingItemCount = this._reader.BeginForEachIndex(foreachIndex);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                this._remainingBlocks = this._reader.MBlockStream->Ranges[foreachIndex].NumberOfBlocks;
                if (this._remainingBlocks == 0)
                {
                    this._reader.MCurrentBlockEnd = (byte*)this._reader.MCurrentBlock + this._reader.MLastBlockSize;
                }
#endif

                return remainingItemCount;
            }

            /// <summary> Ensures that all data has been read for the active iteration index. </summary>
            /// <remarks> EndForEachIndex must always be called balanced by a BeginForEachIndex. </remarks>
            public void EndForEachIndex()
            {
                this._reader.EndForEachIndex();
                this.CheckEndForEachIndex();
            }

            /// <summary> Returns pointer to data. </summary>
            /// <param name="size"> The size of the data to read. </param>
            /// <returns> The pointer to the data. </returns>
            public byte* ReadUnsafePtr(int size)
            {
                this.CheckReadSize(size);

                this._reader.MRemainingItemCount--;

                var ptr = this._reader.MCurrentPtr;
                this._reader.MCurrentPtr += size;

                if (this._reader.MCurrentPtr > this._reader.MCurrentBlockEnd)
                {
                    this._reader.MCurrentBlock = this._reader.MCurrentBlock->Next;
                    this._reader.MCurrentPtr = this._reader.MCurrentBlock->Data;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    this._remainingBlocks--;

                    this.CheckNotReadingOutOfBounds(size);

                    if (this._remainingBlocks <= 0)
                    {
                        this._reader.MCurrentBlockEnd = (byte*)this._reader.MCurrentBlock + this._reader.MLastBlockSize;
                    }
                    else
                    {
                        this._reader.MCurrentBlockEnd = (byte*)this._reader.MCurrentBlock + UnsafeEventStreamBlockData.AllocationSize;
                    }
#else
                    this.reader.m_CurrentBlockEnd = (byte*)this.reader.m_CurrentBlock + UnsafeEventStreamBlockData.AllocationSize;
#endif
                    ptr = this._reader.MCurrentPtr;
                    this._reader.MCurrentPtr += size;
                }

                return ptr;
            }

            /// <summary> Read data. </summary>
            /// <typeparam name="T"> The type of value. </typeparam>
            /// <returns> The returned data. </returns>
            public ref T Read<T>()
                where T : struct
            {
                var size = UnsafeUtility.SizeOf<T>();
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
            public ref T Peek<T>()
                where T : struct
            {
                int size = UnsafeUtility.SizeOf<T>();
                this.CheckReadSize(size);

                return ref this._reader.Peek<T>();
            }

            /// <summary>
            /// The current number of items in the container.
            /// </summary>
            /// <returns>The item count.</returns>
            public int Count()
            {
                this.CheckRead();
                return this._reader.Count();
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckNotReadingOutOfBounds(int size)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (this._remainingBlocks < 0)
                {
                    throw new System.ArgumentException("Reading out of bounds");
                }

                if (this._remainingBlocks == 0 && size + sizeof(void*) > this._reader.MLastBlockSize)
                {
                    throw new System.ArgumentException("Reading out of bounds");
                }
#endif
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckRead()
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(this._mSafety);
#endif
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckReadSize(int size)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(this._mSafety);

                Assert.IsTrue(size <= UnsafeEventStreamBlockData.AllocationSize - sizeof(void*));
                if (this._reader.MRemainingItemCount < 1)
                {
                    throw new ArgumentException("There are no more items left to be read.");
                }
#endif
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckBeginForEachIndex(int forEachIndex)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(this._mSafety);

                if ((uint)forEachIndex >= (uint)this._reader.MBlockStream->RangeCount)
                {
                    throw new System.ArgumentOutOfRangeException(
                        nameof(forEachIndex),
                        $"foreachIndex: {forEachIndex} must be between 0 and ForEachCount: {this._reader.MBlockStream->RangeCount}");
                }
#endif
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckEndForEachIndex()
            {
                if (this._reader.MRemainingItemCount != 0)
                {
                    throw new System.ArgumentException(
                        "Not all elements (Count) have been read. If this is intentional, simply skip calling EndForEachIndex();");
                }

                if (this._reader.MCurrentBlockEnd != this._reader.MCurrentPtr)
                {
                    throw new System.ArgumentException(
                        "Not all data (Data Size) has been read. If this is intentional, simply skip calling EndForEachIndex();");
                }
            }
        }
    }
}
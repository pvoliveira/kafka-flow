namespace KafkaFlow.Consumers
{
    using System;
    using System.Collections.Generic;
    using Confluent.Kafka;

    /// <summary>
    /// The consumer flow manager
    /// </summary>
    public interface IConsumerFlowManager : IDisposable
    {
        /// <summary>
        /// Gets the current consumer status
        /// </summary>
        ConsumerFlowStatus Status { get; }

        /// <summary>
        /// Gets a list of the consumer paused partitions
        /// </summary>
        IReadOnlyList<TopicPartition> PausedPartitions { get; }

        /// <summary>
        /// Pauses a set of partitions
        /// </summary>
        /// <param name="topicPartitions">A list of partitions</param>
        void Pause(IReadOnlyCollection<TopicPartition> topicPartitions);

        /// <summary>
        /// Resumes a set of partitions
        /// </summary>
        /// <param name="topicPartitions">A list of partitions</param>
        void Resume(IReadOnlyCollection<TopicPartition> topicPartitions);
    }
}

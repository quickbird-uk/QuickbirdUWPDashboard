namespace GnatMQ
{
    using uPLibrary.Networking.M2Mqtt;

    public interface IDirectPublish
    {
        /// <summary>
        /// Event that is fired on the threadpool when any message is published.
        /// </summary>
        event MqttBroker.DirectPublishEventHandler MessagePublished;
    }
}
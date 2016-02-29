using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Openfin.Desktop;

namespace embedd_wpf_demo
{
    /// <summary>
    /// A helper class for managing messages sent and received over the
    /// OpenFin interapplication bus for a single topic and remote UUID
    /// </summary>
    /// <remarks>
    /// This helper class provides a mechanism for sending and receiving a simple
    /// simple JSON data object over the InterApplicationBus. Objects are represented
    /// in JSON as a single field, "data", which contains a JSON object declaration.
    /// 
    /// This code is for demonstration purposes only.
    /// </remarks>
    class MessageChannel
    {
        #region Fields

        private Queue<JObject> _messageObjectQueue = new Queue<JObject>();

        #endregion

        #region Properties

        public InterApplicationBus InterApplicationBus { get; protected set; }

        public string RemoteUuid { get; protected set; }

        public string Topic { get; protected set; }

        public bool RemoteSideConnected { get; protected set; }

        #endregion

        #region Events

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        #endregion

        #region Constructors

        public MessageChannel(InterApplicationBus bus, string remoteUuid, string topic)
        {
            InterApplicationBus = bus;
            RemoteUuid = remoteUuid;
            Topic = topic;
            
            // This initialization assumption requires that the MessageChannel be created before
            // the given application UUID / topic has not yet subscribed on the remote end
            RemoteSideConnected = false; 

            InterApplicationBus.subscribe(remoteUuid, topic, MessageReceivedHandler);
            InterApplicationBus.addSubscribeListener(SubscriberAddedHandler);
            InterApplicationBus.addUnsubscribeListener(SuscriberRemovedHander);
        }

        #endregion

        #region Methods

        public void SendData(object data)
        {
            //package the data and send it over the inter application bus
            var message = JObject.FromObject(new DataObject() { Data = data });

            if (RemoteSideConnected)
            {
                InterApplicationBus.send(RemoteUuid, Topic, message);
            }
            else
            {
                _messageObjectQueue.Enqueue(message);
            }
        }

        private void MessageReceivedHandler(string sourceUuid, string topic, object message)
        {
            var messageObject = message as JObject;

            var dataObject = messageObject.ToObject<DataObject>();

            if(MessageReceived != null)
            {
                MessageReceived(this, new MessageReceivedEventArgs(sourceUuid, topic, dataObject.Data));
            }
        }

        private void SubscriberAddedHandler(string uuid, string topic)
        {
            if(RemoteUuid == uuid && Topic == topic)
            {
                RemoteSideConnected = true;

                foreach(var queuedMessage in _messageObjectQueue)
                {
                    InterApplicationBus.send(RemoteUuid, Topic, queuedMessage);
                }
            }
        }

        private void SuscriberRemovedHander(string uuid, string topic)
        {
            if (RemoteUuid == uuid && Topic == topic)
            {
                RemoteSideConnected = false;
            }
        }

        #endregion

        #region Classes

        public class MessageReceivedEventArgs : EventArgs
        {
            public string SourceUuid { get; protected set; }
            public string Topic { get; protected set; }
            public object Data { get; protected set; }

            public MessageReceivedEventArgs(string sourceUuid, string topic, object data)
            {
                SourceUuid = sourceUuid;
                Topic = topic;
                Data = data;
            }
        }

        protected class DataObject
        {
            [JsonProperty(PropertyName = "data")]
            public object Data { get; set; }
        }

        #endregion
    }
}

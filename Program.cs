using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

/*
 * 
 I could not find a good MQTT console app example the only one that work seemed to be one from a Windows form example combined with a managed client console application
that did not work.
https://dzone.com/articles/mqtt-publishing-and-subscribing-messages-to-mqtt-b
https://github.com/chkr1011/MQTTnet/wiki

* 
 * 
 */

namespace Fanuc13319
{
    class Program
    {
        // short _ret = 0;  // Stores our return value
        private IManagedMqttClient managedMqttClientPublisher;
        string mqttServer = "10.1.1.83";
        Node[] nodes;

        MqttFactory mqttFactory;
        /// <summary>
        /// The managed publisher client.
        /// </summary>
        //  static IManagedMqttClient managedMqttClientPublisher;


        static void Main(string[] args)
        {
            // ushort handle = 0;
            Program myObj = new Program();


            SettingsReader sr = new SettingsReader();
            myObj.nodes = (Node[])sr.LoadConfig(typeof(Node[]));

           // myObj.Test();
           // return;
            myObj.MQTTConnect();


            var timer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 60000
            };

            timer.Elapsed += myObj.TimerElapsed;


            // myObj.GetCounter(526);
            // System.Threading.Thread.Sleep( Timeout.InfiniteTimeSpan);
            // System.Threading.Thread.Sleep(35000);
            // Put in OnExit() cleanup handler
            Console.ReadLine();
            myObj.MQTTStop();
            myObj.FanucCleanup();
            // Focas1.cnc_freelibhndl(myObj.fanucHandle);  // Not sure if I should free this handle indescriminately



        }

        private void Test()
        {
            DateTime localDate = DateTime.Now;
            string transDate = localDate.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(transDate);
            MQTTConnect();
            foreach (Node node in nodes)
            {
                try
                {
                    decimal value = GetVariable(node.ip, node.variable);
                    // Only publish value if it has changed.
                    if (value != node.value)
                    {
                        node.value = value;
                        node.transDate = transDate;
                        MQTTPublish(node);
                    }
                   
                }
                catch (Exception e)
                {
                    // no cleanup 
                    continue;
                }

            }
            MQTTStop();

        }
        private void FanucCleanup()
        {
            // Focas1.cnc_freelibhndl(myObj.fanucHandle);  // Not sure if I should free this handle indescriminately
            Console.WriteLine("All Fanuc Handles have been closed.");

        }
        private async void MQTTStop()
        {
            await managedMqttClientPublisher.StopAsync();
            managedMqttClientPublisher = null;
            Console.WriteLine("MQTT Connection has been stopped");

        }

        private async void MQTTConnect()
        {
            mqttFactory = new MqttFactory();
            var tlsOptions = new MqttClientTlsOptions
            {
                UseTls = false,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                AllowUntrustedCertificates = true
            };
            var options = new MqttClientOptions
            {
                ClientId = "ClientPublisher",
                ProtocolVersion = MqttProtocolVersion.V311,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = "10.1.1.83",
                    Port = 1883,  // Test only
                    // Port = 1882,  // Production
                    TlsOptions = tlsOptions
                }
            };
            if (options.ChannelOptions == null)
            {
                throw new InvalidOperationException();
            }
            options.Credentials = new MqttClientCredentials
            {
                Username = "username",
                Password = Encoding.UTF8.GetBytes("password")
            };
            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromSeconds(5);
            managedMqttClientPublisher = mqttFactory.CreateManagedMqttClient();
            managedMqttClientPublisher.UseApplicationMessageReceivedHandler(HandleReceivedApplicationMessage);
            managedMqttClientPublisher.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnPublisherConnected);
            managedMqttClientPublisher.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnPublisherDisconnected);
            await managedMqttClientPublisher.StartAsync(
                new ManagedMqttClientOptions
                {
                    ClientOptions = options
                });


        }

        /// <summary>
        /// The method that handles the timer events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            DateTime localDate = DateTime.Now;
            string transDate = localDate.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(transDate);
            foreach (Node node in nodes)
            {
                try
                {
                    decimal value = GetVariable(node.ip, node.variable);
                    // Only publish value if it has changed.
                    if (value != node.value)
                    {
                        node.value = value;
                        node.transDate = transDate;
                        MQTTPublish(node);
                    }
                }
                catch (Exception ex)
                {
                    // no cleanup 
                    continue;
                }

            }

        }

        async void MQTTPublish(Node node)
        {
            try
            {
                string test = @"{
                  'updateId': 5,
                  'nodeId': 'ns=2;s=cnc362.cnc362.Cycle_Counter_Shift_SL',
                  'name':'Cycle_Counter_Shift_SL',
                  'plexus_Customer_No':'310507',
                  'pcn': 'Avilla',
                  'workcenter_Key': '61314',
                  'workcenter_Code': 'Honda Civic cnc 359 362',  
                  'cnc': '362',
                  'value': 0,
                  'transDate': '2020-06-29 00:00:00'
                }";

                Console.WriteLine("node.nodeId {0}", node.nodeId);
                Console.WriteLine("node.name {0}", node.name);
                // https://stackoverflow.com/questions/7574606/left-function-in-c-sharp/7574645



                // const transDate = moment(new Date()).format("YYYY-MM-DDTHH:mm:ss");
                // yyyy-MM
                string json2 = JsonConvert.SerializeObject(node);
                Console.WriteLine("json2=> {0}", json2);


                var payload = Encoding.UTF8.GetBytes(json2);

                //                var payload = Encoding.UTF8.GetBytes(partCounter);
                var message = new MqttApplicationMessageBuilder().WithTopic("Fanuc13319").WithPayload(payload).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag().Build();

                if (managedMqttClientPublisher != null)
                {
                    await managedMqttClientPublisher.PublishAsync(message);
                    Console.WriteLine("Published Message=>{0}", message);
                }
                else
                {
                    throw new Exception("MQTTPublish => Not connected to MQTT server");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex; //bubble up

            }

        }




        /* number is variable number to be read. 
         * GetCounter(526);//cnc362 10.1.90.4
           GetCounter(511);//cnc422 10.1.90.5
           GetCounter(501);//cnc422 10.1.90.2
         * 
         */
        decimal GetVariable(string ip,short variable)
        {
            short ret = -1;
            decimal value= -1;
            ushort fanucHandle = 0;

            try
            {
                ret = Focas1.cnc_allclibhndl3(ip, 8193, 6, out fanucHandle);
                if (ret == Focas1.EW_OK)
                {
                    Console.WriteLine("We are connected!");
                }
                else
                {
                    // Console.WriteLine("There was an error connecting. Return value: " + _ret);
                    throw new Exception("There was an error connecting. Return value: " + ret);
                }

                Focas1.ODBM macro = new Focas1.ODBM();
                string strVal;
                ret = Focas1.cnc_rdmacro(fanucHandle, variable, 10, macro);
                if (ret == Focas1.EW_OK)
                {
                    // mcr_val = 406000000
                    // dec_val = 6
                    // value = 406.000000
                    strVal = string.Format("{0:d9}", Math.Abs(macro.mcr_val));
                    if (0 < macro.dec_val) strVal = strVal.Insert(9 - macro.dec_val, ".");
                    if (macro.mcr_val < 0) strVal = "-" + strVal;
                    decimal decimalVal;
                    decimalVal = Convert.ToDecimal(strVal);
                    Console.WriteLine("String converted to decimal = {0} ", decimalVal);
                    value = decimalVal;
                    //partCounter = Convert.ToInt32(decimalVal);
                    //                    partCounter2 = macro.mcr_val;
                }
                else
                {
                    throw new Exception("There was an error reading the macro variable. Return value: " + ret);
                    // Console.WriteLine("**********");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in GetVariable => {0}", e);
                throw e;  // will bubble up after finally

            }
            finally
            {
                if (fanucHandle != 0)
                {
                    Focas1.cnc_freelibhndl(fanucHandle);
                    Console.WriteLine("Handle has been freed");
                }
            }
            return value;

        }

        /// <summary>
        /// Handles the publisher connected event.
        /// </summary>
        /// <param name="x">The MQTT client connected event args.</param>
        private static void OnPublisherConnected(MqttClientConnectedEventArgs x)
        {
            Console.WriteLine("Publisher Connected");
        }

        /// <summary>
        /// Handles the publisher disconnected event.
        /// </summary>
        /// <param name="x">The MQTT client disconnected event args.</param>
        private static void OnPublisherDisconnected(MqttClientDisconnectedEventArgs x)
        {
            Console.WriteLine("Publisher Disconnected");
        }

        /// <summary>
        /// Handles the received application message event.
        /// </summary>
        /// <param name="x">The MQTT application message received event args.</param>
        private void HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs x)
        {
            var item = $"Timestamp: {DateTime.Now:O} | Topic: {x.ApplicationMessage.Topic} | Payload: {x.ApplicationMessage.ConvertPayloadToString()} | QoS: {x.ApplicationMessage.QualityOfServiceLevel}";
            // this.BeginInvoke((MethodInvoker)delegate { this.TextBoxSubscriber.Text = item + Environment.NewLine + this.TextBoxSubscriber.Text; });
        }
    }

}
